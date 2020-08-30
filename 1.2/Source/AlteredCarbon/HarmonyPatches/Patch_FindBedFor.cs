using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
	public static class BedPatches
    {
		//static BedPatches()
        //{
		//	var
		//	NeedInterval
		//
		//}

    }
	//[HarmonyPatch(typeof(RestUtility), "FindBedFor")]
	//[HarmonyPatch(new Type[]
	//	{
	//		typeof(Pawn),
	//		typeof(Pawn),
	//		typeof(bool),
	//		typeof(bool),
	//		typeof(bool)
	//	})]
	//public static class RestUtility_FindBedFor_Patch
	//{
	//	[HarmonyPrefix]
	//	public static bool FindBedFor_Pre(Pawn sleeper, Pawn traveler, ref Building_Bed __result, ref List<ThingDef> ___bedDefsBestToWorst_RestEffectiveness, ref List<ThingDef> ___bedDefsBestToWorst_Medical)
	//	{
	//		___bedDefsBestToWorst_RestEffectiveness.Remove(AlteredCarbonDefOf.AC_SleeveCasket);
	//		___bedDefsBestToWorst_Medical.Remove(AlteredCarbonDefOf.AC_SleeveCasket);
	//		if (sleeper.IsEmptySleeve())
	//		{
	//			var bed = FindBedForSleeve(sleeper, traveler);
	//			if (bed != null)
    //            {
	//				__result = bed;
	//				return false;
    //            }
	//		}
	//		return true;
	//	}
	//	public static Building_Bed FindBedForSleeve(Pawn sleeper, Pawn traveler)
	//	{
	//		Building_Bed building_Bed2 = (Building_Bed)GenClosest.ClosestThingReachable(sleeper.Position, sleeper.Map, 
	//			ThingRequest.ForDef(AlteredCarbonDefOf.AC_SleeveCasket), PathEndMode.OnCell, TraverseParms.For(traveler), 9999f, (Thing b) => 
	//			(int)b.Position.GetDangerFor(sleeper, sleeper.Map) <= (int)Danger.Deadly && RestUtility.IsValidBedFor(b, sleeper, traveler, false, false));
	//		if (building_Bed2 != null)
	//		{
	//			return building_Bed2;
	//		}
	//		return null;
	//	}
	//}
	//
	//[HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
	//internal static class RestUtility_Bed_IsValidBedFor_Patch
	//{
	//	[HarmonyPostfix]
	//	public static void Postfix(Thing bedThing, Pawn sleeper, Pawn traveler, ref bool __result)
	//	{
	//		Log.Message("sleeper: " + sleeper, true);
	//		if (!sleeper.IsEmptySleeve() && bedThing.def == AlteredCarbonDefOf.AC_SleeveCasket)
    //        {
	//			__result = false;
    //        }
	//	}
	//}
	////
	////[HarmonyPatch(typeof(RestUtility), "Reset")]
	////internal static class RestUtility_Reset_Postfix
	////{
	////	[HarmonyPostfix]
	////	private static void Reset(ref List<ThingDef> ___bedDefsBestToWorst_RestEffectiveness, ref List<ThingDef> ___bedDefsBestToWorst_Medical)
	////	{
	////		___bedDefsBestToWorst_RestEffectiveness.Remove(AlteredCarbonDefOf.AC_SleeveCasket);
	////		___bedDefsBestToWorst_Medical.Remove(AlteredCarbonDefOf.AC_SleeveCasket);
	////		foreach (var t in ___bedDefsBestToWorst_RestEffectiveness)
    ////        {
	////			Log.Message("BED: " + t, true);
    ////        }
	////	}
	////}
}

