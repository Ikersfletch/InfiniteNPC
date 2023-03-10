using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using System.Reflection;

namespace InfiniteNPC.NPCs
{
    public class SummonedTownProfile : ITownNPCProfile
    {
        public int GetHeadTextureIndex(NPC npc)
        {
            
            int assetLocation = ModContent.GetModHeadSlot(npc.ModNPC.HeadTexture);
            return assetLocation;
        }
        public string GetNameForVariant(NPC npc)
        {
            if (npc.type != ModContent.NPCType<SummonedNPC>()) return "";
            return (npc.ModNPC as SummonedNPC).myData.name;
        }

        public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc)
        {
            return TextureAssets.Item[ItemID.DirtBlock];
        }

        public int RollVariation()
        {
            return 3;
        }
        /*
        public static Asset<Texture2D> ReplaceAssetTextureWithRenderedHead(NPC npc, Asset<Texture2D> replacement)
        {
            // we need to do this to prevent a memory leak
            Texture2D disposeme = replacement.Value;
            if (disposeme != null && disposeme != TextureAssets.Item[ItemID.DirtBlock].Value)
            {
                bool withinTheTextureBuffer = false;
                // okay so its a valid texture and we should dispose of it eventually
                for (int i = 0; i < SummonedNPCHead.NPCTextureBuffer.Length; i ++)
                {
                    if (SummonedNPCHead.NPCTextureBuffer[i] != null && disposeme == SummonedNPCHead.NPCTextureBuffer[i].data)
                    {
                        withinTheTextureBuffer = true;
                    }
                }
                // okay cool it's not being actively used and it's not en route to being disposed? then put it in the stack.
                if (!withinTheTextureBuffer)
                {
                    SummonedNPCHead.disposalStack.Push(disposeme);
                }

            }
            //

            Texture2D rendered = (SummonedNPCHead.NPCTextureBuffer[npc.whoAmI] ?? new SummonedNPCHead.Texture2DDestructor(TextureAssets.Item[ItemID.DirtBlock].Value)).data;


            BindingFlags yes = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
            Type Asset2D = replacement.GetType();

            PropertyInfo loadedStateInfo = Asset2D.GetProperty("State", yes);
            loadedStateInfo.SetValue(replacement, AssetState.Loaded);

            FieldInfo textureFieldInfo = Asset2D.GetField("ownValue", yes);
            textureFieldInfo.SetValue(replacement, rendered);



            return replacement;
        }
        */
    }
}
