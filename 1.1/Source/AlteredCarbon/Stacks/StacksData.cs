using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class StacksData : IExposable
    {
        public Pawn originalPawn;
        public CorticalStack originalStack;

        public List<Pawn> copiedPawns;
        public List<CorticalStack> copiedStacks;

        public void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref originalPawn, "originalPawn", true);
            Scribe_References.Look<CorticalStack>(ref originalStack, "originalStack", true);
            Scribe_Collections.Look<Pawn>(ref copiedPawns, "copiedPawns", LookMode.Reference);
            Scribe_Collections.Look<CorticalStack>(ref copiedStacks, "copiedStacks", LookMode.Reference);
        }
    }
}

