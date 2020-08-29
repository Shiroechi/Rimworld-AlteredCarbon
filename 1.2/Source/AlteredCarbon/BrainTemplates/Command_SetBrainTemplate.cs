using System.Collections.Generic;
using UnityEngine;
using Verse;
using System.Linq;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
    public class Command_SetBrainTemplate : Command
    {
        public Map map;
        public List<Thing> brainTemplates;
        public Building_SleeveGrower building;
        public Command_SetBrainTemplate(Building_SleeveGrower building)
        {
            this.building = building;
            this.map = building.Map;
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            HashSet<ThingDef> brainTemplates = new HashSet<ThingDef>();
            brainTemplates.AddRange(this.map.listerThings.AllThings.Where(x => x.TryGetComp<CompBrainTemplate>() != null).Select(x => x.def));
            foreach (ThingDef brainTemplate in brainTemplates)
            {
                list.Add(new FloatMenuOption(brainTemplate.LabelCap, delegate
                {
                    this.InsertBrainTemplate(brainTemplate);
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }
            if (list.Count == 0)
            {
                list.Add(new FloatMenuOption("None".Translate(), null, MenuOptionPriority.Default, null, null, 29f, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }
        private void InsertBrainTemplate(ThingDef brainTemplate)
        {
            building.activeBrainTemplateToBeProcessed = brainTemplate;
        }
    }
}