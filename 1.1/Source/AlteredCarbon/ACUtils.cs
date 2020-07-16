using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
	[StaticConstructorOnStartup]
	internal static class ACUtils
	{
		public static AlteredCarbonManager ACTracker
		{
			get
			{
				if (aCTracker == null)
				{
					aCTracker = Current.Game.GetComponent<AlteredCarbonManager>();
					return aCTracker;
				}
				return aCTracker;
			}
		}

        public static bool IsCopy(this Pawn pawn)
        {
            Log.Message(" - TryAddRelationships - var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack; - 1", true);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            Log.Message(" - TryAddRelationships - if (this.stacksRelationships.ContainsKey(hediff.stackGroupID)) - 2", true);
            if (ACTracker.stacksRelationships.ContainsKey(hediff.stackGroupID))
            {
                Log.Message(" - TryAddRelationships - if (this.stacksRelationships[hediff.stackGroupID].originalPawn != null) - 3", true);
                if (ACTracker.stacksRelationships[hediff.stackGroupID].originalPawn != null 
                    && pawn != ACTracker.stacksRelationships[hediff.stackGroupID].originalPawn)
                {
                    return true;
                }
                foreach (var copiedPawn in ACTracker.stacksRelationships[hediff.stackGroupID].copiedPawns)
                {
                    Log.Message(" - TryAddRelationships - if (pawn != copiedPawn) - 8", true);
                    if (pawn == copiedPawn)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void ResetACTracker()
		{
			aCTracker = null;
		}

		private static AlteredCarbonManager aCTracker;
	}
}

