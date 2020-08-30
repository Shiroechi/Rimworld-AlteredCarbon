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
            if (def.stackLimit > 1)
            {
                def.stackLimit = 1;
                def.drawGUIOverlay = false;
            }
        }
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            ACUtils.ResetACTracker();
            if (this.stacksIndex == null) this.stacksIndex = new Dictionary<string, CorticalStack>();
            if (this.pawnsWithStacks == null) this.pawnsWithStacks = new HashSet<Pawn>();
            if (this.emptySleeves == null) this.emptySleeves = new HashSet<Pawn>();
            if (this.deadPawns == null) this.deadPawns = new HashSet<Pawn>();
            ResetStackLimitIfNeeded(AlteredCarbonDefOf.AC_FilledCorticalStack);
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            ACUtils.ResetACTracker();
            if (this.stacksIndex == null) this.stacksIndex = new Dictionary<string, CorticalStack>();
            if (this.pawnsWithStacks == null) this.pawnsWithStacks = new HashSet<Pawn>();
            if (this.emptySleeves == null) this.emptySleeves = new HashSet<Pawn>();
            if (this.deadPawns == null) this.deadPawns = new HashSet<Pawn>();
            ResetStackLimitIfNeeded(AlteredCarbonDefOf.AC_FilledCorticalStack);
        }

        public void TryAddRelationships(Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            if (this.stacksRelationships.ContainsKey(hediff.stackGroupID))
            {
                if (this.stacksRelationships[hediff.stackGroupID].originalPawn != null)
                {
                    if (pawn != this.stacksRelationships[hediff.stackGroupID].originalPawn)
                    {
                        pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original,
                            this.stacksRelationships[hediff.stackGroupID].originalPawn);
                        this.stacksRelationships[hediff.stackGroupID]
                            .originalPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                    }
                    else if (this.stacksRelationships[hediff.stackGroupID].copiedPawns != null)
                    {
                        foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns)
                        {
                            if (pawn != copiedPawn)
                            {
                                pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original, copiedPawn);
                                copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                            }
                        }
                    }
                }
                else if (this.stacksRelationships[hediff.stackGroupID].copiedPawns != null)
                {
                    foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns)
                    {
                        if (pawn != copiedPawn)
                        {
                            pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, copiedPawn);
                            copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                        }
                    }
                }
            }
            if (pawn.IsCopy())
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_JustCopy);
                var otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherPawn))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMySpouse, otherPawn);
                }

                otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, otherPawn))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyFiance, otherPawn);
                }

                otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);

                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherPawn))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyLover, otherPawn);
                }
            }
        }

        public void ReplacePawnWithStack(Pawn pawn, CorticalStack stack)
        {
            Log.Message(" - ReplacePawnWithStack - if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>(); - 1", true);
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            Log.Message(" - ReplacePawnWithStack - var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 2", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message(" - ReplacePawnWithStack - if (hediff != null) - 3", true);
            if (hediff != null)
            {
                Log.Message("1 stack.stackGroupID: " + stack.stackGroupID, true);
                Log.Message("1 hediff.stackGroupID: " + hediff.stackGroupID, true);

                Log.Message(" - ReplacePawnWithStack - stack.stackGroupID = hediff.stackGroupID; - 4", true);
                stack.stackGroupID = hediff.stackGroupID;
                Log.Message("2 stack.stackGroupID: " + stack.stackGroupID, true);
                Log.Message("2 hediff.stackGroupID: " + hediff.stackGroupID, true);

                Log.Message(" - ReplacePawnWithStack - if (this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 5", true);
                if (this.stacksRelationships.ContainsKey(hediff.stackGroupID))
                {
                    Log.Message(" - ReplacePawnWithStack - if (this.stacksRelationships[hediff.stackGroupID].originalPawn == pawn) - 6", true);
                    if (this.stacksRelationships[hediff.stackGroupID].originalPawn == pawn)
                    {
                        Log.Message(" - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].originalPawn = null; - 7", true);
                        this.stacksRelationships[hediff.stackGroupID].originalPawn = null;
                        Log.Message(" - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].originalStack = stack; - 8", true);
                        this.stacksRelationships[hediff.stackGroupID].originalStack = stack;
                    }
                    else if (this.stacksRelationships[hediff.stackGroupID].copiedPawns.Contains(pawn))
                    {
                        Log.Message(" - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].copiedPawns.Remove(pawn); - 10", true);
                        this.stacksRelationships[hediff.stackGroupID].copiedPawns.Remove(pawn);
                        Log.Message(" - ReplacePawnWithStack - if (this.stacksRelationships[hediff.stackGroupID].copiedStacks == null) this.stacksRelationships[hediff.stackGroupID].copiedStacks = new List<CorticalStack>(); - 11", true);
                        if (this.stacksRelationships[hediff.stackGroupID].copiedStacks == null) this.stacksRelationships[hediff.stackGroupID].copiedStacks = new List<CorticalStack>();
                        Log.Message(" - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].copiedStacks.Add(stack); - 12", true);
                        this.stacksRelationships[hediff.stackGroupID].copiedStacks.Add(stack);
                    }
                }
            }
        }


        public void ReplaceStackWithPawn(CorticalStack stack, Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            hediff.stackGroupID = stack.stackGroupID;
            if (this.stacksRelationships.ContainsKey(hediff.stackGroupID))
            {
                if (this.stacksRelationships[hediff.stackGroupID].originalStack == stack)
                {
                    this.stacksRelationships[hediff.stackGroupID].originalStack = null;
                    this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn;
                }
                else if (this.stacksRelationships[hediff.stackGroupID].copiedStacks.Contains(stack))
                {
                    this.stacksRelationships[hediff.stackGroupID].copiedStacks.Remove(stack);
                    if (this.stacksRelationships[hediff.stackGroupID].copiedPawns == null) this.stacksRelationships[hediff.stackGroupID].copiedPawns = new List<Pawn>();
                    this.stacksRelationships[hediff.stackGroupID].copiedPawns.Add(pawn);
                }
            }
        }

        public void RegisterStack(CorticalStack stack)
        {
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            if (!this.stacksRelationships.ContainsKey(stack.stackGroupID))
            {
                this.stacksRelationships[stack.stackGroupID] = new StacksData();
            }
            if (stack.isCopied)
            {
                if (this.stacksRelationships[stack.stackGroupID].copiedStacks == null) this.stacksRelationships[stack.stackGroupID].copiedStacks = new List<CorticalStack>();
                this.stacksRelationships[stack.stackGroupID].copiedStacks.Add(stack);
            }
            else
            {
                this.stacksRelationships[stack.stackGroupID].originalStack = stack;
            }
        }

        public void RegisterPawn(Pawn pawn)
        {
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            if (hediff != null)
            {
                if (!this.stacksRelationships.ContainsKey(hediff.stackGroupID))
                {
                    this.stacksRelationships[hediff.stackGroupID] = new StacksData();
                }
                if (hediff.isCopied)
                {
                    if (this.stacksRelationships[hediff.stackGroupID].copiedPawns == null) this.stacksRelationships[hediff.stackGroupID].copiedPawns = new List<Pawn>();

                    this.stacksRelationships[hediff.stackGroupID].copiedPawns.Add(pawn);
                }
                else
                {
                    this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn;
                }
                if (this.emptySleeves != null)
                {
                    this.emptySleeves.Remove(pawn);
                }
                this.pawnsWithStacks.Add(pawn);
            }
        }



        public void RegisterSleeve(Pawn pawn, int stackGroupID = -1)
        {
            ACUtils.ACTracker.pawnsWithStacks.Remove(pawn);
            if (ACUtils.ACTracker.emptySleeves == null) ACUtils.ACTracker.emptySleeves = new HashSet<Pawn>();
            ACUtils.ACTracker.emptySleeves.Add(pawn);
            if (stackGroupID != -1)
            {
                Log.Message("stackGroupID: " + stackGroupID, true);
                if (this.stacksRelationships[stackGroupID].deadPawns == null) 
                    this.stacksRelationships[stackGroupID].deadPawns = new List<Pawn>();
                this.stacksRelationships[stackGroupID].deadPawns.Add(pawn);
            }
        }

        public int GetStackGroupID(CorticalStack corticalStack)
        {
            if (corticalStack.stackGroupID != 0) return corticalStack.stackGroupID;

            if (ACUtils.ACTracker.stacksRelationships != null)
            {
                return ACUtils.ACTracker.stacksRelationships.Count + 1;
            }
            else
            {
                return 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, CorticalStack>(ref this.stacksIndex, "stacksIndex",
                LookMode.Value, LookMode.Reference, ref this.pawnKeys, ref this.stacksValues);
            Scribe_Collections.Look<Pawn>(ref this.pawnsWithStacks, "pawnsWithStacks", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref this.emptySleeves, "emptySleeves", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref this.deadPawns, "deadPawns", LookMode.Reference);
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

