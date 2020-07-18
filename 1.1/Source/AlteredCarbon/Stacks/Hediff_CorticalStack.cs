using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_CorticalStack : Hediff_Implant
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
        public string pawnID;
        public Dictionary<WorkTypeDef, int> priorities;

        public bool hasPawn = false;

        public Gender gender;

        public List<RoyalTitle> royalTitles;
        public Dictionary<Faction, int> favor = new Dictionary<Faction, int>();
        public Dictionary<Faction, Pawn> heirs = new Dictionary<Faction, Pawn>();
        public List<Thing> bondedThings = new List<Thing>();

        public bool isCopied = false;
        public int stackGroupID;
        public override void PostMake()
        {
            base.PostMake();
            if (ACUtils.ACTracker.stacksRelationships != null)
            {
                this.stackGroupID = ACUtils.ACTracker.stacksRelationships.Count + 1;
            }
            else
            {
                this.stackGroupID = 0;
            }
        }

        public void SavePawn(Pawn pawn)
        {
            this.name = pawn.Name;
            if (pawn.playerSettings != null)
            {
                this.hostilityMode = (int)pawn.playerSettings.hostilityResponse;
                this.areaRestriction = pawn.playerSettings.AreaRestriction;
                this.medicalCareCategory = pawn.playerSettings.medCare;
                this.selfTend = pawn.playerSettings.selfTend;
            }
            this.ageChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
            this.foodRestriction = pawn.foodRestriction?.CurrentFoodRestriction;
            this.outfit = pawn.outfits?.CurrentOutfit;
            this.drugPolicy = pawn.drugs?.CurrentPolicy;
            this.times = pawn.timetable?.times;
            this.thoughts = pawn.needs?.mood?.thoughts?.memories?.Memories;
            this.faction = pawn.Faction;
            this.traits = pawn.story?.traits?.allTraits;
            this.relations = pawn.relations?.DirectRelations;
            this.skills = pawn.skills?.skills;
            this.childhood = pawn.story?.childhood?.identifier;
            if (pawn.story?.adulthood != null)
            {
                this.adulthood = pawn.story.adulthood.identifier;
            }
            this.priorities = new Dictionary<WorkTypeDef, int>();
            if (pawn.workSettings != null && Traverse.Create(pawn.workSettings).Field("priorities").GetValue<DefMap<WorkTypeDef, int>>() != null)
            {
                foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    this.priorities[w] = pawn.workSettings.GetPriority(w);
                }
            }
            this.hasPawn = true;
            this.gender = pawn.gender;
            this.pawnID = pawn.ThingID;
            if (ModLister.RoyaltyInstalled)
            {
                this.royalTitles = pawn.royalty?.AllTitlesForReading;
                this.favor = Traverse.Create(pawn.royalty).Field("favor").GetValue<Dictionary<Faction, int>>();
                this.heirs = Traverse.Create(pawn.royalty).Field("heirs").GetValue<Dictionary<Faction, Pawn>>();
                foreach (var map in Find.Maps)
                {
                    foreach (var thing in map.listerThings.AllThings)
                    {
                        var comp = thing.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            this.bondedThings.Add(thing);
                        }
                    }
                }
            }
        }

        public override void Notify_PawnDied()
        {
            if (!this.hasPawn)
            {
                SavePawn(this.pawn);
            }
            base.Notify_PawnDied();
        }

        public override void Notify_PawnKilled()
        {
            if (!this.hasPawn)
            {
                SavePawn(this.pawn);
            }
            base.Notify_PawnKilled();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (!this.pawn.Dead)
            {
                Notify_ColonistKilled_Patch.DisableKilledEffect = true;
                this.pawn.Kill(null);
                Notify_ColonistKilled_Patch.DisableKilledEffect = false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.stackGroupID, "stackGroupID", 0);
            Scribe_Values.Look<bool>(ref this.isCopied, "isCopied", false, false);
            Log.Message(this + " this.stackGroupID: " + this.stackGroupID);
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
            Scribe_Values.Look<string>(ref this.pawnID, "pawnID", null, false);
            Scribe_Collections.Look<Trait>(ref this.traits, "traits");
            Scribe_Collections.Look<SkillRecord>(ref this.skills, "skills");
            Scribe_Collections.Look<DirectPawnRelation>(ref this.relations, "relations");
            Scribe_Collections.Look<WorkTypeDef, int>(ref this.priorities, "priorities");
            Scribe_Values.Look<bool>(ref this.hasPawn, "hasPawn", false, false);
            Scribe_Values.Look<Gender>(ref this.gender, "gender", 0, false);

            if (ModLister.RoyaltyInstalled)
            {
                Scribe_Collections.Look<Faction, int>(ref this.favor, "favor",
                    LookMode.Reference, LookMode.Value,
                    ref this.favorKeys, ref this.favorValues);
                Scribe_Collections.Look<Faction, Pawn>(ref this.heirs, "heirs",
                    LookMode.Reference, LookMode.Reference,
                    ref this.heirsKeys, ref this.heirsValues);

                Scribe_Collections.Look<Thing>(ref this.bondedThings, "bondedThings", LookMode.Reference);
                Scribe_Collections.Look<RoyalTitle>(ref this.royalTitles, "royalTitles", LookMode.Deep);
            }
        }

        private List<Faction> favorKeys = new List<Faction>();
        private List<int> favorValues = new List<int>();

        private List<Faction> heirsKeys = new List<Faction>();
        private List<Pawn> heirsValues = new List<Pawn>();
    }
}

