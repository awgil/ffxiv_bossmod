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
        //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
        //_Gen_Actor1eb174 = 0x1EB174, // R0.500, x0, EventObj type, and more spawn during fight
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
        // TODO: far
        MagitekBlaster = 23952, // Boss->players, 5.0s cast, range 8 circle stack
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

    // TODO: generalize to reusable component
    class GustSlash : Components.Knockback
    {
        private Actor? _source; // note that there are two casters, probably to avoid 32-target limit

        public GustSlash() : base(35, ActionID.MakeSpell(AID.GustSlashAOE), true) { } // TODO: does it really ignore immunes?..

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_source != null && !IsImmune(slot) && !module.Bounds.Contains(AdjustedPosition(_source, actor)))
                hints.Add("About to be knocked into wall!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_source != null && !IsImmune(pcSlot))
                DrawKnockback(pc, AdjustedPosition(_source, pc), arena);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _source = caster;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (_source == caster)
                _source = null;
        }

        private WPos AdjustedPosition(Actor source, Actor target) => target.Position + Distance * source.CastInfo!.Rotation.ToDirection();
    }

    class FireShot : Components.PersistentVoidzoneAtCastTarget
    {
        public FireShot() : base(6, ActionID.MakeSpell(AID.FireShot), m => m.Enemies(OID.FireShot).Where(e => e.EventState != 7), 0) { }
    }

    class AirborneExplosion : Components.LocationTargetedAOEs
    {
        public AirborneExplosion() : base(ActionID.MakeSpell(AID.AirborneExplosion), 10) { }
    }

    // TODO: show aoe near center (width?)
    // TODO: generalize to reusable component
    class RideDown : Components.Knockback
    {
        private Actor? _source; // note that there are two casters, probably to avoid 32-target limit

        public RideDown() : base(12, ActionID.MakeSpell(AID.RideDownAOE), true) { } // TODO: does it really ignore immunes?..

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_source != null && !IsImmune(slot) && !module.Bounds.Contains(AdjustedPosition(_source, actor)))
                hints.Add("About to be knocked into wall!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_source != null && !IsImmune(pcSlot))
                DrawKnockback(pc, AdjustedPosition(_source, pc), arena);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _source = caster;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (_source == caster)
                _source = null;
        }

        private WPos AdjustedPosition(Actor source, Actor target)
        {
            var dir = source.CastInfo!.Rotation.ToDirection();
            var offset = target.Position - source.Position;
            var ortho = offset - dir * dir.Dot(offset);
            return target.Position + Distance * ortho.Normalized();
        }
    }

    class CallRaze : Components.RaidwideCast
    {
        public CallRaze() : base(ActionID.MakeSpell(AID.CallRaze), "Multi raidwide") { }
    }

    // TODO: cleave, distance/invuln hint
    class RawSteel : Components.SingleTargetCast
    {
        public RawSteel() : base(ActionID.MakeSpell(AID.CallRaze), "Proximity tankbuster") { }
    }

    class CloseQuarters : Components.SelfTargetedAOEs
    {
        public CloseQuarters() : base(ActionID.MakeSpell(AID.CloseQuartersAOE), new AOEShapeCircle(15)) { }
    }

    // TODO: far afield
    // TODO: magitek blaster (generic outdoor stack component)
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
                .ActivateOnEnter<RideDown>()
                .ActivateOnEnter<CallRaze>()
                .ActivateOnEnter<RawSteel>()
                .ActivateOnEnter<CloseQuarters>();
        }
    }

    public class CE53HereComesTheCavalry : BossModule
    {
        public CE53HereComesTheCavalry(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-750, 790), 25)) { }
        protected override bool CheckPull() => PrimaryActor.InCombat; // not targetable at start
    }
}
