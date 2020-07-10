using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_CorticalStack : Hediff_Implant
    {
        public override void PostMake()
        {
            base.PostMake();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            this.pawn.Kill(null);
        }
    }
}

