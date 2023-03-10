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
    public class PetrifiedSoul : ModItem
    {

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<PetrifiedSoulTile>());
            Item.rare = ItemRarityID.Blue;
            Item.width = 16;
            Item.height = 26;
            Item.value = Item.sellPrice(0, 0, 0, 15);
            Item.maxStack = 99;
            Item.createTile = ModContent.TileType<PetrifiedSoulTile>();
            Item.material = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "RespiteDesc", "\'A husk to fill with a spirit of the dead\'\nPassively collects Souls of Respite"));
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 30)
                .AddTile(TileID.HeavyWorkBench)
                .AddCondition(Recipe.Condition.InGraveyardBiome)
                .Register();
        }
    }
}
