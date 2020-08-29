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
        public List<string> hediffs;

        public List<SkillOffsets> skillsOffsets;

        public List<SkillOffsets> skillPassionsOffsets;
        public void ApplyEffects()
        {
            foreach (var hediff in this.hediffs)
            {
                var hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediff);
                if (hediffDef != null)
                {
                    pawn.health.AddHediff(HediffMaker.MakeHediff(hediffDef, pawn));
                }
            }

            foreach (var skillOffset in this.skillsOffsets)
            {
                var curLevel = pawn.skills.GetSkill(skillOffset.skill).Level + skillOffset.offset;
                if (curLevel > 20) curLevel = 20;
                pawn.skills.GetSkill(skillOffset.skill).Level = curLevel;
            }

            foreach (var skillPassionOffset in this.skillPassionsOffsets)
            {
                var skill = pawn.skills.GetSkill(skillPassionOffset.skill);
                Log.Message("(int)skill.passion: " + (int)skill.passion, true);
                var finalValue = (int)skill.passion + skillPassionOffset.offset;
                Log.Message("finalValue: " + finalValue, true);
                Log.Message(skill + " - skill.passion: " + skill.passion, true);
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
                            Log.Message("skill.passion = Passion.Major");
                            break;
                    }
                }
                Log.Message(skill + " - skill.passion: " + skill.passion, true);
                Log.Message("----------------", true);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref hediffs, "hediffs", LookMode.Value);
            Scribe_Collections.Look(ref skillsOffsets, "skillsOffsets", LookMode.Deep);
            Scribe_Collections.Look(ref skillPassionsOffsets, "skillPassionsOffsets", LookMode.Deep);

        }
    }
}

