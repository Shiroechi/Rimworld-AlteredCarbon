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
				var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
				if (hediff != null)
				{
					if (hediff.def.spawnThingOnRemoved != null)
					{
						var corticalStack = ThingMaker.MakeThing(hediff.def.spawnThingOnRemoved) as CorticalStack;
						corticalStack.SavePawnToCorticalStack(pawn);
						GenPlace.TryPlaceThing(corticalStack, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
						if (ACUtils.ACTracker.stacksIndex == null) ACUtils.ACTracker.stacksIndex = new Dictionary<string, CorticalStack>();
						ACUtils.ACTracker.stacksIndex[pawn.ThingID + pawn.Name] = corticalStack;
						ACUtils.ACTracker.pawnsWithStacks.Remove(pawn);
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

