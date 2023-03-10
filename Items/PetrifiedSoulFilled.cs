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
using InfiniteNPC.Tiles;

namespace InfiniteNPC.Items
{
    public class PetrifiedSoulFilled : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dirtified Petrified Soul");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<PetrifiedSoulFilledTile>());
            Item.rare = ItemRarityID.Blue;
            Item.width = 16;
            Item.height = 26;
            Item.value = Item.sellPrice(0, 0, 0, 16);
            Item.maxStack = 99;
            Item.createTile = ModContent.TileType<PetrifiedSoulFilledTile>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "RespiteDesc", "Cannot hold Souls of Respite"));
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 5)
                .AddIngredient<PetrifiedSoul>(1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
