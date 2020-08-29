using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	public class CompBrainTemplate : ThingComp
	{
		public CompProperties_BrainTemplate Props => (CompProperties_BrainTemplate)this.props;
		public void SaveBodyData(Pawn pawn)
        {
			var hediff = HediffMaker.MakeHediff(AlteredCarbonDefOf.AC_SleeveBodyData, pawn) as Hediff_SleeveBodyStats;
			hediff.hediffs = Props.hediffs;
			hediff.skillsOffsets = Props.skillsOffsets;
			hediff.skillPassionsOffsets = Props.skillPassionsOffsets;
			pawn.health.AddHediff(hediff);
			Log.Message("SAVING");

		}
	}
}
