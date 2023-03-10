using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using InfiniteNPC.NPCs;

namespace InfiniteNPC.Items
{
    public class VesselOfRespite : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
        }

        public override void SetDefaults()
        {
            Item.maxStack = 20;
            Item.rare = ItemRarityID.Blue;
            Item.width = 16;
            Item.height = 26;
            Item.value = Item.sellPrice(0, 0, 0, 15);
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.useAnimation = 60;
            Item.useTime = 60;
            Item.UseSound = SoundID.Item4;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "RespiteDesc", "\'A vessel awaiting new life\'"));
        }

        public override bool? UseItem(Player player)
        {
            //if (SummonedNPC.OverridingMain) return;
            NPC wow = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), Main.mouseX + (int)(Main.screenPosition.X), Main.mouseY + (int)(Main.screenPosition.Y), ModContent.NPCType<SummonedNPC>());
            //Item.stack--;
            for (int i = 0; i < 35; i ++)
            {
                Dust.NewDust(wow.position, wow.width, wow.height, DustID.MagicMirror, 0, 0, 0,default, 2f);
            }
            return true;
        }
        public override void OnConsumeItem(Player player)
        {
            // I am not going to let NPCs spawn NPCs
            // I don't care if it would be "fun"
            // because it would only be fun for like 30 seconds.
            // and then it would be torture.
            //if (SummonedNPC.OverridingMain) return;
            //int wow = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), Main.mouseX + (int)(Main.screenPosition.X), Main.mouseY + (int)(Main.screenPosition.Y), ModContent.NPCType<SummonedNPC>());
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SoulOfRespite>(10)
                .AddIngredient<PetrifiedSoul>(1)
                .Register();
        }
    }
}
