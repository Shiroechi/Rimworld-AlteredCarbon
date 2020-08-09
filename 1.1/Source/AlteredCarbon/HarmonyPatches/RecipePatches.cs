using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	[HarmonyPatch(typeof(RecipeDef), "AvailableOnNow")]
	public static class AvailableOnNow_Patch
	{
		private static bool Prefix(RecipeDef __instance, Thing thing, ref bool __result)
		{
			if (__instance == AlteredCarbonDefOf.AC_InstallEmptyCorticalStack && thing is Pawn pawn && pawn.IsEmptySleeve())
            {
				__result = false;
				return false;
            }
			return true;
		}
	}
}

