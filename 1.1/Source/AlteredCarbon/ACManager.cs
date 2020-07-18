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
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message("hediff.stackGroupID: " + hediff.stackGroupID);
            Log.Message("stack.stackGroupID: " + stack.stackGroupID);
            stack.stackGroupID = hediff.stackGroupID;
            if (this.stacksRelationships.ContainsKey(hediff.stackGroupID))
            {
                if (this.stacksRelationships[hediff.stackGroupID].originalPawn == pawn)
                {
                    this.stacksRelationships[hediff.stackGroupID].originalPawn = null;
                    this.stacksRelationships[hediff.stackGroupID].originalStack = stack;
                }
                else if (this.stacksRelationships[hediff.stackGroupID].copiedPawns.Contains(pawn))
                {
                    this.stacksRelationships[hediff.stackGroupID].copiedPawns.Remove(pawn);
                    if (this.stacksRelationships[hediff.stackGroupID].copiedStacks == null) this.stacksRelationships[hediff.stackGroupID].copiedStacks = new List<CorticalStack>();
                    this.stacksRelationships[hediff.stackGroupID].copiedStacks.Add(stack);
                }
            }
        }

        public void ReplaceStackWithPawn(CorticalStack stack, Pawn pawn)
        {
            try
            {
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
                Log.Message("hediff.stackGroupID: " + hediff.stackGroupID);
                Log.Message("stack.stackGroupID: " + stack.stackGroupID);
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
            catch (Exception ex)
            {
                Log.Error("Error: " + ex, true);
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
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, CorticalStack>(ref this.stacksIndex, "stacksIndex", 
                LookMode.Value, LookMode.Reference, ref this.pawnKeys, ref this.stacksValues);
            Scribe_Collections.Look<Pawn>(ref this.pawnsWithStacks, "pawnsWithStacks", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref this.deadPawns, "deadPawns", LookMode.Reference);
            Scribe_Collections.Look<int, StacksData>(ref this.stacksRelationships, "stacksRelationships",
                LookMode.Value, LookMode.Deep, ref stacksRelationshipsKeys, ref stacksRelationshipsValues);
        }

        public Dictionary<int, StacksData> stacksRelationships = new Dictionary<int, StacksData>();
        private List<int> stacksRelationshipsKeys = new List<int>();
        private List<StacksData> stacksRelationshipsValues = new List<StacksData>();

        public HashSet<Pawn> pawnsWithStacks = new HashSet<Pawn>();

        public HashSet<Pawn> deadPawns = new HashSet<Pawn>();

        public Dictionary<string, CorticalStack> stacksIndex;
        private List<string> pawnKeys = new List<string>();
        private List<CorticalStack> stacksValues = new List<CorticalStack>();
    }
}

