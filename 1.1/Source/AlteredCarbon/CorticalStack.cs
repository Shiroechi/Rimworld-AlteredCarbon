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
        public Faction faction;
        public List<Trait> traits;
        public List<DirectPawnRelation> relations;
        public List<SkillRecord> skills;
        public string childhood;
        public string adulthood;
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
            this.name = pawn.Name;
            this.faction = pawn.Faction;
            this.traits = pawn.story.traits.allTraits;
            this.relations = pawn.relations.DirectRelations;
            this.skills = pawn.skills.skills;
            this.childhood = pawn.story.childhood.identifier;
            this.adulthood = pawn.story.adulthood.identifier;
            this.hasPawn = true;
        }

        public override void Tick()
        {
            base.Tick();
            Log.Message("this.name: " + this.name);
            Log.Message("this.faction: " + this.faction);
            Log.Message("this.traits: " + this.traits.Count);
            Log.Message("this.relations: " + this.relations.Count);
            Log.Message("this.skills: " + this.skills.Count);
            Log.Message("this.childhood: " + this.childhood);
            Log.Message("this.adulthood: " + this.adulthood);
        }

        public void OverwritePawn(Pawn pawn)
        {
            Log.Message("Overwriting " + pawn);
            if (pawn.Faction != this.faction)
            {
                pawn.SetFaction(this.faction);
            }
            pawn.Name = this.name;
            pawn.story.traits.allTraits = this.traits;
            pawn.relations.ClearAllRelations();
            foreach (var rel in this.relations)
            {

                pawn.relations.AddDirectRelation(rel.def, rel.otherPawn);
            }
            pawn.skills.skills = this.skills;

            Backstory newChildhood = null;
            BackstoryDatabase.TryGetWithIdentifier(this.childhood, out newChildhood, true);
            pawn.story.childhood = newChildhood;

            Backstory newAdulthood = null;
            BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out newAdulthood, true);
            pawn.story.adulthood = newAdulthood;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<Name>(ref this.name, "name", new object[0]);
            Scribe_References.Look<Faction>(ref this.faction, "faction", true);
            Scribe_Values.Look<string>(ref this.childhood, "childhood", null, false);
            Scribe_Values.Look<string>(ref this.adulthood, "adulthood", null, false);
            Scribe_Collections.Look<Trait>(ref this.traits, "traits");
            Scribe_Collections.Look<SkillRecord>(ref this.skills, "skills");
            Scribe_Collections.Look<DirectPawnRelation>(ref this.relations, "relations");
            Scribe_Values.Look<bool>(ref this.hasPawn, "hasPawn", false, false);
        }
    }
}

