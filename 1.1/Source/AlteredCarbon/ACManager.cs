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

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            ACUtils.ResetACTracker();
            if (this.stacksIndex == null) this.stacksIndex = new Dictionary<string, CorticalStack>();
        }
        public override void LoadedGame()
        {
            base.LoadedGame();
            ACUtils.ResetACTracker();
            if (this.stacksIndex == null) this.stacksIndex = new Dictionary<string, CorticalStack>();
            if (this.pawnsWithStacks == null) this.pawnsWithStacks = new HashSet<Pawn>();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (this.stacksRelationships != null)
                {
                    foreach (var data in this.stacksRelationships)
                    {
                        Log.Message("----- Group ID: " + data.Key + " ----------------", true);
                        if (data.Value.originalPawn != null)
                        {
                            Log.Message("Original pawn: " + data.Value.originalPawn + " - " + data.Value.originalPawn.ThingID, true);
                            var hediff = data.Value.originalPawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
                            Log.Message("Original pawn stackGroupID: " + hediff.stackGroupID, true);
                        }
                        if (data.Value.originalStack != null)
                        {
                            Log.Message("Original stack: " + data.Value.originalStack + " - "
                                + data.Value.originalStack?.name, true);
                            Log.Message("Original stack stackGroupID: " + data.Value.originalStack.stackGroupID, true);
                        }
                        if (data.Value.copiedPawns != null)
                        {
                            foreach (var cs in data.Value.copiedPawns)
                            {
                                Log.Message("Copied pawn: " + cs + " - " + cs.ThingID, true);
                                var hediff = cs.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
                                Log.Message("Copied pawn stackGroupID: " + hediff.stackGroupID, true);

                            }
                        }
                        if (data.Value.copiedStacks != null)
                        {
                            foreach (var cs in data.Value.copiedStacks)
                            {
                                Log.Message("Copied stack: " + cs + " - " + cs?.name, true);
                                Log.Message("Copied stack stackGroupID: " + cs.stackGroupID, true);
                            }
                        }
                        Log.Message("------------------------------", true);
                    }
                }

            }
        }

        public void TryAddRelationships(Pawn pawn)
        {
            Log.Message(" - TryAddRelationships - var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 1", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message(" - TryAddRelationships - if (this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 2", true);
            if (this.stacksRelationships.ContainsKey(hediff.stackGroupID))
            {
                Log.Message(" - TryAddRelationships - if (this.stacksRelationships[hediff.stackGroupID].originalPawn != null) - 3", true);
                if (this.stacksRelationships[hediff.stackGroupID].originalPawn != null)
                {
                    Log.Message(" - TryAddRelationships - if (pawn != this.stacksRelationships[hediff.stackGroupID].originalPawn) - 4", true);
                    if (pawn != this.stacksRelationships[hediff.stackGroupID].originalPawn)
                    {
                        pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original,
                            this.stacksRelationships[hediff.stackGroupID].originalPawn);
                        this.stacksRelationships[hediff.stackGroupID]
                            .originalPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                    }
                    else
                    {
                        Log.Message(" - TryAddRelationships - foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns) - 7", true);
                        foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns)
                        {
                            Log.Message(" - TryAddRelationships - if (pawn != copiedPawn) - 8", true);
                            if (pawn != copiedPawn)
                            {
                                Log.Message(" - TryAddRelationships - pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original, copiedPawn); - 9", true);
                                pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Original, copiedPawn);
                                Log.Message(" - TryAddRelationships - copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn); - 10", true);
                                copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                            }
                        }
                    }
                }
                else
                {
                    Log.Message(" - TryAddRelationships - foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns) - 11", true);
                    foreach (var copiedPawn in this.stacksRelationships[hediff.stackGroupID].copiedPawns)
                    {
                        Log.Message(" - TryAddRelationships - if (pawn != copiedPawn) - 12", true);
                        if (pawn != copiedPawn)
                        {
                            Log.Message(" - TryAddRelationships - pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, copiedPawn); - 13", true);
                            pawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, copiedPawn);
                            Log.Message(" - TryAddRelationships - copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn); - 14", true);
                            copiedPawn.relations.AddDirectRelation(AlteredCarbonDefOf.AC_Copy, pawn);
                        }
                    }
                }
            }
            Log.Message(" - TryAddRelationships - if (pawn.IsCopy()) - 15", true);
            if (pawn.IsCopy())
            {
                Log.Message(" - TryAddRelationships - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_JustCopy); - 16", true);
                pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_JustCopy);

                var otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
                Log.Message(" - TryAddRelationships - if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherPawn)) - 18", true);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherPawn))
                {
                    Log.Message(" - TryAddRelationships - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMySpouse, otherPawn); - 19", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMySpouse, otherPawn);
                }

                otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
                Log.Message(" - TryAddRelationships - if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, otherPawn)) - 21", true);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, otherPawn))
                {
                    Log.Message(" - TryAddRelationships - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyFiance, otherPawn); - 22", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyFiance, otherPawn);
                }

                otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);

                Log.Message(" - TryAddRelationships - if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherPawn)) - 24", true);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherPawn))
                {
                    Log.Message(" - TryAddRelationships - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyLover, otherPawn); - 25", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_LostMyLover, otherPawn);
                }
            }
        }

        public void ReplacePawnWithStack(Pawn pawn, CorticalStack stack)
        {
            Log.Message(" - ReplacePawnWithStack - var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 1", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message(" - ReplacePawnWithStack - hediff.stackGroupID = stack.stackGroupID; - 2", true);
            Log.Message("hediff.stackGroupID: " + hediff.stackGroupID);
            Log.Message("stack.stackGroupID: " + stack.stackGroupID);
            stack.stackGroupID = hediff.stackGroupID;
            Log.Message(" - ReplacePawnWithStack - if (this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 3", true);
            if (this.stacksRelationships.ContainsKey(hediff.stackGroupID))
            {
                Log.Message(" - ReplacePawnWithStack - if (this.stacksRelationships[hediff.stackGroupID].originalPawn == pawn) - 4", true);
                if (this.stacksRelationships[hediff.stackGroupID].originalPawn == pawn)
                {
                    Log.Message(" - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].originalPawn = null; - 5", true);
                    this.stacksRelationships[hediff.stackGroupID].originalPawn = null;
                    Log.Message(" - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].originalStack = stack; - 6", true);
                    this.stacksRelationships[hediff.stackGroupID].originalStack = stack;
                }
                else if (this.stacksRelationships[hediff.stackGroupID].copiedPawns.Contains(pawn))
                {
                    Log.Message(" - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].copiedPawns.Remove(pawn); - 8", true);
                    this.stacksRelationships[hediff.stackGroupID].copiedPawns.Remove(pawn);
                    Log.Message(" - ReplacePawnWithStack - this.stacksRelationships[hediff.stackGroupID].copiedStacks.Add(stack); - 9", true);
                    this.stacksRelationships[hediff.stackGroupID].copiedStacks.Add(stack);
                }
            }
        }

        public void ReplaceStackWithPawn(CorticalStack stack, Pawn pawn)
        {
            Log.Message(" - ReplaceStackWithPawn - var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 1", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message("hediff.stackGroupID: " + hediff.stackGroupID);
            Log.Message("stack.stackGroupID: " + stack.stackGroupID);
            Log.Message(" - ReplaceStackWithPawn - hediff.stackGroupID = stack.stackGroupID; - 4", true);
            hediff.stackGroupID = stack.stackGroupID;
            Log.Message(" - ReplaceStackWithPawn - if (this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 5", true);
            if (this.stacksRelationships.ContainsKey(hediff.stackGroupID))
            {
                Log.Message(" - ReplaceStackWithPawn - if (this.stacksRelationships[hediff.stackGroupID].originalStack == stack) - 6", true);
                if (this.stacksRelationships[hediff.stackGroupID].originalStack == stack)
                {
                    Log.Message(" - ReplaceStackWithPawn - this.stacksRelationships[hediff.stackGroupID].originalStack = null; - 7", true);
                    this.stacksRelationships[hediff.stackGroupID].originalStack = null;
                    Log.Message(" - ReplaceStackWithPawn - this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn; - 8", true);
                    this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn;
                }
                else if (this.stacksRelationships[hediff.stackGroupID].copiedStacks.Contains(stack))
                {
                    Log.Message(" - ReplaceStackWithPawn - this.stacksRelationships[hediff.stackGroupID].copiedStacks.Remove(stack); - 10", true);
                    this.stacksRelationships[hediff.stackGroupID].copiedStacks.Remove(stack);
                    Log.Message(" - ReplaceStackWithPawn - this.stacksRelationships[hediff.stackGroupID].copiedPawns.Add(pawn); - 11", true);
                    this.stacksRelationships[hediff.stackGroupID].copiedPawns.Add(pawn);
                }
            }
        }

        public void RegisterStack(CorticalStack stack)
        {
            Log.Message(" - RegisterStack - if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>(); - 1", true);
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            Log.Message(" - RegisterStack - if (!this.stacksRelationships.ContainsKey(stack.stackGroupID)) - 2", true);
            if (!this.stacksRelationships.ContainsKey(stack.stackGroupID))
            {
                Log.Message(" - RegisterStack - this.stacksRelationships[stack.stackGroupID] = new StacksData(); - 3", true);
                this.stacksRelationships[stack.stackGroupID] = new StacksData();
            }
            Log.Message(" - RegisterStack - if (stack.isCopied) - 4", true);
            if (stack.isCopied)
            {
                Log.Message(" - RegisterStack - this.stacksRelationships[stack.stackGroupID].copiedStacks.Add(stack); - 5", true);
                if (this.stacksRelationships[stack.stackGroupID].copiedStacks == null) this.stacksRelationships[stack.stackGroupID].copiedStacks = new List<CorticalStack>();
                this.stacksRelationships[stack.stackGroupID].copiedStacks.Add(stack);
            }
            else
            {
                Log.Message(" - RegisterStack - this.stacksRelationships[stack.stackGroupID].originalStack = stack; - 6", true);
                this.stacksRelationships[stack.stackGroupID].originalStack = stack;
            }
        }

        public void RegisterPawn(Pawn pawn)
        {
            Log.Message(" - RegisterPawn - if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>(); - 7", true);
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            Log.Message(" - RegisterPawn - var hediff = pawn.health.hediffSet.hediffs.First(x => x.def == AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 8", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message(" - RegisterPawn - if (hediff != null) - 9", true);
            if (hediff != null)
            {
                Log.Message(" - RegisterPawn - if (!this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 10", true);
                if (!this.stacksRelationships.ContainsKey(hediff.stackGroupID))
                {
                    Log.Message(" - RegisterPawn - this.stacksRelationships[hediff.stackGroupID] = new StacksData(); - 11", true);
                    this.stacksRelationships[hediff.stackGroupID] = new StacksData();
                }
                Log.Message(" - RegisterPawn - if (hediff.isCopied) - 12", true);
                if (hediff.isCopied)
                {
                    if (this.stacksRelationships[hediff.stackGroupID].copiedPawns == null) this.stacksRelationships[hediff.stackGroupID].copiedPawns = new List<Pawn>();

                    Log.Message(" - RegisterPawn - this.stacksRelationships[hediff.stackGroupID].copiedPawns.Add(pawn); - 13", true);
                    this.stacksRelationships[hediff.stackGroupID].copiedPawns.Add(pawn);
                }
                else
                {
                    Log.Message(" - RegisterPawn - this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn; - 14", true);
                    this.stacksRelationships[hediff.stackGroupID].originalPawn = pawn;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, CorticalStack>(ref this.stacksIndex, "stacksIndex", 
                LookMode.Value, LookMode.Reference, ref this.pawnKeys, ref this.stacksValues);
            Scribe_Collections.Look<Pawn>(ref this.pawnsWithStacks, "pawnsWithStacks", LookMode.Reference);
            Scribe_Collections.Look<int, StacksData>(ref this.stacksRelationships, "stacksRelationships",
                LookMode.Value, LookMode.Deep, ref stacksRelationshipsKeys, ref stacksRelationshipsValues);
        }

        public Dictionary<int, StacksData> stacksRelationships = new Dictionary<int, StacksData>();
        private List<int> stacksRelationshipsKeys = new List<int>();
        private List<StacksData> stacksRelationshipsValues = new List<StacksData>();

        public HashSet<Pawn> pawnsWithStacks = new HashSet<Pawn>();

        public Dictionary<string, CorticalStack> stacksIndex;
        private List<string> pawnKeys = new List<string>();
        private List<CorticalStack> stacksValues = new List<CorticalStack>();
    }
}