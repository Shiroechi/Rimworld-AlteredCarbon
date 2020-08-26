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
    }

    [StaticConstructorOnStartup]
    public static class DefsRemover
    {
        static DefsRemover()
        {
            if (!AlteredCarbonMod.settings.allowAC_Apparel_ProtectorateArmor)
            {
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Apparel_ProtectorateArmor"));
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Apparel_ProtectorateArmorHelmet"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_BullpupPistol)
            {
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Gun_BullpupPistol"));
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Bullet_BullpupPistol"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_MACRevolver)
            {
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Gun_MACRevolver"));
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Bullet_MACRevolver"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_MACRifle)
            {
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Gun_MACRifle"));
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Bullet_MACRifle"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_QuickfirePistol)
            {
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Gun_QuickfirePistol"));
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Bullet_QuickfirePistol"));
            }
            if (!AlteredCarbonMod.settings.allowAC_Gun_ShockPDW)
            {
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Gun_ShockPDW"));
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(ThingDef.Named("AC_Bullet_ShockPDW"));
            }
        }
    }
}
