using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

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
                    pawn.health.AddHediff(AlteredCarbonDefOf.AC_CorticalStack, neckRecord);
                    ACUtils.ACTracker.RegisterPawn(pawn);
                    ACUtils.ACTracker.pawnsWithStacks.Add(pawn);
                }
            }
        }
    }
}

