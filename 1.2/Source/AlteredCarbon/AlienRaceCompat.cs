using System;
using AlienRace;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	public static class AlienRaceCompat
	{
		public static void SetSkinColor(Pawn pawn, Color color)
		{
			AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienPartGenerator.AlienComp>(pawn);
			alienComp.GetChannel("skin").first = color;

		}
		public static bool AlienRacesIsActive => ModLister.HasActiveModWithName("Humanoid Alien Races 2.0");
	}
}
