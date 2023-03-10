using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using InfiniteNPC.Items;
using Ionic.Zip;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Audio;

namespace InfiniteNPC.Tiles
{
    public class PetrifiedSoulTile : ModTile
    {
        private void lightBrightness (int i, int j, out float brightness) => brightness = 0.4f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.03f + ((MathF.E * (float)i) - MathF.Truncate(MathF.E * (float)i)) * (float)j);
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(0,1);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 20;
            //TileObjectData.newTile.DrawXOffset = -2;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide | AnchorType.Table, 1, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.Direction = Terraria.Enums.TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = Terraria.Enums.TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);
            TileObjectData.addTile(Type);
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Petrified Soul");
            AddMapEntry(new Color(174,174,174), name);
            AddMapEntry(new Color(182,184,146), name);

        }
        public override string HighlightTexture => Texture + "_Highlight";
        public override void RandomUpdate(int i, int j)
        {
            if (Main.rand.NextBool(20))
            {
                SetSoulCharge(i, j, true);
                SpawnDust(i, j);
            }
        }
        public override void MouseOver(int i, int j)
        {
            GetSoulCharge(i, j, out bool charge);
            if (charge)
            {
                Main.LocalPlayer.noThrow = 2;
                Main.LocalPlayer.cursorItemIconEnabled = true;
                Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<PetrifiedSoul>();
            } else if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<SoulOfRespite>() && Main.LocalPlayer.HeldItem.stack > 0)
            {
                Main.LocalPlayer.noThrow = 2;
                Main.LocalPlayer.cursorItemIconEnabled = true;
                Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<SoulOfRespite>();
            }
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return false;
        }
        public override ushort GetMapOption(int i, int j)
        {
            GetSoulCharge(i, j, out bool charge);
            return (ushort)(charge ? 1 : 0);
        }
        public override bool RightClick(int i, int j)
        {
            GetSoulCharge(i, j, out bool initialCharge);
            if (initialCharge)
            {
                Main.LocalPlayer.noThrow = 2;
                int id = Item.NewItem(new EntitySource_TileInteraction(Main.LocalPlayer, i, j), i * 16 + 8, j * 16, 0, 0, ModContent.ItemType<SoulOfRespite>(), 1, false, 0, false);
                Main.item[id].noGrabDelay = 50;
                Main.item[id].velocity = new Vector2(0f, -2f);
                SetSoulCharge(i, j, false);
                SpawnDust(i, j);
                SoundEngine.PlaySound(SoundID.NPCDeath6.WithVolumeScale(0.2f), new Vector2(i, j) * 16f);
                return true;
            } else if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<SoulOfRespite>() && Main.LocalPlayer.HeldItem.stack > 0)
            {
                Main.LocalPlayer.noThrow = 2;
                Main.LocalPlayer.HeldItem.stack--;
                if (Main.mouseItem != null && Main.mouseItem.type == ModContent.ItemType<SoulOfRespite>()) Main.mouseItem.stack--;

                SetSoulCharge(i, j, true);
                SpawnDust(i, j);
                return true;
            }
            return false;
        }
        public void SpawnDust(int i, int j)
        {
            for (int n = 0; n < 5; n++)
            {
                float brightness = 0.9f;
                Dust.NewDust(new Vector2(i, j) * 16f, 16, 32, DustID.TintableDustLighted, 0f, 1f, 0, new Color(0.9764706f * brightness, brightness, 0.5411765f * brightness));
            }
        }
        public void GetTop(int i, int j, out int topX, out int topY)
        {
            topX = i;
            topY = (Main.tile[i, j].TileFrameY == 0 || Main.tile[i, j].TileFrameY == 36) ? j : j - 1;
        }

        public void GetSoulCharge(int i, int j, out bool charge)
        {
            charge = (Main.tile[i, j].TileFrameY == 36 || Main.tile[i, j].TileFrameY == 54);
        }

        public void GetFacing(int i, int j, out int facing)
        {
            facing = (Main.tile[i, j].TileFrameX == 0) ? 0 : 1;
        }

        public void SetSoulCharge(int i, int j, bool charge)
        {
            GetTop(i, j, out int topX, out int topY);
            Main.tile[topX, topY].TileFrameY = (short)(charge ? 36 : 0);
            Main.tile[topX, topY + 1].TileFrameY = (short)(charge ? 54 : 18);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ModContent.ItemType<PetrifiedSoul>());
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            GetSoulCharge(i, j, out bool charge);
            if (charge)
            {
                lightBrightness(i, j, out float brightness);
                r = 0.9764706f * brightness;
                g = 1f * brightness;
                b = 0.5411765f * brightness;
            } else
            {
                r = 0f;
                g = 0f;
                b = 0f;
            }

        }
    }
}
