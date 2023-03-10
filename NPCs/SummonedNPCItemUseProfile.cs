using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace InfiniteNPC.NPCs
{
    public class SummonedNPCItemUseProfile
    {
        /// <summary>
        /// If it returns true for a given item, then Summoned NPCs will call the related GetFakeMouseForItem to decide when to use that item
        /// </summary>
        public Predicate<Item> RegisterThisItemUnderThisProfile
        {
            get;
            private set;
        }

        /// <summary>
        /// If RegisterThisItemUnderThisProfile succeeded, this function is called to get the <see cref="NPC"/>'s fake mouse position and whether or not it's pressed.
        /// The returned <see cref="Tuple{bool,bool,Vector2}"/>'s items represent the following:
        /// <list type="bullet">Item1 - <see cref="bool"/> : Whether the fake left mouse button is considered as pressed</list>
        /// <list type="bullet">Item2 - <see cref="bool"/> : Whether the fake right mouse button is considered as pressed</list>
        /// <list type="bullet">Item3 - <see cref="Vector2"/> : The world position of the fake mouse cursor</list>
        /// </summary>
        public Func<NPC, Tuple<bool,bool,Vector2>> GetFakeMouseForItem
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public SummonedNPCItemUseProfile(string name, Predicate<Item> registerThisItemUnderThisProfile, Func<NPC, Tuple<bool, bool, Vector2>> getFakeMouseForItem)
        {
            Name = name;
            RegisterThisItemUnderThisProfile = registerThisItemUnderThisProfile;
            GetFakeMouseForItem = getFakeMouseForItem;
        }

        /// <summary>
        /// Constructs a list of the default profiles which come with <see cref="InfiniteNPC"/>
        /// </summary>
        /// <returns>The default list of profiles that comes with <see cref="InfiniteNPC"/></returns>
        public static List<SummonedNPCItemUseProfile> GetBaseProfileList()
        {
            List<SummonedNPCItemUseProfile> list = new List<SummonedNPCItemUseProfile>();

            Predicate<NPC> untargetable = npc => npc == null || !npc.active || npc.friendly || npc.townNPC || npc.CountsAsACritter || npc.isLikeATownNPC;
            // by default, set all items to be used as weapons which attack the enemy directly.
            list.Add(new SummonedNPCItemUseProfile("Default Profile", i => true, npc => {
                int target = -1;
                float closest = 1000000f;
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    // continue if invalid target
                    if (untargetable(Main.npc[i]) || i == npc.whoAmI) continue;

                    // find distance to potential target
                    float dist = Vector2.DistanceSquared(Main.npc[i].Center, npc.Center);

                    // if its closer than the current candidate and there's line of sight, change candidates.
                    if (dist < closest && Collision.CanHitLine(npc.Center, 1, 1, Main.npc[i].Center, 16, 16))
                    {
                        closest = dist;
                        target = i;
                    }
                }
                return new Tuple<bool, bool, Vector2>(target != -1, false, (target == -1) ? npc.Center : Main.npc[target].Center);
            }));
            // for non-projectile melee, don't swing until an enemy is close, and don't bother aiming.
            list.Add(new SummonedNPCItemUseProfile(
                "Basic Swing Melee",
                i => i.DamageType.CountsAsClass(DamageClass.Melee) && i.shoot == 0,
                npc => {

                    int target = -1;
                    float closest = 250000f;
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        // continue if invalid target
                        if (untargetable(Main.npc[i]) || i == npc.whoAmI) continue;

                        // find distance to potential target
                        float dist = Vector2.DistanceSquared(Main.npc[i].Center, npc.Center);

                        // if its closer than the current candidate and there's line of sight, change candidates.
                        if (dist < closest && (dist < 62500f || Collision.CanHitLine(npc.Center, 1, 1, Main.npc[i].Center, 16, 16)))
                        {
                            closest = dist;
                            target = i;
                        }
                    }

                    if (target == -1)
                        return new Tuple<bool, bool, Vector2>(false, false, npc.Center);

                    Vector2 difference = Main.npc[target].Center - npc.Center;
                    return new Tuple<bool, bool, Vector2>(true, false, npc.Center + new Vector2(Math.Sign(difference.X) * 1500, 0));
                }
                ));
            // bows should correct for gravity a bit.
            list.Add(new SummonedNPCItemUseProfile(
                "Arrow Gravity Correction",
                i => i.ammo == AmmoID.Arrow && i.shoot == ProjectileID.WoodenArrowFriendly && i.CountsAsClass(DamageClass.Ranged),
                npc => {

                    int target = -1;
                    float closest = 1000000f;
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        // continue if invalid target
                        if (untargetable(Main.npc[i]) || i == npc.whoAmI) continue;

                        // find distance to potential target
                        float dist = Vector2.DistanceSquared(Main.npc[i].Center, npc.Center);

                        // if its closer than the current candidate and there's line of sight, change candidates.
                        if (dist < closest && (Collision.CanHitLine(npc.Center, 1, 1, Main.npc[i].Center, 16, 16)))
                        {
                            closest = dist;
                            target = i;
                        }
                    }
                    if (target == -1)
                        return new Tuple<bool, bool, Vector2>(false, false, npc.Center);

                    // wow look at my.. "wonderful" targeting. 
                    Vector2 targetCenter = Main.npc[target].Center;
                    Vector2 difference = npc.Center - targetCenter;

                    return new Tuple<bool, bool, Vector2>(true, false, npc.Center + new Vector2(0f, difference.Y * Math.Abs(difference.X)) / (npc.ModNPC as SummonedNPC).myData.weapon.shootSpeed);

                }
                ));


            // hehe zenith go brrr
            list.Add(new SummonedNPCItemUseProfile(
                "Zenith",
                i => i.type == ItemID.Zenith,
                npc => {
                    int target = -1;
                    float closest = 1000000f;
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        // continue if invalid target
                        if (untargetable(Main.npc[i]) || i == npc.whoAmI) continue;

                        // find distance to potential target
                        float dist = Vector2.DistanceSquared(Main.npc[i].Center, npc.Center);

                        // if its closer than the current candidate and there's line of sight, change candidates.
                        if (dist < closest)
                        {
                            closest = dist;
                            target = i;
                        }
                    }
                    return new Tuple<bool, bool, Vector2>(target != -1, false, (target == -1) ? npc.Center : Main.npc[target].Center);
                }
                ));

            // Flails let's go
            list.Add(new SummonedNPCItemUseProfile(
                "Thrown Flail",
                i => i.type == ItemID.Mace || i.type == ItemID.FlamingMace || i.type == ItemID.BallOHurt || i.type == ItemID.BlueMoon || i.type == ItemID.DaoofPow || i.type == ItemID.DripplerFlail || i.type == ItemID.FlowerPow || i.type == ItemID.Sunfury || i.type == ItemID.TheMeatball,
                npc =>
                {
                    SummonedNPC modNPC = (npc.ModNPC as SummonedNPC);
                    if (modNPC.itemUseStorage.Count == 0)
                    {
                        modNPC.itemUseStorage.Add(0);
                        modNPC.itemUseStorage.Add(0);
                        modNPC.itemUseStorage.Add(npc.Center);
                    }
                    int target = -1;
                    float closest = 1000000f;
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        // continue if invalid target
                        if (untargetable(Main.npc[i]) || i == npc.whoAmI) continue;

                        // find distance to potential target
                        float dist = Vector2.DistanceSquared(Main.npc[i].Center, npc.Center);

                        // if its closer than the current candidate and there's line of sight, change candidates.
                        if (dist < closest && Collision.CanHitLine(npc.Center, 1, 1, Main.npc[i].Center, 16, 16))
                        {
                            closest = dist;
                            target = i;
                        }
                    }

                    if (target == -1)
                    {
                        modNPC.itemUseStorage[0] = 0;
                        modNPC.itemUseStorage[1] = 0;
                        return new Tuple<bool, bool, Vector2>(false, false, (Vector2)modNPC.itemUseStorage[2]);
                    }

                    bool mouseDown = true;


                    int p = 0;
                    bool found_projectile = false;
                    for (; p < Main.projectile.Length; p++)
                    {
                        if (Main.projectile[p] == null || !Main.projectile[p].active || Main.projectile[p].owner != modNPC.dummyPlayer.whoAmI) continue;
                        int type = Main.projectile[p].type;
                        if (type != modNPC.myData.weapon.shoot) continue;
                        found_projectile = true;
                        break;
                    }

                    if (Vector2.DistanceSquared(Main.npc[target].Center, npc.Center) > 10000f)
                    {
                        modNPC.itemUseStorage[0] = (int)modNPC.itemUseStorage[0] + 1;

                        if ((int)modNPC.itemUseStorage[0] < 30)
                        {
                            mouseDown = true;
                        }
                        else
                        {
                            mouseDown = false;
                            if (found_projectile && Vector2.DistanceSquared(Main.npc[target].Center, Main.projectile[p].Center) < 128f)
                            {
                                mouseDown = true;
                            }
                        }

                    }
                    else
                    {
                        if ((int)modNPC.itemUseStorage[0] > 30 && found_projectile)
                        {
                            mouseDown = false;
                        }
                        else
                        {
                            modNPC.itemUseStorage[0] = 0;
                        }
                    }

                    modNPC.itemUseStorage[2] = Main.npc[target].Center;

                    return new Tuple<bool, bool, Vector2>(mouseDown, false, (Vector2)modNPC.itemUseStorage[2]);
                }
                ));

            // If an item places a tile, don't use it at all.
            list.Add(new SummonedNPCItemUseProfile(
                "Placeable Item",
                i => i.createTile != -1,
                npc => new Tuple<bool, bool, Vector2>(false, false, npc.Center)
                ));

            // sentries are only summoned every 2 seconds, but no consideration for line of sight is made
            list.Add(new SummonedNPCItemUseProfile(
                "Sentry",
                i => i.sentry,
                npc =>
                {
                    SummonedNPC modNPC = npc.ModNPC as SummonedNPC;

                    if (modNPC.itemUseStorage.Count == 0)
                        modNPC.itemUseStorage.Add(0);

                    if ((int)modNPC.itemUseStorage[0] > 0)
                        modNPC.itemUseStorage[0] = (int)modNPC.itemUseStorage[0] - 1;

                    int target = -1;

                    if ((int)modNPC.itemUseStorage[0] <= 0)
                    {
                        float closest = 1000000f;
                        for (int i = 0; i < Main.npc.Length; i++)
                        {
                            // continue if invalid target
                            if (untargetable(Main.npc[i]) || i == npc.whoAmI) continue;

                            // find distance to potential target
                            float dist = Vector2.DistanceSquared(Main.npc[i].Center, npc.Center);

                            // if its closer than the current candidate and there's line of sight, change candidates.
                            if (dist < closest)
                            {
                                closest = dist;
                                target = i;
                            }
                        }
                    }

                    if (target != -1)
                        modNPC.itemUseStorage[0] = 240;

                    return new Tuple<bool, bool, Vector2>(target != -1, false, (target == -1) ? npc.Center : Main.npc[target].Center);

                }
                ));

            // only summon a minion when first given the staff
            list.Add(new SummonedNPCItemUseProfile(
                "Minion Staff",
                i => i.CountsAsClass(DamageClass.Summon) && !i.CountsAsClass(DamageClass.SummonMeleeSpeed) && i.shoot > 0 && !i.sentry,
                npc =>
                {
                    SummonedNPC modNPC = npc.ModNPC as SummonedNPC;

                    if (modNPC.itemUseStorage.Count == 0)
                    {
                        modNPC.itemUseStorage.Add(true);
                        return new Tuple<bool, bool, Vector2>(true, false, npc.Center);
                    }
                    return new Tuple<bool, bool, Vector2>(false, false, npc.Center);
                }
                ));

            // Crimson Rod/ Nimbus Rod
            list.Add(new SummonedNPCItemUseProfile(
                "Magic Rain Weapon",
                i => i.type == ItemID.CrimsonRod || i.type == ItemID.NimbusRod,
                npc =>
                {
                    SummonedNPC modNPC = npc.ModNPC as SummonedNPC;
                    if (modNPC.itemUseStorage.Count == 0)
                        modNPC.itemUseStorage.Add(0);
                    int target = -1;

                    if ((int)modNPC.itemUseStorage[0] > 0)
                        modNPC.itemUseStorage[0] = (int)modNPC.itemUseStorage[0] - 1;

                    if ((int)modNPC.itemUseStorage[0] <= 0)
                    {
                        float closest = 1000000f;
                        for (int i = 0; i < Main.npc.Length; i++)
                        {
                            // continue if invalid target
                            if (untargetable(Main.npc[i]) || i == npc.whoAmI) continue;

                            // find distance to potential target
                            float dist = Vector2.DistanceSquared(Main.npc[i].Center, npc.Center);

                            // if its closer than the current candidate and there's line of sight, change candidates.
                            if (dist < closest && Collision.CanHitLine(npc.Center, 1, 1, Main.npc[i].Center, 16, 16))
                            {
                                closest = dist;
                                target = i;
                            }
                        }
                    }

                    if (target != -1)
                        modNPC.itemUseStorage[0] = 120;

                    return new Tuple<bool, bool, Vector2>(target != -1, false, (target == -1) ? npc.Center : (Main.npc[target].Center + new Vector2(Main.npc[target].velocity.X * 25f, -250f)));
                }
                ));

            // Vilethorn-esque Magic weapons
            list.Add(new SummonedNPCItemUseProfile(
                "Vilethorn Esque",
                i => i.type == ItemID.Vilethorn || i.type == ItemID.CrystalVileShard || i.type == ItemID.NettleBurst,
                npc => {
                    int target = -1;
                    float closest = 250000f;
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        // continue if invalid target
                        if (untargetable(Main.npc[i]) || i == npc.whoAmI) continue;

                        // find distance to potential target
                        float dist = Vector2.DistanceSquared(Main.npc[i].Center, npc.Center);

                        // if its closer than the current candidate and there's line of sight, change candidates.
                        if (dist < closest)
                        {
                            closest = dist;
                            target = i;
                        }
                    }
                    return new Tuple<bool, bool, Vector2>(target != -1, false, (target == -1) ? npc.Center : Main.npc[target].Center);
                }
                ));
            // Weapons which shoot something from the top of the screen
            list.Add(new SummonedNPCItemUseProfile(
                "Showering Weapons",
                i => i.type == ItemID.Starfury || i.type == ItemID.BloodRainBow || i.type == ItemID.DaedalusStormbow || i.type == ItemID.LunarFlareBook || i.type == ItemID.StarWrath || i.type == ItemID.BlizzardStaff || i.type == ItemID.MeteorStaff,
                npc => {
                    int target = -1;
                    float closest = 1000000f;
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        // continue if invalid target
                        if (untargetable(Main.npc[i]) || i == npc.whoAmI) continue;

                        // find distance to potential target
                        float dist = Vector2.DistanceSquared(Main.npc[i].Center, npc.Center);

                        // if its closer than the current candidate and there's line of sight, change candidates.
                        if (dist < closest && (Collision.CanHitLine(npc.Center + new Vector2(0f, -500f), 1, 1, Main.npc[i].Center, 16, 16)))
                        {
                            closest = dist;
                            target = i;
                        }
                    }
                    return new Tuple<bool, bool, Vector2>(target != -1, false, (target == -1) ? npc.Center : Main.npc[target].Center);
                }
                ));

            return list;

        }

    }
}
