using CsvHelper.TypeConversion;
using InfiniteNPC.NPCs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfiniteNPC
{
	public class InfiniteNPC : Mod
	{
		public static InfiniteNPC Instance => ModContent.GetInstance<InfiniteNPC>();
        /// <summary>
        /// An ordered database of profiles.
        /// </summary>
        public static List<SummonedNPCItemUseProfile> ProfileDatabase = new List<SummonedNPCItemUseProfile>();
        /// <summary>
        /// A dictionary which takes the ItemID as the key and returns the index of the associated profile in ProfileDatabase
        /// </summary>
        public static Dictionary<int, int> ItemIDAssignments = new Dictionary<int, int>();

        /// <summary>
        /// For all items where <paramref name="shouldRegisterItem"/> is true, register them to use <paramref name="runtimeMouseFunction"/> for the <see cref="SummonedNPC"/>'s AI.
        /// </summary>
        /// <param name="shouldRegisterItem">The condition to register this item under the given <paramref name="runtimeMouseFunction"/></param>
        /// <param name="runtimeMouseFunction">A function which, given the <see cref="SummonedNPC"/>, returns a <see cref="Tuple{bool,bool,Vector2}"/>,
        /// whose items represent the status of the fake left mouse button, the status of the fake right mouse button, and the world position of the fake mouse respectively.</param>
        /// <returns>The index of the newly created <see cref="SummonedNPCItemUseProfile"/> within <see cref="InfiniteNPC.ProfileDatabase"/></returns>
        public static int RegisterNewItemProfile(string profileName, Predicate<Item> shouldRegisterItem, Func<NPC, Tuple<bool, bool, Vector2>> runtimeMouseFunction)
        {
            if (shouldRegisterItem == null) return -1;
            if (runtimeMouseFunction == null) return -1;

            SummonedNPCItemUseProfile newProfile = new(profileName, shouldRegisterItem, runtimeMouseFunction);
            int databaseIndex = ProfileDatabase.Count;
            ProfileDatabase.Add(newProfile);
            MakeItemIDAssignments(databaseIndex, newProfile);
            return databaseIndex;
        }
        /// <summary>
        /// Replace the Item's
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="runtimeMouseFunction"></param>
        /// <returns></returns>
        public static int OverrideProfileForItem(int itemID, string profileName, Func<NPC, Tuple<bool,bool,Vector2>> runtimeMouseFunction)
        {
            if (itemID < 0 || itemID >= ItemLoader.ItemCount) return -1;

            if (!ItemIDAssignments.TryGetValue(itemID, out int assignment)) return RegisterNewItemProfile(profileName, i => i.type == itemID, runtimeMouseFunction);

            int itemsAssignedToProfile = 0;

            ItemIDAssignments.Values.ToList().ForEach(i => { itemsAssignedToProfile += assignment == i ? 1 : 0; });

            if (itemsAssignedToProfile == 1)
            {
                ProfileDatabase[assignment] = new SummonedNPCItemUseProfile(profileName, i => i.type == itemID, runtimeMouseFunction);
                return assignment;

            } else
            {
                ProfileDatabase.Add(new SummonedNPCItemUseProfile(profileName, i => i.type == itemID, runtimeMouseFunction));
                ItemIDAssignments[itemID] = ProfileDatabase.Count - 1;
                return ProfileDatabase.Count - 1;
            }
        }

        public static bool OverrideProfileUsedByItem(int itemID, string profileName, Func<NPC, Tuple<bool, bool, Vector2>> runtimeMouseFunction)
        {
            if (itemID < 0 || itemID >= ItemLoader.ItemCount) return false;

            if (!ItemIDAssignments.TryGetValue(itemID, out int assignment)) return RegisterNewItemProfile(profileName, i => i.type == itemID, runtimeMouseFunction) != -1;

            ProfileDatabase[assignment] = new SummonedNPCItemUseProfile(profileName, ProfileDatabase[assignment].RegisterThisItemUnderThisProfile, runtimeMouseFunction);

            return true;
        }

        public static void RevertItemIDToBase(int itemID)
        {
            throw new NotImplementedException();
        }
        internal static void MakeItemIDAssignments(SummonedNPCItemUseProfile profile) => MakeItemIDAssignments(ProfileDatabase.IndexOf(profile), profile);
        internal static void MakeItemIDAssignments(int databaseIndex, SummonedNPCItemUseProfile profile)
        {
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                AssignItemIDIfPassed(i, databaseIndex, profile);
            }
        }

        internal static bool AssignItemIDIfPassed(int itemID, SummonedNPCItemUseProfile profile) => AssignItemIDIfPassed(itemID, ProfileDatabase.IndexOf(profile), profile);
        internal static bool AssignItemIDIfPassed(int itemID, int databaseIndex, SummonedNPCItemUseProfile profile)
        {
            Item genericItem = new Item(itemID);
            if (profile.RegisterThisItemUnderThisProfile(genericItem))
            {
                if (ItemIDAssignments.ContainsKey(itemID))
                    ItemIDAssignments[itemID] = databaseIndex;
                else ItemIDAssignments.Add(itemID, databaseIndex);
                return true;
            }
            return false;
        }

        public static void ConstructProfileDatabase()
        {
            ProfileDatabase.Clear();
            ItemIDAssignments.Clear();
            ProfileDatabase = SummonedNPCItemUseProfile.GetBaseProfileList();
            int i = 0;
            ProfileDatabase.ForEach(profile => { MakeItemIDAssignments(i,profile); i++; });
        }

        
        public override object Call(params object[] args)
        {
            if (args.Length == 0) return null;
            switch (args[0])
            {
                case "SummonedNPC dummyPlayer":
                    {
                        if (args.Length == 1) return null;
                        if (args[1].GetType() != typeof(int)) return null;
                        int npc = (int)args[1];
                        if (Main.npc[npc] == null || !Main.npc[npc].active || Main.npc[npc].type != ModContent.NPCType<SummonedNPC>()) return null;
                        return (Main.npc[npc].ModNPC as SummonedNPC).dummyPlayer;
                    }

                case "SummonedNPC itemUseStorage":
                    {
                        if (args.Length == 1) return null;
                        if (args[1].GetType() != typeof(int)) return null;
                        int npc = (int)args[1];
                        if (Main.npc[npc] == null || !Main.npc[npc].active || Main.npc[npc].type != ModContent.NPCType<SummonedNPC>()) return null;
                        return (Main.npc[npc].ModNPC as SummonedNPC).itemUseStorage;
                    }

                case "Register New Item Use Profile":
                    {
                        if (args.Length <= 3) return null;
                        if (args[1] as string == null) return null;
                        if (args[2] as Predicate<Item> == null) return null;
                        if (args[3] as Func<NPC, Tuple<bool, bool, Vector2>> == null) return null;
                        return RegisterNewItemProfile((string)args[1], (Predicate<Item>)args[2], (Func<NPC, Tuple<bool, bool, Vector2>>)args[3]);
                    }
                case "Override Item Use Profile For Item Type Only":
                    {
                        if (args.Length <= 3) return null;
                        if (args[1].GetType() != typeof(int)) return null;
                        if (args[2] as string == null) return null;
                        if (args[3] as Func<NPC, Tuple<bool, bool, Vector2>> == null) return null;
                        return OverrideProfileForItem((int)args[1], (string)args[2], (Func<NPC, Tuple<bool, bool, Vector2>>)args[3]);
                    }
                case "Override Item Use Profile Used By Item Type":
                    {
                        if (args.Length <= 3) return null;
                        if (args[1].GetType() != typeof(int)) return null;
                        if (args[2] as string == null) return null;
                        if (args[3] as Func<NPC, Tuple<bool, bool, Vector2>> == null) return null;
                        return OverrideProfileUsedByItem((int)args[1], (string)args[2], (Func<NPC, Tuple<bool, bool, Vector2>>)args[3]);
                    }
                case "Revert Item Profile Assignment To Initial Construction":
                    {
                        if (args.Length == 1) return null;
                        if (args[1].GetType() != typeof(int)) return null;



                        break;
                    }
                case "Get Item Profile Assignment":

                    break;

            }

            return null; // null is the most "nothing" of all the possible returns
        }
        public override void PostSetupContent()
        {
            ConstructProfileDatabase();
        }
    }
}