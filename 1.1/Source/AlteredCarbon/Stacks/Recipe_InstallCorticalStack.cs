using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_InstallCorticalStack : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
            {
                Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - GetPartsToApplyOn - if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record)) - 1", true);
                if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record))
                {
                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - GetPartsToApplyOn - return false; - 2", true);
                    return false;
                }
                Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - GetPartsToApplyOn - if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record)) - 3", true);
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
                {
                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - GetPartsToApplyOn - return false; - 4", true);
                    return false;
                }
                return (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && (x.def == recipe.addsHediff || !recipe.CompatibleWithHediff(x.def)))) ? true : false;
                Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - GetPartsToApplyOn - }); - 6", true);
            });
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - if (billDoer != null) - 7", true);
            if (billDoer != null)
            {
                Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill)) - 8", true);
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - foreach (var i in ingredients) - 9", true);
                    foreach (var i in ingredients)
                    {
                        Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - if (i is CorticalStack c) - 10", true);
                        if (i is CorticalStack c)
                        {
                            Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - c.stackCount = 1; - 11", true);
                            c.stackCount = 1;
                            Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - Traverse.Create(c).Field(\"mapIndexOrState\").SetValue((sbyte)-1); - 12", true);
                            Traverse.Create(c).Field("mapIndexOrState").SetValue((sbyte)-1);
                            Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - GenPlace.TryPlaceThing(c, billDoer.Position, billDoer.Map, ThingPlaceMode.Near); - 13", true);
                            GenPlace.TryPlaceThing(c, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                        }
                    }
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }
            var thing = ingredients.Where(x => x is CorticalStack).FirstOrDefault();
            Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - if (thing is CorticalStack corticalStack) - 17", true);
            if (thing is CorticalStack corticalStack)
            {
                Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - var hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn) as Hediff_CorticalStack; - 18", true);
                var hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn) as Hediff_CorticalStack;
                Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - hediff.stackGroupID = corticalStack.stackGroupID; - 19", true);
                hediff.stackGroupID = corticalStack.stackGroupID;
                Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - pawn.health.AddHediff(recipe.addsHediff, part); - 20", true);
                pawn.health.AddHediff(recipe.addsHediff, part);
                Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - if (corticalStack.hasPawn) - 21", true);
                if (corticalStack.hasPawn)
                {
                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - if (pawn.IsColonist) - 22", true);
                    if (pawn.IsColonist)
                    {
                        Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - Find.StoryWatcher.statsRecord.Notify_ColonistKilled(); - 23", true);
                        Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
                    }
                    PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, null, PawnDiedOrDownedThoughtsKind.Died);
                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - pawn.health.NotifyPlayerOfKilled(null, null, null); - 25", true);
                    pawn.health.NotifyPlayerOfKilled(null, null, null);
                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - ACUtils.ACTracker.stacksIndex.Remove(corticalStack.pawnID + corticalStack.name); - 26", true);
                    ACUtils.ACTracker.stacksIndex.Remove(corticalStack.pawnID + corticalStack.name);
                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - corticalStack.OverwritePawn(pawn); - 27", true);
                    corticalStack.OverwritePawn(pawn);
                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - ACUtils.ACTracker.ReplaceStackWithPawn(corticalStack, pawn); - 28", true);
                    ACUtils.ACTracker.ReplaceStackWithPawn(corticalStack, pawn);

                    var naturalMood = pawn.story.traits.GetTrait(TraitDefOf.NaturalMood);
                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - var nerves = pawn.story.traits.GetTrait(TraitDefOf.Nerves); - 30", true);
                    var nerves = pawn.story.traits.GetTrait(TraitDefOf.Nerves);

                    Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - if ((naturalMood != null && naturalMood.Degree == -2) - 31", true);
                    if ((naturalMood != null && naturalMood.Degree == -2)
                            || pawn.story.traits.HasTrait(TraitDefOf.BodyPurist)
                            || (nerves != null && nerves.Degree == -2)
                            || pawn.story.traits.HasTrait(AlteredCarbonDefOf.AC_Sleever))
                    {
                        Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeveDouble); - 32", true);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeveDouble);
                    }
                    else
                    {
                        Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeve); - 33", true);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeve);
                    }
                }

		ACUtils.ACTracker.RegisterPawn(pawn);
                Log.Message("Recipe_InstallCorticalStack : Recipe_Surgery - ApplyOnPawn - ACUtils.ACTracker.TryAddRelationships(pawn); - 36", true);
                ACUtils.ACTracker.TryAddRelationships(pawn);
            }
        }
    }
}
