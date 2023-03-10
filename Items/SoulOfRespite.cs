using Terraria.ID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteNPC.Items
{
    public class SoulOfRespite : ModItem
    {

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            ItemID.Sets.ItemIconPulse[Type] = true; 
            ItemID.Sets.ItemNoGravity[Type] = true; 
        }

        public override void SetDefaults()
        {
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.Orange;
            Item.width = 26;
            Item.height = 32;
            Item.value = Item.sellPrice(0,0,2,0);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "RespiteDesc", "\'The essence of short relief\'"));
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, new Color( 249, 255, 138).ToVector3() * 0.55f * Main.essScale); // Makes this item glow when thrown out of inventory.
        }

    }

    public class SoulRespiteGlobalDrop : GlobalNPC
    {
        public override void ModifyGlobalLoot(GlobalLoot globalLoot)
        {
            globalLoot.Add(ItemDropRule.ByCondition(new SoulRespiteDropCondition(), ModContent.ItemType<SoulOfRespite>()));
        }
    }

    public class SoulRespiteDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            if (info.npc != null)
            {
                if (info.npc.type == NPCID.Ghost) return info.rng.NextBool(3);
                if (info.npc.type == NPCID.Guide && info.player.killGuide) return false;
                if (info.npc.isLikeATownNPC) return info.rng.NextBool(3);
                if (info.npc.CountsAsACritter) return info.rng.NextBool(60);
            }

            return info.player.ZoneGraveyard && info.rng.NextBool(15);
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "Dropped upon a regretful death";
        }
    }
}
