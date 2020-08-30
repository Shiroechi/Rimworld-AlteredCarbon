using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class AlteredCarbonManager : GameComponent
    {
        public AlteredCarbonManager()
        {

        }

        public AlteredCarbonManager(Game game)
        {

        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 60 == 0 && this.stacksRelationships != null)
            {
                foreach (var stackGroup in this.stacksRelationships)
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
                }

                Log.Message("-----------", true);
            }
        }
        public void ResetStackLimitIfNeeded(ThingDef def)
        {
            Log.Message("AlteredCarbonManager : GameComponent - ResetStackLimitIfNeeded - if (def.stackLimit > 1) - 17", true);
            if (def.stackLimit > 1)
            {
                Log.Message("AlteredCarbonManager : GameComponent - ResetStackLimitIfNeeded - def.stackLimit = 1; - 18", true);
                def.stackLimit = 1;
                Log.Message("AlteredCarbonManager : GameComponent - ResetStackLimitIfNeeded - def.drawGUIOverlay = false; - 19", true);
                def.drawGUIOverlay = false;
            }
        }
        public override void StartedNewGame()
        {
            Log.Message("AlteredCarbonManager : GameComponent - StartedNewGame - base.StartedNewGame(); - 20", true);
            base.StartedNewGame();
            Log.Message("AlteredCarbonManager : GameComponent - StartedNewGame - ACUtils.ResetACTracker(); - 21", true);
            ACUtils.ResetACTracker();
            Log.Message("AlteredCarbonManager : GameComponent - StartedNewGame - if (this.stacksIndex == null) this.stacksIndex = new Dictionary<string, CorticalStack>(); - 22", true);
            if (this.stacksIndex == null) this.stacksIndex = new Dictionary<string, CorticalStack>();
            Log.Message("AlteredCarbonManager : GameComponent - StartedNewGame - if (this.pawnsWithStacks == null) this.pawnsWithStacks = new HashSet<Pawn>(); - 23", true);
            if (this.pawnsWithStacks == null) this.pawnsWithStacks = new HashSet<Pawn>();
            Log.Message("AlteredCarbonManager : GameComponent - StartedNewGame - if (this.emptySleeves == null) this.emptySleeves = new HashSet<Pawn>(); - 24", true);
            if (this.emptySleeves == null) this.emptySleeves = new HashSet<Pawn>();
            Log.Message("AlteredCarbonManager : GameComponent - StartedNewGame - if (this.deadPawns == null) this.deadPawns = new HashSet<Pawn>(); - 25", true);
            if (this.deadPawns == null) this.deadPawns = new HashSet<Pawn>();
            Log.Message("AlteredCarbonManager : GameComponent - StartedNewGame - ResetStackLimitIfNeeded(AlteredCarbonDefOf.AC_FilledCorticalStack); - 26", true);
            ResetStackLimitIfNeeded(AlteredCarbonDefOf.AC_FilledCorticalStack);
        }

        public override void LoadedGame()
        {
            Log.Message("AlteredCarbonManager : GameComponent - LoadedGame - base.LoadedGame(); - 27", true);
            base.LoadedGame();
            Log.Message("AlteredCarbonManager : GameComponent - LoadedGame - ACUtils.ResetACTracker(); - 28", true);
            ACUtils.ResetACTracker();
            Log.Message("AlteredCarbonManager : GameComponent - LoadedGame - if (this.stacksIndex == null) this.stacksIndex = new Dictionary<string, CorticalStack>(); - 29", true);
            if (this.stacksIndex == null) this.stacksIndex = new Dictionary<string, CorticalStack>();
            Log.Message("AlteredCarbonManager : GameComponent - LoadedGame - if (this.pawnsWithStacks == null) this.pawnsWithStacks = new HashSet<Pawn>(); - 30", true);
            if (this.pawnsWithStacks == null) this.pawnsWithStacks = new HashSet<Pawn>();
            Log.Message("AlteredCarbonManager : GameComponent - LoadedGame - if (this.emptySleeves == null) this.emptySleeves = new HashSet<Pawn>(); - 31", true);
            if (this.emptySleeves == null) this.emptySleeves = new HashSet<Pawn>();
            Log.Message("AlteredCarbonManager : GameComponent - LoadedGame - if (this.deadPawns == null) this.deadPawns = new HashSet<Pawn>(); - 32", true);
            if (this.deadPawns == null) this.deadPawns = new HashSet<Pawn>();
            Log.Message("AlteredCarbonManager : GameComponent - LoadedGame - ResetStackLimitIfNeeded(AlteredCarbonDefOf.AC_FilledCorticalStack); - 33", true);
            ResetStackLimitIfNeeded(AlteredCarbonDefOf.AC_FilledCorticalStack);
        }

        public void TryAddRelationships(Pawn pawn)
        {
            Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 34", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (hediff != null && this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 35", true);
            if (hediff != null && this.stacksRelationships.ContainsKey(hediff.stackGroupID))
            {
                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (this.stacksRelationships[hediff.stackGroupID].originalPawn != null) - 36", true);
                if (this.stacksRelationships[hediff.stackGroupID].originalPawn != null)
                {
                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn != this.stacksRelationships[hediff.stackGroupID].originalPawn) - 37", true);
                    if (pawn != this.stacksRelationships[hediff.stackGroupID].originalPawn)
                    {
                        pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original,
                            this.stacksRelationships[hediff.stackGroupID].originalPawn);
                        this.stacksRelationships[hediff.stackGroupID]
                            .originalPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                    }
                    else if (this.stacksRelationships[hediff.stackGroupID].copiedPawns != null)
                    {
                        Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns) - 41", true);
                        foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns)
                        {
                            Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn != copiedPawn) - 42", true);
                            if (pawn != copiedPawn)
                            {
                                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original, copiedPawn); - 43", true);
                                pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original, copiedPawn);
                                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn); - 44", true);
                                copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                            }
                        }
                    }
                }
                else if (this.stacksRelationships[hediff.stackGroupID].copiedPawns != null)
                {
                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns) - 46", true);
                    foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns)
                    {
                        Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn != copiedPawn) - 47", true);
                        if (pawn != copiedPawn)
                        {
                            Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, copiedPawn); - 48", true);
                            pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, copiedPawn);
                            Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn); - 49", true);
                            copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                        }
                    }
                }
            }
            else
            {
                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - foreach (var stackGroup in this.stacksRelationships) - 50", true);
                foreach (var stackGroup in this.stacksRelationships)
                {
                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (stackGroup.Value.copiedPawns != null) - 51", true);
                    if (stackGroup.Value.copiedPawns != null)
                    {
                        Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn == stackGroup.Value.originalPawn && stackGroup.Value.copiedPawns != null) - 52", true);
                        if (pawn == stackGroup.Value.originalPawn && stackGroup.Value.copiedPawns != null)
                        {
                            Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - foreach (var copiedPawn in stackGroup.Value.copiedPawns) - 53", true);
                            foreach (var copiedPawn in stackGroup.Value.copiedPawns)
                            {
                                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn != copiedPawn) - 54", true);
                                if (pawn != copiedPawn)
                                {
                                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original, copiedPawn); - 55", true);
                                    pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original, copiedPawn);
                                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn); - 56", true);
                                    copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                                }
                            }
                        }
                        else if (stackGroup.Value.copiedPawns != null)
                        {
                            Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - foreach (var copiedPawn in stackGroup.Value.copiedPawns) - 58", true);
                            foreach (var copiedPawn in stackGroup.Value.copiedPawns)
                            {
                                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn == copiedPawn && stackGroup.Value.originalPawn != null) - 59", true);
                                if (pawn == copiedPawn && stackGroup.Value.originalPawn != null)
                                {
                                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original, stackGroup.Value.originalPawn); - 60", true);
                                    pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original, stackGroup.Value.originalPawn);
                                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - stackGroup.Value.originalPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn); - 61", true);
                                    stackGroup.Value.originalPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                                }
                            }
                        }
                    }
                }
            }
            Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn.IsCopy()) - 62", true);
            if (pawn.IsCopy())
            {
                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_JustCopy); - 63", true);
                pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_JustCopy);
                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - var otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse); - 64", true);
                var otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherPawn)) - 65", true);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherPawn))
                {
                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMySpouse, otherPawn); - 66", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMySpouse, otherPawn);
                }

                otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, otherPawn)) - 68", true);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, otherPawn))
                {
                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyFiance, otherPawn); - 69", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyFiance, otherPawn);
                }

                otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);

                Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherPawn)) - 71", true);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherPawn))
                {
                    Log.Message("AlteredCarbonManager : GameComponent - TryAddRelationships - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyLover, otherPawn); - 72", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyLover, otherPawn);
                }
            }
        }

        public void ReplacePawnWithStack(Pawn pawn, CorticalStack stack)
        {
            Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>(); - 73", true);
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 74", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - if (hediff != null) - 75", true);
            if (hediff != null)
            {
                Log.Message("1 stack.stackGroupID: " + stack.stackGroupID, true);
                Log.Message("1 hediff.stackGroupID: " + hediff.stackGroupID, true);

                stack.stackGroupID = hediff.stackGroupID;
                Log.Message("2 stack.stackGroupID: " + stack.stackGroupID, true);
                Log.Message("2 hediff.stackGroupID: " + hediff.stackGroupID, true);

                Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - if (this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 81", true);
                if (this.stacksRelationships.ContainsKey(hediff.stackGroupID))
                {
                    Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - if (this.stacksRelationships[hediff.stackGroupID].originalPawn == pawn) - 82", true);
                    if (this.stacksRelationships[hediff.stackGroupID].originalPawn == pawn)
                    {
                        Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].originalPawn = null; - 83", true);
                        this.stacksRelationships[hediff.stackGroupID].originalPawn = null;
                        Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].originalStack = stack; - 84", true);
                        this.stacksRelationships[hediff.stackGroupID].originalStack = stack;
                    }
                    else if (this.stacksRelationships[hediff.stackGroupID].copiedPawns.Contains(pawn))
                    {
                        Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].copiedPawns.Remove(pawn); - 86", true);
                        this.stacksRelationships[hediff.stackGroupID].copiedPawns.Remove(pawn);
                        Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - if (this.stacksRelationships[hediff.stackGroupID].copiedStacks == null) this.stacksRelationships[hediff.stackGroupID].copiedStacks = new List<CorticalStack>(); - 87", true);
                        if (this.stacksRelationships[hediff.stackGroupID].copiedStacks == null) this.stacksRelationships[hediff.stackGroupID].copiedStacks = new List<CorticalStack>();
                        Log.Message("AlteredCarbonManager : GameComponent - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].copiedStacks.Add(stack); - 88", true);
                        this.stacksRelationships[hediff.stackGroupID].copiedStacks.Add(stack);
                    }
                }
            }
        }

        public void ReplaceStackWithPawn(CorticalStack stack, Pawn pawn)
        {
            Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 89", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - if (hediff != null) - 90", true);
            if (hediff != null)
            {
                Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - hediff.stackGroupID = stack.stackGroupID; - 91", true);
                hediff.stackGroupID = stack.stackGroupID;
                Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - if (this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 92", true);
                if (this.stacksRelationships.ContainsKey(hediff.stackGroupID))
                {
                    Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - if (this.stacksRelationships[hediff.stackGroupID].originalStack == stack) - 93", true);
                    if (this.stacksRelationships[hediff.stackGroupID].originalStack == stack)
                    {
                        Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - this.stacksRelationships[hediff.stackGroupID].originalStack = null; - 94", true);
                        this.stacksRelationships[hediff.stackGroupID].originalStack = null;
                        Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn; - 95", true);
                        this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn;
                    }
                    else if (this.stacksRelationships[hediff.stackGroupID].copiedStacks.Contains(stack))
                    {
                        Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - this.stacksRelationships[hediff.stackGroupID].copiedStacks.Remove(stack); - 97", true);
                        this.stacksRelationships[hediff.stackGroupID].copiedStacks.Remove(stack);
                        Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - if (this.stacksRelationships[hediff.stackGroupID].copiedPawns == null) this.stacksRelationships[hediff.stackGroupID].copiedPawns = new List<Pawn>(); - 98", true);
                        if (this.stacksRelationships[hediff.stackGroupID].copiedPawns == null) this.stacksRelationships[hediff.stackGroupID].copiedPawns = new List<Pawn>();
                        Log.Message("AlteredCarbonManager : GameComponent - ReplaceStackWithPawn - this.stacksRelationships[hediff.stackGroupID].copiedPawns.Add(pawn); - 99", true);
                        this.stacksRelationships[hediff.stackGroupID].copiedPawns.Add(pawn);
                    }
                }
            }
        }

        public void RegisterStack(CorticalStack stack)
        {
            Log.Message("AlteredCarbonManager : GameComponent - RegisterStack - if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>(); - 100", true);
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            Log.Message("AlteredCarbonManager : GameComponent - RegisterStack - if (!this.stacksRelationships.ContainsKey(stack.stackGroupID)) - 101", true);
            if (!this.stacksRelationships.ContainsKey(stack.stackGroupID))
            {
                Log.Message("AlteredCarbonManager : GameComponent - RegisterStack - this.stacksRelationships[stack.stackGroupID] = new StacksData(); - 102", true);
                this.stacksRelationships[stack.stackGroupID] = new StacksData();
            }
            Log.Message("AlteredCarbonManager : GameComponent - RegisterStack - if (stack.isCopied) - 103", true);
            if (stack.isCopied)
            {
                Log.Message("AlteredCarbonManager : GameComponent - RegisterStack - if (this.stacksRelationships[stack.stackGroupID].copiedStacks == null) this.stacksRelationships[stack.stackGroupID].copiedStacks = new List<CorticalStack>(); - 104", true);
                if (this.stacksRelationships[stack.stackGroupID].copiedStacks == null) this.stacksRelationships[stack.stackGroupID].copiedStacks = new List<CorticalStack>();
                Log.Message("AlteredCarbonManager : GameComponent - RegisterStack - this.stacksRelationships[stack.stackGroupID].copiedStacks.Add(stack); - 105", true);
                this.stacksRelationships[stack.stackGroupID].copiedStacks.Add(stack);
            }
            else
            {
                Log.Message("AlteredCarbonManager : GameComponent - RegisterStack - this.stacksRelationships[stack.stackGroupID].originalStack = stack; - 106", true);
                this.stacksRelationships[stack.stackGroupID].originalStack = stack;
            }
        }

        public void RegisterPawn(Pawn pawn)
        {
            Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>(); - 107", true);
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 108", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - if (hediff != null) - 109", true);
            if (hediff != null)
            {
                Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - if (!this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 110", true);
                if (!this.stacksRelationships.ContainsKey(hediff.stackGroupID))
                {
                    Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - this.stacksRelationships[hediff.stackGroupID] = new StacksData(); - 111", true);
                    this.stacksRelationships[hediff.stackGroupID] = new StacksData();
                }
                Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - if (hediff.isCopied) - 112", true);
                if (hediff.isCopied)
                {
                    Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - if (this.stacksRelationships[hediff.stackGroupID].copiedPawns == null) this.stacksRelationships[hediff.stackGroupID].copiedPawns = new List<Pawn>(); - 113", true);
                    if (this.stacksRelationships[hediff.stackGroupID].copiedPawns == null) this.stacksRelationships[hediff.stackGroupID].copiedPawns = new List<Pawn>();

                    this.stacksRelationships[hediff.stackGroupID].copiedPawns.Add(pawn);
                }
                else
                {
                    Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn; - 115", true);
                    this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn;
                }
                Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - if (this.emptySleeves != null) - 116", true);
                if (this.emptySleeves != null)
                {
                    Log.Message("AlteredCarbonManager : GameComponent - RegisterPawn - this.emptySleeves.Remove(pawn); - 117", true);
                    this.emptySleeves.Remove(pawn);
                }
                this.pawnsWithStacks.Add(pawn);
            }
        }

        public void RegisterSleeve(Pawn pawn, int stackGroupID = -1)
        {
            Log.Message("AlteredCarbonManager : GameComponent - RegisterSleeve - ACUtils.ACTracker.pawnsWithStacks.Remove(pawn); - 119", true);
            ACUtils.ACTracker.pawnsWithStacks.Remove(pawn);
            Log.Message("AlteredCarbonManager : GameComponent - RegisterSleeve - if (ACUtils.ACTracker.emptySleeves == null) ACUtils.ACTracker.emptySleeves = new HashSet<Pawn>(); - 120", true);
            if (ACUtils.ACTracker.emptySleeves == null) ACUtils.ACTracker.emptySleeves = new HashSet<Pawn>();
            Log.Message("AlteredCarbonManager : GameComponent - RegisterSleeve - ACUtils.ACTracker.emptySleeves.Add(pawn); - 121", true);
            ACUtils.ACTracker.emptySleeves.Add(pawn);
            Log.Message("AlteredCarbonManager : GameComponent - RegisterSleeve - if (stackGroupID != -1) - 122", true);
            if (stackGroupID != -1)
            {
                Log.Message("stackGroupID: " + stackGroupID, true);
                Log.Message("AlteredCarbonManager : GameComponent - RegisterSleeve - if (this.stacksRelationships[stackGroupID].deadPawns == null) - 124", true);
                if (this.stacksRelationships[stackGroupID].deadPawns == null)
                    this.stacksRelationships[stackGroupID].deadPawns = new List<Pawn>();
                Log.Message("AlteredCarbonManager : GameComponent - RegisterSleeve - this.stacksRelationships[stackGroupID].deadPawns.Add(pawn); - 126", true);
                this.stacksRelationships[stackGroupID].deadPawns.Add(pawn);
            }
        }

        public int GetStackGroupID(CorticalStack corticalStack)
        {
            Log.Message("AlteredCarbonManager : GameComponent - GetStackGroupID - if (corticalStack.stackGroupID != 0) return corticalStack.stackGroupID; - 127", true);
            if (corticalStack.stackGroupID != 0) return corticalStack.stackGroupID;

            Log.Message("AlteredCarbonManager : GameComponent - GetStackGroupID - if (ACUtils.ACTracker.stacksRelationships != null) - 128", true);
            if (ACUtils.ACTracker.stacksRelationships != null)
            {
                Log.Message("AlteredCarbonManager : GameComponent - GetStackGroupID - return ACUtils.ACTracker.stacksRelationships.Count + 1; - 129", true);
                return ACUtils.ACTracker.stacksRelationships.Count + 1;
            }
            else
            {
                Log.Message("AlteredCarbonManager : GameComponent - GetStackGroupID - return 0; - 130", true);
                return 0;
            }
        }

        public override void ExposeData()
        {
            Log.Message("AlteredCarbonManager : GameComponent - ExposeData - base.ExposeData(); - 131", true);
            base.ExposeData();
            Scribe_Collections.Look<string, CorticalStack>(ref this.stacksIndex, "stacksIndex",
                LookMode.Value, LookMode.Reference, ref this.pawnKeys, ref this.stacksValues);
            Log.Message("AlteredCarbonManager : GameComponent - ExposeData - Scribe_Collections.Look<Pawn>(ref this.pawnsWithStacks, \"pawnsWithStacks\", LookMode.Reference); - 133", true);
            Scribe_Collections.Look<Pawn>(ref this.pawnsWithStacks, "pawnsWithStacks", LookMode.Reference);
            Log.Message("AlteredCarbonManager : GameComponent - ExposeData - Scribe_Collections.Look<Pawn>(ref this.emptySleeves, \"emptySleeves\", LookMode.Reference); - 134", true);
            Scribe_Collections.Look<Pawn>(ref this.emptySleeves, "emptySleeves", LookMode.Reference);
            Log.Message("AlteredCarbonManager : GameComponent - ExposeData - Scribe_Collections.Look<Pawn>(ref this.deadPawns, saveDestroyedThings: true, \"deadPawns\", LookMode.Reference); - 135", true);
            Scribe_Collections.Look<Pawn>(ref this.deadPawns, saveDestroyedThings: true, "deadPawns", LookMode.Reference);
            Scribe_Collections.Look<int, StacksData>(ref this.stacksRelationships, "stacksRelationships",
                LookMode.Value, LookMode.Deep, ref stacksRelationshipsKeys, ref stacksRelationshipsValues);
        }

        public Dictionary<int, StacksData> stacksRelationships = new Dictionary<int, StacksData>();
        private List<int> stacksRelationshipsKeys = new List<int>();
        private List<StacksData> stacksRelationshipsValues = new List<StacksData>();

        public HashSet<Pawn> pawnsWithStacks = new HashSet<Pawn>();
        public HashSet<Pawn> emptySleeves = new HashSet<Pawn>();
        public HashSet<Pawn> deadPawns = new HashSet<Pawn>();

        public Dictionary<string, CorticalStack> stacksIndex;
        private List<string> pawnKeys = new List<string>();
        private List<CorticalStack> stacksValues = new List<CorticalStack>();
    }
}