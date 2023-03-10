using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;

namespace InfiniteNPC.NPCs
{
    public class SummonedNPCOrder
    {
        public int OrderFromTeam
        {
            get;
            private set;
        } = -2;
        public Point OrderTile
        {
            get;
            private set;
        }
        public int ElapsedTicks
        {
            get;
            private set;
        } = 0;
        public int Lifetime
        {
            get;
            private set;
        }
        public enum SummonedNPCOrderType : byte
        {
            Wander,
            AskForHome,
            AskForItem,
            AskForCash,
            GuardArea,
            GuardEntity,
            Adventure,
            PvMe
        }
        public SummonedNPCOrderType OrderType
        {
            get;
            private set;
        } = SummonedNPCOrderType.Wander;

        public TagCompound SaveIO()
        {
            TagCompound savedata = new TagCompound();
            savedata.Add("OrderFromTeam", OrderFromTeam);
            savedata.Add("OrderTileX", OrderTile.X);
            savedata.Add("OrderTileY", OrderTile.Y);
            savedata.Add("ElapsedTicks", ElapsedTicks);
            savedata.Add("Lifetime", Lifetime);
            savedata.Add("OrderType", (byte)OrderType);
            return savedata;
        }
        public SummonedNPCOrder(int orderFromTeam, Point orderTile, int elapsedTicks, int lifetime, SummonedNPCOrderType orderType)
        {
            OrderFromTeam = orderFromTeam;
            OrderTile = orderTile;
            ElapsedTicks = elapsedTicks;
            Lifetime = lifetime;
            OrderType = orderType;
        }
        public SummonedNPCOrder(TagCompound savedata)
        {
            if (savedata.TryGet<int>("OrderFromTeam", out int orderFromTeam)) OrderFromTeam = orderFromTeam;
            if (savedata.TryGet("OrderTileX", out int x) && savedata.TryGet("OrderTileY", out int y)) OrderTile = new Point(x, y);
            if (savedata.TryGet<int>("ElapsedTicks", out int elapsedTicks)) ElapsedTicks = elapsedTicks;
            if (savedata.TryGet<int>("Lifetime", out int lifetime)) Lifetime = lifetime;
            if (savedata.TryGet<byte>("OrderType", out byte orderType)) OrderType = (SummonedNPCOrderType)orderType;
        }
    }
}
