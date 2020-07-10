using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class CorticalStack : ThingWithComps
    {
        public Name name;
        public int hostilityMode;
        public Faction faction;
        public List<Thought_Memory> thoughts;
        public List<Trait> traits;
        public List<DirectPawnRelation> relations;
        public List<SkillRecord> skills;
        public string childhood;
        public string adulthood;
        public Dictionary<WorkTypeDef, int> priorities;

        public bool hasPawn = false;

        public override string Label
        {
            get
            {
                if (this.name != null)
                {
                    return this.name + " - " + base.Label;
                }
                else
                {
                    return base.Label;
                }
            }
        }
        public void SavePawnToCorticalStack(Pawn pawn)
        {
            Log.Message("Saving " + pawn);
            Log.Message(" - SavePawnToCorticalStack - this.name = pawn.Name; - 2", true);
            this.name = pawn.Name;
            Log.Message(" - SavePawnToCorticalStack - this.hostilityMode = (int)pawn.playerSettings.hostilityResponse; - 3", true);
            this.hostilityMode = (int)pawn.playerSettings.hostilityResponse;
            Log.Message(" - SavePawnToCorticalStack - this.thoughts = pawn.needs.mood.thoughts.memories.Memories; - 4", true);
            this.thoughts = pawn.needs.mood.thoughts.memories.Memories;
            Log.Message(" - SavePawnToCorticalStack - this.faction = pawn.Faction; - 5", true);
            this.faction = pawn.Faction;
            Log.Message(" - SavePawnToCorticalStack - this.traits = pawn.story.traits.allTraits; - 6", true);
            this.traits = pawn.story.traits.allTraits;
            Log.Message(" - SavePawnToCorticalStack - this.relations = pawn.relations.DirectRelations; - 7", true);
            this.relations = pawn.relations.DirectRelations;
            Log.Message(" - SavePawnToCorticalStack - this.skills = pawn.skills.skills; - 8", true);
            this.skills = pawn.skills.skills;
            Log.Message(" - SavePawnToCorticalStack - this.childhood = pawn.story.childhood.identifier; - 9", true);
            this.childhood = pawn.story.childhood.identifier;
            Log.Message(" - SavePawnToCorticalStack - this.adulthood = pawn.story.adulthood.identifier; - 10", true);
            if (pawn.story.adulthood != null)
            {
                this.adulthood = pawn.story.adulthood.identifier;
            }
            Log.Message(" - SavePawnToCorticalStack - this.priorities = new Dictionary<WorkTypeDef, int>(); - 11", true);
            this.priorities = new Dictionary<WorkTypeDef, int>();
            Log.Message(" - SavePawnToCorticalStack - foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs) - 12", true);
            foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
            {
                Log.Message(" - SavePawnToCorticalStack - priorities[w] = pawn.workSettings.GetPriority(w); - 13", true);
                this.priorities[w] = pawn.workSettings.GetPriority(w);
            }
            this.hasPawn = true;
        }

        public override void Tick()
        {
            base.Tick();
        }

        public void OverwritePawn(Pawn pawn)
        {
            Log.Message("Overwriting " + pawn);
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

            //pawn.skills.skills = this.skills;

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
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<Name>(ref this.name, "name", new object[0]);
            Scribe_Values.Look<int>(ref this.hostilityMode, "hostilityMode");
            Scribe_Collections.Look<Thought_Memory>(ref this.thoughts, "thoughts");
            Scribe_References.Look<Faction>(ref this.faction, "faction", true);
            Scribe_Values.Look<string>(ref this.childhood, "childhood", null, false);
            Scribe_Values.Look<string>(ref this.adulthood, "adulthood", null, false);
            Scribe_Collections.Look<Trait>(ref this.traits, "traits");
            Scribe_Collections.Look<SkillRecord>(ref this.skills, "skills");
            Scribe_Collections.Look<DirectPawnRelation>(ref this.relations, "relations");
            Scribe_Collections.Look<WorkTypeDef, int>(ref this.priorities, "priorities");
            Scribe_Values.Look<bool>(ref this.hasPawn, "hasPawn", false, false);
        }
    }
}

