using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    class AlteredCarbonMod : Mod
    {
        public static AlteredCarbonSettings settings;
        public AlteredCarbonMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<AlteredCarbonSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        // Return the name of the mod in the settings tab in game
        public override string SettingsCategory()
        {
            return "Altered Carbon";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            DefsRemover.DoDefsRemoval();
        }
    }

    [StaticConstructorOnStartup]
    public static class DefsRemover
    {
        static DefsRemover()
        {
            DoDefsRemoval();
        }
        public static void RemoveDef(ThingDef def)
        {
            def.researchPrerequisites?.Clear();
            def.weaponTags?.Clear();
            def.deepCommonality = 0;
            def.generateCommonality = 0;
            def.tradeability = Tradeability.None;
            def.thingCategories?.Clear();
            def.thingCategories?.Add(ThingCategoryDefOf.Chunks);
            foreach (var recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                if (recipe.ProducedThingDef == def)
                {
                    recipe.recipeUsers?.Clear();
                    recipe.researchPrerequisites?.Clear();
                    recipe.researchPrerequisite = null;
                }
            }
            if (DefDatabase<ThingDef>.AllDefsListForReading.Contains(def))
            {
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(def);
            }
        }
        public static void DoDefsRemoval()
        {
            if (!AlteredCarbonMod.settings.allowAC_Apparel_ProtectorateArmor)
            {
                RemoveDef(ThingDef.Named("AC_Apparel_ProtectorateArmor"));
                RemoveDef(ThingDef.Named("AC_Apparel_ProtectorateArmorHelmet"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_BullpupPistol)
            {
                RemoveDef(ThingDef.Named("AC_Gun_BullpupPistol"));
                RemoveDef(ThingDef.Named("AC_Bullet_BullpupPistol"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_MACRevolver)
            {
                RemoveDef(ThingDef.Named("AC_Gun_MACRevolver"));
                RemoveDef(ThingDef.Named("AC_Bullet_MACRevolver"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_MACRifle)
            {
                RemoveDef(ThingDef.Named("AC_Gun_MACRifle"));
                RemoveDef(ThingDef.Named("AC_Bullet_MACRifle"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_QuickfirePistol)
            {
                RemoveDef(ThingDef.Named("AC_Gun_QuickfirePistol"));
                RemoveDef(ThingDef.Named("AC_Bullet_QuickfirePistol"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_ShockPDW)
            {
                RemoveDef(ThingDef.Named("AC_Gun_ShockPDW"));
                RemoveDef(ThingDef.Named("AC_Bullet_ShockPDW"));
            }
        }
    }
}
