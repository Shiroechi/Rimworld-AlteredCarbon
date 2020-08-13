using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	[StaticConstructorOnStartup]
	public class CompSleeveGrowerPostDraw : ThingComp
	{
		public Graphic glass;
		public Graphic Glass
		{
			get
			{
				if (glass == null)
				{
					glass = GraphicDatabase.Get<Graphic_Single>("Things/Building/Misc/SleeveGrowingVatTop", ShaderDatabase.CutoutComplex, new Vector3(6, 6), Color.white);
				}
				return glass;
			}
		}
		public override void PostDraw()
		{
			base.PostDraw();
			Glass.Draw(this.parent.DrawPos + Altitudes.AltIncVect, Rot4.North, this.parent);
		}
	}
}
