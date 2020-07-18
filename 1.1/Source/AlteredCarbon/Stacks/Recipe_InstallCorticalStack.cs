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
				IEnumerable<Hediff> source = pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == record);
				if (source.Count() == 1 && source.First().def == recipe.addsHediff)
				{
					return false;
				}
				if (record.parent != null && !pawn.health.hediffSet.GetNotMissingParts().Contains(record.parent))
				{
					return false;
				}
				return (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) || pawn.health.hediffSet.HasDirectlyAddedPartFor(record)) ? true : false;
			});
		}

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            bool flag = MedicalRecipesUtility.IsClean(pawn, part);
            
            bool flag2 = !PawnGenerator.IsBeingGenerated(pawn) && IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    foreach (var i in ingredients)
                    {
                        if (i is CorticalStack c)
                        {
                            c.stackCount = 1;
                            Traverse.Create(c).Field("mapIndexOrState").SetValue((sbyte)-1);
                            GenPlace.TryPlaceThing(c, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                        }
                        Log.Message("2: " + i + " - " + i.Destroyed);
                    }
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, billDoer.Position, billDoer.Map);
                if (flag && flag2 && part.def.spawnThingOnRemoved != null)
                {
                    ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn);
                }
                if (flag2)
                {
                    ReportViolation(pawn, billDoer, pawn.FactionOrExtraHomeFaction, -70, "GoodwillChangedReason_NeedlesslyInstalledWorseBodyPart".Translate(recipe.addsHediff.label));
                }
            }
            else if (pawn.Map != null)
            {
                MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, pawn.Position, pawn.Map);
            }
            else
            {
                pawn.health.RestorePart(part);
            }
            var thing = ingredients.Where(x => x is CorticalStack).FirstOrDefault();
            if (thing is CorticalStack corticalStack)
            {
                var hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn) as Hediff_CorticalStack;
                hediff.stackGroupID = corticalStack.stackGroupID;
                pawn.health.AddHediff(recipe.addsHediff, part);
                if (corticalStack.hasPawn)
                {
                    if (pawn.IsColonist)
                    {
                        Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
                    }
                    PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, null, PawnDiedOrDownedThoughtsKind.Died);
                    pawn.health.NotifyPlayerOfKilled(null, null, null);
                    ACUtils.ACTracker.stacksIndex.Remove(corticalStack.pawnID + corticalStack.name);
            
                    corticalStack.OverwritePawn(pawn);
            
                    ACUtils.ACTracker.ReplaceStackWithPawn(corticalStack, pawn);
            
                    var naturalMood = pawn.story.traits.GetTrait(TraitDefOf.NaturalMood);
                    var nerves = pawn.story.traits.GetTrait(TraitDefOf.Nerves);
            
                    if ((naturalMood != null && naturalMood.Degree == -2)
                            || pawn.story.traits.HasTrait(TraitDefOf.BodyPurist)
                            || (nerves != null && nerves.Degree == -2)
                            || pawn.story.traits.HasTrait(AlteredCarbonDefOf.AC_Sleever))
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeveDouble);
                    }
                    else
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeve);
                    }
            
                    if (pawn.story.traits.HasTrait(TraitDefOf.DislikesMen) && pawn.gender == Gender.Male)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_MansBody);
                    }
                    if (pawn.story.traits.HasTrait(TraitDefOf.DislikesWomen) && pawn.gender == Gender.Female)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WomansBody);
                    }
                }
                else
                {
                    ACUtils.ACTracker.RegisterPawn(pawn);
                }
                if (pawn.story.traits.HasTrait(AlteredCarbonDefOf.AC_AntiStack))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMySoul);
                }
                ACUtils.ACTracker.pawnsWithStacks.Add(pawn);
                ACUtils.ACTracker.TryAddRelationships(pawn);
            }
        }

		public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			if ((pawn.Faction == billDoerFaction || pawn.Faction == null) && !pawn.IsQuestLodger())
			{
				return false;
			}
			if (recipe.addsHediff.addedPartProps != null && recipe.addsHediff.addedPartProps.betterThanNatural)
			{
				return false;
			}
			return HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;
		}
	}
}

