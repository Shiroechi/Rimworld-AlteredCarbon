using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_SleeveBodyStats : Hediff
    {
        public List<SkillOffsets> skillsOffsets;

        public List<SkillOffsets> skillPassionsOffsets;
        public void ApplyEffects()
        {
            List<SkillOffsets> negativeSkillsOffset = new List<SkillOffsets>();
            foreach (var skillOffset in this.skillsOffsets)
            {
                var curLevel = pawn.skills.GetSkill(skillOffset.skill).Level + skillOffset.offset;
                if (curLevel > 20) curLevel = 20;

                var negativeSkillOffset = new SkillOffsets
                {
                    skill = skillOffset.skill,
                    offset = pawn.skills.GetSkill(skillOffset.skill).Level - curLevel
                };
                negativeSkillsOffset.Add(negativeSkillOffset);
                pawn.skills.GetSkill(skillOffset.skill).Level = curLevel;
            }
            List<SkillOffsets> negativeSkillsPassionOffset = new List<SkillOffsets>();

            foreach (var skillPassionOffset in this.skillPassionsOffsets)
            {
                Log.Message("skillPassionOffset: " + skillPassionOffset.offset, true);
                var skill = pawn.skills.GetSkill(skillPassionOffset.skill);
                Log.Message("(int)skill.passion: " + (int)skill.passion, true);
                var finalValue = (int)skill.passion + skillPassionOffset.offset;
                Log.Message("finalValue: " + finalValue, true);
                Log.Message("1: " + pawn.skills.GetSkill(skillPassionOffset.skill) + " - skill.passion: " + pawn.skills.GetSkill(skillPassionOffset.skill).passion, true);

                var negativeSkillOffset = new SkillOffsets
                {
                    skill = skillPassionOffset.skill,
                    offset = (int)skill.passion - skillPassionOffset.offset
                };
                negativeSkillsPassionOffset.Add(negativeSkillOffset);
                if (finalValue <= 2)
                {
                    switch (finalValue)
                    {
                        case 0:
                            skill.passion = Passion.None;
                            Log.Message("skill.passion = Passion.None");
                            break;
                        case 1:
                            skill.passion = Passion.Minor;
                            Log.Message("skill.passion = Passion.Minor");
                            break;
                        case 2:
                            skill.passion = Passion.Major;
                            Log.Message("skill.passion = Passion.Major");
                            break;
                        default:
                            skill.passion = Passion.None;
                            Log.Message("default: skill.passion = Passion.None;");
                            break;
                    }
                }
                else
                {
                    skill.passion = Passion.Major;
                    Log.Message("2 skill.passion = Passion.Major");
                }
            }
            foreach (var c1 in negativeSkillsOffset)
            {
                Log.Message("1 Negative: " + c1.skill + " - " + c1.offset, true);
            }

            foreach (var c2 in negativeSkillsPassionOffset)
            {
                Log.Message("2 Negative: " + c2.skill + " - " + c2.offset, true);
            }
            var corticalHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            if (corticalHediff != null)
            {
                corticalHediff.negativeSkillsOffsets = negativeSkillsOffset;
                corticalHediff.negativeSkillPassionsOffsets = negativeSkillsPassionOffset;
            }
            Log.Message("corticalHediff: " + corticalHediff, true);
            Log.Message("----------------", true);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref skillsOffsets, "skillsOffsets", LookMode.Deep);
            Scribe_Collections.Look(ref skillPassionsOffsets, "skillPassionsOffsets", LookMode.Deep);
        }
    }
}

