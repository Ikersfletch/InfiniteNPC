using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.GameInput;
using static Terraria.ModLoader.PlayerDrawLayer;
using InfiniteNPC.NPCs;
using Terraria.ModLoader.IO;
using System.IO;
using ReLogic;
using Terraria.ID;
using Terraria.Audio;
using InfiniteNPC.Items;
using Humanizer;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.Initializers;
using Steamworks;
using Microsoft.Xna.Framework.Input;
using System.Drawing.Text;
using ReLogic.Graphics;

namespace InfiniteNPC.UI
{
    public class NPCSummonUI : ModSystem
    {

        public static List<Tuple<int, int, SummonedNPCData>> EquipStack = new List<Tuple<int, int, SummonedNPCData>>();

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (Main.dedServ) return;
            int layer = layers.FindIndex(i => i.Name == "Vanilla: Inventory");
            NPCEquipper npcSummoning = new NPCEquipper();
            layers.Insert(layer, npcSummoning);
        }
        public override void NetReceive(BinaryReader reader)
        {
            int listSize = reader.ReadInt32();

            for (int i = 0; i < listSize; i ++)
            {
                int netID = reader.ReadInt32();
                int whoAmI = reader.ReadInt32();
                TagCompound p = TagIO.Read(reader);
                SummonedNPCData newData = new SummonedNPCData();
                newData.LoadData(ref p);

                if (Main.npc[whoAmI].active && Main.npc[whoAmI].netID == netID && Main.npc[whoAmI].type == ModContent.NPCType<SummonedNPC>())
                {
                    ((SummonedNPC)Main.npc[whoAmI].ModNPC).myData = newData;
                    ((SummonedNPC)Main.npc[whoAmI].ModNPC).clearWeaponUse = true;
                }
            }
            

        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(EquipStack.Count);

            EquipStack.ForEach(
                i => {
                    writer.Write(i.Item1);
                    writer.Write(i.Item2);
                    TagCompound p = new TagCompound();
                    i.Item3.SaveData(ref p);
                    TagIO.Write(p, writer);
                }
            );

            EquipStack.Clear();

        }
        public static void PushNPCUpdate(SummonedNPC equipschanged)
        {
            Tuple<int, int, SummonedNPCData> tuple = new Tuple<int,int, SummonedNPCData>(equipschanged.NPC.netID, equipschanged.NPC.whoAmI, equipschanged.myData);
            List<Tuple<int, int, SummonedNPCData>> replaced = EquipStack.Where(i => i.Item1 != tuple.Item1 && i.Item2 != tuple.Item2).ToList();
            replaced.Add(tuple);
            EquipStack = replaced;
        }
    
    }

    public class NPCEquipper : GameInterfaceLayer
    {
        public static int PositionX(float gridPosition) => (int)(73f + gridPosition * 56f * Main.inventoryScale);
        public static int PositionY(float gridPosition) => (int)((float)Main.instance.invBottom + (gridPosition) * 56f * Main.inventoryScale);

        public static bool SummoningInterfaceOpen = true;
        public static int targetNPC = -1;
        public static Color clipboard = Color.White;
        public static Vector3 editingColorHSL = Vector3.One;
        public static SummonedNPCData data;
        public static bool colorMenuOpen = false;
        public static int colorMenuTab = 0;
        public static bool colorMenuSetup = false;
        public static UIPanel menuBGElement = new UIPanel();
        public static bool hairStyleMenuOpen = false;
        public NPCEquipper(string name = "InfiniteNPC: NPC Equipping Interface", InterfaceScaleType scaling = InterfaceScaleType.UI) : base(name, scaling)
        {
        }

        public static bool IsSummonedNPC(int npc) => Main.npc[npc].active && Main.npc[npc].type == ModContent.NPCType<SummonedNPC>();
        public static SummonedNPC NPCBase(int npc) => IsSummonedNPC(npc) ? (Main.npc[npc].ModNPC as SummonedNPC) : null;
        public static SummonedNPCData NPCData(int npc) => IsSummonedNPC(npc) ? (Main.npc[npc].ModNPC as SummonedNPC).myData : null;

        public static Dictionary<string, string> wardrobeTexturePaths = new Dictionary<string, string>() {
            // sex
            {"FemaleIcon", "UI/ClothStyleFemale" },
            {"MaleIcon", "UI/ClothStyleMale" },

            // wardrobe
            {"StyleSwap", "UI/ClothStyleSwap" },
            {"HairStyleColored", "UI/HairStyle_Hair" },
            {"HairStyleUncolored", "UI/HairStyle_Arrow" },
            {"HairColored", "UI/ColorHair" },
            {"EyeColored", "UI/ColorEye" },
            {"EyeUncolored", "UI/ColorEyeBack" },
            {"PantsColored", "UI/ColorPants" },
            {"ShirtColored", "UI/ColorShirt" },
            {"UndershirtColored", "UI/ColorUndershirt" },
            {"ShoesColored", "UI/ColorShoes" },
            {"SkinColored", "UI/ColorSkin" }, // crayola vibes rn

            // misc
            {"Copy", "UI/Copy"},
            {"Paste", "UI/Paste"},
            {"Randomize", "UI/Randomize"},
            {"PagePrev", "UI/PageLeft"},
            {"PageNext", "UI/PageRight"}
        };
        public static Dictionary<string, string> wardrobeNames = new Dictionary<string, string>() {
            // sex
            {"FemaleIcon", "Gender" },
            {"MaleIcon", "Gender" },

            // wardrobe
            {"StyleSwap", "Clothes" },
            {"HairStyleColored", "Hairstyle" },
            //{"HairStyleUncolored", "UI/HairStyle_Arrow" },
            {"HairColored", "Hair" },
            {"EyeColored", "Eyes" },
            //{"EyeUncolored", "UI/ColorEyeBack" },
            {"PantsColored", "Pants" },
            {"ShirtColored", "Shirt" },
            {"UndershirtColored", "Undershirt" },
            {"ShoesColored", "Shoes" },
            {"SkinColored", "Skin" }, // crayola vibes rn

            // misc
            {"Copy", "Copy Color"},
            {"Paste", "Paste Color"},
            {"Randomize", "Randomize Color"},
        };

        protected override bool DrawSelf()
        {
            if (!Main.playerInventory) CloseNPCEquipper();
            if (!SummoningInterfaceOpen || targetNPC == -1) return true;


            SummonedNPC npc = NPCBase(targetNPC);
            
            if (npc == null) { CloseNPCEquipper(); return true; }

            data = (SummonedNPCData)npc.myData.Clone();

            Main.hidePlayerCraftingMenu = true;

            float realScale = Main.inventoryScale;

            Main.inventoryScale *= 1.25f;

            Item[] weaponInv = new Item[] { data.weapon };

            Item[] armor = new Item[30];
            for (int i = 0; i < 10; i++)
            {

                if (data.armor[i] == null) data.armor[i] = new Item();
                if (data.dyes[i] == null) data.dyes[i] = new Item();

                armor[i + 10] = data.armor[i];
                armor[i + 20] = data.dyes[i];
            }
            DrawColorMenu(data, Main.spriteBatch, 0f, 6.5f);

            ItemSlotWrapped(Main.spriteBatch, Main.LocalPlayer, ref weaponInv, PositionX(0.0f), PositionY(1.5f), ItemSlot.Context.BankItem, 0);

            DrawArmorSlots(Main.LocalPlayer, Main.spriteBatch, 1f, 1.5f, ref armor);

            DrawWardrobeInterface(data, Main.spriteBatch, 1f, 3.5f);


            data.weapon = weaponInv[0];

            for (int i = 10; i < 20; i++)
                data.armor[i - 10] = armor[i];

            for (int i = 20; i < 30; i++)
                data.dyes[i - 20] = armor[i];


            if (data != npc.myData)
            {
                npc.myData = data;
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    NPCSummonUI.PushNPCUpdate(npc);
                }
                npc.clearWeaponUse = true;
            }
            npc.dummyPlayer.socialIgnoreLight = true;
            npc.dummyPlayer.direction = 1;
            Vector2 oPosition = npc.dummyPlayer.position;
            npc.dummyPlayer.position = new Vector2(PositionX(0.25f), PositionY(3f))+ Main.screenPosition;
            Main.spriteBatch.End();
            Main.PlayerRenderer.DrawPlayers(Main.Camera, new Player[] { npc.dummyPlayer });
            Main.spriteBatch.Begin();
            npc.dummyPlayer.position = oPosition;
            npc.SetDummyAppearance();


            Main.inventoryScale = realScale;

            return true;
        }

        public static void CopyColor()
        {
            clipboard = data.wardrobe[colorMenuTab];
        }

        public static void PasteColor()
        {
            data.wardrobe[colorMenuTab] = clipboard;
        }

        public static Vector3 GetRandomColorVector() => new Vector3(Main.rand.NextFloat(), Main.rand.NextFloat(), Main.rand.NextFloat());
        public static void RandomizeColor()
        {
            data.wardrobe[colorMenuTab] = ScaledHslToRgb(GetRandomColorVector());
        }

        public static void OpenNPCEquipper(int forNPC)
        {
            SummoningInterfaceOpen = true;
            Main.playerInventory = true;
            targetNPC = forNPC;
            colorMenuOpen = false;
            hairStyleMenuOpen = false;
            menuBGElement.RemoveAllChildren();
        }


        public static void CloseNPCEquipper()
        {
            SummoningInterfaceOpen = false;
            colorMenuOpen = false;
            hairStyleMenuOpen = false;
            menuBGElement.RemoveAllChildren();
        }
        public void SetHairStyle(int hairId)
        {
            data.hairstyle = hairId;
        } 

        public void DrawArmorSlots(Player player, SpriteBatch spriteBatch, float offsetX, float offsetY, ref Item[] inv)
        {
            for (int i = 10; i < 20; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int slot = i + 10 * j;
                    int context = (j == 1) ? ItemSlot.Context.EquipDye : ((i < 13) ? ItemSlot.Context.EquipArmorVanity : ItemSlot.Context.EquipAccessoryVanity);
                    ItemSlotWrapped(spriteBatch, player, ref inv, PositionX((float)i - 10 + offsetX), PositionY((float)j + offsetY), context, slot);
                }
            }
        }
        public void ItemSlotWrapped(SpriteBatch spriteBatch, Player player, ref Item[] inv, int positionX, int positionY, int context, int slot)
        {
            if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, positionX, positionY, (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale, (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale) && !PlayerInput.IgnoreMouseInterface)
            {
                player.mouseInterface = true;
                ItemSlot.Handle(inv, context, slot);
            }

            ItemSlot.Draw(spriteBatch, inv, context, slot, new Vector2(positionX, positionY));
        }
        public void DrawWardrobeInterface(SummonedNPCData data, SpriteBatch spriteBatch, float offsetX, float offsetY)
        {
            DrawAtOffset(out bool hoverStyle, offsetX, offsetY, "StyleSwap");

            if (hoverStyle && Main.mouseLeft && Main.mouseLeftRelease)
            {
                data.CycleSkin();
                SoundEngine.PlaySound(SoundID.MenuTick);
            }

            offsetX += 1f;

            DrawAtOffset(out bool hoverSex, offsetX, offsetY, data.Female() ? "FemaleIcon" : "MaleIcon");

            if (hoverSex && Main.mouseLeft && Main.mouseLeftRelease)
            {
                data.SwapSex();
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            offsetX += 1f;

            DrawAtOffset(out bool hoverHairStyle, i => hairStyleMenuOpen, offsetX, offsetY, "HairStyleUncolored");
            DrawAtOffset(out _, false, offsetX, offsetY, "HairStyleColored", data.wardrobe[0]);

            if (hoverHairStyle && Main.mouseLeft && Main.mouseLeftRelease)
            {
                ToggleHairStyleMenu();
            }

            offsetX += 1f;

            DrawAtOffset(out bool hoverHairColor, i => i == 0 && colorMenuOpen, offsetX, offsetY, "HairColored", data.wardrobe[0]);

            if (hoverHairColor && Main.mouseLeft && Main.mouseLeftRelease) ToggleColorMenu(0);

            offsetX += 1f;

            DrawAtOffset(out bool hoverEye, i => i == 1 && colorMenuOpen, offsetX, offsetY, "EyeUncolored");
            DrawAtOffset(out bool _, false, offsetX, offsetY, "EyeColored", data.wardrobe[1]);

            if (hoverEye && Main.mouseLeft && Main.mouseLeftRelease) ToggleColorMenu(1);

            offsetX += 1f;

            DrawAtOffset(out bool hoverSkin, i => i == 2 && colorMenuOpen, offsetX, offsetY, "SkinColored", data.wardrobe[2]);

            if (hoverSkin && Main.mouseLeft && Main.mouseLeftRelease) ToggleColorMenu(2);
            offsetX += 1f;

            DrawAtOffset(out bool hoverUndershirt, i => i == 3 && colorMenuOpen, offsetX, offsetY, "UndershirtColored", data.wardrobe[3]);

            if (hoverUndershirt && Main.mouseLeft && Main.mouseLeftRelease) ToggleColorMenu(3);
            offsetX += 1f;

            DrawAtOffset(out bool hoverShirt, i => i == 4 && colorMenuOpen, offsetX, offsetY, "ShirtColored", data.wardrobe[4]);

            if (hoverShirt && Main.mouseLeft && Main.mouseLeftRelease) ToggleColorMenu(4);
            offsetX += 1f;

            DrawAtOffset(out bool hoverPants, i => i == 5 && colorMenuOpen, offsetX, offsetY, "PantsColored", data.wardrobe[5]);

            if (hoverPants && Main.mouseLeft && Main.mouseLeftRelease) ToggleColorMenu(5);
            offsetX += 1f;

            DrawAtOffset(out bool hoverShoes, i => i == 6 && colorMenuOpen, offsetX, offsetY, "ShoesColored", data.wardrobe[6]);

            if (hoverShoes && Main.mouseLeft && Main.mouseLeftRelease) ToggleColorMenu(6);
        }
        public void ToggleColorMenu(int toTab)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            colorMenuOpen = (colorMenuTab == toTab) ? !colorMenuOpen : true;
            colorMenuTab = toTab;
            hairStyleMenuOpen = false;
            menuBGElement.RemoveAllChildren();
            if (colorMenuOpen)
            {
                colorMenuSetup = true;
                menuBGElement.Append(CreateHSLSlider(HSLSliderId.Hue));
                menuBGElement.Append(CreateHSLSlider(HSLSliderId.Saturation));
                menuBGElement.Append(CreateHSLSlider(HSLSliderId.Luminance));
            }

        }

        public static int HairStylePage = 0;
        public void ToggleHairStyleMenu()
        {
            colorMenuOpen = false;
            hairStyleMenuOpen = !hairStyleMenuOpen;
            menuBGElement.RemoveAllChildren();
            SoundEngine.PlaySound(SoundID.MenuTick);
            if (hairStyleMenuOpen)
            {
                HairStylePage = 0;
            }
        }
        public static Vector3 RgbToScaledHsl(Color color)
        {
            Vector3 vector = Main.rgbToHsl(color);
            vector.Z = (vector.Z - 0.15f) / 0.85f;
            vector = Vector3.Clamp(vector, Vector3.Zero, Vector3.One);
            return vector;
        }
        public static bool renaming = false;
        public void DrawColorMenu(SummonedNPCData data, SpriteBatch spriteBatch, float offsetX, float offsetY)
        {
            bool mouseDown = Main.mouseLeft && Main.mouseLeftRelease;
            //menuBGElement.BackgroundColor = Color.Aqua;
            menuBGElement.Left = new StyleDimension(65f, 0f);
            menuBGElement.Width = new StyleDimension(484f, 0f);
            menuBGElement.Top = new StyleDimension(Main.instance.invBottom + 56f,0f);
            if (colorMenuOpen) menuBGElement.Height = new StyleDimension(250f, 0f);
            else if (hairStyleMenuOpen) menuBGElement.Height = new StyleDimension(400f, 0f);
            else menuBGElement.Height = new StyleDimension(178f, 0f);
            Main.LocalPlayer.mouseInterface = WithinRect(menuBGElement.GetClippingRectangle(Main.spriteBatch), new Point(Main.mouseX, Main.mouseY));
            menuBGElement.Recalculate();
            editingColorHSL = RgbToScaledHsl(data.wardrobe[colorMenuTab]);
            menuBGElement.Draw(spriteBatch);
            if (colorMenuOpen)
            {
                DrawButton(out bool CopyHover, "Copy Color to Clipboard", wardrobeTexturePaths["Copy"], 2f, 5.5f, 0.6f);
                DrawButton(out bool PasteHover, "Paste Color to Clipboard", wardrobeTexturePaths["Paste"], 3f, 5.5f, 0.6f);
                DrawButton(out bool RandoHover, "Randomize Color to Clipboard", wardrobeTexturePaths["Randomize"], 4f, 5.5f, 0.6f);

                if (mouseDown)
                {
                    if (CopyHover) CopyColor();
                    if (PasteHover) PasteColor();
                    if (RandoHover) RandomizeColor();
                }

            } else if (hairStyleMenuOpen)
            {
                int pageCount = (int)MathF.Ceiling((float)Main.maxHairStyles / 45f);
                DrawButton(out bool prevHover, "Prev Page", wardrobeTexturePaths["PagePrev"], 0f, 7.5f, 0.9f);
                DrawButton(out bool nextHover, "Next Page", wardrobeTexturePaths["PageNext"], 10f, 7.5f, 0.9f);

                Main.spriteBatch.DrawString(FontAssets.ItemStack.Value, $"Page {HairStylePage + 1}/{pageCount}", new Vector2(PositionX(1f), PositionY(4.75f)), Color.White);

                if (mouseDown && prevHover)
                {
                    HairStylePage--;
                }

                if (mouseDown && nextHover)
                {
                    HairStylePage++;
                }
                if (HairStylePage >= pageCount)
                {
                    HairStylePage = 0;
                }
                else if (HairStylePage < 0)
                {
                    HairStylePage = pageCount - 1;
                }


                for (int i = 0; i < 9; i ++)
                {
                    for (int j = 0; j < 5; j ++)
                    {
                        int hairStyle = 45 * HairStylePage + i + 9 * j;
                        if (hairStyle >= Main.maxHairStyles) continue;
                        Vector2 slotPos = new Vector2(PositionX(1f + (float)i), PositionY(5.5f + (float)j));
                        Rectangle boundingRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, (int)(0.95f * (PositionX(2f + (float)i) - slotPos.X)), (int)(0.9f * (PositionY(6.5f + (float)j) - slotPos.Y)));
                        bool hovering = WithinRect(boundingRect, new Point(Main.mouseX, Main.mouseY));
                        ItemSlot.Draw(
                            Main.spriteBatch, 
                            new Item[] {new Item()}, 
                            (data.hairstyle == hairStyle) ? 
                                ItemSlot.Context.ShopItem : 
                                (hovering ? ItemSlot.Context.CreativeInfinite : ItemSlot.Context.ChestItem), 
                            0, 
                            slotPos, 
                            Color.White
                        );
                        Player dummy = ((SummonedNPC)Main.npc[targetNPC].ModNPC).dummyPlayer;
                        int direction = dummy.direction;
                        dummy.hair = hairStyle;
                        dummy.direction = 1;
                        Main.PlayerRenderer.DrawPlayerHead(Main.Camera, dummy, boundingRect.Center.ToVector2(), 1f, 1f);
                        dummy.hair = data.hairstyle;
                        dummy.direction = direction;

                        if (hovering && mouseDown)
                        {
                            data.hairstyle = hairStyle;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }

                    }
                }
            } else
            {
                Main.spriteBatch.DrawString(FontAssets.ItemStack.Value, data.name + (renaming && MathF.Sin((float)Main.timeForVisualEffects * 0.1f) > 0f ? "|" : ""), new Vector2(PositionX(2.5f), PositionY(4.75f)), Color.White);
                Vector2 slotPos = new Vector2(PositionX(1f), PositionY(4.5f));
                ItemSlot.Draw(
                    Main.spriteBatch,
                    new Item[] { new Item() },
                    (renaming) ? ItemSlot.Context.ShopItem : ItemSlot.Context.BankItem,
                    0,
                    slotPos,
                    Color.White
                );
                DrawButton(out bool renameHover, "Rename NPC", "UI/Rename", 1f, 4.5f, 0.9f, 0.9f, 0.6f, 0.6f);
                if (mouseDown && renameHover)
                    renaming = !renaming;
                PlayerInput.WritingText = renaming;
                if (renaming)
                    data.name = Main.GetInputText(data.name);
            }

        }
        public void DrawButton(out bool highlighted, string mouseMessage, string texturePath, float offsetX, float offsetY, float iconPercent)
        {
            DrawButton(out bool h, mouseMessage, texturePath, offsetX, offsetY, 1f, 1f, iconPercent, iconPercent);
            highlighted = h;
        }
        public void DrawButton(out bool highlighted, string mouseMessage, string texturePath, float offsetX, float offsetY, float offsetW, float offsetH, float iconOffsetW, float iconOffsetH)
        {
            int boundingX = PositionX(offsetX);
            int boundingY = PositionY(offsetY);
            Rectangle copyBox = new Rectangle(boundingX, boundingY, PositionX(offsetX + offsetH) - boundingX, PositionY(offsetY + offsetW) - boundingY);

            float leftX = (offsetW - iconOffsetW) * 0.5f;
            float rightX = offsetW - leftX;
            float topY = (offsetH - iconOffsetH) * 0.5f;
            float bottomY = offsetH - topY;

            int iconX = PositionX(offsetX + leftX);
            int iconY = PositionY(offsetY + topY);
            Rectangle iconBox = new Rectangle(iconX, iconY, PositionX(offsetX + rightX) - iconX, PositionY(offsetY + bottomY) - iconY);

            Main.spriteBatch.Draw(InfiniteNPC.Instance.Assets.Request<Texture2D>(texturePath).Value, iconBox, Color.White);

            highlighted = (WithinRect(copyBox, new Point(Main.mouseX, Main.mouseY)));
            if (highlighted) {
                Main.instance.MouseText(mouseMessage);
                Main.LocalPlayer.mouseInterface = true;
                Main.spriteBatch.Draw(InfiniteNPC.Instance.Assets.Request<Texture2D>(texturePath + "_Highlight").Value, iconBox, Color.White);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }

        }
        public enum HSLSliderId
        {
            Hue,
            Saturation,
            Luminance
        }
        public UIColoredSlider CreateHSLSlider(HSLSliderId id)
        {
            UIColoredSlider uIColoredSlider = CreateHSLSliderButtonBase(id);
            uIColoredSlider.VAlign = 0.6f;
            uIColoredSlider.HAlign = 0f;
            uIColoredSlider.Width = StyleDimension.FromPixelsAndPercent(-10f, 1f);
            uIColoredSlider.Top.Set(30 * (int)id, 0f);
            //uIColoredSlider.OnMouseDown += Click_ColorPicker;
            uIColoredSlider.SetSnapPoint("Middle", (int)id, null, new Vector2(0f, 20f));
            return uIColoredSlider;
        }
        public UIColoredSlider CreateHSLSliderButtonBase(HSLSliderId id)
        {
            switch (id)
            {
                case HSLSliderId.Saturation:
                    return new UIColoredSlider(LocalizedText.Empty, () => GetHSLSliderPosition(HSLSliderId.Saturation), delegate (float x) {
                        UpdateHSLValue(HSLSliderId.Saturation, x);
                    }, UpdateHSL_S, (float x) => GetHSLSliderColorAt(HSLSliderId.Saturation, x), Color.Transparent);
                case HSLSliderId.Luminance:
                    return new UIColoredSlider(LocalizedText.Empty, () => GetHSLSliderPosition(HSLSliderId.Luminance), delegate (float x) {
                        UpdateHSLValue(HSLSliderId.Luminance, x);
                    }, UpdateHSL_L, (float x) => GetHSLSliderColorAt(HSLSliderId.Luminance, x), Color.Transparent);
                default:
                    return new UIColoredSlider(LocalizedText.Empty, () => GetHSLSliderPosition(HSLSliderId.Hue), delegate (float x) {
                        UpdateHSLValue(HSLSliderId.Hue, x);
                    }, UpdateHSL_H, (float x) => GetHSLSliderColorAt(HSLSliderId.Hue, x), Color.Transparent);
            }
        }

        public void UpdateHSL_H()
        {
            float value = UILinksInitializer.HandleSliderHorizontalInput(editingColorHSL.X, 0f, 1f, PlayerInput.CurrentProfile.InterfaceDeadzoneX, 0.35f);
            UpdateHSLValue(HSLSliderId.Hue, value);
        }

        public void UpdateHSL_S()
        {
            float value = UILinksInitializer.HandleSliderHorizontalInput(editingColorHSL.Y, 0f, 1f, PlayerInput.CurrentProfile.InterfaceDeadzoneX, 0.35f);
            UpdateHSLValue(HSLSliderId.Saturation, value);
        }

        public void UpdateHSL_L()
        {
            float value = UILinksInitializer.HandleSliderHorizontalInput(editingColorHSL.Z, 0f, 1f, PlayerInput.CurrentProfile.InterfaceDeadzoneX, 0.35f);
            UpdateHSLValue(HSLSliderId.Luminance, value);
        }

        public float GetHSLSliderPosition(HSLSliderId id)
        {
            switch (id)
            {
                case HSLSliderId.Hue:
                    return editingColorHSL.X;
                case HSLSliderId.Saturation:
                    return editingColorHSL.Y;
                case HSLSliderId.Luminance:
                    return editingColorHSL.Z;
                default:
                    return 1f;
            }
        }
        public static Color ScaledHslToRgb(Vector3 hsl) => ScaledHslToRgb(hsl.X, hsl.Y, hsl.Z);
        public static Color ScaledHslToRgb(float hue, float saturation, float luminosity) => Main.hslToRgb(hue, saturation, (float)((double)luminosity * 0.850000023841858 + 0.150000005960464));

        public void UpdateHSLValue(HSLSliderId id, float value)
        {
            switch (id)
            {
                case HSLSliderId.Hue:
                    editingColorHSL.X = value;
                    break;
                case HSLSliderId.Saturation:
                    editingColorHSL.Y = value;
                    break;
                case HSLSliderId.Luminance:
                    editingColorHSL.Z = value;
                    break;
            }

            Color color = ScaledHslToRgb(editingColorHSL.X, editingColorHSL.Y, editingColorHSL.Z);
            ApplyPendingColor(color);
        }

        public Color GetHSLSliderColorAt(HSLSliderId id, float pointAt)
        {
            switch (id)
            {
                case HSLSliderId.Hue:
                    return ScaledHslToRgb(pointAt, 1f, 0.5f);
                case HSLSliderId.Saturation:
                    return ScaledHslToRgb(editingColorHSL.X, pointAt, editingColorHSL.Z);
                case HSLSliderId.Luminance:
                    return ScaledHslToRgb(editingColorHSL.X, editingColorHSL.Y, pointAt);
                default:
                    return Color.White;
            }
        }

        public void ApplyPendingColor(Color pendingColor)
        {
            data.wardrobe[colorMenuTab] = pendingColor;
        }

        public bool WithinRect(Rectangle r, Point p) => (p.X >= r.Left && p.X <= r.Right && p.Y >= r.Top && p.Y <= r.Bottom);
        public void DrawAtOffset(out bool highlight, float offsetX, float offsetY, string key) { DrawAtOffset(out bool h, true, i => false, offsetX, offsetY, key, Color.White); highlight = h; }
        public void DrawAtOffset(out bool highlight, Predicate<int> thisButtonTab, float offsetX, float offsetY, string key) { DrawAtOffset(out bool h, true, thisButtonTab, offsetX, offsetY, key, Color.White); highlight = h; }
        public void DrawAtOffset(out bool highlight, Predicate<int> thisButtonTab, float offsetX, float offsetY, string key, Color tint) { DrawAtOffset(out bool h, true, thisButtonTab, offsetX, offsetY, key, tint); highlight = h; }
        public void DrawAtOffset(out bool highlight, bool drawSlot, float offsetX, float offsetY, string key, Color tint) { DrawAtOffset(out bool h, drawSlot, i => false, offsetX, offsetY, key, tint); highlight = h; }
        public void DrawAtOffset(out bool highlight, bool drawSlot, Predicate<int> thisButtonTab, float offsetX, float offsetY, string key, Color tint)
        {
            int positionX = PositionX(offsetX);
            int positionY = PositionY(offsetY);
            int drawX = PositionX(offsetX + 0.15f);
            int drawY = PositionY(offsetY + 0.15f);
            int drawW = PositionX(offsetX + 0.8f) - drawX;
            int drawH = PositionY(offsetY + 0.8f) - drawY;
            Rectangle drawBox = new Rectangle(drawX, drawY, drawW, drawH);
            if (drawSlot)
            {
                Item emptyInv = new Item();
                ItemSlot.Draw(
                    Main.spriteBatch, 
                    ref emptyInv, 
                    thisButtonTab(colorMenuTab) ? ItemSlot.Context.ShopItem : ItemSlot.Context.BankItem, 
                    new Vector2(positionX, positionY)
                );
            }

            Main.spriteBatch.Draw(
                InfiniteNPC.Instance.Assets.Request<Texture2D>(wardrobeTexturePaths[key]).Value,
                drawBox,
                tint
            );

            int width = PositionX(offsetX + 0.98f) - positionX;
            int height = PositionY(offsetY + 0.98f) - positionY;
            Rectangle buttonHitbox = new Rectangle(positionX, positionY, width, height);

            highlight = (WithinRect(buttonHitbox, new Point(Main.mouseX, Main.mouseY)));

            if (highlight)
            {
                Main.LocalPlayer.mouseInterface = true;
                if (wardrobeNames.TryGetValue(key, out string name))
                {
                    Main.instance.MouseText(name);
                }
                if (InfiniteNPC.Instance.RequestAssetIfExists(wardrobeTexturePaths[key] + "_Highlight", out ReLogic.Content.Asset<Texture2D> asset))
                {

                    Main.spriteBatch.Draw(
                        asset.Value,
                        drawBox,
                        Color.White
                    );
                }
            }
        }
    }
}
