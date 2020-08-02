using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_ExtractStack : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil extractStack = Toils_General.Wait(120, 0);
            ToilEffects.WithProgressBarToilDelay(extractStack, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(extractStack, TargetIndex.A);
            ToilFailConditions.FailOnCannotTouch<Toil>(extractStack, TargetIndex.A, PathEndMode.OnCell);
            yield return extractStack;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Corpse corpse = (Corpse)TargetThingA;
                    Hediff_CorticalStack hediff = corpse.InnerPawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => 
                    x.def.defName == "AC_CorticalStack") as Hediff_CorticalStack;
                    if (hediff != null)
                    {
                        if (hediff.def.spawnThingOnRemoved != null)
                        {
                            var corticalStack = ThingMaker.MakeThing(hediff.def.spawnThingOnRemoved) as CorticalStack;
                            if (hediff.hasPawn)
                            {
                                corticalStack.SavePawnFromHediff(hediff);
                            }
                            else
                            {
                                corticalStack.SavePawnToCorticalStack(corpse.InnerPawn);
                            }
                            GenPlace.TryPlaceThing(corticalStack, TargetThingA.Position, GetActor().Map, ThingPlaceMode.Near);
                            ACUtils.ACTracker.RegisterStack(corticalStack);
                            ACUtils.ACTracker.RegisterSleeve(corpse.InnerPawn);
                        }
                        corpse.InnerPawn.health.RemoveHediff(hediff);
                    }
                }
            };
        }
    }
}

