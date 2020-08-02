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
	public class Building_SleeveGrower : Building_Casket
	{
		public bool IsOperating
		{
			get
			{
				CompPowerTrader compPowerTrader = this.powerTrader;
				if (compPowerTrader == null || compPowerTrader.PowerOn)
				{
					CompBreakdownable compBreakdownable = this.breakdownable;
					return compBreakdownable == null || !compBreakdownable.BrokenDown;
				}
				return false;
			}
		}
		
		public Pawn InnerPawn
		{
			get
			{
				return this.innerContainer.FirstOrDefault<Thing>() as Pawn;
			}
		}
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.powerTrader = base.GetComp<CompPowerTrader>();
			this.breakdownable = base.GetComp<CompBreakdownable>();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (base.Faction == Faction.OfPlayer && innerContainer.Count > 0 && def.building.isPlayerEjectable && !this.active)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.action = this.EjectContents;
				command_Action.defaultLabel = "CommandPodEject".Translate();
				command_Action.defaultDesc = "CommandPodEjectDesc".Translate();
				if (innerContainer.Count == 0)
				{
					command_Action.Disable("CommandPodEjectFailEmpty".Translate());
				}
				command_Action.hotKey = KeyBindingDefOf.Misc8;
				command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
				yield return command_Action;
			}

			Command_Action command_Action2 = new Command_Action();
			command_Action2.action = new Action(this.CreateSleeve);
			command_Action2.defaultLabel = "AlteredCarbon.CreateSleeveBody".Translate();
			command_Action2.defaultDesc = "AlteredCarbon.CreateSleeveBodyDesc".Translate();
			command_Action2.hotKey = KeyBindingDefOf.Misc8;
			command_Action2.icon = ContentFinder<Texture2D>.Get("UI/Commands/delete_stack", true);
			yield return command_Action2;
			yield break;
		}
		
		public void CreateSleeve()
		{
			Find.WindowStack.Add(new CustomizeSleeveWindow(this));
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false, 0))
			{
				FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null,
					MenuOptionPriority.Default, null, null, 0f, null, null);
				yield return floatMenuOption;
			}
			else if (!this.IsOperating)
			{
				FloatMenuOption floatMenuOption2 = new FloatMenuOption(Translator.Translate("CannotUseNotOperating"),
					null, MenuOptionPriority.Default, null, null, 0f, null, null);
				yield return floatMenuOption2;
			}
			else if (this.ContainedThing != null && !this.active)
			{
				string label = "AlteredCarbon.ReleaseSleeve".Translate();
				Action action = delegate ()
				{
					Job job = JobMaker.MakeJob(AlteredCarbonDefOf.AC_ReleaseSleeve, this);
					job.count = 1;
					myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				};
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption
						(label, action, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn,
						this, "ReservedBy");
			}
			yield break;
		}

		public override string GetInspectString()
		{
			if (this.ContainedThing != null)
			{
				return base.GetInspectString() + "\n" + "GrowthProgress".Translate() +
					Math.Round(((float)this.curTicksToGrow / this.totalTicksToGrow) * 100f, 2).ToString() + "%";
			}
			else
			{
				return base.GetInspectString();
			}
		}

		public Graphic glass;
		public Graphic Glass
        {
			get
            {
				if (glass == null)
                {
					glass = GraphicDatabase.Get<Graphic_Single>("Things/Building/Misc/SleeveGrowingVatTop", ShaderDatabase.MetaOverlay,
					new Vector3(6, 6), Color.white);
				}
				return glass;
            }
        }

		public Graphic fetus;
		public Graphic Fetus
		{
			get
			{
				if (fetus == null)
				{
					fetus = GraphicDatabase.Get<Graphic_Single>("Pawn/Humanlike/Vat/Fetus", ShaderDatabase.MetaOverlay,
					new Vector3(1, 1), this.InnerPawn.DrawColor);
				}
				return fetus;
			}
		}

		public Graphic child;
		public Graphic Child
		{
			get
			{
				if (child == null)
				{
					child = GraphicDatabase.Get<Graphic_Single>("Pawn/Humanlike/Vat/Child", ShaderDatabase.MetaOverlay,
					new Vector3(1, 1), this.InnerPawn.DrawColor);
				}
				return child;
			}
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			if (this.innerContainer != null && this.innerContainer.Count > 0 && (this.ContainedThing is Pawn || this.ContainedThing is Corpse))
			{
				Vector3 newPos = drawLoc;
				newPos.z += 0.5f;
				var growthValue = (float)this.curTicksToGrow / this.totalTicksToGrow;
				if (growthValue < 0.33f)
                {
					Fetus.Draw(newPos, Rot4.North, this);
                }
				else if (growthValue < 0.66f)
                {
					Child.Draw(newPos, Rot4.North, this);
				}
				else
                {
					this.ContainedThing.Rotation = Rot4.South;
					this.ContainedThing.DrawAt(newPos, flip);
				}
				base.DrawAt(drawLoc, flip);
				Glass.Draw(drawLoc, Rot4.North, this);
			}
			else
			{
				base.DrawAt(drawLoc, flip);
				Glass.Draw(drawLoc, Rot4.North, this);
			}
		}

		public void StartGrowth(Pawn newSleeve, int totalTicksToGrow, int totalGrowthCost)
        {
			this.TryAcceptThing(newSleeve);
			this.totalTicksToGrow = totalTicksToGrow;
			this.curTicksToGrow = 0;

			this.totalGrowthCost = totalGrowthCost;
			this.active = true;

		}
		public void FinishGrowth()
        {
			this.active = false;
		}

        public override void Open()
        {
			this.EjectContents();
        }
        public override void Tick()
		{
			if (this.ContainedThing == null && this.curTicksToGrow > 0)
			{
				curTicksToGrow = 0;
			}
			if (this.ContainedThing is Pawn && base.GetComp<CompRefuelable>().HasFuel && powerTrader.PowerOn && this.active)
			{
				var fuelCost = this.totalGrowthCost / (float)this.totalTicksToGrow;
				base.GetComp<CompRefuelable>().ConsumeFuel(fuelCost);
				if (this.curTicksToGrow < totalTicksToGrow)
				{
					curTicksToGrow++;
				}
				else
				{
					this.FinishGrowth();
				}
			}
		}
		
		public override void EjectContents()
		{
			ThingDef filth_Slime = ThingDefOf.Filth_Slime;
			foreach (Thing thing in this.innerContainer)
			{
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					PawnComponentsUtility.AddComponentsForSpawn(pawn);
					pawn.filth.GainFilth(filth_Slime);
				}
			}
			if (!base.Destroyed)
			{
				SoundStarter.PlayOneShot(SoundDefOf.CryptosleepCasket_Eject,
					SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), 0));
			}
			this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near, null, null);
			this.contentsKnown = true;
		}
		
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.totalTicksToGrow, "totalTicksToGrow", 0, true);
			Scribe_Values.Look<int>(ref this.curTicksToGrow, "curTicksToGrow", 0, true);

			Scribe_Values.Look<float>(ref this.totalGrowthCost, "totalGrowthCost", 0f, true);
			Scribe_Values.Look<bool>(ref this.contentsKnown, "contentsKnown", false, true);
			Scribe_Values.Look<bool>(ref this.active, "active", false, true);
		}

		public override bool Accepts(Thing thing)
		{
			return this.innerContainer.Count == 0;
		}

		public int totalTicksToGrow = 0;
		public int curTicksToGrow = 0;

		public float totalGrowthCost = 0;
		public bool active;

		private CompPowerTrader powerTrader;

		private CompBreakdownable breakdownable;

	}
}

