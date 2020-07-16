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
            Log.Message(" - ApplyOnPawn - bool flag = MedicalRecipesUtility.IsClean(pawn, part); - 1", true);
            bool flag = MedicalRecipesUtility.IsClean(pawn, part);

            bool flag2 = !PawnGenerator.IsBeingGenerated(pawn) && IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            Log.Message(" - ApplyOnPawn - if (billDoer != null) - 3", true);
            if (billDoer != null)
            {
                Log.Message(" - ApplyOnPawn - if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill)) - 4", true);
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    Log.Message(" - ApplyOnPawn - foreach (var i in ingredients) - 5", true);
                    foreach (var i in ingredients)
                    {
                        Log.Message(" - ApplyOnPawn - if (i is CorticalStack c) - 6", true);
                        if (i is CorticalStack c)
                        {
                            Log.Message(" - ApplyOnPawn - c.stackCount = 1; - 7", true);
                            c.stackCount = 1;
                            Log.Message(" - ApplyOnPawn - Traverse.Create(c).Field(\"mapIndexOrState\").SetValue((sbyte)-1); - 8", true);
                            Traverse.Create(c).Field("mapIndexOrState").SetValue((sbyte)-1);
                            Log.Message(" - ApplyOnPawn - GenPlace.TryPlaceThing(c, billDoer.Position, billDoer.Map, ThingPlaceMode.Near); - 9", true);
                            GenPlace.TryPlaceThing(c, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                        }
                        Log.Message("2: " + i + " - " + i.Destroyed);
                    }
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                Log.Message(" - ApplyOnPawn - MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, billDoer.Position, billDoer.Map); - 13", true);
                MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, billDoer.Position, billDoer.Map);
                Log.Message(" - ApplyOnPawn - if (flag && flag2 && part.def.spawnThingOnRemoved != null) - 14", true);
                if (flag && flag2 && part.def.spawnThingOnRemoved != null)
                {
                    Log.Message(" - ApplyOnPawn - ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn); - 15", true);
                    ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn);
                }
                Log.Message(" - ApplyOnPawn - if (flag2) - 16", true);
                if (flag2)
                {
                    Log.Message(" - ApplyOnPawn - ReportViolation(pawn, billDoer, pawn.FactionOrExtraHomeFaction, -70, \"GoodwillChangedReason_NeedlesslyInstalledWorseBodyPart\".Translate(recipe.addsHediff.label)); - 17", true);
                    ReportViolation(pawn, billDoer, pawn.FactionOrExtraHomeFaction, -70, "GoodwillChangedReason_NeedlesslyInstalledWorseBodyPart".Translate(recipe.addsHediff.label));
                }
            }
            else if (pawn.Map != null)
            {
                Log.Message(" - ApplyOnPawn - MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, pawn.Position, pawn.Map); - 19", true);
                MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, pawn.Position, pawn.Map);
            }
            else
            {
                Log.Message(" - ApplyOnPawn - pawn.health.RestorePart(part); - 20", true);
                pawn.health.RestorePart(part);
            }
            var thing = ingredients.Where(x => x is CorticalStack).FirstOrDefault();
            Log.Message(" - ApplyOnPawn - if (thing is CorticalStack corticalStack) - 22", true);
            if (thing is CorticalStack corticalStack)
            {
                Log.Message(" - ApplyOnPawn - var hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn) as Hediff_CorticalStack; - 23", true);
                var hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn) as Hediff_CorticalStack;
                Log.Message(" - ApplyOnPawn - hediff.stackGroupID = corticalStack.stackGroupID; - 24", true);
                hediff.stackGroupID = corticalStack.stackGroupID;
                Log.Message(" - ApplyOnPawn - pawn.health.AddHediff(recipe.addsHediff, part); - 25", true);
                pawn.health.AddHediff(recipe.addsHediff, part);
                Log.Message(" - ApplyOnPawn - if (corticalStack.hasPawn) - 26", true);
                if (corticalStack.hasPawn)
                {
                    Log.Message(" - ApplyOnPawn - if (pawn.IsColonist) - 27", true);
                    if (pawn.IsColonist)
                    {
                        Log.Message(" - ApplyOnPawn - Find.StoryWatcher.statsRecord.Notify_ColonistKilled(); - 28", true);
                        Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
                    }
                    PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, null, PawnDiedOrDownedThoughtsKind.Died);
                    Log.Message(" - ApplyOnPawn - pawn.health.NotifyPlayerOfKilled(null, null, null); - 30", true);
                    pawn.health.NotifyPlayerOfKilled(null, null, null);
                    Log.Message(" - ApplyOnPawn - ACUtils.ACTracker.stacksIndex.Remove(corticalStack.pawnID + corticalStack.name); - 31", true);
                    ACUtils.ACTracker.stacksIndex.Remove(corticalStack.pawnID + corticalStack.name);

                    corticalStack.OverwritePawn(pawn);
                    Log.Message(" - ApplyOnPawn - ACUtils.ACTracker.ReplaceStackWithPawn(corticalStack, pawn); - 33", true);
                    ACUtils.ACTracker.ReplaceStackWithPawn(corticalStack, pawn);
                    Log.Message(" - ApplyOnPawn - var naturalMood = pawn.story.traits.GetTrait(TraitDefOf.NaturalMood); - 34", true);
                    var naturalMood = pawn.story.traits.GetTrait(TraitDefOf.NaturalMood);
                    Log.Message(" - ApplyOnPawn - var nerves = pawn.story.traits.GetTrait(TraitDefOf.Nerves); - 35", true);
                    var nerves = pawn.story.traits.GetTrait(TraitDefOf.Nerves);

                    Log.Message(" - ApplyOnPawn - if ((naturalMood != null && naturalMood.Degree == -2) - 36", true);
                    if ((naturalMood != null && naturalMood.Degree == -2)
                            || pawn.story.traits.HasTrait(TraitDefOf.BodyPurist)
                            || (nerves != null && nerves.Degree == -2)
                            || pawn.story.traits.HasTrait(AlteredCarbonDefOf.AC_Sleever))
                    {
                        Log.Message(" - ApplyOnPawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeveDouble); - 37", true);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeveDouble);
                    }
                    else
                    {
                        Log.Message(" - ApplyOnPawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeve); - 38", true);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeve);
                    }

                    Log.Message(" - ApplyOnPawn - if (pawn.story.traits.HasTrait(TraitDefOf.DislikesMen) && pawn.gender == Gender.Male) - 39", true);
                    if (pawn.story.traits.HasTrait(TraitDefOf.DislikesMen) && pawn.gender == Gender.Male)
                    {
                        Log.Message(" - ApplyOnPawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_MansBody); - 40", true);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_MansBody);
                    }
                    Log.Message(" - ApplyOnPawn - if (pawn.story.traits.HasTrait(TraitDefOf.DislikesWomen) && pawn.gender == Gender.Female) - 41", true);
                    if (pawn.story.traits.HasTrait(TraitDefOf.DislikesWomen) && pawn.gender == Gender.Female)
                    {
                        Log.Message(" - ApplyOnPawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WomansBody); - 42", true);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WomansBody);
                    }
                }
                else
                {
                    Log.Message(" - ApplyOnPawn - ACUtils.ACTracker.RegisterPawn(pawn); - 43", true);
                    ACUtils.ACTracker.RegisterPawn(pawn);
                }
                Log.Message(" - ApplyOnPawn - if (pawn.story.traits.HasTrait(AlteredCarbonDefOf.AC_AntiStack)) - 44", true);
                if (pawn.story.traits.HasTrait(AlteredCarbonDefOf.AC_AntiStack))
                {
                    Log.Message(" - ApplyOnPawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMySoul); - 45", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMySoul);
                }
                ACUtils.ACTracker.pawnsWithStacks.Add(pawn);
                Log.Message(" - ApplyOnPawn - ACUtils.ACTracker.TryAddRelationships(pawn); - 47", true);
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
