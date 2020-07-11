using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{

	[HarmonyPatch(typeof(Pawn_HealthTracker))]
	[HarmonyPatch("NotifyPlayerOfKilled")]
	internal static class DeadPawnMessageReplacement
	{
		private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, DamageInfo? dinfo, Hediff hediff, Caravan caravan)
		{
			var stackHediff = ___pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) =>
			x.def == AlteredCarbonDefOf.AC_CorticalStack);
			if (stackHediff != null)
			{
				TaggedString taggedString = "";
				taggedString = (dinfo.HasValue ? dinfo.Value.Def.deathMessage
					.Formatted(___pawn.LabelShortCap, ___pawn.Named("PAWN")) : ((hediff == null) 
					? "AlteredCarbon.PawnDied".Translate(___pawn.LabelShortCap, ___pawn.Named("PAWN")) 
					: "AlteredCarbon.PawnDiedBecauseOf".Translate(___pawn.LabelShortCap, hediff.def.LabelCap, 
					___pawn.Named("PAWN"))));
				taggedString = taggedString.AdjustedFor(___pawn);
				TaggedString label = "AlteredCarbon.SleeveDeath".Translate() + ": " + ___pawn.LabelShortCap;
				Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.NeutralEvent, ___pawn);
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike")]
	public class AppendThoughts_ForHumanlike_Patch
	{
		[HarmonyPrefix]
		public static bool Prefix(ref Pawn victim)
		{
			var stackHediff = victim.health.hediffSet.hediffs.FirstOrDefault((Hediff x) =>
				x.def == AlteredCarbonDefOf.AC_CorticalStack);
			if (stackHediff != null)
			{
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_Relations")]
	public class AppendThoughts_Relations_Patch
	{
		[HarmonyPrefix]
		public static bool Prefix(ref Pawn victim)
		{
			var stackHediff = victim.health.hediffSet.hediffs.FirstOrDefault((Hediff x) =>
				x.def == AlteredCarbonDefOf.AC_CorticalStack);
			if (stackHediff != null)
			{
				return false;
			}
			return true;
		}
	}
}

