using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.RealmReborn.Raid.T01Caduceus
{
    public enum OID : uint
    {
        Boss = 0x7D7, // x1, and more spawn during fight
        Helper = 0x1B2, // x1
        DarkMatterSlime = 0x7D8, // spawn during fight
        Platform = 0x1E8729, // x13
        Regorge = 0x1E8B20, // EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttackBoss = 1207, // Boss->self, no cast, range 6+R ?-degree cone
        HoodSwing = 1208, // Boss->self, no cast, range 8+R ?-degree cone cleave
        WhipBack = 1209, // Boss->self, 2.0s cast, range 6+R 120-degree cone (baited backward cleave)
        Regorge = 1210, // Boss->location, no cast, range 4 circle aoe that leaves voidzone
        SteelScales = 1211, // Boss->self, no cast, single-target, damage up buff stack

        PlatformExplosion = 674, // Helper->self, no cast, hits players on glowing platforms and spawns dark matter slime on them
        AutoAttackSlime = 872, // DarkMatterSlime->player, no cast, single-target
        Rupture = 1213, // DarkMatterSlime->self, no cast, range 16+R circle aoe suicide (damage depends on cur hp?)
        Devour = 1454, // Boss->DarkMatterSlime, no cast, single-target visual
    };

    // TODO: regorge: cast -> ~2.1s later voidzone appears -> ~15s later it disappears due to eventstate=7 -> ~3.2s later it is destroyed
    // TODO: merge: happens if bosses are 'close enough' and more than 20s passed since split

    //class XXX : Components.SelfTargetedAOEs
    //{
    //    public XXX() : base(ActionID.MakeSpell(AID.XXX), new AOEShapeRect(40, 2)) { }
    //}

    class Platforms : BossComponent
    {
        public const float HexaPlatformSide = 9;
        public const float OctoPlatformLong = 13;
        public const float OctoPlatformShort = 7;
        public const float HexaCenterToSideCornerX = HexaPlatformSide * 0.8660254f; // sqrt(3) / 2
        public const float HexaCenterToSideCornerZ = HexaPlatformSide * 0.5f;
        public const float HexaNeighbourDistX = HexaCenterToSideCornerX * 2;
        public const float HexaNeighbourDistZ = HexaPlatformSide * 1.5f;

        public static WPos ClosestPlatformCenter = new(0.6f, -374); // (0,0) on hexa grid
        public static (int, int)[] HexaPlatforms = { (0, 0), (0, 1), (1, 1), (0, 2), (1, 2), (2, 2), (3, 2), (0, 3), (1, 3), (2, 3), (1, 4), (2, 4) };
        public static WPos[] HexaPlatformCenters = HexaPlatforms.Select(HexaCenter).ToArray();
        public static WDir OctoCenterOffset = 0.5f * new WDir(OctoPlatformShort, OctoPlatformLong - HexaPlatformSide);
        public static WPos OctaPlatformCenter = HexaCenter((3, 4)) - OctoCenterOffset;

        private static WPos HexaCenter((int x, int y) c) => ClosestPlatformCenter - new WDir(c.x * HexaNeighbourDistX + ((c.y & 1) != 0 ? HexaCenterToSideCornerX : 0), c.y * HexaNeighbourDistZ);
        private static IEnumerable<WPos> HexaPoly(WPos center)
        {
            yield return center + new WDir(HexaCenterToSideCornerX, -HexaCenterToSideCornerZ);
            yield return center + new WDir(HexaCenterToSideCornerX, HexaCenterToSideCornerZ);
            yield return center + new WDir(0, HexaPlatformSide);
            yield return center - new WDir(HexaCenterToSideCornerX, -HexaCenterToSideCornerZ);
            yield return center - new WDir(HexaCenterToSideCornerX, HexaCenterToSideCornerZ);
            yield return center - new WDir(0, HexaPlatformSide);
        }
        private static IEnumerable<WPos> OctaPoly()
        {
            yield return OctaPlatformCenter + new WDir(OctoCenterOffset.X, -OctoCenterOffset.Z - HexaPlatformSide);
            yield return OctaPlatformCenter + new WDir(OctoCenterOffset.X + HexaCenterToSideCornerX, -OctoCenterOffset.Z - HexaCenterToSideCornerZ);
            yield return OctaPlatformCenter + new WDir(OctoCenterOffset.X + HexaCenterToSideCornerX, +OctoCenterOffset.Z + HexaCenterToSideCornerZ);
            yield return OctaPlatformCenter + new WDir(OctoCenterOffset.X, +OctoCenterOffset.Z + HexaPlatformSide);
            yield return OctaPlatformCenter - new WDir(OctoCenterOffset.X, -OctoCenterOffset.Z - HexaPlatformSide);
            yield return OctaPlatformCenter - new WDir(OctoCenterOffset.X + HexaCenterToSideCornerX, -OctoCenterOffset.Z - HexaCenterToSideCornerZ);
            yield return OctaPlatformCenter - new WDir(OctoCenterOffset.X + HexaCenterToSideCornerX, +OctoCenterOffset.Z + HexaCenterToSideCornerZ);
            yield return OctaPlatformCenter - new WDir(OctoCenterOffset.X, +OctoCenterOffset.Z + HexaPlatformSide);
        }

        private BitMask _activePlatforms;

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            int i = 0;
            foreach (var p in HexaPlatformCenters)
                arena.AddPolygon(HexaPoly(p), _activePlatforms[i++] ? ArenaColor.Enemy : ArenaColor.Border);
            arena.AddPolygon(OctaPoly(), _activePlatforms[i++] ? ArenaColor.Enemy : ArenaColor.Border);
        }

        public override void OnActorEState(BossModule module, Actor actor, ushort state)
        {
            if (actor.OID == (uint)OID.Platform)
            {
                int i = Array.FindIndex(HexaPlatformCenters, c => actor.Position.InCircle(c, 2));
                if (i == -1)
                    i = HexaPlatformCenters.Length;
                _activePlatforms[i] = state == 2;
            }
        }
    }

    class T01CaduceusStates : StateMachineBuilder
    {
        public T01CaduceusStates(BossModule module) : base(module)
        {
            TrivialPhase();
                //.ActivateOnEnter<Platforms>();
        }
    }

    public class T01Caduceus : BossModule
    {
        public T01Caduceus(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-26, -407), 35, 43))
        {
            ActivateComponent<Platforms>();
        }

        // don't activate module created for clone (this is a hack...)
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat && PrimaryActor.HP.Cur > PrimaryActor.HP.Max / 2; }
    }
}
