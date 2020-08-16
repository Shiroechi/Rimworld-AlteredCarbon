using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using static RimWorld.QuestGen.QuestGen_Pawns;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("SpawnSetup")]
    public static class Patch_SpawnSetup
    {
        [HarmonyPostfix]
        public static void Postfix(Thing __instance)
        {
            if (__instance is Pawn pawn && pawn.RaceProps.Humanlike && pawn.kindDef
                .HasModExtension<StackSpawnModExtension>())
            {
                var extension = pawn.kindDef.GetModExtension<StackSpawnModExtension>();
                if (extension.SpawnsWithStack && Rand.Chance((float)extension.ChanceToSpawnWithStack / 100f))
                {
                    BodyPartRecord neckRecord = pawn.def.race.body.AllParts.FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Neck);
                    var hediff = HediffMaker.MakeHediff(AlteredCarbonDefOf.AC_CorticalStack, pawn, neckRecord) as Hediff_CorticalStack;
                    hediff.gender = pawn.gender;
                    pawn.health.AddHediff(hediff, neckRecord);
                    ACUtils.ACTracker.RegisterPawn(pawn);
                }
            }
        }
    }

    [HarmonyPatch(typeof(QuestGen_Pawns))]
    [HarmonyPatch("GetPawn")]
    public static class GetPawnSetup
    {
        [HarmonyPostfix]
        public static void Postfix(Quest quest, GetPawnParms parms, Pawn __result)
        {
            if (__result.kindDef == PawnKindDefOf.Empire_Royal_Bestower)
            {
                ThingOwner<Thing> innerContainer = __result.inventory.innerContainer;
                innerContainer.TryAdd(ThingMaker.MakeThing(AlteredCarbonDefOf.AC_EmptyCorticalStack), 1);
            }
        }
    }

    [HarmonyPatch(typeof(LordJob_BestowingCeremony))]
    [HarmonyPatch("FinishCeremony")]
    public static class FinishCeremony_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(LordJob_BestowingCeremony __instance, Pawn pawn)
        {
            if (__instance.bestower.kindDef == PawnKindDefOf.Empire_Royal_Bestower)
            {
                ThingOwner<Thing> innerContainer = __instance.bestower.inventory.innerContainer;
                for (int num = innerContainer.Count - 1; num >= 0; num--)
                {
                    if (innerContainer[num].def == AlteredCarbonDefOf.AC_EmptyCorticalStack)
                    {
                        innerContainer.TryDrop(innerContainer[num], ThingPlaceMode.Near, out Thing lastResultingThing);
                    }
                }
            }
        }
    }

    
    
}

