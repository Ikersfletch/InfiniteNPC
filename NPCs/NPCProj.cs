using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using static Humanizer.In;

namespace InfiniteNPC.NPCs
{
    public class NPCProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public int NPCOwner = -1;
        public override void SetDefaults(Projectile projectile)
        {
            if (SummonedNPC.OverridingMain)
            {
                NPCOwner = SummonedNPC.overridingNPCs.Peek();
            }
        }
        public override bool PreAI(Projectile projectile)
        {
            SummonedNPC Owner = GetNPCOwnerOrNull(projectile);
            if (Owner == null || NPCOwner == -1 || !Main.npc[NPCOwner].active || Main.npc[NPCOwner].type != ModContent.NPCType<SummonedNPC>())
                return base.PreAI(projectile);

            Owner.OverrideMainPlayer();


            return base.PreAI(projectile);
        }
        public override void AI(Projectile projectile)
        {
            if (NPCOwner == -1 || !Main.npc[NPCOwner].active || Main.npc[NPCOwner].type != ModContent.NPCType<SummonedNPC>()) return;

            projectile.npcProj = true;
            projectile.noDropItem = true;

            // ((SummonedNPC)Main.npc[NPCOwner].ModNPC).dummyPlayer.heldProj = projectile.whoAmI;


        }
        public override void PostAI(Projectile projectile)
        {
            SummonedNPC Owner = GetNPCOwnerOrNull(projectile);
            if (Owner == null || NPCOwner == -1)
            {
                base.PostAI(projectile);
                return;
            }

            Owner.RestoreMainPlayer();

            base.PostAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            SummonedNPC Owner = GetNPCOwnerOrNull(projectile);
            if (Owner == null || NPCOwner == -1 || !Main.npc[NPCOwner].active || Main.npc[NPCOwner].type != ModContent.NPCType<SummonedNPC>())
                return base.PreDraw(projectile, ref lightColor);

            Owner.OverrideMainPlayer();

            return base.PreDraw(projectile, ref lightColor);
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            SummonedNPC Owner = GetNPCOwnerOrNull(projectile);
            if (Owner == null || NPCOwner == -1)
            {
                base.PostDraw(projectile, lightColor);
                return;
            }

            Owner.RestoreMainPlayer();

            base.PostDraw(projectile, lightColor);
        }
        public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
        {
            SummonedNPC Owner = GetNPCOwnerOrNull(projectile);
            if (Owner == null || NPCOwner == -1 || !Main.npc[NPCOwner].active || Main.npc[NPCOwner].type != ModContent.NPCType<SummonedNPC>())
                return base.Colliding(projectile, projHitbox, targetHitbox);

            Owner.OverrideMainPlayer();

            bool? collide = base.Colliding(projectile, projHitbox, targetHitbox);

            Owner.RestoreMainPlayer();

            return collide;
        }

        public Player OwnerDummyPlayer => ((SummonedNPC)Main.npc[NPCOwner].ModNPC).dummyPlayer;


        public override void Load()
        {
            On.Terraria.Main.GetPlayerArmPosition += ArmPos;
            On.Terraria.Main.DrawProj += DrawProj;
            On.Terraria.Projectile.AI += OnProjAI;
            On.Terraria.Projectile.Update += OnProjUpdate;
            On.Terraria.Projectile.Kill += OnKill;
        }
        public override void Unload()
        {
            On.Terraria.Main.GetPlayerArmPosition -= ArmPos;
            On.Terraria.Main.DrawProj -= DrawProj;
            On.Terraria.Projectile.AI -= OnProjAI;
            On.Terraria.Projectile.Update -= OnProjUpdate;
            On.Terraria.Projectile.Kill -= OnKill;
        }

        public static Vector2 ArmPos(On.Terraria.Main.orig_GetPlayerArmPosition orig, Projectile proj)
        {
            return orig(proj);

            if (!proj.TryGetGlobalProjectile<NPCProj>(out _) || proj.GetGlobalProjectile<NPCProj>().NPCOwner == -1) return orig(proj);

            return orig(proj) - SummonedNPC.mainplayer.position + (proj.GetGlobalProjectile<NPCProj>().OwnerDummyPlayer.position);

        }

        public static void DrawProj(On.Terraria.Main.orig_DrawProj orig, Main self, int i)
        {
            Projectile proj = Main.projectile[i];
            SummonedNPC Owner = GetNPCOwnerOrNull(proj);
            if (Owner == null || proj.GetGlobalProjectile<NPCProj>().NPCOwner == -1)
            {
                orig(self, i);
            }
            else
            {
                /*
                if (NPCProj.mainplayer.heldProj == self.whoAmI)
                {
                    g.OwnerDummyPlayer.heldProj = self.whoAmI;
                    self.position += g.OwnerDummyPlayer.position - NPCProj.mainplayer.position;
                    NPCProj.mainplayer.heldProj = -1;
                }
                */
                Owner.OverrideMainPlayer();
                orig(self, i);
                Owner.RestoreMainPlayer();
            }
        }

        public static NPCProj GlobalOrNull(Projectile i) => i.TryGetGlobalProjectile<NPCProj>(out NPCProj r) ? r : null;
        public static SummonedNPC GetNPCOwnerOrNull(Projectile i)
        {
            NPCProj p = GlobalOrNull(i);
            return
                (p == null) ?
                    null :
                    (
                        (p.NPCOwner == -1) ?
                            null :
                            ((SummonedNPC)Main.npc[p.NPCOwner].ModNPC)
                    );
        }
        public void OnProjAI(On.Terraria.Projectile.orig_AI orig, Projectile self)
        {
            SummonedNPC Owner = GetNPCOwnerOrNull(self);
            if (Owner == null || self.GetGlobalProjectile<NPCProj>().NPCOwner == -1)
            {
                orig(self);
            }
            else
            {
                /*
                if (NPCProj.mainplayer.heldProj == self.whoAmI)
                {
                    g.OwnerDummyPlayer.heldProj = self.whoAmI;
                    self.position += g.OwnerDummyPlayer.position - NPCProj.mainplayer.position;
                    NPCProj.mainplayer.heldProj = -1;
                }
                */
                Owner.OverrideMainPlayer();
                orig(self);
                Owner.RestoreMainPlayer();
            }
        }

        public static void OnProjUpdate(On.Terraria.Projectile.orig_Update orig, Projectile self, int i)
        {
            SummonedNPC Owner = GetNPCOwnerOrNull(self);
            if (Owner == null || self.GetGlobalProjectile<NPCProj>().NPCOwner == -1)
            {
                orig(self, i);
            }
            else
            {
                Owner.OverrideMainPlayer();
                orig(self, i);
                Owner.RestoreMainPlayer();

                if (self.minion || self.sentry)
                {
                    if (Owner.myData.weapon.shoot != self.type) self.Kill();
                }

                //SummonedNPC.ResetMainPlayer();
            }
        }

        public static void OnKill(On.Terraria.Projectile.orig_Kill orig, Projectile self) 
        {
            SummonedNPC Owner = GetNPCOwnerOrNull(self);
            if ((!self.minion && !self.sentry) || Owner == null )
            {
                orig(self);
            } else
            {
                // when the stardust dragon needs special treatment
                if (ProjectileID.Sets.NeedsUUID[self.type])
                {
                    int byUUID = (int)self.ai[0];


                } else if (Owner.myData.weapon.shoot != self.type)
                {
                    orig(self);
                }
            }
        }
    }

}
