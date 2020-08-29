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

        public void CopyStatsFromBrainTemplate(Thing brainTemplate)
        {

        }

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

                pawn.skills.GetSkill(skillOffset.skill).Level += skillOffset.offset;
            }

            foreach (var skillPassionOffset in this.skillPassionsOffsets)
            {
                var skill = pawn.skills.GetSkill(skillPassionOffset.skill);
                var offset = ((int)skill.passion - skillPassionOffset.offset) + 2;
                Log.Message("offset: " + offset, true);
                if (offset > 0)
                {
                    var finalValue = (int)skill.passion + offset;
                    Log.Message("finalValue: " + finalValue, true);
                    Log.Message("skill.passion: " + skill.passion, true);
                    switch (finalValue)
                    {
                        case 0:
                            skill.passion = Passion.None;
                            break;
                        case 1:
                            skill.passion = Passion.Minor;
                            break;
                        case 2:
                            skill.passion = Passion.Major;
                            break;
                    }
                    Log.Message("skill.passion: " + skill.passion, true);
                }
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

