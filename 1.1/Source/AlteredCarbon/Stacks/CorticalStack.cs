using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

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
        public string pawnID;

        public List<RoyalTitle> royalTitles;
        public Dictionary<Faction, int> favor = new Dictionary<Faction, int>();
        public Dictionary<Faction, Pawn> heirs = new Dictionary<Faction, Pawn>();
        public List<Thing> bondedThings = new List<Thing>();

        public bool isCopied = false;
        public int stackGroupID;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && !hasPawn && this.def.defName == "AC_FilledCorticalStack")
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer));
                this.SavePawnToCorticalStack(pawn);
                if (ACUtils.ACTracker.stacksRelationships != null)
                {
                    this.stackGroupID = ACUtils.ACTracker.stacksRelationships.Count + 1;
                }
                else
                {
                    this.stackGroupID = 0;
                }
                Log.Message("1 RegisterStack: " + this.stackGroupID);
                ACUtils.ACTracker.RegisterStack(this);
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false, 0))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            else if (this.def == AlteredCarbonDefOf.AC_FilledCorticalStack)
            {
                string label = "AlteredCarbon.WipeStack".Translate();
                Action action = delegate ()
                {
                    Job job = JobMaker.MakeJob(AlteredCarbonDefOf.AC_WipeStack, this);
                    job.count = 1;
                    myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                };
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption
                        (label, action, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn,
                        this, "ReservedBy");
            }
            if (this.hasPawn)
            {
                if (this.Map.listerThings.ThingsOfDef(AlteredCarbonDefOf.AC_EmptyCorticalStack)
                        .Where(x => myPawn.CanReserveAndReach(x, PathEndMode.ClosestTouch, Danger.Deadly)).Any())
                {
                    if (myPawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt >= 10)
                    {
                        string label = "AlteredCarbon.DuplicateStack".Translate();
                        Action action = delegate ()
                        {
                            Job job = JobMaker.MakeJob(AlteredCarbonDefOf.AC_CopyStack, this);
                            job.count = 1;
                            myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        };
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption
                                (label, action, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn,
                                this, "ReservedBy");
                    }
                    else if (myPawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt < 10)
                    {
                        FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("AlteredCarbon.CannotCopyNoIntellectual"), null,
                            MenuOptionPriority.Default, null, null, 0f, null, null);
                        yield return floatMenuOption;
                    }
                }
                else
                {
                    FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate(
                        "AlteredCarbon.CannotCopyNoOtherEmptyStacks"), null,
                            MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return floatMenuOption;
                }
            }

            yield break;
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
        public void EmptyStack()
        {
            Find.WindowStack.Add(new Dialog_MessageBox("AlteredCarbon.EmptyStackConfirmation".Translate(),
                "No".Translate(), null,
                "Yes".Translate(), delegate ()
                {
                    Log.Message(" - EmptyStack - var newStack = ThingMaker.MakeThing(AlteredCarbonDefOf.AC_EmptyCorticalStack); - 1", true);
                    var newStack = ThingMaker.MakeThing(AlteredCarbonDefOf.AC_EmptyCorticalStack);
                    Log.Message(" - EmptyStack - GenSpawn.Spawn(newStack, this.Position, this.Map); - 2", true);
                    GenSpawn.Spawn(newStack, this.Position, this.Map);
                    Log.Message(" - EmptyStack - Find.Selector.Select(newStack); - 3", true);
                    Find.Selector.Select(newStack);
                    Log.Message(" - EmptyStack - if (this.faction == Faction.OfPlayer) - 4", true);
                    if (this.faction == Faction.OfPlayer)
                    {
                        Log.Message(" - EmptyStack - ACUtils.ACTracker.stacksIndex.Remove(this.pawnID + this.name); - 5", true);
                        ACUtils.ACTracker.stacksIndex.Remove(this.pawnID + this.name);
                        Log.Message(" - EmptyStack - Pawn tempPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer)); - 6", true);
                        Pawn tempPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer));
                        Log.Message(" - EmptyStack - this.OverwritePawn(tempPawn); - 7", true);
                        this.OverwritePawn(tempPawn);
                        Log.Message(" - EmptyStack - PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(tempPawn, null, PawnDiedOrDownedThoughtsKind.Died); - 8", true);
                        PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(tempPawn, null, PawnDiedOrDownedThoughtsKind.Died);
                        Log.Message(" - EmptyStack - tempPawn.health.NotifyPlayerOfKilled(null, null, null); - 9", true);
                        tempPawn.health.NotifyPlayerOfKilled(null, null, null);
                        Log.Message(" - EmptyStack - Find.StoryWatcher.statsRecord.Notify_ColonistKilled(); - 10", true);
                        Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
                    }
                    this.Destroy();
                    Log.Message(" - EmptyStack - }, null, false, null, null)); - 12", true);
                }, null, false, null, null));
        }
        public void SavePawnFromHediff(Hediff_CorticalStack hediff)
        {
            Log.Message(" - SavePawnFromHediff - this.name = hediff.name; - 13", true);
            this.name = hediff.name;
            Log.Message(" - SavePawnFromHediff - this.hostilityMode = hediff.hostilityMode; - 14", true);
            this.hostilityMode = hediff.hostilityMode;
            Log.Message(" - SavePawnFromHediff - this.areaRestriction = hediff.areaRestriction; - 15", true);
            this.areaRestriction = hediff.areaRestriction;
            this.ageChronologicalTicks = hediff.ageChronologicalTicks;
            Log.Message(" - SavePawnFromHediff - this.medicalCareCategory = hediff.medicalCareCategory; - 17", true);
            this.medicalCareCategory = hediff.medicalCareCategory;
            Log.Message(" - SavePawnFromHediff - this.selfTend = hediff.selfTend; - 18", true);
            this.selfTend = hediff.selfTend;
            Log.Message(" - SavePawnFromHediff - this.foodRestriction = hediff.foodRestriction; - 19", true);
            this.foodRestriction = hediff.foodRestriction;
            Log.Message(" - SavePawnFromHediff - this.outfit = hediff.outfit; - 20", true);
            this.outfit = hediff.outfit;
            Log.Message(" - SavePawnFromHediff - this.drugPolicy = hediff.drugPolicy; - 21", true);
            this.drugPolicy = hediff.drugPolicy;
            Log.Message(" - SavePawnFromHediff - this.times = hediff.times; - 22", true);
            this.times = hediff.times;
            Log.Message(" - SavePawnFromHediff - this.thoughts = hediff.thoughts; - 23", true);
            this.thoughts = hediff.thoughts;
            Log.Message(" - SavePawnFromHediff - this.faction = hediff.faction; - 24", true);
            this.faction = hediff.faction;
            Log.Message(" - SavePawnFromHediff - this.traits = hediff.traits; - 25", true);
            this.traits = hediff.traits;
            Log.Message(" - SavePawnFromHediff - this.relations = hediff.relations; - 26", true);
            this.relations = hediff.relations;
            Log.Message(" - SavePawnFromHediff - this.skills = hediff.skills; - 27", true);
            this.skills = hediff.skills;
            Log.Message(" - SavePawnFromHediff - this.childhood = hediff.childhood; - 28", true);
            this.childhood = hediff.childhood;
            Log.Message(" - SavePawnFromHediff - this.adulthood = hediff.adulthood; - 29", true);
            this.adulthood = hediff.adulthood;
            Log.Message(" - SavePawnFromHediff - this.priorities = hediff.priorities; - 30", true);
            this.priorities = hediff.priorities;
            Log.Message(" - SavePawnFromHediff - this.hasPawn = true; - 31", true);
            this.hasPawn = true;
            Log.Message(" - SavePawnFromHediff - this.gender = hediff.gender; - 32", true);
            this.gender = hediff.gender;
            Log.Message(" - SavePawnFromHediff - this.pawnID = hediff.pawnID; - 33", true);
            this.pawnID = hediff.pawnID;

            Log.Message(" - SavePawnFromHediff - if (ModLister.RoyaltyInstalled) - 34", true);
            if (ModLister.RoyaltyInstalled)
            {
                Log.Message(" - SavePawnFromHediff - this.royalTitles = hediff.royalTitles; - 35", true);
                this.royalTitles = hediff.royalTitles;
                Log.Message(" - SavePawnFromHediff - this.favor = hediff.favor; - 36", true);
                this.favor = hediff.favor;
                Log.Message(" - SavePawnFromHediff - this.heirs = hediff.heirs; - 37", true);
                this.heirs = hediff.heirs;
                Log.Message(" - SavePawnFromHediff - this.bondedThings = hediff.bondedThings; - 38", true);
                this.bondedThings = hediff.bondedThings;
            }
            this.isCopied = hediff.isCopied;
            this.stackGroupID = hediff.stackGroupID;
        }

        public void SavePawnToCorticalStack(Pawn pawn)
        {
            Log.Message(" - SavePawnToCorticalStack - this.name = pawn.Name; - 39", true);
            this.name = pawn.Name;
            Log.Message(" - SavePawnToCorticalStack - this.hostilityMode = (int)pawn.playerSettings.hostilityResponse; - 40", true);
            this.hostilityMode = (int)pawn.playerSettings.hostilityResponse;
            Log.Message(" - SavePawnToCorticalStack - this.areaRestriction = pawn.playerSettings.AreaRestriction; - 41", true);
            this.areaRestriction = pawn.playerSettings.AreaRestriction;
            this.ageChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
            Log.Message(" - SavePawnToCorticalStack - this.medicalCareCategory = pawn.playerSettings.medCare; - 43", true);
            this.medicalCareCategory = pawn.playerSettings.medCare;
            Log.Message(" - SavePawnToCorticalStack - this.selfTend = pawn.playerSettings.selfTend; - 44", true);
            this.selfTend = pawn.playerSettings.selfTend;
            Log.Message(" - SavePawnToCorticalStack - this.foodRestriction = pawn.foodRestriction.CurrentFoodRestriction; - 45", true);
            this.foodRestriction = pawn.foodRestriction.CurrentFoodRestriction;
            Log.Message(" - SavePawnToCorticalStack - this.outfit = pawn.outfits.CurrentOutfit; - 46", true);
            this.outfit = pawn.outfits.CurrentOutfit;
            Log.Message(" - SavePawnToCorticalStack - this.drugPolicy = pawn.drugs.CurrentPolicy; - 47", true);
            this.drugPolicy = pawn.drugs.CurrentPolicy;
            Log.Message(" - SavePawnToCorticalStack - this.times = pawn.timetable.times; - 48", true);
            this.times = pawn.timetable.times;
            Log.Message(" - SavePawnToCorticalStack - this.thoughts = pawn.needs?.mood?.thoughts?.memories?.Memories; - 49", true);
            this.thoughts = pawn.needs?.mood?.thoughts?.memories?.Memories;
            Log.Message(" - SavePawnToCorticalStack - this.faction = pawn.Faction; - 50", true);
            this.faction = pawn.Faction;
            Log.Message(" - SavePawnToCorticalStack - this.traits = pawn.story.traits.allTraits; - 51", true);
            this.traits = pawn.story.traits.allTraits;
            Log.Message(" - SavePawnToCorticalStack - this.relations = pawn.relations.DirectRelations; - 52", true);
            this.relations = pawn.relations.DirectRelations;
            Log.Message(" - SavePawnToCorticalStack - this.skills = pawn.skills.skills; - 53", true);
            this.skills = pawn.skills.skills;
            Log.Message(" - SavePawnToCorticalStack - this.childhood = pawn.story.childhood.identifier; - 54", true);
            this.childhood = pawn.story.childhood.identifier;
            Log.Message(" - SavePawnToCorticalStack - if (pawn.story.adulthood != null) - 55", true);
            if (pawn.story.adulthood != null)
            {
                Log.Message(" - SavePawnToCorticalStack - this.adulthood = pawn.story.adulthood.identifier; - 56", true);
                this.adulthood = pawn.story.adulthood.identifier;
            }
            Log.Message(" - SavePawnToCorticalStack - if (pawn.workSettings != null) - 57", true);
            if (pawn.workSettings != null)
            {
                Log.Message(" - SavePawnToCorticalStack - this.priorities = new Dictionary<WorkTypeDef, int>(); - 58", true);
                this.priorities = new Dictionary<WorkTypeDef, int>();
                Log.Message(" - SavePawnToCorticalStack - foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs) - 59", true);
                foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    Log.Message(" - SavePawnToCorticalStack - this.priorities[w] = pawn.workSettings.GetPriority(w); - 60", true);
                    this.priorities[w] = pawn.workSettings.GetPriority(w);
                }
            }

            this.hasPawn = true;

            this.gender = pawn.gender;
            Log.Message(" - SavePawnToCorticalStack - this.pawnID = pawn.ThingID; - 63", true);
            this.pawnID = pawn.ThingID;

            Log.Message(" - SavePawnToCorticalStack - if (ModLister.RoyaltyInstalled) - 64", true);
            if (ModLister.RoyaltyInstalled)
            {
                Log.Message(" - SavePawnToCorticalStack - this.royalTitles = pawn.royalty.AllTitlesForReading; - 65", true);
                this.royalTitles = pawn.royalty.AllTitlesForReading;
                Log.Message(" - SavePawnToCorticalStack - this.favor = Traverse.Create(pawn.royalty).Field(\"favor\").GetValue<Dictionary<Faction, int>>(); - 66", true);
                this.favor = Traverse.Create(pawn.royalty).Field("favor").GetValue<Dictionary<Faction, int>>();
                Log.Message(" - SavePawnToCorticalStack - this.heirs = Traverse.Create(pawn.royalty).Field(\"heirs\").GetValue<Dictionary<Faction, Pawn>>(); - 67", true);
                this.heirs = Traverse.Create(pawn.royalty).Field("heirs").GetValue<Dictionary<Faction, Pawn>>();
                Log.Message(" - SavePawnToCorticalStack - foreach (var map in Find.Maps) - 68", true);
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

        public void CopyFromOtherStack(CorticalStack otherStack)
        {
            this.name = otherStack.name;
            this.hostilityMode = otherStack.hostilityMode;
            this.areaRestriction = otherStack.areaRestriction;
            this.ageChronologicalTicks = otherStack.ageChronologicalTicks;
            this.medicalCareCategory = otherStack.medicalCareCategory;
            this.selfTend = otherStack.selfTend;
            this.foodRestriction = otherStack.foodRestriction;
            this.outfit = otherStack.outfit;
            this.drugPolicy = otherStack.drugPolicy;
            this.times = otherStack.times;
            this.thoughts = otherStack.thoughts;
            this.faction = otherStack.faction;
            this.traits = otherStack.traits;
            this.relations = otherStack.relations;
            this.skills = otherStack.skills;
            this.childhood = otherStack.childhood;
            this.adulthood = otherStack.adulthood;
            this.priorities = otherStack.priorities;
            this.hasPawn = true;
            this.gender = otherStack.gender;
            this.pawnID = otherStack.pawnID;

            if (ModLister.RoyaltyInstalled)
            {
                this.royalTitles = otherStack.royalTitles;
                this.favor = otherStack.favor;
                this.heirs = otherStack.heirs;
                this.bondedThings = otherStack.bondedThings;
            }
            this.isCopied = true;
            this.stackGroupID = otherStack.stackGroupID;
        }

        public override void Tick()
        {
            base.Tick();
        }

        public void OverwritePawn(Pawn pawn)
        {
            Log.Message(" - OverwritePawn - var extension = this.def.GetModExtension<StackSavingOptionsModExtension>(); - 74", true);
            var extension = this.def.GetModExtension<StackSavingOptionsModExtension>();
            Log.Message(" - OverwritePawn - if (pawn.Faction != this.faction) - 75", true);
            if (pawn.Faction != this.faction)
            {
                Log.Message(" - OverwritePawn - pawn.SetFaction(this.faction); - 76", true);
                pawn.SetFaction(this.faction);
            }
            pawn.Name = this.name;
            for (int num = pawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
            {
                Log.Message(" - OverwritePawn - pawn.needs.mood.thoughts.memories.RemoveMemory(pawn.needs.mood.thoughts.memories.Memories[num]); - 78", true);
                pawn.needs.mood.thoughts.memories.RemoveMemory(pawn.needs.mood.thoughts.memories.Memories[num]);
            }
            Log.Message(" - OverwritePawn - if (this.thoughts != null) - 79", true);
            if (this.thoughts != null)
            {
                Log.Message(" - OverwritePawn - foreach (var thought in this.thoughts) - 80", true);
                foreach (var thought in this.thoughts)
                {
                    Log.Message(" - OverwritePawn - if (thought is Thought_MemorySocial && thought.otherPawn == null) - 81", true);
                    if (thought is Thought_MemorySocial && thought.otherPawn == null)
                    {
                        Log.Message(" - OverwritePawn - continue; - 82", true);
                        continue;
                    }
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought, thought.otherPawn);
                }
            }
            pawn.story.traits.allTraits.Clear();
            Log.Message(" - OverwritePawn - foreach (var trait in this.traits) - 85", true);
            foreach (var trait in this.traits)
            {
                Log.Message(" - OverwritePawn - if (extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName)) - 86", true);
                if (extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName))
                {
                    Log.Message(" - OverwritePawn - continue; - 87", true);
                    continue;
                }
                pawn.story.traits.GainTrait(trait);
            }
            pawn.relations.ClearAllRelations();
            Log.Message(" - OverwritePawn - foreach (var rel in this.relations) - 90", true);
            foreach (var rel in this.relations)
            {
                Log.Message(" - OverwritePawn - pawn.relations.AddDirectRelation(rel.def, rel.otherPawn); - 91", true);
                pawn.relations.AddDirectRelation(rel.def, rel.otherPawn);
            }
            pawn.skills.skills.Clear();
            Log.Message(" - OverwritePawn - foreach (var skill in this.skills) - 93", true);
            foreach (var skill in this.skills)
            {
                Log.Message(" - OverwritePawn - var newSkill = new SkillRecord(pawn, skill.def); - 94", true);
                var newSkill = new SkillRecord(pawn, skill.def);
                Log.Message(" - OverwritePawn - newSkill.passion = skill.passion; - 95", true);
                newSkill.passion = skill.passion;
                Log.Message(" - OverwritePawn - newSkill.levelInt = skill.levelInt; - 96", true);
                newSkill.levelInt = skill.levelInt;
                Log.Message(" - OverwritePawn - newSkill.xpSinceLastLevel = skill.xpSinceLastLevel; - 97", true);
                newSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                Log.Message(" - OverwritePawn - newSkill.xpSinceMidnight = skill.xpSinceMidnight; - 98", true);
                newSkill.xpSinceMidnight = skill.xpSinceMidnight;
                Log.Message(" - OverwritePawn - pawn.skills.skills.Add(newSkill); - 99", true);
                pawn.skills.skills.Add(newSkill);
            }

            pawn.playerSettings.hostilityResponse = (HostilityResponseMode)this.hostilityMode;

            Backstory newChildhood = null;
            Log.Message(" - OverwritePawn - BackstoryDatabase.TryGetWithIdentifier(this.childhood, out newChildhood, true); - 102", true);
            BackstoryDatabase.TryGetWithIdentifier(this.childhood, out newChildhood, true);
            Log.Message(" - OverwritePawn - pawn.story.childhood = newChildhood; - 103", true);
            pawn.story.childhood = newChildhood;
            Log.Message(" - OverwritePawn - if (this.adulthood?.Length > 0) - 104", true);
            if (this.adulthood?.Length > 0)
            {
                Log.Message(" - OverwritePawn - Backstory newAdulthood = null; - 105", true);
                Backstory newAdulthood = null;
                Log.Message(" - OverwritePawn - BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out newAdulthood, true); - 106", true);
                BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out newAdulthood, true);
                Log.Message(" - OverwritePawn - pawn.story.adulthood = newAdulthood; - 107", true);
                pawn.story.adulthood = newAdulthood;
            }
            else
            {
                Log.Message(" - OverwritePawn - pawn.story.adulthood = null; - 108", true);
                pawn.story.adulthood = null;
            }
            pawn.Notify_DisabledWorkTypesChanged();
            Log.Message(" - OverwritePawn - if (priorities != null) - 110", true);
            if (priorities != null)
            {
                Log.Message(" - OverwritePawn - foreach (var priority in priorities) - 111", true);
                foreach (var priority in priorities)
                {
                    Log.Message(" - OverwritePawn - pawn.workSettings.SetPriority(priority.Key, priority.Value); - 112", true);
                    pawn.workSettings.SetPriority(priority.Key, priority.Value);
                }
            }
            pawn.playerSettings.AreaRestriction = this.areaRestriction;
            Log.Message(" - OverwritePawn - pawn.playerSettings.medCare = this.medicalCareCategory; - 114", true);
            pawn.playerSettings.medCare = this.medicalCareCategory;
            Log.Message(" - OverwritePawn - pawn.playerSettings.selfTend = this.selfTend; - 115", true);
            pawn.playerSettings.selfTend = this.selfTend;
            Log.Message(" - OverwritePawn - pawn.foodRestriction.CurrentFoodRestriction = this.foodRestriction; - 116", true);
            pawn.foodRestriction.CurrentFoodRestriction = this.foodRestriction;
            Log.Message(" - OverwritePawn - if (pawn.outfits == null) pawn.outfits = new Pawn_OutfitTracker(); - 117", true);
            if (pawn.outfits == null) pawn.outfits = new Pawn_OutfitTracker();
            Log.Message(" - OverwritePawn - pawn.outfits.CurrentOutfit = this.outfit; - 118", true);
            pawn.outfits.CurrentOutfit = this.outfit;
            Log.Message(" - OverwritePawn - if (pawn.drugs == null) pawn.drugs = new Pawn_DrugPolicyTracker(); - 119", true);
            if (pawn.drugs == null) pawn.drugs = new Pawn_DrugPolicyTracker();
            Log.Message(" - OverwritePawn - pawn.drugs.CurrentPolicy = this.drugPolicy; - 120", true);
            pawn.drugs.CurrentPolicy = this.drugPolicy;
            pawn.ageTracker.AgeChronologicalTicks = this.ageChronologicalTicks;
            Log.Message(" - OverwritePawn - if (pawn.timetable == null) pawn.timetable = new Pawn_TimetableTracker(pawn); - 122", true);
            if (pawn.timetable == null) pawn.timetable = new Pawn_TimetableTracker(pawn);
            Log.Message(" - OverwritePawn - pawn.timetable.times = this.times; - 123", true);
            pawn.timetable.times = this.times;

            Log.Message(" - OverwritePawn - if (pawn.gender != this.gender) - 124", true);
            if (pawn.gender != this.gender)
            {
                Log.Message(" - OverwritePawn - if (pawn.story.traits.HasTrait(TraitDefOf.BodyPurist)) - 125", true);
                if (pawn.story.traits.HasTrait(TraitDefOf.BodyPurist))
                {
                    Log.Message(" - OverwritePawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGenderDouble); - 126", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGenderDouble);
                }
                else
                {
                    Log.Message(" - OverwritePawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGender); - 127", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGender);
                }
            }
            Log.Message(" - OverwritePawn - if (ModLister.RoyaltyInstalled) - 128", true);
            if (ModLister.RoyaltyInstalled)
            {
                Log.Message(" - OverwritePawn - if (pawn.royalty == null) pawn.royalty = new Pawn_RoyaltyTracker(pawn); - 129", true);
                if (pawn.royalty == null) pawn.royalty = new Pawn_RoyaltyTracker(pawn);
                Log.Message(" - OverwritePawn - foreach (var title in this.royalTitles) - 130", true);
                foreach (var title in this.royalTitles)
                {
                    Log.Message(" - OverwritePawn - pawn.royalty.SetTitle(title.faction, title.def, false, false, false); - 131", true);
                    pawn.royalty.SetTitle(title.faction, title.def, false, false, false);
                }
                Log.Message(" - OverwritePawn - foreach (var heir in this.heirs) - 132", true);
                foreach (var heir in this.heirs)
                {
                    Log.Message(" - OverwritePawn - pawn.royalty.SetHeir(heir.Value, heir.Key); - 133", true);
                    pawn.royalty.SetHeir(heir.Value, heir.Key);
                }
                Log.Message(" - OverwritePawn - foreach (var fav in this.favor) - 134", true);
                foreach (var fav in this.favor)
                {
                    Log.Message(" - OverwritePawn - pawn.royalty.SetFavor(fav.Key, fav.Value); - 135", true);
                    pawn.royalty.SetFavor(fav.Key, fav.Value);
                }
                Log.Message(" - OverwritePawn - foreach (var bonded in this.bondedThings) - 136", true);
                foreach (var bonded in this.bondedThings)
                {
                    Log.Message(" - OverwritePawn - var comp = bonded.TryGetComp<CompBladelinkWeapon>(); - 137", true);
                    var comp = bonded.TryGetComp<CompBladelinkWeapon>();
                    Log.Message(" - OverwritePawn - if (comp != null) - 138", true);
                    if (comp != null)
                    {
                        Log.Message(" - OverwritePawn - comp.bondedPawn = pawn; - 139", true);
                        comp.bondedPawn = pawn;
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.stackGroupID, "stackGroupID", 0);
            Log.Message(this + " this.stackGroupID: " + this.stackGroupID);

            Scribe_Values.Look<bool>(ref this.isCopied, "isCopied", false, false);

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

