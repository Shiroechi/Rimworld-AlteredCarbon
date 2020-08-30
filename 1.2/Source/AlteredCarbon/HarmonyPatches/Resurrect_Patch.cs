using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{

    [StaticConstructorOnStartup]
    internal static class Resurrect_Patch
    {

        [HarmonyPatch(typeof(Pawn_HealthTracker), "Notify_Resurrected")]
        public static class Notify_Resurrected_Patch
        {
            public static void Postfix(Pawn ___pawn)
            {
                foreach (var stackGroup in ACUtils.ACTracker.stacksRelationships)
                {
                    Log.Message("stackGroup: " + stackGroup.Key, true);
                    Log.Message("originalPawn: " + stackGroup.Value.originalPawn + " - " + stackGroup.Value.originalPawn?.GetHashCode());
                    Log.Message("originalStack: " + stackGroup.Value.originalStack);
                    if (stackGroup.Value.copiedPawns != null)
                    {
                        foreach (var p in stackGroup.Value.copiedPawns)
                        {
                            Log.Message("copiedPawns: " + p);
                        }
                    }
                    if (stackGroup.Value.copiedStacks != null)
                    {
                        foreach (var p in stackGroup.Value.copiedStacks)
                        {
                            Log.Message("copiedStacks: " + p);
                        }
                    }
                    if (stackGroup.Value.deadPawns != null)
                    {
                        foreach (var p in stackGroup.Value.deadPawns)
                        {
                            Log.Message("deadPawns: " + p);
                        }
                    }

                    if (stackGroup.Value.deadPawns != null && stackGroup.Value.deadPawns.Contains(___pawn))
                    {
                        Log.Message("Found stackGroup");
                        stackGroup.Value.deadPawns.Remove(___pawn);
                        stackGroup.Value.copiedPawns.Remove(___pawn);
                        if (ACUtils.ACTracker.emptySleeves != null && ACUtils.ACTracker.emptySleeves.Contains(___pawn))
                        {
                            ACUtils.ACTracker.emptySleeves.Remove(___pawn);
                        }
                    }
                }
            }
        }
    }
}

