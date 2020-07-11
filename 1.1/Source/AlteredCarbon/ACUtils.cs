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

		public static void ResetACTracker()
		{
			aCTracker = null;
		}

		private static AlteredCarbonManager aCTracker;
	}
}

