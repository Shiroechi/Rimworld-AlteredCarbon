using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    class AlteredCarbonSettings : ModSettings
    {
        public bool allowStacksToBeStacked = false;

        public bool allowAC_Apparel_ProtectorateArmor = true;
        public bool allowAC_Gun_BullpupPistol = true;
        public bool allowAC_Gun_MACRevolver = true;
        public bool allowAC_Gun_MACRifle = true;
        public bool allowAC_Gun_QuickfirePistol = true;
        public bool allowAC_Gun_ShockPDW = true;
        public void ResetSavedDefs()
        {
            allowAC_Apparel_ProtectorateArmor = true;
            allowAC_Gun_BullpupPistol = true;
            allowAC_Gun_MACRevolver = true;
            allowAC_Gun_MACRifle = true;
            allowAC_Gun_QuickfirePistol = true;
            allowAC_Gun_ShockPDW = true;
        }

        public override void ExposeData()
        {

            base.ExposeData();
            Scribe_Values.Look(ref allowAC_Apparel_ProtectorateArmor, "allowAC_Apparel_ProtectorateArmor", true);
            Scribe_Values.Look(ref allowAC_Gun_BullpupPistol, "allowAC_Gun_BullpupPistol", true);
            Scribe_Values.Look(ref allowAC_Gun_MACRevolver, "allowAC_Gun_MACRevolver", true);
            Scribe_Values.Look(ref allowAC_Gun_MACRifle, "allowAC_Gun_MACRifle", true);
            Scribe_Values.Look(ref allowAC_Gun_QuickfirePistol, "allowAC_Gun_QuickfirePistol", true);
            Scribe_Values.Look(ref allowAC_Gun_ShockPDW, "allowAC_Gun_ShockPDW", true);
            Scribe_Values.Look(ref allowStacksToBeStacked, "allowStacksToBeStacked", true);

        }

        // Draw the actual settings window that shows up after selecting Z-Levels in the list
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("allowStacksToBeStacked".Translate(), ref allowStacksToBeStacked);
            listingStandard.CheckboxLabeled("allowAC_Apparel_ProtectorateArmor".Translate(), ref allowAC_Apparel_ProtectorateArmor);
            listingStandard.CheckboxLabeled("allowAC_Gun_BullpupPistol".Translate(), ref allowAC_Gun_BullpupPistol);
            listingStandard.CheckboxLabeled("allowAC_Gun_MACRevolver".Translate(), ref allowAC_Gun_MACRevolver);
            listingStandard.CheckboxLabeled("allowAC_Gun_MACRifle".Translate(), ref allowAC_Gun_MACRifle);
            listingStandard.CheckboxLabeled("allowAC_Gun_QuickfirePistol".Translate(), ref allowAC_Gun_QuickfirePistol);
            listingStandard.CheckboxLabeled("allowAC_Gun_ShockPDW".Translate(), ref allowAC_Gun_ShockPDW);
            listingStandard.End();
        }
    }
}
