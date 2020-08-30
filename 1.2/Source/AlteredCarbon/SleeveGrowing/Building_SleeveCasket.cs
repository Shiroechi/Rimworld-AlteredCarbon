using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AlteredCarbon
{
	public class Building_SleeveCasket : Building_Bed
	{
		public Graphic glass;
		public Graphic Glass
        {
			get
            {
				if (glass == null)
                {
					Log.Message(this.Rotation.ToStringWord().ToLower());
					glass = GraphicDatabase.Get<Graphic_Single>("Things/Building/Misc/SleeveCasketTop_" + this.Rotation.ToStringWord().ToLower(), ShaderDatabase.MetaOverlay,
						new Vector2(1, 2), Color.white);
				}
				return glass;
            }
        }

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			Glass.Draw(drawLoc, this.Rotation, this);
		}
	}
}

