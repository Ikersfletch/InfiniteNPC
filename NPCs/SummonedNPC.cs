using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.Graphics.Renderers;
using Terraria.GameInput;
using InfiniteNPC.UI;
using Terraria.DataStructures;
using Terraria.Map;
using Terraria.UI;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Microsoft.CodeAnalysis;
using System.Reflection;
using ReLogic.Content;
using Terraria.Graphics;
using System.Xml.Schema;
using System.Linq;

namespace InfiniteNPC.NPCs
{

    public class SummonedNPCHead : ModMapLayer
    {

        public class Texture2DDestructor
        {
            public Texture2D data;

            public Texture2DDestructor(Texture2D _data)
            {
                data = _data;
            }
            ~Texture2DDestructor()
            {
                disposalStack.Push(data);
            }
        }
        public static Texture2DDestructor[] NPCTextureBuffer = new Texture2DDestructor[Main.npc.Length];
        public static Texture2DDestructor[] NPCTextureBufferNoOutline = new Texture2DDestructor[Main.npc.Length];
        public static Stack<Texture2D> disposalStack = new Stack<Texture2D>();
        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            /*
            Main.spriteBatch.End();
            MapNPCRenderer.PrepareRenderTarget(Main.instance.GraphicsDevice, Main.spriteBatch);
            Main.spriteBatch.Begin();
            */

