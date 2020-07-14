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
	public class Building_SleeveGrower : Building_CryptosleepCasket
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
			IEnumerator<Gizmo> enumerator = null;
		
			if (this.ContainedThing == null)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.action = new Action(this.CreateSleeve);
				command_Action.defaultLabel = "AlteredCarbon.CreateSleeveBody".Translate();
				command_Action.defaultDesc = "AlteredCarbon.CreateSleeveBodyDesc".Translate();
				command_Action.hotKey = KeyBindingDefOf.Misc8;
				command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/delete_stack", true);
				yield return command_Action;
			}
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
			else if (this.ContainedThing != null)
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
		
		public void DoMutation(string pawnKindDefname)
		{
			PawnKindDef pawnKindDef = PawnKindDef.Named(pawnKindDefname);
			NameTriple nameTriple = this.InnerPawn.Name as NameTriple;
			Pawn mutant = PawnGenerator.GeneratePawn(
			new PawnGenerationRequest(pawnKindDef, this.InnerPawn.Faction, PawnGenerationContext.NonPlayer, -1,
				false, false, false, false, false, false, 1f, false, true, true, true, false, false, false,
				false, 0f, null, 1f, null, null,
				this.InnerPawn.story.traits.allTraits.Select(x => x.def),
				this.InnerPawn.story.traits.allTraits.Select(x => x.def),
				null,
				20,
				20,
				this.InnerPawn.gender,
				null,
				nameTriple.Last,
				nameTriple.First,
				null));
			ThingDef filth_Slime = ThingDefOf.Filth_Slime;
			if (mutant.Faction != Faction.OfPlayer)
			{
				mutant.SetFaction(Faction.OfPlayer);
				var letterLabel = "LetterLabelMessageRecruitSuccess".Translate() + ": " + mutant.LabelShortCap;
				Find.LetterStack.ReceiveLetter(letterLabel, letterLabel, LetterDefOf.PositiveEvent, mutant, null, null, null, null);
			}
			mutant.Name = new NameTriple(nameTriple.First, nameTriple.Nick, nameTriple.Last);
			mutant.story.traits = this.InnerPawn.story.traits;
			mutant.relations = this.InnerPawn.relations;
			mutant.skills = this.InnerPawn.skills;
			mutant.story.childhood = this.InnerPawn.story.childhood;
			mutant.story.adulthood = this.InnerPawn.story.adulthood;
			this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
			GenSpawn.Spawn(mutant, this.Position, this.Map);
			mutant.filth.GainFilth(filth_Slime);
			this.innerContainer.TryAdd(mutant);
			PortraitsCache.SetDirty(mutant);
			PortraitsCache.PortraitsCacheUpdate();
		}
		
		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			var glass = GraphicDatabase.Get<Graphic_Single>("Building/FEVvatglass", ShaderDatabase.MetaOverlay,
					new Vector3(6, 6), Color.white);
			if (this.innerContainer != null && this.innerContainer.Count > 0 && (this.ContainedThing is Pawn || this.ContainedThing is Corpse))
			{
				Vector3 newPos = drawLoc;
				newPos.z += 0.5f;
		
				this.ContainedThing.Rotation = Rot4.South;
				this.ContainedThing.DrawAt(newPos, flip);
		
				base.DrawAt(drawLoc, flip);
				glass.Draw(drawLoc, Rot4.North, this);
			}
			else
			{
				base.DrawAt(drawLoc, flip);
				glass.Draw(drawLoc, Rot4.North, this);
			}
		}
		
		public override void Tick()
		{
			base.Tick();
			if (this.ContainedThing == null && this.mutationProgress > 0)
			{
				mutationProgress = 0;
			}
			if (this.ContainedThing is Pawn && !this.InnerPawn.Dead && base.GetComp<CompRefuelable>().HasFuel)
			{
				base.GetComp<CompRefuelable>().ConsumeFuel(0.01f);
				if (this.mutationProgress < 1000)
				{
					mutationProgress++;
				}
				else
				{
					this.DoMutation("FCPSuperMutant_Colonist");
					mutationProgress = 0;
				}
			}
		}
		
		public override string GetInspectString()
		{
			return base.GetInspectString() + "\n" + "MutationProgress".Translate() +
				(((float)this.mutationProgress / 1000f) * 100f).ToString() + "%";
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
			Scribe_Values.Look<int>(ref this.mutationProgress, "mutationProgress", 0, true);
		}
		
		public override bool Accepts(Thing thing)
		{
			return this.innerContainer.Count == 0;
		}

		private CompPowerTrader powerTrader;

		private CompBreakdownable breakdownable;

		public int mutationProgress = 0;
	}
}

