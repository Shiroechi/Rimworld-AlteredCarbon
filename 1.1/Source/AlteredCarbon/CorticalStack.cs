using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class CorticalStack : ThingWithComps
    {
        public Name name;
        public int hostilityMode;
        public Area areaRestriction;
        public MedicalCareCategory medicalCareCategory;
        public bool selfTend;
        public long ageChronologicalTicks;
        public List<TimeAssignmentDef> times;
        public FoodRestriction foodRestriction;
        public Outfit outfit;
        public DrugPolicy drugPolicy;
        public Faction faction;
        public List<Thought_Memory> thoughts;
        public List<Trait> traits;
        public List<DirectPawnRelation> relations;
        public List<SkillRecord> skills;
        public string childhood;
        public string adulthood;
        public Dictionary<WorkTypeDef, int> priorities;
        public bool hasPawn = false;

        public Gender gender;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && !hasPawn && this.def.defName == "AC_FilledCorticalStack")
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer));
                this.SavePawnToCorticalStack(pawn);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.hasPawn)
            {
                stringBuilder.Append("AlteredCarbon.Name".Translate() + ": " + this.name + "\n");
                stringBuilder.Append("AlteredCarbon.faction".Translate() + ": " + this.faction + "\n");

                Backstory newChildhood = null;
                BackstoryDatabase.TryGetWithIdentifier(this.childhood, out newChildhood, true);
                stringBuilder.Append("AlteredCarbon.childhood".Translate() + ": " + newChildhood.title.CapitalizeFirst() + "\n");

                if (this.adulthood?.Length > 0)
                {
                    Backstory newAdulthood = null;
                    BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out newAdulthood, true);
                    stringBuilder.Append("AlteredCarbon.adulthood".Translate() + ": " + newAdulthood.title.CapitalizeFirst() + "\n");
                }
                stringBuilder.Append("AlteredCarbon.ageChronologicalTicks".Translate() + ": " + (int)(this.ageChronologicalTicks / 3600000) + "\n");

            }
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            IEnumerator<Gizmo> enumerator = null;

            if (this.hasPawn)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = new Action(this.EmptyStack);
                command_Action.defaultLabel = "AlteredCarbon.EmptyStack".Translate();
                command_Action.defaultDesc = "AlteredCarbon.EmptyStackDesc".Translate();
                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
                yield return command_Action;
            }
            yield break;
        }

        public void EmptyStack()
        {
            var newStack = ThingMaker.MakeThing(AlteredCarbonDefOf.AC_EmptyCorticalStack);
            GenSpawn.Spawn(newStack, this.Position, this.Map);
            Find.Selector.Select(newStack);
            this.Destroy();
        }
        public void SavePawnFromHediff(Hediff_CorticalStack hediff)
        {
            this.name = hediff.name;
            this.hostilityMode = hediff.hostilityMode;
            this.areaRestriction = hediff.areaRestriction;
            this.ageChronologicalTicks = hediff.ageChronologicalTicks;
            this.medicalCareCategory = hediff.medicalCareCategory;
            this.selfTend = hediff.selfTend;
            this.foodRestriction = hediff.foodRestriction;
            this.outfit = hediff.outfit;
            this.drugPolicy = hediff.drugPolicy;
            this.times = hediff.times;
            this.thoughts = hediff.thoughts;
            this.faction = hediff.faction;
            this.traits = hediff.traits;
            this.relations = hediff.relations;
            this.skills = hediff.skills;
            this.childhood = hediff.childhood;
            this.adulthood = hediff.adulthood;
            this.priorities = hediff.priorities;
            this.hasPawn = true;
            this.gender = hediff.gender;
        }

        public void SavePawnToCorticalStack(Pawn pawn)
        {
            this.name = pawn.Name;
            this.hostilityMode = (int)pawn.playerSettings.hostilityResponse;
            this.areaRestriction = pawn.playerSettings.AreaRestriction;
            this.ageChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
            this.medicalCareCategory = pawn.playerSettings.medCare;
            this.selfTend = pawn.playerSettings.selfTend;
            this.foodRestriction = pawn.foodRestriction.CurrentFoodRestriction;
            this.outfit = pawn.outfits.CurrentOutfit;
            this.drugPolicy = pawn.drugs.CurrentPolicy;
            this.times = pawn.timetable.times;
            this.thoughts = pawn.needs.mood.thoughts.memories.Memories;
            this.faction = pawn.Faction;
            this.traits = pawn.story.traits.allTraits;
            this.relations = pawn.relations.DirectRelations;
            this.skills = pawn.skills.skills;
            this.childhood = pawn.story.childhood.identifier;
            if (pawn.story.adulthood != null)
            {
                this.adulthood = pawn.story.adulthood.identifier;
            }
            this.priorities = new Dictionary<WorkTypeDef, int>();
            foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
            {
                this.priorities[w] = pawn.workSettings.GetPriority(w);
            }
            this.hasPawn = true;

            this.gender = pawn.gender;
        }

        public override void Tick()
        {
            base.Tick();
        }

        public void OverwritePawn(Pawn pawn)
        {
            if (pawn.Faction != this.faction)
            {
                pawn.SetFaction(this.faction);
            }
            pawn.Name = this.name;
            for (int num = pawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
            {
                pawn.needs.mood.thoughts.memories.RemoveMemory(pawn.needs.mood.thoughts.memories.Memories[num]);
            }
            foreach (var thought in this.thoughts)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }
            pawn.story.traits.allTraits.Clear();
            foreach (var trait in this.traits)
            {
                pawn.story.traits.GainTrait(trait);
            }
            pawn.relations.ClearAllRelations();
            foreach (var rel in this.relations)
            {
                pawn.relations.AddDirectRelation(rel.def, rel.otherPawn);
            }
            pawn.skills.skills.Clear();
            foreach (var skill in this.skills)
            {
                var newSkill = new SkillRecord(pawn, skill.def);
                newSkill.passion = skill.passion;
                newSkill.levelInt = skill.levelInt;
                newSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                newSkill.xpSinceMidnight = skill.xpSinceMidnight;
                pawn.skills.skills.Add(newSkill);
            }

            pawn.playerSettings.hostilityResponse = (HostilityResponseMode)this.hostilityMode;

            Backstory newChildhood = null;
            BackstoryDatabase.TryGetWithIdentifier(this.childhood, out newChildhood, true);
            pawn.story.childhood = newChildhood;
            if (this.adulthood?.Length > 0)
            {
                Backstory newAdulthood = null;
                BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out newAdulthood, true);
                pawn.story.adulthood = newAdulthood;
            }
            else
            {
                pawn.story.adulthood = null;
            }
            pawn.Notify_DisabledWorkTypesChanged();
            foreach (var priority in priorities)
            {
                pawn.workSettings.SetPriority(priority.Key, priority.Value);
            }
            pawn.playerSettings.AreaRestriction = this.areaRestriction;
            pawn.playerSettings.medCare = this.medicalCareCategory;
            pawn.playerSettings.selfTend = this.selfTend;
            pawn.foodRestriction.CurrentFoodRestriction = this.foodRestriction;
            pawn.outfits.CurrentOutfit = this.outfit;
            pawn.drugs.CurrentPolicy = this.drugPolicy;
            pawn.ageTracker.AgeChronologicalTicks = this.ageChronologicalTicks;
            pawn.timetable.times = this.times;

            if (pawn.gender != this.gender)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.BodyPurist))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGenderDouble);
                }
                else
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGender);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<Name>(ref this.name, "name", new object[0]);
            Scribe_Values.Look<int>(ref this.hostilityMode, "hostilityMode");
            Scribe_References.Look<Area>(ref this.areaRestriction, "areaRestriction", false);
            Scribe_Values.Look<MedicalCareCategory>(ref this.medicalCareCategory, "medicalCareCategory", 0, false);
            Scribe_Values.Look<bool>(ref this.selfTend, "selfTend", false, false);
            Scribe_Values.Look<long>(ref this.ageChronologicalTicks, "ageChronologicalTicks", 0, false);

            Scribe_References.Look<Outfit>(ref this.outfit, "outfit", false);
            Scribe_References.Look<FoodRestriction>(ref this.foodRestriction, "foodPolicy", false);
            Scribe_References.Look<DrugPolicy>(ref this.drugPolicy, "drugPolicy", false);

            Scribe_Collections.Look<TimeAssignmentDef>(ref this.times, "times");
            Scribe_Collections.Look<Thought_Memory>(ref this.thoughts, "thoughts");
            Scribe_References.Look<Faction>(ref this.faction, "faction", true);
            Scribe_Values.Look<string>(ref this.childhood, "childhood", null, false);
            Scribe_Values.Look<string>(ref this.adulthood, "adulthood", null, false);
            Scribe_Collections.Look<Trait>(ref this.traits, "traits");
            Scribe_Collections.Look<SkillRecord>(ref this.skills, "skills");
            Scribe_Collections.Look<DirectPawnRelation>(ref this.relations, "relations");
            Scribe_Collections.Look<WorkTypeDef, int>(ref this.priorities, "priorities");
            Scribe_Values.Look<bool>(ref this.hasPawn, "hasPawn", false, false);

            Scribe_Values.Look<Gender>(ref this.gender, "gender", 0, false);

        }
    }
}

