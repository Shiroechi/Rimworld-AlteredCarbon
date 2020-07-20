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
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            if (ACTracker.stacksRelationships.ContainsKey(hediff.stackGroupID))
            {
                if (ACTracker.stacksRelationships[hediff.stackGroupID].originalPawn != null 
                    && pawn != ACTracker.stacksRelationships[hediff.stackGroupID].originalPawn)
                {
                    return true;
                }
                if (ACTracker.stacksRelationships[hediff.stackGroupID].copiedPawns != null)
                {
                    foreach (var copiedPawn in ACTracker.stacksRelationships[hediff.stackGroupID].copiedPawns)
                    {
                        if (pawn == copiedPawn)
                        {
                            return true;
                        }
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