            bool rendered = false;
            if (!rendered)
            {
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<SummonedNPC>())
                    {
                        SummonedNPC npc = (Main.npc[i].ModNPC as SummonedNPC);
                        Texture2D tex = TextureAssets.NpcHead[NPC.TypeToDefaultHeadIndex(NPCID.Guide)].Value;
                        if (NPCTextureBuffer[i] != null)
                        {
                            tex = NPCTextureBuffer[i].data;
                        }

                        MapOverlayDrawContext.DrawResult result = context.Draw(tex, Main.npc[i].Top * 0.0625f, Alignment.Center);

                        if (result.IsMouseOver) text = npc.myData.name;
                    } else
                    {
                        NPCTextureBuffer[i] = null;
                    }
                }
            }
            while (disposalStack.TryPop(out Texture2D disposal))
            {
                if (disposal != null)
                disposal.Dispose();
            }
        }
        public static Texture2D MakeNPCHeadTexture(SummonedNPC npc)
        {
            if (Main.dedServ) return null;

            // This code is copied almost wholesale from the Vanilla code which draws the player map icons.
            // It was slightly adapted for the NPCs here.
            GraphicsDevice device = Main.instance.GraphicsDevice;
            RenderTarget2D target = new RenderTarget2D(device, 48, 48, false, device.PresentationParameters.BackBufferFormat, DepthFormat.None, 0,RenderTargetUsage.PreserveContents);
            RenderTarget2D finalTarget = new RenderTarget2D(device, 48, 48, false, device.PresentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            device.SetRenderTarget(target);
            device.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null);
            Main.PlayerRenderer.DrawPlayerHead(Main.Camera, npc.dummyPlayer, new Vector2(24f));
            Main.spriteBatch.End();

            Effect pixelShader = Main.pixelShader;
            EffectPass _coloringShader = pixelShader.CurrentTechnique.Passes["ColorOnly"];

            device.SetRenderTarget(finalTarget);
            device.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null);

            _coloringShader.Apply();
            int num = 2;
            int num2 = num * 2;
            for (int i = -num2; i <= num2; i += num)
            {
                for (int j = -num2; j <= num2; j += num)
                {
                    if (Math.Abs(i) + Math.Abs(j) == num2)
                    {
                        Main.spriteBatch.Draw(target, new Vector2(i, j), Color.Black);
                    }
                }
            }

            num2 = num;
            for (int k = -num2; k <= num2; k += num)
            {
                for (int l = -num2; l <= num2; l += num)
                {
                    if (Math.Abs(k) + Math.Abs(l) == num2)
                    {
                        Main.spriteBatch.Draw(target, new Vector2(k, l), Color.White);
                    }
                }
            }

            pixelShader.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(target, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            device.SetRenderTarget(null);
            NPCTextureBufferNoOutline[npc.NPC.whoAmI] = new Texture2DDestructor(target);
            NPCTextureBuffer[npc.NPC.whoAmI] = new Texture2DDestructor(finalTarget);
            return finalTarget;
        }
        public static void ClearBuffer()
        {
            NPCTextureBuffer = new Texture2DDestructor[Main.npc.Length];
            NPCTextureBufferNoOutline = new Texture2DDestructor[Main.npc.Length];
        }
    }
    public class NPCAmmo : GlobalItem
    {
        public override bool NeedsAmmo(Item item, Player player)
        {
            if (SummonedNPC.OverridingMain)
            {
                return false;
            }
            return base.NeedsAmmo(item, player);
        }

        public override bool? CanAutoReuseItem(Item item, Player player)
        {
            if (SummonedNPC.OverridingMain)
            {
                return true;
            }
            return base.CanAutoReuseItem(item, player);
        }


    }

    [AutoloadHead, Autoload(true)]
    public class SummonedNPC : ModNPC
    {
        public static SummonedNPCData NextMoveIn = new SummonedNPCData();
        public SummonedNPCData myData = new SummonedNPCData();
        public Player dummyPlayer;
        public bool loadedSaveData = false;
        public bool controlUse = false;
        public static Player mainplayer;
        public static bool OverridingMain => overridingNPCs.Count > 0;
        public static Stack<int> overridingNPCs = new Stack<int>();
        public static Point realmouse;
        public Point FakeMouse => (firingAt + NPC.Center - Main.screenPosition).ToPoint();
        public Vector2 firingAt = Vector2.Zero;

        public List<object> itemUseStorage = new List<object> { };

        public Stack<SummonedNPCOrder> currentOrders = new Stack<SummonedNPCOrder>();

        // IO
        public override void SaveData(TagCompound tag)
        {
            myData.SaveData(ref tag);

            List<TagCompound> currentOrderTagCompounds = new List<TagCompound>();

            currentOrders.ToList().ForEach(order => currentOrderTagCompounds.Add(order.SaveIO()));

            tag.Add("CurrentOrders", currentOrderTagCompounds);


        }
        public override void LoadData(TagCompound tag)
        {
            myData = new SummonedNPCData();
            myData.LoadData(ref tag);
            loadedSaveData = true;
            SetDummyAppearance();
            SummonedNPCHead.ClearBuffer();

            currentOrders.Clear();
            if (tag.TryGet("CurrentOrders", out List<TagCompound> currentOrderTagCompounds)) {
                currentOrderTagCompounds.ForEach(orderTag => currentOrders.Push(new SummonedNPCOrder(orderTag)));
            }
        }
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {// + new Vector2(0.0f,2.0f);
            //SetDummyAppearance(false, false);
            //Main.PlayerRenderer.DrawPlayer(Main.Camera, dummyPlayer, NPC.position + new Vector2(0.0f, -2.0f), 0f, dummyPlayer.fullRotationOrigin, 0f, 1f);
            SetDummyAppearance(false, false);
            return false;
        }

        // uses the player renderer to render the dummy player manually
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SetDummyAppearance(false,false);
            Vector2 oPosition = dummyPlayer.position;
            dummyPlayer.position = NPC.position + new Vector2(0f, -2f);
            Main.spriteBatch.End();
            Main.PlayerRenderer.DrawPlayers(Main.Camera, new List<Player>() { dummyPlayer });
            Main.spriteBatch.Begin();
            dummyPlayer.position = oPosition;
        }

        
        public override bool PreAI()
        {
            NPCID.Sets.AttackType[Type] = 1;

            return true;
        }

        public bool PvP = false;

        public bool clearWeaponUse = false;

        // the AI function
        public override void AI()
        {
            // consider the dummy player as if it was the main player
            OverrideMainPlayer();

            if (clearWeaponUse) { itemUseStorage.Clear(); clearWeaponUse = false; }

            // reset various stats in case they became incorrect
            dummyPlayer.statLife = NPC.life;
            dummyPlayer.statLifeMax = NPC.lifeMax;
            dummyPlayer.active = true;
            dummyPlayer.statManaMax = 800;
            dummyPlayer.statMana = 800;
            dummyPlayer.manaCost = 0f;
            dummyPlayer.whoAmI = Main.myPlayer;
            dummyPlayer.whoAmI = Main.myPlayer;
            dummyPlayer.enemySpawns = false;
            dummyPlayer.width = NPC.width;
            dummyPlayer.height = NPC.height;

            dummyPlayer.Bottom = NPC.Bottom;
            dummyPlayer.direction = NPC.direction;
            dummyPlayer.Update(254);
            NPC.direction = dummyPlayer.direction;
            dummyPlayer.velocity = NPC.velocity * new Vector2(2.0f, 1.0f);
            dummyPlayer.oldVelocity = NPC.oldVelocity * new Vector2(2.0f, 1.0f);
            dummyPlayer.Top = NPC.Top;
            NPC.aiStyle = 7;

            // if the NPC is sitting, make the dummy sit as well.
            if (NPC.ai[0] == 5)
            {
                dummyPlayer.sitting.isSitting = true;
                dummyPlayer.position += new Vector2(-NPC.direction * 6, 0f);
                dummyPlayer.FloorVisuals(false);
            }
            dummyPlayer.selectedItem = 0;

            bool hasAssignment = InfiniteNPC.ItemIDAssignments.TryGetValue(myData.weapon.type, out int index);
            if (hasAssignment) {
                Tuple<bool, bool, Vector2> itemUseData = InfiniteNPC.ProfileDatabase[index].GetFakeMouseForItem(NPC);

                dummyPlayer.controlUseItem = itemUseData.Item1;
                firingAt = itemUseData.Item3 - NPC.Center;
            }

            // return the actual player to their status
            RestoreMainPlayer();

            SummonedNPCHead.MakeNPCHeadTexture(this);

        }

        public override void OnKill()
        {
            DropItem(myData.weapon);
            for (int i = 0; i < 10; i++)
            {
                DropItem(myData.armor[i]);
                DropItem(myData.dyes[i]);
            }
        }

        public void DropItem(Item i)
        {
            if (i == null || i.IsAir) return;
            Item.NewItem(new EntitySource_DropAsItem(NPC), NPC.getRect(), i.type, i.stack, false, i.prefix);
        }

        public static Vector2 realZoom;
        // internally label the dummy player as the client so that items and projectiles update as such
        public void OverrideMainPlayer()
        {
            // IS THIS RIGHT??? I DON'T KNOW~~
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            // only mark down the main player's location if Main.player[Main.myPlayer] is actually the real player
            if (overridingNPCs.Count == 0)
            {
                mainplayer = Main.player[Main.myPlayer];
                realmouse = new Point(PlayerInput.MouseX, PlayerInput.MouseY);
                //realZoom = Main.GameViewMatrix.Zoom;
            }


            // fill in the data necessary for the NPC to replace the player
            Point fakemouse = FakeMouse;
            Main.mouseX = PlayerInput.MouseX = fakemouse.X;
            Main.mouseY = PlayerInput.MouseY = fakemouse.Y;

            Main.player[Main.myPlayer] = dummyPlayer;
            //Main.GameViewMatrix.Zoom = Vector2.One;
            dummyPlayer.whoAmI = Main.myPlayer;


            // add this NPC's id to the stack        
            overridingNPCs.Push(NPC.whoAmI);
        }
        // Pop one layer off the stack of overrides, and if the layer is the last one, restore the player's data
        public void RestoreMainPlayer()
        {
            overridingNPCs.TryPop(out _);

            if (OverridingMain) return;

            Main.player[Main.myPlayer] = mainplayer;

            Main.mouseX = PlayerInput.MouseX = realmouse.X;
            Main.mouseY = PlayerInput.MouseY = realmouse.Y;
           // Main.GameViewMatrix.Zoom = realZoom;
        }

        // fully clears out the stack.
        // exists purely as an emergency case where a call to OverrideMainPlayer isn't paired with a call to RestoreMainPlayer
        public static void ResetMainPlayer()
        {
            overridingNPCs.Clear();
            Main.player[Main.myPlayer] = mainplayer;

            Main.mouseX = PlayerInput.MouseX = realmouse.X;
            Main.mouseY = PlayerInput.MouseY = realmouse.Y;
        }
        public override bool CanGoToStatue(bool toKingStatue)
        {
            return (myData.Male == toKingStatue);
        }
        public override void Load()
        {
            On.Terraria.Player.ItemCheckWrapped += ItemCheckWrapped;
            On.Terraria.Player.UpdateProjectileCaches += UpdateProjectileCaches;
        }
        public override void Unload()
        {
            On.Terraria.Player.ItemCheckWrapped -= ItemCheckWrapped;
            On.Terraria.Player.UpdateProjectileCaches -= UpdateProjectileCaches;
        }

        public static void ItemCheckWrapped(On.Terraria.Player.orig_ItemCheckWrapped orig, Player self, int i)
        {
            if (OverridingMain)
            {
                int before = self.whoAmI;
                self.whoAmI = Main.myPlayer;
                self.ItemCheck(Main.myPlayer);
                self.whoAmI = before;
            }
            else
            {
                orig(self, i);
            }
        }

        public static void UpdateProjectileCaches(On.Terraria.Player.orig_UpdateProjectileCaches orig, Player self, int i)
        {

            if (!OverridingMain)
            {
                orig(self, i);
            }
            else
            {
                for (int j = 0; j < self.ownedProjectileCounts.Length; j++)
                {
                    self.ownedProjectileCounts[j] = 0;
                }
                for (int j = 0; j < 1000; j++)
                {
                    if (!Main.projectile[j].active || Main.projectile[j].owner != Main.myPlayer)
                        continue;
                    if (!Main.projectile[j].TryGetGlobalProjectile<NPCProj>(out NPCProj globalProj)) continue;

                    if (globalProj.NPCOwner != overridingNPCs.Peek()) continue;
                    
                    self.ownedProjectileCounts[Main.projectile[j].type]++;

                    switch (Main.projectile[j].type)
                    {
                        case ProjectileID.StormTigerGem:
                            {
                                int originalDamage2 = Main.projectile[j].originalDamage;
                                if (self.highestStormTigerGemOriginalDamage < originalDamage2)
                                    self.highestStormTigerGemOriginalDamage = originalDamage2;

                                break;
                            }
                        case ProjectileID.AbigailCounter :
                            {
                                int originalDamage = Main.projectile[j].originalDamage;
                                if (self.highestAbigailCounterOriginalDamage < originalDamage)
                                    self.highestAbigailCounterOriginalDamage = originalDamage;

                                break;
                            }
                    }
                }
            }
        }


        // Setup the NPC type data
        public override ITownNPCProfile TownNPCProfile()
        {
            return new SummonedTownProfile();
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
            NPCID.Sets.SpawnsWithCustomName[Type] = true;
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
        }
        public override bool CanChat()
        {
            return true;
        }

        public override string GetChat()
        {
            return "...";
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (firstButton)
            {
                NPCEquipper.OpenNPCEquipper(NPC.whoAmI);
                Main.CloseNPCChatOrSign();

            } else
            {

            }
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "Equipment";
            button2 = "";
        }
        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Guide;
            myData = new SummonedNPCData();
            dummyPlayer = new Player();

            // before a player can customize them, the player must fulfill a request from the NPC?
            currentOrders.Push(new SummonedNPCOrder(
                 -1,
                 (NPC.position / 16f).ToPoint(),
                 0,
                 -1,
                 Main.rand.NextFromList(new SummonedNPCOrder.SummonedNPCOrderType[] {
                    SummonedNPCOrder.SummonedNPCOrderType.AskForCash,
                    SummonedNPCOrder.SummonedNPCOrderType.AskForHome,
                    SummonedNPCOrder.SummonedNPCOrderType.AskForItem,
                 })
                ));

           // SetDummyAppearance(true);
        }

        // ** Change the dummy's appearance
        public void SetDummyAppearance(bool overrideInstance = false, bool overrideWeapon = true)
        {
            if (overrideInstance)
                dummyPlayer = new Player();
            if (overrideWeapon)
                dummyPlayer.inventory[0] = myData.weapon;
            bool flag = false;
            for (int i = 0; i < myData.armor.Length; i++)
            {
                dummyPlayer.armor[i + 10] = myData.armor[i];
                dummyPlayer.dye[i] = myData.dyes[i].Clone();
            }
            dummyPlayer.skinVariant = myData.skin;
            dummyPlayer.direction = NPC.direction;
            dummyPlayer.position = NPC.position;
            dummyPlayer.hair = myData.hairstyle;
            dummyPlayer.hairColor = myData.wardrobe[0];
            dummyPlayer.eyeColor = myData.wardrobe[1];
            dummyPlayer.skinColor = myData.wardrobe[2];
            dummyPlayer.underShirtColor = myData.wardrobe[3];
            dummyPlayer.shirtColor = myData.wardrobe[4];
            dummyPlayer.pantsColor = myData.wardrobe[5];
            dummyPlayer.shoeColor = myData.wardrobe[6];
            dummyPlayer.UpdateDyes();
            NPC.GivenName = myData.name;
        }
    }
}
