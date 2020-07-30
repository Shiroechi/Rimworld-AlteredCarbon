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

    [HarmonyPatch(typeof(PawnTechHediffsGenerator))]
    [HarmonyPatch("GenerateTechHediffsFor")]
    public static class Patch_GenerateTechHediffsFor
    {
        public static bool Prefix(Pawn pawn, List<ThingDef> ___tmpGeneratedTechHediffsList)
        {
            Log.Message(" - Prefix - float partsMoney = pawn.kindDef.techHediffsMoney.RandomInRange; - 1", true);
            float partsMoney = pawn.kindDef.techHediffsMoney.RandomInRange;
            Log.Message(" - Prefix - int num = pawn.kindDef.techHediffsMaxAmount; - 2", true);
            int num = pawn.kindDef.techHediffsMaxAmount;
            Log.Message(" - Prefix - if (pawn.kindDef.techHediffsRequired != null) - 3", true);
            if (pawn.kindDef.techHediffsRequired != null)
            {
                Log.Message(" - Prefix - foreach (ThingDef item in pawn.kindDef.techHediffsRequired) - 4", true);
                foreach (ThingDef item in pawn.kindDef.techHediffsRequired)
                {
                    Log.Message(" - Prefix - partsMoney -= item.BaseMarketValue; - 5", true);
                    partsMoney -= item.BaseMarketValue;
                    Log.Message(" - Prefix - num--; - 6", true);
                    num--;
                    //PawnTechHediffsGenerator.InstallPart(pawn, item);
                }
            }
            Log.Message(" - Prefix - if (pawn.kindDef.techHediffsTags == null || pawn.kindDef.techHediffsChance <= 0f) - 8", true);
            if (pawn.kindDef.techHediffsTags == null || pawn.kindDef.techHediffsChance <= 0f)
            {
                Log.Message(" - Prefix - return false; - 9", true);
                return false;
            }
            ___tmpGeneratedTechHediffsList.Clear();
            for (int i = 0; i < num; i++)
            {
                Log.Message(" - Prefix - if (Rand.Value > pawn.kindDef.techHediffsChance) - 11", true);
                if (Rand.Value > pawn.kindDef.techHediffsChance)
                {
                    Log.Message(" - Prefix - break; - 12", true);
                    break;
                }
                IEnumerable<ThingDef> source = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.isTechHediff && !___tmpGeneratedTechHediffsList.Contains(x) && x.BaseMarketValue <= partsMoney && x.techHediffsTags != null && pawn.kindDef.techHediffsTags.Any((string tag) => x.techHediffsTags.Contains(tag)) && (pawn.kindDef.techHediffsDisallowTags == null || !pawn.kindDef.techHediffsDisallowTags.Any((string tag) => x.techHediffsTags.Contains(tag))));
                Log.Message(" - Prefix - if (source.Any()) - 14", true);
                if (source.Any())
                {
                    foreach (var t in source)
                    {
                        Log.Message("Source: " + t, true);
                    }
                    Log.Message(" - Prefix - ThingDef thingDef = source.RandomElementByWeight((ThingDef w) => w.BaseMarketValue); - 15", true);
                    ThingDef thingDef = source.RandomElementByWeight((ThingDef w) => w.BaseMarketValue);
                    Log.Message(" - Prefix - partsMoney -= thingDef.BaseMarketValue; - 16", true);
                    Log.Message("thingDef: " + thingDef, true);

                    partsMoney -= thingDef.BaseMarketValue;
                    //PawnTechHediffsGenerator.InstallPart(pawn, thingDef);
                    Log.Message(" - Prefix - ___tmpGeneratedTechHediffsList.Add(thingDef); - 18", true);
                    ___tmpGeneratedTechHediffsList.Add(thingDef);
                }
            }
            return false;
        }
    }
    
}

