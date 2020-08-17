using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	public static class AlienRaceCompat
	{
		public static void SetSkinColor(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			alienComp.GetChannel("skin").first = color;
		}

		public static List<ThingDef> GetAllAlienRaces(ExcludeRacesModExtension raceOptions)
        {
			return DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.Where(x => !raceOptions.racesToExclude.Contains(x.defName)).Cast<ThingDef>().ToList();
		}

		public static bool AlienRacesIsActive => ModLister.HasActiveModWithName("Humanoid Alien Races 2.0");
	}
}
