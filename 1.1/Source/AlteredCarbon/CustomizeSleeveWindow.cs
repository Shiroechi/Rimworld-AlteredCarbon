using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{

    public class CustomizeSleeveWindow : Window
    {
        //Variables
        public Pawn newSleeve;
        public int finalExtraPrintingTimeCost = 0;
        public bool refreshAndroidPortrait = false;
        public Vector2 upgradesScrollPosition = new Vector2();
        public Vector2 traitsScrollPosition = new Vector2();
        List<Trait> allTraits = new List<Trait>();

        //Customization
        public PawnKindDef currentPawnKindDef;
        public Backstory newChildhoodBackstory;
        public Backstory newAdulthoodBackstory;
        public Trait replacedTrait;
        public Trait newTrait;

        //Original android values
        public List<Trait> originalTraits = new List<Trait>();
        public Building_SleeveGrower sleeveGrower;
        //Static Values
        public override Vector2 InitialSize => new Vector2(898f, 608f);
        public static readonly float upgradesOffset = 640f;
        private static readonly Vector2 PawnPortraitSize = new Vector2(200f, 280f);

        public CustomizeSleeveWindow(Building_SleeveGrower sleeveGrower)
        {
            this.sleeveGrower = sleeveGrower;
            currentPawnKindDef = PawnKindDefOf.Colonist;
            newSleeve = GetNewPawn();
        }

        public override void DoWindowContents(Rect inRect)
        {
            //Detect changes
            if (refreshAndroidPortrait)
            {
                newSleeve.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(newSleeve);
                PortraitsCache.PortraitsCacheUpdate();
                refreshAndroidPortrait = false;
            }

            Rect pawnRect = new Rect(inRect);
            pawnRect.width = PawnPortraitSize.x + 16f;
            pawnRect.height = PawnPortraitSize.y + 16f;
            pawnRect = inRect.RightHalf();
            pawnRect = inRect.RightHalf();
            pawnRect.x += 16f;
            pawnRect.y += 16f;

            //Draw Pawn stuff.
            if (newSleeve != null)
            {
                //Pawn
                Rect pawnRenderRect = new Rect(pawnRect.xMin + (pawnRect.width - PawnPortraitSize.x) / 2f - 10f, pawnRect.yMin + 20f, PawnPortraitSize.x, PawnPortraitSize.y);
                GUI.DrawTexture(pawnRenderRect, PortraitsCache.Get(newSleeve, PawnPortraitSize, default(Vector3), 1f));

                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(0f, 0f, inRect.width, 32f), "AlteredCarbon.SleeveCustomization".Translate());

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;

                //Hair customization
                float finalPawnCustomizationWidthOffset = (pawnRect.x + pawnRect.width + 16f + (inRect.width - upgradesOffset));

                {
                    Rect rowRect = new Rect(pawnRect.x + pawnRect.width + 16f, pawnRect.y, inRect.width - finalPawnCustomizationWidthOffset, 24f);

                    //Color
                    //newAndroid.story.hairColor
                    Rect hairColorRect = new Rect(rowRect);
                    hairColorRect.width = hairColorRect.height;

                    Widgets.DrawBoxSolid(hairColorRect, newSleeve.story.hairColor);
                    Widgets.DrawBox(hairColorRect);
                    Widgets.DrawHighlightIfMouseover(hairColorRect);

                    if (Widgets.ButtonInvisible(hairColorRect))
                    {
                        //Change color
                        Func<Color, Action> setColorAction = (Color color) => delegate {
                            newSleeve.story.hairColor = color;
                            newSleeve.Drawer.renderer.graphics.ResolveAllGraphics();
                            PortraitsCache.SetDirty(newSleeve);
                            PortraitsCache.PortraitsCacheUpdate();
                        };

                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        //foreach(Color hairColor in HairColors)
                        //{
                        //    list.Add(new FloatMenuOption("ChangeColor".Translate(), setColorAction(hairColor), MenuOptionPriority.Default, null, null, 24f, delegate (Rect rect)
                        //    {
                        //        Rect colorRect = new Rect(rect);
                        //        colorRect.x += 8f;
                        //        Widgets.DrawBoxSolid(colorRect, hairColor);
                        //        Widgets.DrawBox(colorRect);
                        //        return false;
                        //    }, null));
                        //}
                        Find.WindowStack.Add(new FloatMenu(list));
                    }

                    Rect hairTypeRect = new Rect(rowRect);
                    hairTypeRect.width -= hairColorRect.width;
                    hairTypeRect.width -= 8f;
                    hairTypeRect.x = hairColorRect.x + hairColorRect.width + 8f;

                    if (Widgets.ButtonText(hairTypeRect, newSleeve?.story?.hairDef?.LabelCap ?? "Bald"))
                    {
                        IEnumerable<HairDef> hairs = 
                            from hairdef in DefDatabase<HairDef>.AllDefs
                            where (newSleeve.gender == Gender.Female && (hairdef.hairGender == HairGender.Any || hairdef.hairGender == HairGender.Female || hairdef.hairGender == HairGender.FemaleUsually)) || (newSleeve.gender == Gender.Male && (hairdef.hairGender == HairGender.Any || hairdef.hairGender == HairGender.Male || hairdef.hairGender == HairGender.MaleUsually))
                            select hairdef;

                        if(hairs != null)
                        {
                            FloatMenuUtility.MakeMenu<HairDef>(hairs, hairDef => hairDef.LabelCap, (HairDef hairDef) => delegate
                            {
                                newSleeve.story.hairDef = hairDef;
                                newSleeve.Drawer.renderer.graphics.ResolveAllGraphics();
                                PortraitsCache.SetDirty(newSleeve);
                                PortraitsCache.PortraitsCacheUpdate();
                            });
                        }
                    }
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public Pawn GetNewPawn(Gender gender = Gender.Female)
        {
            //Make base pawn.
            Pawn pawn;

            pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, Faction.OfPlayer, RimWorld.PawnGenerationContext.NonPlayer,
            -1, true, false, false, false, false, false, 0f, false, true, true, false, false, false, true, fixedGender: gender));

            //Post process age to adulthood. Two methods.
            LifeStageAge adultLifestage = pawn.RaceProps.lifeStageAges.Last();
            if (adultLifestage != null)
            {
                long ageInTicks = (long)Math.Ceiling(adultLifestage.minAge) * (long)GenDate.TicksPerYear;

                pawn.ageTracker.AgeBiologicalTicks = ageInTicks;
                pawn.ageTracker.AgeChronologicalTicks = ageInTicks;
            }
            else
            {
                //Max age
                long ageInTicks = (long)(pawn.RaceProps.lifeExpectancy * (long)GenDate.TicksPerYear * 0.2f);

                pawn.ageTracker.AgeBiologicalTicks = ageInTicks;
                pawn.ageTracker.AgeChronologicalTicks = ageInTicks;
            }

            //Destroy all equipment and items in inventory.
            pawn?.equipment.DestroyAllEquipment();
            pawn?.inventory.DestroyAll();

            //Strip off clothes and replace with bandages.
            pawn.apparel.DestroyAll();

            //Refresh disabled skills and work.
            if (pawn.workSettings != null)
            {
                //Todo: Fix this.
                pawn.workSettings.EnableAndInitialize();
            }
            //newAndroid.story.Notify_TraitChanged();
            if (pawn.skills != null)
            {
                pawn.skills.Notify_SkillDisablesChanged();
            }
            if (!pawn.Dead && pawn.RaceProps.Humanlike)
            {
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }

            return pawn;
        }
    }
}
