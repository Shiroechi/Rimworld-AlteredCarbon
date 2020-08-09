using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class Recipe_RemoveCorticalStack : Recipe_Surgery
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < allHediffs.Count; i++)
			{
				if (allHediffs[i].Part != null && allHediffs[i].def == recipe.removesHediff && allHediffs[i].Visible)
				{
					yield return allHediffs[i].Part;
				}
			}
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			MedicalRecipesUtility.IsClean(pawn, part);
			bool flag = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			if (billDoer != null)
			{
				if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
				if (!pawn.health.hediffSet.GetNotMissingParts().Contains(part))
				{
					return;
				}
				foreach (var h in pawn.health.hediffSet.hediffs)
                {
					if (h is Hediff_CorticalStack h2)
                    {
						Log.Message("HEDIFF: " + h2 + " - " + h2.gender, true);
                    }
                }
				var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
				if (hediff != null)
				{
					if (hediff.def.spawnThingOnRemoved != null)
					{
						Log.Message("HEDIFF.Gender: " + hediff.gender, true);
						var corticalStack = ThingMaker.MakeThing(hediff.def.spawnThingOnRemoved) as CorticalStack;
						Log.Message("hediff.gender 4: " + hediff.gender, true);
						Log.Message("corticalStack.gender 4: " + corticalStack.gender, true);

						hediff.SavePawn(pawn);
						corticalStack.SavePawnFromHediff(hediff);
						Log.Message("hediff.gender 3: " + hediff.gender, true);
						Log.Message("corticalStack.gender 3: " + corticalStack.gender, true);

						corticalStack.gender = hediff.gender;
						GenPlace.TryPlaceThing(corticalStack, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
						if (ACUtils.ACTracker.stacksIndex == null) ACUtils.ACTracker.stacksIndex = new Dictionary<string, CorticalStack>();
						ACUtils.ACTracker.stacksIndex[pawn.ThingID + pawn.Name] = corticalStack;
						ACUtils.ACTracker.RegisterSleeve(pawn);
						ACUtils.ACTracker.ReplacePawnWithStack(pawn, corticalStack);
					}
					pawn.health.RemoveHediff(hediff);
					ACUtils.ACTracker.deadPawns.Add(pawn);
				}
			}
			if (flag)
			{
				ReportViolation(pawn, billDoer, pawn.FactionOrExtraHomeFaction, -70, "GoodwillChangedReason_RemovedImplant".Translate(part.LabelShort));
			}
		}
	}
}

