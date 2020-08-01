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
        public override Vector2 InitialSize => new Vector2(728f, 538f);   //860x570
        public static readonly float upgradesOffset = 640f;
        private static readonly Vector2 PawnPortraitSize = new Vector2(200f, 280f);


        //RectValues
        public Rect lblName;
        public Rect lblGender;
        public Rect lblSkinColour;
        public Rect lblHeadShape;
        public Rect lblBodyShape;
        public Rect lblHairColour;
        public Rect lblHairType;
        public Rect lblTimeToGrow;
        public Rect lblRequireBiomass;

        public Rect lblLevelOfBeauty;
        public Rect lblLevelOfQuality;

        public Rect btnGenderMale;
        public Rect btnGenderFemale;

        public Rect btnSkinColourOutline;
        public Rect btnSkinColour1;
        public Rect btnSkinColour2;
        public Rect btnSkinColour3;
        public Rect btnSkinColour4;
        public Rect btnSkinColour5;

        public Rect btnHeadShapeOutline;
        public Rect btnHeadShapeArrowLeft;
        public Rect btnHeadShapeArrowRight;
        public Rect btnHeadShapeSelection;

        public Rect btnBodyShapeOutline;
        public Rect btnBodyShapeArrowLeft;
        public Rect btnBodyShapeArrowRight;
        public Rect btnBodyShapeSelection;

        public Rect btnHairColourOutlinePremade;
        public Rect btnHairColourOutline;
        public Rect btnHairColourOutline2;
        public Rect btnHairColour1;
        public Rect btnHairColour2;
        public Rect btnHairColour3;
        public Rect btnHairColour4;
        public Rect btnHairColour5;
        public Rect btnHairColour6;
        public Rect btnHairColour7;
        public Rect btnHairColour8;

        public Rect btnHairTypeOutline;
        public Rect btnHairTypeArrowLeft;
        public Rect btnHairTypeArrowRight;
        public Rect btnHairTypeSelection;

        public Rect pawnBox;
        public Rect pawnBoxPawn;

        public Rect btnLevelOfBeauty1;
        public Rect btnLevelOfBeauty2;
        public Rect btnLevelOfBeauty3;

        public Rect btnLevelOfQuality1;
        public Rect btnLevelOfQuality2;
        public Rect btnLevelOfQuality3;

        public Rect btnAccept;
        public Rect btnCancel;


        public float leftOffset = 20;
        public float topOffset = 50;
        public float optionOffset = 15;

        public float labelWidth = 120;
        public float buttonWidth = 120;
        public float buttonHeight = 30;
        public float buttonOffsetFromText = 10;
        public float buttonOffsetFromButton = 15;

        public float smallButtonOptionWidth = 44;  //for 5 elements   5 on each side + (buttonoptionwidth + offset) * number of buttons
        public float smallButtonOptionWidthHair = 25;  //for 8 elements  (255[OUTLINEWIDTH]-10)/NUM - 5     
        public float pawnBoxSide = 200;
        public float pawnSpacingFromEdge = 5;


        public Texture2D texDarkness;
        public Texture2D texColor;

        public Color selectedColor = Color.white;
        public Color selectedColorFinal;

        public int hairTypeIndex = 0;
        public int maleHeadTypeIndex = 0;
        public int femaleHeadTypeIndex = 0;
        public int maleBodyTypeIndex = 0;
        public int femaleBodyTypeIndex = 0;

        public int baseTicksToGrow = 1800000;
        public int baseTicksToGrow2 = 0;
        public int baseTicksToGrow3 = 0;

        public int baseMeatCost = 250;
        public int baseMeatCost2 = 0;
        public int baseMeatCost3 = 0;


        //button text subtle copied from Rimworld basecode but with minor changes to fit this UI
        public static bool ButtonTextSubtleCentered(Rect rect, string label, Vector2 functionalSizeOffset = default(Vector2))
        {
            Rect rect2 = rect;
            rect2.width += functionalSizeOffset.x;
            rect2.height += functionalSizeOffset.y;
            bool flag = false;
            if (Mouse.IsOver(rect2))
            {
                flag = true;
                GUI.color = GenUI.MouseoverColor;
            }
            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
            GUI.color = Color.white;
            Rect rect3 = new Rect(rect);
            if (flag)
            {
                rect3.x += 2f;
                rect3.y -= 2f;
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Widgets.Label(rect3, label);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = true;
            return Widgets.ButtonInvisible(rect2, false);
        }

        public float returnYfromPrevious(Rect rect)
        {
            float y;
            y = rect.y;
            y += rect.height;
            y += optionOffset;

            return y;
        }

        public float returnButtonOffset(Rect rect)
        {
            float y;
            y = rect.width + buttonOffsetFromText;
            return y;
        }


        public CustomizeSleeveWindow(Building_SleeveGrower sleeveGrower)
        {
            this.sleeveGrower = sleeveGrower;
            currentPawnKindDef = PawnKindDefOf.Colonist;
            newSleeve = GetNewPawn();


            //860x570
            //init Rects
            //gender
            lblGender = new Rect(leftOffset, topOffset, labelWidth, buttonHeight);
            btnGenderMale = lblGender;
            btnGenderMale.width = buttonWidth;
            btnGenderMale.x += returnButtonOffset(lblGender);
            btnGenderFemale = btnGenderMale;
            btnGenderFemale.x += btnGenderMale.width + buttonOffsetFromButton;

            //skin colour
            lblSkinColour = new Rect(leftOffset, returnYfromPrevious(lblGender), labelWidth, buttonHeight);
            btnSkinColourOutline = lblSkinColour;
            btnSkinColourOutline.x += returnButtonOffset(lblGender);
            btnSkinColourOutline.width = 10 + (smallButtonOptionWidth + 5) * 5; //5 on each side + (buttonoptionwidth + offset) * number of buttons
            btnSkinColour1 = new Rect(btnSkinColourOutline.x + 5, btnSkinColourOutline.y + 5, smallButtonOptionWidth, btnSkinColourOutline.height - 10);
            btnSkinColour2 = new Rect(btnSkinColour1.x + 5 + smallButtonOptionWidth, btnSkinColourOutline.y + 5, smallButtonOptionWidth, btnSkinColourOutline.height - 10);
            btnSkinColour3 = new Rect(btnSkinColour2.x + 5 + smallButtonOptionWidth, btnSkinColourOutline.y + 5, smallButtonOptionWidth, btnSkinColourOutline.height - 10);
            btnSkinColour4 = new Rect(btnSkinColour3.x + 5 + smallButtonOptionWidth, btnSkinColourOutline.y + 5, smallButtonOptionWidth, btnSkinColourOutline.height - 10);
            btnSkinColour5 = new Rect(btnSkinColour4.x + 5 + smallButtonOptionWidth, btnSkinColourOutline.y + 5, smallButtonOptionWidth, btnSkinColourOutline.height - 10);

            //head shape
            lblHeadShape = new Rect(leftOffset, returnYfromPrevious(lblSkinColour), labelWidth, buttonHeight);
            btnHeadShapeOutline = lblHeadShape;
            btnHeadShapeOutline.x += returnButtonOffset(lblHeadShape);
            btnHeadShapeOutline.width = buttonWidth * 2 + buttonOffsetFromButton;
            btnHeadShapeArrowLeft = new Rect(btnHeadShapeOutline.x + 2, btnHeadShapeOutline.y, btnHeadShapeOutline.height, btnHeadShapeOutline.height);
            btnHeadShapeArrowRight = new Rect(btnHeadShapeOutline.x + btnHeadShapeOutline.width - btnHeadShapeOutline.height - 2, btnHeadShapeOutline.y, btnHeadShapeOutline.height, btnHeadShapeOutline.height);
            btnHeadShapeSelection = new Rect(btnHeadShapeOutline.x + 5 + btnHeadShapeArrowLeft.width, btnHeadShapeOutline.y, btnHeadShapeOutline.width - 2 * (btnHeadShapeArrowLeft.width) - 10, btnHeadShapeOutline.height);


            //body shape
            lblBodyShape = new Rect(leftOffset, returnYfromPrevious(lblHeadShape), labelWidth, buttonHeight);
            btnBodyShapeOutline = lblBodyShape;
            btnBodyShapeOutline.x += returnButtonOffset(lblBodyShape);
            btnBodyShapeOutline.width = buttonWidth * 2 + buttonOffsetFromButton;
            btnBodyShapeArrowLeft = new Rect(btnBodyShapeOutline.x + 2, btnBodyShapeOutline.y, btnBodyShapeOutline.height, btnBodyShapeOutline.height);
            btnBodyShapeArrowRight = new Rect(btnBodyShapeOutline.x + btnBodyShapeOutline.width - btnBodyShapeOutline.height - 2, btnBodyShapeOutline.y, btnBodyShapeOutline.height, btnBodyShapeOutline.height);
            btnBodyShapeSelection = new Rect(btnBodyShapeOutline.x + 5 + btnBodyShapeArrowLeft.width, btnBodyShapeOutline.y, btnBodyShapeOutline.width - 2 * (btnBodyShapeArrowLeft.width) - 10, btnBodyShapeOutline.height);

            //hair colour
            lblHairColour = new Rect(leftOffset, returnYfromPrevious(lblBodyShape), labelWidth, buttonHeight);
            btnHairColourOutlinePremade = lblHairColour;
            btnHairColourOutlinePremade.x += returnButtonOffset(lblHairColour);
            btnHairColourOutlinePremade.width = buttonWidth * 2 + buttonOffsetFromButton;
            btnHairColourOutline = btnHairColourOutlinePremade;
            btnHairColourOutline.y = returnYfromPrevious(btnHairColourOutlinePremade);
            btnHairColourOutline2 = btnHairColourOutline;
            btnHairColourOutline2.y = returnYfromPrevious(btnHairColourOutline);
            btnHairColour1 = new Rect(btnHairColourOutlinePremade.x + 5, btnHairColourOutlinePremade.y + 5, smallButtonOptionWidthHair, btnHairColourOutlinePremade.height - 10);
            btnHairColour2 = new Rect(btnHairColour1.x + 5 + smallButtonOptionWidthHair, btnHairColourOutlinePremade.y + 5, smallButtonOptionWidthHair, btnHairColourOutlinePremade.height - 10);
            btnHairColour3 = new Rect(btnHairColour2.x + 5 + smallButtonOptionWidthHair, btnHairColourOutlinePremade.y + 5, smallButtonOptionWidthHair, btnHairColourOutlinePremade.height - 10);
            btnHairColour4 = new Rect(btnHairColour3.x + 5 + smallButtonOptionWidthHair, btnHairColourOutlinePremade.y + 5, smallButtonOptionWidthHair, btnHairColourOutlinePremade.height - 10);
            btnHairColour5 = new Rect(btnHairColour4.x + 5 + smallButtonOptionWidthHair, btnHairColourOutlinePremade.y + 5, smallButtonOptionWidthHair, btnHairColourOutlinePremade.height - 10);
            btnHairColour6 = new Rect(btnHairColour5.x + 5 + smallButtonOptionWidthHair, btnHairColourOutlinePremade.y + 5, smallButtonOptionWidthHair, btnHairColourOutlinePremade.height - 10);
            btnHairColour7 = new Rect(btnHairColour6.x + 5 + smallButtonOptionWidthHair, btnHairColourOutlinePremade.y + 5, smallButtonOptionWidthHair, btnHairColourOutlinePremade.height - 10);
            btnHairColour8 = new Rect(btnHairColour7.x + 5 + smallButtonOptionWidthHair, btnHairColourOutlinePremade.y + 5, smallButtonOptionWidthHair, btnHairColourOutlinePremade.height - 10);


            //hairtype
            lblHairType = new Rect(leftOffset, returnYfromPrevious(btnHairColourOutline2), labelWidth, buttonHeight);
            btnHairTypeOutline = lblHairType;
            btnHairTypeOutline.x += returnButtonOffset(lblHairType);
            btnHairTypeOutline.width = buttonWidth * 2 + buttonOffsetFromButton;
            btnHairTypeArrowLeft = new Rect(btnHairTypeOutline.x + 2, btnHairTypeOutline.y, btnHairTypeOutline.height, btnHairTypeOutline.height);
            btnHairTypeArrowRight = new Rect(btnHairTypeOutline.x + btnHairTypeOutline.width - btnHairTypeOutline.height - 2, btnHairTypeOutline.y, btnHairTypeOutline.height, btnHairTypeOutline.height);
            btnHairTypeSelection = new Rect(btnHairTypeOutline.x + 5 + btnHairTypeArrowLeft.width, btnHairTypeOutline.y, btnHairTypeOutline.width - 2 * (btnHairTypeArrowLeft.width) - 10, btnHairTypeOutline.height);



            //timetogrow
            lblTimeToGrow = new Rect(leftOffset, returnYfromPrevious(lblHairType), labelWidth * 3, buttonHeight);

            //biomass
            lblRequireBiomass = new Rect(leftOffset, lblTimeToGrow.y + lblTimeToGrow.height, labelWidth * 3, buttonHeight);

            //Pawn
            pawnBox = new Rect(labelWidth + buttonOffsetFromText + buttonWidth * 2 + buttonOffsetFromButton + 30 + leftOffset, topOffset, pawnBoxSide, pawnBoxSide);
            pawnBoxPawn = new Rect(pawnBox.x + pawnSpacingFromEdge, pawnBox.y + pawnSpacingFromEdge, pawnBox.width - pawnSpacingFromEdge * 2, pawnBox.height - pawnSpacingFromEdge * 2);

            //Levels of Beauty
            lblLevelOfBeauty = new Rect(pawnBox.x, pawnBox.y + pawnBox.height + optionOffset, pawnBox.width, buttonHeight);
            btnLevelOfBeauty1 = new Rect(lblLevelOfBeauty.x, lblLevelOfBeauty.y + lblLevelOfBeauty.height + 2, (pawnBox.width - 9) / 3, buttonHeight);
            btnLevelOfBeauty2 = btnLevelOfBeauty1;
            btnLevelOfBeauty2.x += (pawnBox.width) / 3;
            btnLevelOfBeauty3 = btnLevelOfBeauty2;
            btnLevelOfBeauty3.x += (pawnBox.width) / 3;

            //Levels of Quality
            lblLevelOfQuality = new Rect(lblLevelOfBeauty.x, btnLevelOfBeauty1.y + btnLevelOfBeauty1.height + optionOffset / 2, pawnBox.width, buttonHeight);
            btnLevelOfQuality1 = new Rect(lblLevelOfQuality.x, lblLevelOfQuality.y + lblLevelOfQuality.height + 2, (pawnBox.width - 9) / 3, buttonHeight);
            btnLevelOfQuality2 = btnLevelOfQuality1;
            btnLevelOfQuality2.x += (pawnBox.width) / 3;
            btnLevelOfQuality3 = btnLevelOfQuality2;
            btnLevelOfQuality3.x += (pawnBox.width) / 3;

            //Accept/Cancel buttons
            btnAccept = new Rect(InitialSize.x * .5f - buttonWidth / 2 - buttonOffsetFromButton / 2 - buttonWidth / 2, InitialSize.y - buttonHeight - 38, buttonWidth, buttonHeight);
            btnCancel = new Rect(InitialSize.x * .5f + buttonWidth / 2 + buttonOffsetFromButton / 2 - buttonWidth / 2, InitialSize.y - buttonHeight - 38, buttonWidth, buttonHeight);

            //Create textures
            updateColors();

        }

        public Color rgbConvert(float r, float g, float b)
        {
            return new Color(((1f / 255f) * r), ((1f / 255f) * g), ((1f / 255f) * b));
        }
        public void updateColors()
        {
            //Create textures
            texColor = new Texture2D(Convert.ToInt32(buttonWidth * 2 + buttonOffsetFromButton), Convert.ToInt32(buttonHeight));
            float red = -1;
            float blue = -1;
            float green = -1;
            float value = -1;
            for (float x = 0; x < texColor.width; x++)
            {

                //red 255 to 0 1/4   0 to 255 3/4 to 4/4
                //green  0 to 255 1/4   255 to 0 1/4 to 2/4
                //blue   0 to 255 2/4  to 3/4   255 to 0 3/4 4/4
                float quarter = (buttonWidth * 2 + buttonOffsetFromButton) / 3;
                if (x < quarter)
                {
                    red = 1 - (1 / quarter) * x; //go from 255 to 0
                    green = (1 / quarter) * x; //builds up from 0 to 255
                    blue = 0;

                }
                if (x >= quarter && x <= quarter * 2)
                {
                    red = 0;
                    green = 1 - (1 / quarter) * (x - quarter); //go from 255 to 0
                    blue = (1 / quarter) * (x - quarter); //builds up from 0 to 255
                }
                if (x > quarter * 2 && x < quarter * 3)
                {
                    red = (1 / quarter) * (x - quarter * 2); //builds up from 0 to 255
                    green = 0;
                    blue = 1 - (1 / quarter) * (x - quarter * 2); //go from 255 to 0
                }
                //float value = (1 / (buttonWidth * 2 + buttonOffsetFromButton)) * x;
                for (float y = 0; y < texColor.height; y++)
                {
                    value = 1;// (1 / (buttonHeight)) * y;
                    if (x < quarter)
                    {
                        if (y > texColor.height / 2)
                        {
                            blue = (1 / (buttonHeight / 2)) * (y - buttonHeight / 2);
                        }
                        else
                        {
                            blue = 0;
                        }


                    }
                    if (x >= quarter && x <= quarter * 2)
                    {
                        if (y > texColor.height / 2)
                        {
                            red = (1 / (buttonHeight / 2)) * (y - buttonHeight / 2);
                        }
                        else
                        {
                            red = 0;
                        }
                    }
                    if (x > quarter * 2 && x < quarter * 3)
                    {
                        if (y > texColor.height / 2)
                        {
                            green = (1 / (buttonHeight / 2)) * (y - buttonHeight / 2);
                        }
                        else
                        {
                            green = 0;
                        }
                    }
                    texColor.SetPixel(Convert.ToInt32(x), Convert.ToInt32(y), new Color(red * value, green * value, blue * value));
                }
                //Log.Message(value.ToString());
            }
            texColor.Apply(false);

            //
            texDarkness = new Texture2D(Convert.ToInt32(buttonWidth * 2 + buttonOffsetFromButton), Convert.ToInt32(buttonHeight));
            float valueMod = 0;
            for (float x = 0; x < texDarkness.width; x++)
            {
                if (x > texDarkness.width / 2)
                {
                    value = 1 - (1 / (buttonWidth + buttonOffsetFromButton / 2)) * (x - texDarkness.width / 2);
                }
                else
                {
                    value = 1;
                    valueMod = 1 - (1 / (buttonWidth + buttonOffsetFromButton / 2)) * x;

                }
                for (float y = 0; y < texDarkness.height; y++)
                {
                    texDarkness.SetPixel(Convert.ToInt32(x), Convert.ToInt32(y), new Color((valueMod + selectedColor.r) * value, (valueMod + selectedColor.g) * value, (valueMod + selectedColor.b) * value));
                }
                //Log.Message(value.ToString());
            }
            texDarkness.Apply(false);
        }

        public void setHair(Color color)
        {
            selectedColor = color;
            newSleeve.story.hairColor = selectedColor;
            refreshAndroidPortrait = true;
            updateColors();
        }

        public void UpdateSleeveGraphic()
        {
            newSleeve.Drawer.renderer.graphics.ResolveAllGraphics();
            PortraitsCache.SetDirty(newSleeve);
            PortraitsCache.PortraitsCacheUpdate();
        }

        private static readonly string[] HeadsFolderPaths = new string[2]
        {
            "Things/Pawn/Humanlike/Heads/Male",
            "Things/Pawn/Humanlike/Heads/Female"
        };

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
                //GUI.DrawTexture(pawnRenderRect, PortraitsCache.Get(newSleeve, PawnPortraitSize, default(Vector3), 1f));

                Text.Font = GameFont.Medium;

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(0f, 0f, inRect.width, 32f), "AlteredCarbon.SleeveCustomization".Translate());

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;



                //Saakra's Code



                //Gender
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lblGender, "Gender".Translate() + ":");
                if (Widgets.ButtonText(btnGenderMale, "Male".Translate().CapitalizeFirst()))
                {
                    newSleeve = GetNewPawn(Gender.Male);
                }
                if (Widgets.ButtonText(btnGenderFemale, "Female".Translate().CapitalizeFirst()))
                {
                    newSleeve = GetNewPawn(Gender.Female);
                }

                //Skin Colour
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lblSkinColour, "SkinColour".Translate().CapitalizeFirst() + ":");
                Widgets.DrawMenuSection(btnSkinColourOutline);
                Widgets.DrawShadowAround(btnSkinColourOutline);
                if (Widgets.ButtonInvisible(btnSkinColour1))
                {
                    newSleeve.story.melanin = 0.1f;
                    UpdateSleeveGraphic();
                }
                Widgets.DrawBoxSolid(btnSkinColour1, rgbConvert(242, 237, 224));

                if (Widgets.ButtonInvisible(btnSkinColour2))
                {
                    newSleeve.story.melanin = 0.3f;
                    UpdateSleeveGraphic();
                }
                Widgets.DrawBoxSolid(btnSkinColour2, rgbConvert(255, 239, 213));

                if (Widgets.ButtonInvisible(btnSkinColour3))
                {
                    newSleeve.story.melanin = 0.5f;
                    UpdateSleeveGraphic();
                }
                Widgets.DrawBoxSolid(btnSkinColour3, rgbConvert(255, 239, 189));

                if (Widgets.ButtonInvisible(btnSkinColour4))
                {
                    newSleeve.story.melanin = 0.7f;
                    UpdateSleeveGraphic();
                }
                Widgets.DrawBoxSolid(btnSkinColour4, rgbConvert(228, 158, 90));

                if (Widgets.ButtonInvisible(btnSkinColour5))
                {
                    newSleeve.story.melanin = 0.9f;
                    UpdateSleeveGraphic();
                }
                Widgets.DrawBoxSolid(btnSkinColour5, rgbConvert(130, 91, 48));

                //Head Shape
                Widgets.Label(lblHeadShape, "HeadShape".Translate().CapitalizeFirst() + ":");
                Widgets.DrawHighlight(btnHeadShapeOutline);
                if (ButtonTextSubtleCentered(btnHeadShapeArrowLeft, "<"))
                {
                    if (newSleeve.gender == Gender.Male)
                    {
                        if (maleHeadTypeIndex == 0)
                        {
                            maleHeadTypeIndex = GraphicDatabaseHeadRecords.maleHeads.Count - 1;
                        }
                        else
                        {
                            maleHeadTypeIndex--;
                        }

                        typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                            GraphicDatabaseHeadRecords.maleHeads.ElementAt(maleHeadTypeIndex).graphicPath);
                    }
                    else if (newSleeve.gender == Gender.Female)
                    {
                        if (femaleHeadTypeIndex == 0)
                        {
                            femaleHeadTypeIndex = GraphicDatabaseHeadRecords.femaleHeads.Count - 1;
                        }
                        else
                        {
                            femaleHeadTypeIndex--;
                        }

                        typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                            GraphicDatabaseHeadRecords.femaleHeads.ElementAt(femaleHeadTypeIndex).graphicPath);
                    }

                    UpdateSleeveGraphic();
                }
                if (ButtonTextSubtleCentered(btnHeadShapeSelection, "HeadTypeReplace".Translate()))
                {
                    if (newSleeve.gender == Gender.Male)
                    {
                        FloatMenuUtility.MakeMenu<GraphicDatabaseHeadRecords.HeadGraphicRecord>(GraphicDatabaseHeadRecords.maleHeads, head => head.graphicPath, 
                            (GraphicDatabaseHeadRecords.HeadGraphicRecord head) => delegate
                        {
                            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                            head.graphicPath);
                            UpdateSleeveGraphic();
                        });
                    }
                    else if (newSleeve.gender == Gender.Female)
                        {
                            FloatMenuUtility.MakeMenu<GraphicDatabaseHeadRecords.HeadGraphicRecord>(GraphicDatabaseHeadRecords.femaleHeads, head => head.graphicPath,
                                (GraphicDatabaseHeadRecords.HeadGraphicRecord head) => delegate
                                {
                                    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                                head.graphicPath);
                                    UpdateSleeveGraphic();
                                });
                        }

                }
                if (ButtonTextSubtleCentered(btnHeadShapeArrowRight, ">"))
                {
                    if (newSleeve.gender == Gender.Male)
                    {
                        if (maleHeadTypeIndex == GraphicDatabaseHeadRecords.maleHeads.Count - 1)
                        {
                            maleHeadTypeIndex = 0;
                        }
                        else
                        {
                            maleHeadTypeIndex++;
                        }
                        typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                            GraphicDatabaseHeadRecords.maleHeads.ElementAt(maleHeadTypeIndex).graphicPath);
                    }
                    else if (newSleeve.gender == Gender.Female)
                    {
                        if (femaleHeadTypeIndex == GraphicDatabaseHeadRecords.femaleHeads.Count - 1)
                        {
                            femaleHeadTypeIndex = 0;
                        }
                        else
                        {
                            femaleHeadTypeIndex++;
                        }
                        typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                            GraphicDatabaseHeadRecords.femaleHeads.ElementAt(femaleHeadTypeIndex).graphicPath);
                    }
                    UpdateSleeveGraphic();
                }


                //Body Shape
                Widgets.Label(lblBodyShape, "BodyShape".Translate().CapitalizeFirst() + ":");
                Widgets.DrawHighlight(btnBodyShapeOutline);
                if (ButtonTextSubtleCentered(btnBodyShapeArrowLeft, "<"))
                {
                    if (newSleeve.gender == Gender.Male)
                    {
                        if (maleBodyTypeIndex == 0)
                        {
                            maleBodyTypeIndex = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Female).Count() - 1;
                        }
                        else
                        {
                            maleBodyTypeIndex--;
                        }
                        newSleeve.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Female).ElementAt(maleBodyTypeIndex);
                    }
                    else if (newSleeve.gender == Gender.Female)
                    {
                        if (femaleBodyTypeIndex == 0)
                        {
                            femaleBodyTypeIndex = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Male).Count() - 1;
                        }
                        else
                        {
                            femaleBodyTypeIndex--;
                        }
                        newSleeve.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Male).ElementAt(femaleBodyTypeIndex);
                    }

                    UpdateSleeveGraphic();
                }
                if (ButtonTextSubtleCentered(btnBodyShapeSelection, "BodyTypeReplace".Translate()))
                {
                    if (newSleeve.gender == Gender.Male)
                    {
                        IEnumerable<BodyTypeDef> bodyTypes = from bodyType in DefDatabase<BodyTypeDef>
                            .AllDefs.Where(x => x != BodyTypeDefOf.Female) select bodyType;
                        FloatMenuUtility.MakeMenu<BodyTypeDef>(bodyTypes, bodyType => bodyType.LabelCap, (BodyTypeDef bodyType) => delegate
                        {
                            newSleeve.story.bodyType = bodyType;
                        });
                    }
                    else if (newSleeve.gender == Gender.Female)
                    {
                        IEnumerable<BodyTypeDef> bodyTypes = from bodyType in DefDatabase<BodyTypeDef>
                            .AllDefs.Where(x => x != BodyTypeDefOf.Male)
                                                             select bodyType;
                        FloatMenuUtility.MakeMenu<BodyTypeDef>(bodyTypes, bodyType => bodyType.LabelCap, (BodyTypeDef bodyType) => delegate
                        {
                            newSleeve.story.bodyType = bodyType;
                        });
                    }
                    UpdateSleeveGraphic();

                }
                if (ButtonTextSubtleCentered(btnBodyShapeArrowRight, ">"))
                {
                    if (newSleeve.gender == Gender.Male)
                    {
                        if (maleBodyTypeIndex == DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Female).Count() - 1)
                        {
                            maleBodyTypeIndex = 0;
                        }
                        else
                        {
                            maleBodyTypeIndex++;
                        }
                        newSleeve.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Female).ElementAt(maleBodyTypeIndex);
                    }
                    else if (newSleeve.gender == Gender.Female)
                    {
                        if (femaleBodyTypeIndex == DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Male).Count() - 1)
                        {
                            femaleBodyTypeIndex = 0;
                        }
                        else
                        {
                            femaleBodyTypeIndex++;
                        }
                        newSleeve.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Male).ElementAt(femaleBodyTypeIndex);
                    }
                    UpdateSleeveGraphic();
                }


                //Hair Colour
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lblHairColour, "HairColour".Translate().CapitalizeFirst() + ":");

                Widgets.DrawMenuSection(btnHairColourOutlinePremade);
                Widgets.DrawShadowAround(btnHairColourOutlinePremade);


                Widgets.DrawBoxSolid(btnHairColour1, rgbConvert(51, 51, 51));
                Widgets.DrawBoxSolid(btnHairColour2, rgbConvert(79, 71, 66));
                Widgets.DrawBoxSolid(btnHairColour3, rgbConvert(64, 51, 38));
                Widgets.DrawBoxSolid(btnHairColour4, rgbConvert(77, 51, 26));
                Widgets.DrawBoxSolid(btnHairColour5, rgbConvert(90, 58, 32));
                Widgets.DrawBoxSolid(btnHairColour6, rgbConvert(132, 83, 47));
                Widgets.DrawBoxSolid(btnHairColour7, rgbConvert(193, 146, 85));
                Widgets.DrawBoxSolid(btnHairColour8, rgbConvert(237, 202, 156));



                if (Widgets.ButtonInvisible(btnHairColour1))
                {
                    setHair(rgbConvert(51, 51, 51));
                }
                else
                if (Widgets.ButtonInvisible(btnHairColour2))
                {
                    setHair(rgbConvert(79, 71, 66));
                }
                else
                if (Widgets.ButtonInvisible(btnHairColour3))
                {
                    setHair(rgbConvert(64, 51, 38));
                }
                else
                if (Widgets.ButtonInvisible(btnHairColour4))
                {
                    setHair(rgbConvert(77, 51, 26));
                }
                else
                if (Widgets.ButtonInvisible(btnHairColour5))
                {
                    setHair(rgbConvert(90, 58, 32));
                }
                else
                if (Widgets.ButtonInvisible(btnHairColour6))
                {
                    setHair(rgbConvert(132, 83, 47));
                }
                else
                if (Widgets.ButtonInvisible(btnHairColour7))
                {
                    setHair(rgbConvert(193, 146, 85));
                }
                else
                if (Widgets.ButtonInvisible(btnHairColour8))
                {
                    setHair(rgbConvert(237, 202, 156));
                }

                Widgets.DrawMenuSection(btnHairColourOutline);
                Widgets.DrawTextureFitted(btnHairColourOutline, texColor, 1);
                Widgets.DrawMenuSection(btnHairColourOutline2);
                Widgets.DrawTextureFitted(btnHairColourOutline2, texDarkness, 1);

                //if click in texColour box
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(btnHairColourOutline))
                {
                    Vector2 mPos = Event.current.mousePosition;
                    float x = mPos.x - btnHairColourOutline.x;
                    float y = mPos.y - btnHairColourOutline.y;
                    //Log.Message(x.ToString());
                    //Log.Message(y.ToString());

                    setHair(texColor.GetPixel(Convert.ToInt32(x), Convert.ToInt32(btnHairColourOutline.height - y)));
                    Event.current.Use();
                }

                //if click in Darkness box
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(btnHairColourOutline2))
                {
                    Vector2 mPos = Event.current.mousePosition;
                    float x = mPos.x - btnHairColourOutline2.x;
                    float y = mPos.y - btnHairColourOutline2.y;
                    //Log.Message(x.ToString());
                    //Log.Message(y.ToString());

                    selectedColorFinal = texDarkness.GetPixel(Convert.ToInt32(x), Convert.ToInt32(btnHairColourOutline2.height - y));
                    updateColors();
                    newSleeve.story.hairColor = selectedColorFinal;
                    refreshAndroidPortrait = true;
                    Event.current.Use();
                }

                //Hair Type
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lblHairType, "HairType".Translate().CapitalizeFirst() + ":");
                Widgets.DrawHighlight(btnHairTypeOutline);
                if (ButtonTextSubtleCentered(btnHairTypeArrowLeft, "<"))
                {
                    if (hairTypeIndex == 0)
                    {
                        hairTypeIndex = DefDatabase<HairDef>.AllDefs.Count() - 1;
                    }
                    else
                    {
                        hairTypeIndex--;
                    }
                    newSleeve.story.hairDef = DefDatabase<HairDef>.AllDefs.ElementAt(hairTypeIndex);
                    UpdateSleeveGraphic();
                }
                if (ButtonTextSubtleCentered(btnHairTypeSelection, newSleeve.story.hairDef.LabelCap))
                {
                    IEnumerable<HairDef> hairs =
                        from hairdef in DefDatabase<HairDef>.AllDefs select hairdef;
                    FloatMenuUtility.MakeMenu<HairDef>(hairs, hairDef => hairDef.LabelCap, (HairDef hairDef) => delegate
                    {
                        newSleeve.story.hairDef = hairDef;
                        UpdateSleeveGraphic();
                    });
                }
                if (ButtonTextSubtleCentered(btnHairTypeArrowRight, ">"))
                {
                    if (hairTypeIndex == DefDatabase<HairDef>.AllDefs.Count() - 1)
                    {
                        hairTypeIndex = 0;
                    }
                    else
                    {
                        hairTypeIndex++;
                    }
                    newSleeve.story.hairDef = DefDatabase<HairDef>.AllDefs.ElementAt(hairTypeIndex);
                    UpdateSleeveGraphic();
                }
                
                //Time to Grow
                Widgets.Label(lblTimeToGrow, "TimeToGrow".Translate().CapitalizeFirst() + ": " + GenDate.ToStringTicksToDays(baseTicksToGrow + baseTicksToGrow2 + baseTicksToGrow3));//PUT TIME TO GROW INFO HERE

                //Require Biomass
                Widgets.Label(lblRequireBiomass, "RequireBiomass".Translate().CapitalizeFirst() + ": " + (baseMeatCost + baseMeatCost2 + baseMeatCost3));//PUT REQUIRED BIOMASS HERE

                //Vertical Divider
                //Widgets.DrawLineVertical((pawnBox.x + (btnGenderFemale.x + btnGenderFemale.width)) / 2, pawnBox.y, InitialSize.y - pawnBox.y - (buttonHeight + 53));

                //Pawn Box
                Widgets.DrawMenuSection(pawnBox);
                Widgets.DrawShadowAround(pawnBox);
                GUI.DrawTexture(pawnBoxPawn, PortraitsCache.Get(newSleeve, pawnBoxPawn.size, default(Vector3), 1f));

                //Levels of Beauty
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(lblLevelOfBeauty, "LevelOfBeauty".Translate().CapitalizeFirst() + ":");
                if (Widgets.ButtonText(btnLevelOfBeauty1, "1"))
                {
                    var trait = new Trait(TraitDefOf.Beauty, -2);
                    newSleeve.story.traits.GainTrait(trait);
                    baseTicksToGrow2 = -420000;
                    baseMeatCost2 = -50;
                }
                if (Widgets.ButtonText(btnLevelOfBeauty2, "2"))
                {

                }
                if (Widgets.ButtonText(btnLevelOfBeauty3, "3"))
                {
                    var trait = new Trait(TraitDefOf.Beauty, 2);
                    newSleeve.story.traits.GainTrait(trait);
                    baseTicksToGrow2 = 420000;
                    baseMeatCost2 = 50;
                }

                //Levels of Quality

                Widgets.Label(lblLevelOfQuality, "LevelofQuality".Translate().CapitalizeFirst() + ":");
                if (Widgets.ButtonText(btnLevelOfQuality1, "1"))
                {
                    baseTicksToGrow3 = -420000;
                    baseMeatCost3 = -50;
                }
                if (Widgets.ButtonText(btnLevelOfQuality2, "2"))
                {

                }
                if (Widgets.ButtonText(btnLevelOfQuality3, "3"))
                {
                    baseTicksToGrow3 = 420000;
                    baseMeatCost3 = 50;
                }

                if (Widgets.ButtonText(btnAccept, "Accept".Translate().CapitalizeFirst()))
                {
                    sleeveGrower.StartGrowth(newSleeve, baseTicksToGrow + baseTicksToGrow2 + baseTicksToGrow3, baseMeatCost + baseMeatCost2 + baseMeatCost3);
                    this.Close();
                }
                if (Widgets.ButtonText(btnCancel, "Cancel".Translate().CapitalizeFirst()))
                {
                    this.Close();
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public Pawn GetNewPawn(Gender gender = Gender.Female)
        {
            maleBodyTypeIndex = 0;
            femaleBodyTypeIndex = 0;
            hairTypeIndex = 0;
            femaleHeadTypeIndex = 0;
            maleHeadTypeIndex = 0;
            
            //Make base pawn.
            Pawn pawn;

            pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, Faction.OfAncients, PawnGenerationContext.NonPlayer,
            -1, true, false, false, false, false, false, 0f, false, true, true, false, false, false, true, fixedGender: gender, fixedBiologicalAge: 20, fixedChronologicalAge: 20));

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
