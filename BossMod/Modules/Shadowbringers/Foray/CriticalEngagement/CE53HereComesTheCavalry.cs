using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE53HereComesTheCavalry
{
    public enum OID : uint
    {
        Boss = 0x31C7, // R7.200, x1
        Helper = 0x233C, // R0.500, x4
        ImperialAssaultCraft = 0x2EE8, // R0.500, x22, also helper?
        Cavalry = 0x31C6, // R4.000, x9, and more spawn during fight
        FireShot = 0x1EB1D3, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 6497, // Cavalry/Boss->player, no cast, single-target
        KillZone = 24700, // ImperialAssaultCraft->self, no cast, range 25-30 donut deathwall

        StormSlash = 23931, // Cavalry->self, 5.0s cast, range 8 120-degree cone
        MagitekBurst = 23932, // Cavalry->location, 5.0s cast, range 8 circle
        BurnishedJoust = 23936, // Cavalry->location, 3.0s cast, width 6 rect charge

        GustSlash = 23933, // Boss->self, 7.0s cast, range 60 ?-degree cone visual
        GustSlashAOE = 23934, // Helper->self, 8.0s cast, ???, knockback 'forward' 35
        CallFireShot = 23935, // Boss->self, 3.0s cast, single-target, visual
        FireShot = 23937, // ImperialAssaultCraft->location, 3.0s cast, range 6 circle
        Burn = 23938, // ImperialAssaultCraft->self, no cast, range 6 circle
        CallStrategicRaid = 24578, // Boss->self, 3.0s cast, single-target, visual
        AirborneExplosion = 24872, // ImperialAssaultCraft->location, 9.0s cast, range 10 circle
        RideDown = 23939, // Boss->self, 6.0s cast, range 60 width 60 rect visual
        RideDownAOE = 23940, // Helper->self, 6.5s cast, ???, knockback side 12
        CallRaze = 23948, // Boss->self, 3.0s cast, single-target, visual
        Raze = 23949, // ImperialAssaultCraft->location, no cast, ???, raidwide?
        RawSteel = 23943, // Boss->player, 5.0s cast, width 4 rect charge cleaving tankbuster
        CloseQuarters = 23944, // Boss->self, 5.0s cast, single-target
        CloseQuartersAOE = 23945, // Helper->self, 5.0s cast, range 15 circle
        FarAfield = 23946, // Boss->self, 5,0s cast, single-target
        FarAfield2 = 23947, // Helper->self, 5,0s cast, range 10-30 donut
        CallControlledBurn = 23950, // Boss->self, 5.0s cast, single-target, visual (spread)
        CallControlledBurnAOE = 23951, // ImperialAssaultCraft->players, 5.0s cast, range 6 circle spread
        MagitekBlaster = 23952, // Boss->players, 5.0s cast, range 8 circle stack
    };

    public enum TetherID : uint
    {
        Tankbuster = 57, // Boss->player
    };

    class StormSlash : Components.SelfTargetedAOEs
    {
        public StormSlash() : base(ActionID.MakeSpell(AID.StormSlash), new AOEShapeCone(8, 60.Degrees())) { }
    }

    class MagitekBurst : Components.LocationTargetedAOEs
    {
        public MagitekBurst() : base(ActionID.MakeSpell(AID.MagitekBurst), 8) { }
    }

    class BurnishedJoust : Components.ChargeAOEs
    {
        public BurnishedJoust() : base(ActionID.MakeSpell(AID.BurnishedJoust), 3) { }
    }

    // note: there are two casters, probably to avoid 32-target limit - we only want to show one
    class GustSlash : Components.KnockbackFromCastTarget
    {
        public GustSlash() : base(ActionID.MakeSpell(AID.GustSlashAOE), 35, true, 1, null, Kind.DirForward) { }
    }

    class FireShot : Components.PersistentVoidzoneAtCastTarget
    {
        public FireShot() : base(6, ActionID.MakeSpell(AID.FireShot), m => m.Enemies(OID.FireShot).Where(e => e.EventState != 7), 0) { }
    }

    class AirborneExplosion : Components.LocationTargetedAOEs
    {
        public AirborneExplosion() : base(ActionID.MakeSpell(AID.AirborneExplosion), 10) { }
    }

    class RideDownAOE : Components.SelfTargetedAOEs
    {
        public RideDownAOE() : base(ActionID.MakeSpell(AID.RideDown), new AOEShapeRect(60, 5)) { }
    }

    // note: there are two casters, probably to avoid 32-target limit - we only want to show one
    // TODO: generalize to reusable component
    class RideDownKnockback : Components.Knockback
    {
        private List<Source> _sources = new();
        private static AOEShapeCone _shape = new(30, 90.Degrees());

        public RideDownKnockback() : base(ActionID.MakeSpell(AID.RideDownAOE), false, 1) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => _sources;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _sources.Clear();
                // charge always happens through center, so create two sources with origin at center looking orthogonally
                _sources.Add(new(module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
                _sources.Add(new(module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _sources.Clear();
        }
    }

    class RawSteel : Components.BaitAwayChargeCast //TODO: find out optimal distance, test results so far: distance ~6.4 (inside hitbox) and 1 vuln stack: 79194 damage, at distance ~22.2 and 4 vuln stacks i took 21083 damage, since hitbox is 7.2 it is probably starting to be optimal around distance 15
    {
        private ulong target;
        private DateTime _activation;
        public RawSteel() : base(ActionID.MakeSpell(AID.RawSteel), 2) { }
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var bait in ActiveBaitsOn(pc))
            {
                if ((module.PrimaryActor.Position - pc.Position).Length() < 15)
                    bait.Shape.Outline(arena, BaitOrigin(bait), bait.Rotation);
                if ((module.PrimaryActor.Position - pc.Position).Length() > 15)
                    bait.Shape.Outline(arena, BaitOrigin(bait), bait.Rotation, ArenaColor.Safe);
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
                if (target == actor.InstanceID && (module.PrimaryActor.Position - actor.Position).Length() < 15)
                    hints.Add("Go further away from boss!");
                if (target == actor.InstanceID && (module.PrimaryActor.Position - actor.Position).Length() > 15)
                    hints.Add("Good!", false);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            base.OnTethered(module, source, tether);
            if (tether.ID == (uint)TetherID.Tankbuster)
            {
                target = tether.Target;
                _activation = module.WorldState.CurrentTime.AddSeconds(5);
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if (target == actor.InstanceID && CurrentBaits.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 15));
            if (CurrentBaits.Count > 0)
                hints.PredictedDamage.Add((module.Raid.WithSlot(true).WhereActor(p => p.InstanceID == target).Mask(), _activation));
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (CurrentBaits.Count > 0)
                hints.Add("Proximity Tankbuster + Charge");
        }
    }

    class CallRaze : Components.RaidwideCast
    {
        public CallRaze() : base(ActionID.MakeSpell(AID.CallRaze), "Multi raidwide") { }
    }

    class CloseQuarters : Components.SelfTargetedAOEs
    {
        public CloseQuarters() : base(ActionID.MakeSpell(AID.CloseQuartersAOE), new AOEShapeCircle(15)) { }
    }

    class CallControlledBurn : Components.SpreadFromCastTargets
    {
        public CallControlledBurn() : base(ActionID.MakeSpell(AID.CallControlledBurnAOE), 6) { }
    }

    class MagitekBlaster : Components.StackWithCastTargets
    {
        public MagitekBlaster() : base(ActionID.MakeSpell(AID.MagitekBlaster), 8) { }
    }

    class FarAfield : Components.SelfTargetedAOEs
    {
        public FarAfield() : base(ActionID.MakeSpell(AID.FarAfield2), new AOEShapeDonut(10, 30)) { }
    }
    
    class CE53HereComesTheCavalryStates : StateMachineBuilder
    {
        public CE53HereComesTheCavalryStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<StormSlash>()
                .ActivateOnEnter<MagitekBurst>()
                .ActivateOnEnter<BurnishedJoust>()
                .ActivateOnEnter<GustSlash>()
                .ActivateOnEnter<FireShot>()
                .ActivateOnEnter<AirborneExplosion>()
                .ActivateOnEnter<RideDownAOE>()
                .ActivateOnEnter<RideDownKnockback>()
                .ActivateOnEnter<CallRaze>()
                .ActivateOnEnter<RawSteel>()
                .ActivateOnEnter<CloseQuarters>()
                .ActivateOnEnter<CallControlledBurn>()
                .ActivateOnEnter<MagitekBlaster>()
                .ActivateOnEnter<FarAfield>();
        }
    }

    [ModuleInfo(CFCID = 778, DynamicEventID = 22)]
    public class CE53HereComesTheCavalry : BossModule
    {
        public CE53HereComesTheCavalry(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-750, 790), 25)) { }

        protected override bool CheckPull() => PrimaryActor.InCombat; // not targetable at start

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            base.DrawEnemies(pcSlot, pc);
            Arena.Actors(Enemies(OID.Cavalry), ArenaColor.Enemy);
        }
    }
}
