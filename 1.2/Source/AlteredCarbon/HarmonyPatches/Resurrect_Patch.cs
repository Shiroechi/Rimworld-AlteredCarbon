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
                    Log.Message("2 stackGroup: " + stackGroup.Key, true);
                    Log.Message("2 originalPawn: " + stackGroup.Value.originalPawn + " - " + stackGroup.Value.originalPawn?.GetHashCode());
                    Log.Message("2 originalStack: " + stackGroup.Value.originalStack);
                    if (stackGroup.Value.copiedPawns != null)
                    {
                        foreach (var p in stackGroup.Value.copiedPawns)
                        {
                            Log.Message("2 copiedPawns: " + p);
                        }
                    }
                    if (stackGroup.Value.copiedStacks != null)
                    {
                        foreach (var p in stackGroup.Value.copiedStacks)
                        {
                            Log.Message("2 copiedStacks: " + p);
                        }
                    }
                    if (stackGroup.Value.deadPawns != null)
                    {
                        foreach (var p in stackGroup.Value.deadPawns)
                        {
                            Log.Message("2 deadPawns: " + p);
                        }
                    }

                    if (stackGroup.Value.deadPawns != null && stackGroup.Value.deadPawns.Contains(___pawn))
                    {
                        Log.Message("Found stackGroup");
                        stackGroup.Value.deadPawns.Remove(___pawn);
                        stackGroup.Value.copiedPawns.Add(___pawn);
                        if (ACUtils.ACTracker.emptySleeves != null && ACUtils.ACTracker.emptySleeves.Contains(___pawn))
                        {
                            ACUtils.ACTracker.emptySleeves.Remove(___pawn);
                        }
                        ACUtils.ACTracker.TryAddRelationships(___pawn);
                    }
                }
            }
        }
    }
}

