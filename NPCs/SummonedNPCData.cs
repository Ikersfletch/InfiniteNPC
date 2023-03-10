using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using IL.Terraria.DataStructures;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace InfiniteNPC.NPCs
{
    public class SummonedNPCData : ICloneable, IEquatable<SummonedNPCData>
    {
        public Item[] armor;
        public Item[] dyes;
        public Item weapon;
        public Color[] wardrobe;
        public int skin;
        public int hairstyle;
        public string name;
        public static Vector3 GetRandomColorVector() => new Vector3(Main.rand.NextFloat(), Main.rand.NextFloat(), Main.rand.NextFloat());
        public static Color ScaledHslToRgb(Vector3 hsl) => ScaledHslToRgb(hsl.X, hsl.Y, hsl.Z);
        public static Color ScaledHslToRgb(float hue, float saturation, float luminosity) => Main.hslToRgb(hue, saturation, (float)((double)luminosity * 0.850000023841858 + 0.150000005960464));

        public static int GetRandomBaseWeapon => Main.rand.NextFromList(new int[] {
        
            ItemID.WoodYoyo,
            ItemID.CopperShortsword,
            ItemID.WoodenBow,
            ItemID.WoodenBoomerang,
            ItemID.WandofSparking,
            (NPC.downedBoss2) ? ( Main.ActiveWorldFileData.HasCrimson ? ItemID.TheUndertaker : ItemID.Musket ) : ( Main.ActiveWorldFileData.HasCrimson ? ItemID.ShadewoodBow : ItemID.EbonwoodBow ),
            (NPC.downedGoblins) ? ItemID.Harpoon : ItemID.PalmWoodBow,
            (NPC.downedQueenBee) ? ItemID.Boomstick : ItemID.RichMahoganyBow,
            (NPC.downedQueenBee) ? ItemID.BeeKeeper : ItemID.RichMahoganySword,
            ItemID.SlimeGun,
            ItemID.WaterGun,
            (NPC.downedBoss3) ? ItemID.WaterBolt : ItemID.WandofSparking,
            (Main.hardMode) ? ItemID.Shotgun : ((NPC.downedQueenBee) ? ItemID.Boomstick : ItemID.RichMahoganyBow),
            (NPC.downedDeerclops) ? ItemID.PewMaticHorn : ItemID.BorealWoodBow,
            (NPC.downedBoss3) ? ItemID.Starfury : ItemID.PalmWoodSword,
            (Main.bloodMoon) ? ItemID.BloodyMachete : ItemID.WoodenBoomerang,
            ItemID.BladedGlove,
            ItemID.Blowpipe ,
            (NPC.downedBoss1) ? ItemID.AmethystStaff : ItemID.WandofSparking,
            (Main.ActiveWorldFileData.HasCorruption) ? ((Main.hardMode) ? ItemID.Bladetongue : ItemID.BloodButcherer) : ((Main.hardMode) ? ItemID.Toxikarp : ItemID.PurpleClubberfish),
            (Main.hardMode) ? ItemID.PearlwoodSword : ItemID.WoodenSword,
            (Main.hardMode) ? ItemID.PearlwoodBow : ItemID.WoodenBow,
            (NPC.downedEmpressOfLight) ? ItemID.SparkleGuitar : ItemID.CarbonGuitar,
            (NPC.FindFirstNPC(NPCID.Steampunker) >= 0) ? ItemID.IvyGuitar : ItemID.CarbonGuitar
        });


        public SummonedNPCData()
        {
            weapon = new Item(GetRandomBaseWeapon);
            hairstyle = Main.rand.Next(Main.numberOfHairstyles);
            skin = Main.rand.Next(10);
            if (Female() && Main.rand.NextBool()) SwapSex(); 
            if (!Main.rand.NextBool(15))
            {
                switch (hairstyle + 1)
                {
                    case 5:
                    case 6:
                    case 7:
                    case 10:
                    case 12:
                    case 19:
                    case 22:
                    case 23:
                    case 26:
                    case 27:
                    case 30:
                    case 33:
                        skin = Main.rand.NextFromList<int>(new int[] { PlayerVariantID.FemaleStarter, PlayerVariantID.FemaleSticker, PlayerVariantID.FemaleGangster, PlayerVariantID.FemaleCoat, PlayerVariantID.FemaleDress });
                        break;
                }
            }
            /*
            public const int MaleStarter = 0;
            public const int MaleSticker = 1;
            public const int MaleGangster = 2;
            public const int MaleCoat = 3;
            public const int MaleDress = 8;

            public const int FemaleStarter = 4;
            public const int FemaleSticker = 5;
            public const int FemaleGangster = 6;
            public const int FemaleCoat = 7;
            public const int FemaleDress = 9;
        */
            wardrobe = new Color[7];
            wardrobe[1] = new Color(0, 0, 0, 1);
            // eye color
            while ((int)wardrobe[1].R + (int)wardrobe[1].G + (int)wardrobe[1].B > 300)
                wardrobe[1] = ScaledHslToRgb(GetRandomColorVector());
            wardrobe[1].A = (byte)255;

            wardrobe[2] = new Color(0, 0, 0, 1);
            // skin color
            float num = (float)Main.rand.Next(60, 120) * 0.01f;
            if ((double)num > 1.0)
                num = 1f;
            wardrobe[2].R = (byte)((double)Main.rand.Next(240, (int)byte.MaxValue) * (double)num);
            wardrobe[2].G = (byte)((double)Main.rand.Next(110, 140) * (double)num);
            wardrobe[2].B = (byte)((double)Main.rand.Next(75, 110) * (double)num);
            wardrobe[2].A = (byte)255;

            //hairColor
            wardrobe[0] = ScaledHslToRgb(GetRandomColorVector());
            wardrobe[3] = ScaledHslToRgb(GetRandomColorVector());
            wardrobe[4] = ScaledHslToRgb(GetRandomColorVector());
            wardrobe[5] = ScaledHslToRgb(GetRandomColorVector());
            wardrobe[6] = ScaledHslToRgb(GetRandomColorVector());

            armor = new Item[10];
            dyes = new Item[10];
            for (int i = 0; i < 10; i++)
            {
                armor[i] = new Item();
                dyes[i] = new Item();
            }


            name = NPCNameDepository.NewName(Main.rand, Female() ? NPCNameDepository.NameCategory.GivenFeminine : NPCNameDepository.NameCategory.GivenMasculine);

        }
        public SummonedNPCData(Item[] _armor, Item[] _dyes, Item _weapon, Color[] _wardrobe, int _skin, int _hairstyle, string _name)
        {
            armor = _armor;
            dyes = _dyes;
            weapon = _weapon;
            wardrobe = _wardrobe;
            skin = _skin;
            hairstyle = _hairstyle;
            name = _name;
        }
        public void SaveData(ref TagCompound tag)
        {
            List<TagCompound> sArmor = new List<TagCompound>();

            for (int i = 0; i < 10; i ++)
            {
                sArmor.Add(ItemIO.Save(armor[i]));
                sArmor.Add(ItemIO.Save(dyes[i]));
            }

            List<Color> Colors = wardrobe.ToList<Color>();
            tag.Add("Equips", sArmor);
            tag.Add("Wardrobe", Colors);
            tag.Add("Name", name);
            tag.Add("Skin", skin);
            tag.Add("Hairstyle", hairstyle);
            tag.Add("Weapon", ItemIO.Save(weapon));

        }
        public void LoadData(ref TagCompound tag)
        {
            List<Color> Colors = tag.GetList<Color>("Wardrobe").ToList();
            wardrobe = Colors.ToArray();
            name = tag.GetString("Name");
            weapon = ItemIO.Load(tag.Get<TagCompound>("Weapon"));
            skin = tag.GetInt("Skin");
            hairstyle = tag.GetInt("Hairstyle");

            if (tag.TryGet<List<TagCompound>>("Equips", out List<TagCompound> sArmor))
            for (int i = 0; i < 10; i ++)
            {
                int index1 = i * 2;
                int index2 = index1 + 1;
                armor[i] = ItemIO.Load(sArmor[index1]);
                dyes[i] = ItemIO.Load(sArmor[index2]);
            }


        }
        public object Clone()
        {
            return new SummonedNPCData(armor, dyes, weapon, wardrobe, skin, hairstyle, name);
        }

        public void SetHairColor(Color replacement)
        {
            wardrobe[0] = replacement;
        }
        public void SetEyeColor(Color replacement)
        {
            wardrobe[1] = replacement;
        }
        public void SetSkinColor(Color replacement)
        {
            wardrobe[2] = replacement;
        }
        public void SetUndershirtColor(Color replacement)
        {
            wardrobe[3] = replacement;
        }
        public void SetShirtColor(Color replacement)
        {
            wardrobe[4] = replacement;
        }
        public void SetPantsColor(Color replacement)
        {
            wardrobe[5] = replacement;
        }
        public void SetShoeColor(Color replacement)
        {
            wardrobe[6] = replacement;
        }
        public void CycleSkin()
        {
            switch(skin)
            {
                case 3:
                    skin = 8;
                    break;
                case 8:
                    skin = 0;
                    break;
                case 7:
                    skin = 9;
                    break;
                case 9:
                    skin = 4;
                    break;
                default:
                    skin++;
                    break;
            }
        }
        public bool Male => !Female();
        public bool Female()
        {
            return (skin >= 4 && skin <= 7) || skin == 9;
        }
        public void SwapSex()
        {
            switch (skin)
            {
                case 8:
                    skin = 9;
                    break;
                case 9:
                    skin = 8;
                    break;
                default:
                    skin += (skin < 4) ? 4 : -4;
                    break;
            }
        }
        public bool Equals(SummonedNPCData other)
        {
            if (other.name != name) return false;
            if (other.hairstyle != hairstyle) return false;
            if (other.skin != skin) return false;
            if (!ItemEquals(other.weapon, weapon)) return false;
            for (int i = 0; i < 7; i++) if (!other.wardrobe[i].Equals(wardrobe[i])) return false;
            for (int i = 0; i < 10; i ++)
            {
                if (!ItemEquals(other.armor[i], armor[i])) return false;
                if (!ItemEquals(other.dyes[i], dyes[i])) return false;
            }
            return true;
        }

        public static bool ItemEquals(Item a, Item b)
        {
            if (a.type != b.type) return false;
            if (a.prefix != b.prefix) return false;
            if (a.stack != b.stack) return false;
            return true;
        }


        public static bool operator ==(SummonedNPCData a, SummonedNPCData b) => a.Equals(b);
        public static bool operator !=(SummonedNPCData a, SummonedNPCData b) => !(a == b);

    }

}
