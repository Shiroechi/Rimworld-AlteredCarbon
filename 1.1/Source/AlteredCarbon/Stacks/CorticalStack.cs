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
                var pawnKind = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike).RandomElement();
                var faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).RandomElement();
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction));
                this.SavePawnToCorticalStack(pawn);
                if (ACUtils.ACTracker.stacksRelationships != null)
                {
                    this.stackGroupID = ACUtils.ACTracker.stacksRelationships.Count + 1;
                }
                else
                {
                    this.stackGroupID = 0;
                }
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
            else if (this.def == AlteredCarbonDefOf.AC_FilledCorticalStack && myPawn.skills.GetSkill(SkillDefOf.Intellectual).Level >= 5)
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
            else if (this.def == AlteredCarbonDefOf.AC_FilledCorticalStack && myPawn.skills.GetSkill(SkillDefOf.Intellectual).Level < 5)
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption("AlteredCarbon.CantWipeStackTooDumb".Translate(), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
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
                stringBuilder.Append("Gender".Translate() + ": " + this.gender.GetLabel().CapitalizeFirst() + "\n");

            }
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (this.Destroyed)
            {
                this.KillInnerPawn();
            }
        }
        public void KillInnerPawn(bool affectFactionRelationship = false, Pawn affecter = null)
        {
            if (this.hasPawn)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer));
                this.OverwritePawn(pawn);
                if (affectFactionRelationship)
                {
                    this.faction.TryAffectGoodwillWith(affecter.Faction, -70, canSendMessage: true, canSendHostilityLetter: true,
                        "AlteredCarbon.GoodwillChangedReason_ErasedPawn".Translate(pawn.Named("PAWN")), affecter);
                    QuestUtility.SendQuestTargetSignals(pawn.questTags, "SurgeryViolation", pawn.Named("SUBJECT"));
                }
                pawn.Kill(null);
            }
        }
        public void EmptyStack(bool affectFactionRelationship = false, Pawn affecter = null)
        {
            Find.WindowStack.Add(new Dialog_MessageBox("AlteredCarbon.EmptyStackConfirmation".Translate(),
                "No".Translate(), null,
                "Yes".Translate(), delegate ()
                {
                    var newStack = ThingMaker.MakeThing(AlteredCarbonDefOf.AC_EmptyCorticalStack);
                    GenSpawn.Spawn(newStack, this.Position, this.Map);
                    Find.Selector.Select(newStack);
                    ACUtils.ACTracker.stacksIndex.Remove(this.pawnID + this.name);
                    this.KillInnerPawn(affectFactionRelationship, affecter);
                    this.Destroy();
                }, null, false, null, null));
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
            this.pawnID = hediff.pawnID;

            if (ModLister.RoyaltyInstalled)
            {
                this.royalTitles = hediff.royalTitles;
                this.favor = hediff.favor;
                this.heirs = hediff.heirs;
                this.bondedThings = hediff.bondedThings;
            }
            this.isCopied = hediff.isCopied;
            this.stackGroupID = hediff.stackGroupID;
        }

        public void SavePawnToCorticalStack(Pawn pawn)
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
                        if (comp != null)
                        {
                            Log.Message("Checking: " + thing, true);
                        }
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            Log.Message("Adding: " + thing, true);
                            this.bondedThings.Add(thing);
                        }
                    }
                    foreach (var gear in pawn.apparel.WornApparel)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                            Log.Message("Checking: " + gear, true);
                        }
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            Log.Message("Adding: " + gear, true);
                            this.bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in pawn.equipment.AllEquipmentListForReading)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                            Log.Message("Checking: " + gear, true);
                        }
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            Log.Message("Adding: " + gear, true);
                            this.bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in pawn.inventory.innerContainer)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                            Log.Message("Checking: " + gear, true);
                        }
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            Log.Message("Adding: " + gear, true);
                            this.bondedThings.Add(gear);
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

        public void OverwritePawn(Pawn pawn)
        {
            Log.Message(" - OverwritePawn - var extension = this.def.GetModExtension<StackSavingOptionsModExtension>(); - 1", true);
            var extension = this.def.GetModExtension<StackSavingOptionsModExtension>();
            Log.Message(" - OverwritePawn - if (pawn.Faction != this.faction) - 2", true);
            if (pawn.Faction != this.faction)
            {
                Log.Message(" - OverwritePawn - pawn.SetFaction(this.faction); - 3", true);
                pawn.SetFaction(this.faction);
            }
            pawn.Name = this.name;
            if (pawn.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                for (int num = pawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    Log.Message(" - OverwritePawn - pawn.needs.mood.thoughts.memories.RemoveMemory(pawn.needs.mood.thoughts.memories.Memories[num]); - 5", true);
                    pawn.needs.mood.thoughts.memories.RemoveMemory(pawn.needs.mood.thoughts.memories.Memories[num]);
                }
            }

            Log.Message(" - OverwritePawn - if (this.thoughts != null) - 6", true);
            if (this.thoughts != null)
            {
                Log.Message(" - OverwritePawn - foreach (var thought in this.thoughts) - 7", true);
                foreach (var thought in this.thoughts)
                {
                    Log.Message(" - OverwritePawn - if (thought is Thought_MemorySocial && thought.otherPawn == null) - 8", true);
                    if (thought is Thought_MemorySocial && thought.otherPawn == null)
                    {
                        Log.Message(" - OverwritePawn - continue; - 9", true);
                        continue;
                    }
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought, thought.otherPawn);
                }
            }
            pawn.story.traits.allTraits.Clear();
            Log.Message(" - OverwritePawn - foreach (var trait in this.traits) - 12", true);
            if (this.traits != null)
            {
                foreach (var trait in this.traits)
                {
                    Log.Message(" - OverwritePawn - if (extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName)) - 13", true);
                    if (extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName))
                    {
                        Log.Message(" - OverwritePawn - continue; - 14", true);
                        continue;
                    }
                    pawn.story.traits.GainTrait(trait);
                }
            }
            pawn.relations.ClearAllRelations();
            Log.Message(" - OverwritePawn - foreach (var rel in this.relations) - 17", true);
            foreach (var rel in this.relations)
            {
                Log.Message(" - OverwritePawn - pawn.relations.AddDirectRelation(rel.def, rel.otherPawn); - 18", true);
                pawn.relations.AddDirectRelation(rel.def, rel.otherPawn);
            }
            pawn.skills.skills.Clear();
            Log.Message(" - OverwritePawn - foreach (var skill in this.skills) - 20", true);
            if (this.skills != null)
            {
                foreach (var skill in this.skills)
                {
                    Log.Message(" - OverwritePawn - var newSkill = new SkillRecord(pawn, skill.def); - 21", true);
                    var newSkill = new SkillRecord(pawn, skill.def);
                    Log.Message(" - OverwritePawn - newSkill.passion = skill.passion; - 22", true);
                    newSkill.passion = skill.passion;
                    Log.Message(" - OverwritePawn - newSkill.levelInt = skill.levelInt; - 23", true);
                    newSkill.levelInt = skill.levelInt;
                    Log.Message(" - OverwritePawn - newSkill.xpSinceLastLevel = skill.xpSinceLastLevel; - 24", true);
                    newSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                    Log.Message(" - OverwritePawn - newSkill.xpSinceMidnight = skill.xpSinceMidnight; - 25", true);
                    newSkill.xpSinceMidnight = skill.xpSinceMidnight;
                    Log.Message(" - OverwritePawn - pawn.skills.skills.Add(newSkill); - 26", true);
                    pawn.skills.skills.Add(newSkill);
                }
            }
            if (pawn.playerSettings == null) pawn.playerSettings = new Pawn_PlayerSettings(pawn);
            pawn.playerSettings.hostilityResponse = (HostilityResponseMode)this.hostilityMode;

            Backstory newChildhood = null;
            Log.Message(" - OverwritePawn - BackstoryDatabase.TryGetWithIdentifier(this.childhood, out newChildhood, true); - 29", true);
            BackstoryDatabase.TryGetWithIdentifier(this.childhood, out newChildhood, true);
            Log.Message(" - OverwritePawn - pawn.story.childhood = newChildhood; - 30", true);
            pawn.story.childhood = newChildhood;
            Log.Message(" - OverwritePawn - if (this.adulthood?.Length > 0) - 31", true);
            if (this.adulthood?.Length > 0)
            {
                Log.Message(" - OverwritePawn - Backstory newAdulthood = null; - 32", true);
                Backstory newAdulthood = null;
                Log.Message(" - OverwritePawn - BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out newAdulthood, true); - 33", true);
                BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out newAdulthood, true);
                Log.Message(" - OverwritePawn - pawn.story.adulthood = newAdulthood; - 34", true);
                pawn.story.adulthood = newAdulthood;
            }
            else
            {
                Log.Message(" - OverwritePawn - pawn.story.adulthood = null; - 35", true);
                pawn.story.adulthood = null;
            }
            if (pawn.workSettings == null) pawn.workSettings = new Pawn_WorkSettings();
            pawn.Notify_DisabledWorkTypesChanged();
            Log.Message(" - OverwritePawn - if (priorities != null) - 37", true);
            if (priorities != null)
            {
                Log.Message(" - OverwritePawn - foreach (var priority in priorities) - 38", true);
                foreach (var priority in priorities)
                {
                    Log.Message(" - OverwritePawn - pawn.workSettings.SetPriority(priority.Key, priority.Value); - 39", true);
                    pawn.workSettings.SetPriority(priority.Key, priority.Value);
                }
            }
            pawn.playerSettings.AreaRestriction = this.areaRestriction;
            Log.Message(" - OverwritePawn - pawn.playerSettings.medCare = this.medicalCareCategory; - 41", true);
            pawn.playerSettings.medCare = this.medicalCareCategory;
            Log.Message(" - OverwritePawn - pawn.playerSettings.selfTend = this.selfTend; - 42", true);
            pawn.playerSettings.selfTend = this.selfTend;
            Log.Message(" - OverwritePawn - pawn.foodRestriction.CurrentFoodRestriction = this.foodRestriction; - 43", true);
            if (pawn.foodRestriction == null) pawn.foodRestriction = new Pawn_FoodRestrictionTracker();
            pawn.foodRestriction.CurrentFoodRestriction = this.foodRestriction;
            Log.Message(" - OverwritePawn - if (pawn.outfits == null) pawn.outfits = new Pawn_OutfitTracker(); - 44", true);
            if (pawn.outfits == null) pawn.outfits = new Pawn_OutfitTracker();
            Log.Message(" - OverwritePawn - pawn.outfits.CurrentOutfit = this.outfit; - 45", true);
            pawn.outfits.CurrentOutfit = this.outfit;
            Log.Message(" - OverwritePawn - if (pawn.drugs == null) pawn.drugs = new Pawn_DrugPolicyTracker(); - 46", true);
            if (pawn.drugs == null) pawn.drugs = new Pawn_DrugPolicyTracker();
            Log.Message(" - OverwritePawn - pawn.drugs.CurrentPolicy = this.drugPolicy; - 47", true);
            pawn.drugs.CurrentPolicy = this.drugPolicy;
            pawn.ageTracker.AgeChronologicalTicks = this.ageChronologicalTicks;
            Log.Message(" - OverwritePawn - if (pawn.timetable == null) pawn.timetable = new Pawn_TimetableTracker(pawn); - 49", true);
            if (pawn.timetable == null) pawn.timetable = new Pawn_TimetableTracker(pawn);
            Log.Message(" - OverwritePawn - pawn.timetable.times = this.times; - 50", true);
            pawn.timetable.times = this.times;

            Log.Message(" - OverwritePawn - if (pawn.gender != this.gender) - 51", true);
            if (pawn.gender != this.gender)
            {
                Log.Message(" - OverwritePawn - if (pawn.story.traits.HasTrait(TraitDefOf.BodyPurist)) - 52", true);
                if (pawn.story.traits.HasTrait(TraitDefOf.BodyPurist))
                {
                    Log.Message(" - OverwritePawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGenderDouble); - 53", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGenderDouble);
                }
                else
                {
                    Log.Message(" - OverwritePawn - pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGender); - 54", true);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGender);
                }
            }
            Log.Message(" - OverwritePawn - if (ModLister.RoyaltyInstalled) - 55", true);
            if (ModLister.RoyaltyInstalled)
            {
                Log.Message(" - OverwritePawn - if (pawn.royalty == null) pawn.royalty = new Pawn_RoyaltyTracker(pawn); - 56", true);
                if (pawn.royalty == null) pawn.royalty = new Pawn_RoyaltyTracker(pawn);
                Log.Message(" - OverwritePawn - foreach (var title in this.royalTitles) - 57", true);
                if (this.royalTitles != null)
                {
                    foreach (var title in this.royalTitles)
                    {
                        Log.Message(" - OverwritePawn - pawn.royalty.SetTitle(title.faction, title.def, false, false, false); - 58", true);
                        pawn.royalty.SetTitle(title.faction, title.def, false, false, false);
                    }
                }
                Log.Message(" - OverwritePawn - foreach (var heir in this.heirs) - 59", true);
                if (this.heirs != null)
                {
                    foreach (var heir in this.heirs)
                    {
                        Log.Message(" - OverwritePawn - pawn.royalty.SetHeir(heir.Value, heir.Key); - 60", true);
                        pawn.royalty.SetHeir(heir.Value, heir.Key);
                    }
                }

                Log.Message(" - OverwritePawn - foreach (var fav in this.favor) - 61", true);
                if (this.favor != null)
                {
                    foreach (var fav in this.favor)
                    {
                        Log.Message(" - OverwritePawn - pawn.royalty.SetFavor(fav.Key, fav.Value); - 62", true);
                        pawn.royalty.SetFavor(fav.Key, fav.Value);
                    }
                }

                Log.Message(" - OverwritePawn - foreach (var bonded in this.bondedThings) - 63", true);
                if (this.bondedThings != null)
                {
                    foreach (var bonded in this.bondedThings)
                    {
                        Log.Message(" - OverwritePawn - var comp = bonded.TryGetComp<CompBladelinkWeapon>(); - 64", true);
                        var comp = bonded.TryGetComp<CompBladelinkWeapon>();
                        Log.Message(" - OverwritePawn - if (comp != null) - 65", true);
                        if (comp != null)
                        {
                            Log.Message(" - OverwritePawn - comp.bondedPawn = pawn; - 66", true);
                            comp.bondedPawn = pawn;
                        }
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

