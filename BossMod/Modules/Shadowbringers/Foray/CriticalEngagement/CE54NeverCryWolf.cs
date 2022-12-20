using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE54NeverCryWolf
{
    public enum OID : uint
    {
        Boss = 0x319C, // R9.996, x1
        Helper = 0x233C, // R0.500, x18
        IceSprite = 0x319D, // R0.800, spawn during fight
        Icicle = 0x319E, // R3.000, spawn during fight
        Imaginifer = 0x319F, // R0.500, spawn during fight
        //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
        //_Gen_Actor1eb177 = 0x1EB177, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 6499, // Boss->player, no cast, single-target

        IcePillar = 23581, // Boss->self, 3.0s cast, single-target, viusal
        IcePillarAOE = 23582, // Icicle->self, 3.0s cast, range 4 circle aoe (pillar drop)
        PillarPierce = 23583, // Icicle->self, 3.0s cast, range 80 width 4 rect aoe (pillar fall)
        Shatter = 23584, // Icicle->self, 3.0s cast, range 8 circle aoe (pillar explosion after lunar cry)
        Tramontane = 23585, // Boss->self, 3.0s cast, single-target, visual
        BracingWind = 23586, // IceSprite->self, 9.0s cast, range 60 width 12 rect, visual
        BracingWindAOE = 24787, // Helper->self, no cast, range 60 width 12 rect knock-forward 40
        LunarCry = 23588, // Boss->self, 14.0s cast, range 80 circle LOSable aoe

        ThermalGust = 23589, // Imaginifer->self, 2.0s cast, range 60 width 4 rect aoe (when adds appear)
        GlaciationEnrage = 22881, // Boss->self, 20.0s cast, single-target, visual
        GlaciationEnrageAOE = 23625, // Helper->self, no cast, ???, raidwide (deadly if adds aren't killed)
        AgeOfEndlessFrostFirst = 23590, // Boss->self, 5.0s cast, single-target, visual
        AgeOfEndlessFrostFirstAOE = 23592, // Helper->self, 5.0s cast, range 40 ?-degree cone
        AgeOfEndlessFrostRest = 22883, // Boss->self, no cast, single-target
        AgeOfEndlessFrostRestAOE = 23593, // Helper->self, 0.5s cast, range 40 ?-degree cone

        StormWithout = 23594, // Boss->self, 5.0s cast, single-target
        StormWithoutAOE = 23595, // Helper->self, 5.0s cast, range 10-40 donut
        StormWithin = 23596, // Boss->self, 5.0s cast, single-target
        StormWithinAOE = 23597, // Helper->self, 5.0s cast, range 10 circle
        AncientGlacier = 23600, // Boss->self, 3.0s cast, single-target, visual
        AncientGlacierAOE = 23601, // Helper->location, 3.0s cast, range 6 circle puddle
        Glaciation = 23602, // Boss->self, 5.0s cast, single-target, visual
        GlaciationAOE = 23603, // Helper->self, 5.6s cast, ???, raidwide

        TeleportBoss = 23621, // Boss->location, no cast, teleport
        TeleportImaginifer = 23622, // Imaginifer->location, no cast, ???, teleport
        ActivateImaginifer = 23623, // Imaginifer->self, no cast, single-target, visual
    };

    class IcePillar : Components.SelfTargetedAOEs
    {
        public IcePillar() : base(ActionID.MakeSpell(AID.IcePillarAOE), new AOEShapeCircle(4)) { }
    }

    class PillarPierce : Components.SelfTargetedAOEs
    {
        public PillarPierce() : base(ActionID.MakeSpell(AID.PillarPierce), new AOEShapeRect(80, 2)) { }
    }

    class Shatter : Components.SelfTargetedAOEs
    {
        public Shatter() : base(ActionID.MakeSpell(AID.Shatter), new AOEShapeCircle(8)) { }
    }

    // TODO: generalize
    class BracingWind : Components.Knockback
    {
        private List<Actor> _sources = new();
        private static AOEShapeRect _shape = new(60, 6);

        public BracingWind() : base(40, ActionID.MakeSpell(AID.BracingWind), true) { } // TODO: does it really ignore immunes?..

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!IsImmune(slot) && !module.Bounds.Contains(AdjustedPosition(actor)))
                hints.Add("About to be knocked into wall!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!IsImmune(pcSlot))
                DrawKnockback(pc, AdjustedPosition(pc), arena);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _sources.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _sources.Remove(caster);
        }

        private WPos AdjustedPosition(Actor target)
        {
            var source = _sources.Find(a => _shape.Check(target.Position, a));
            var dir = source?.CastInfo?.Rotation.ToDirection() ?? new();
            return target.Position + Distance * dir;
        }
    }

    class LunarCry : Components.CastLineOfSightAOE
    {
        private HashSet<ulong> _badPillars = new();

        public LunarCry() : base(ActionID.MakeSpell(AID.LunarCry), 80, false) { }
        public override IEnumerable<Actor> BlockerActors(BossModule module) => module.Enemies(OID.Icicle).Where(a => !_badPillars.Contains(a.InstanceID));

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.PillarPierce)
                _badPillars.Add(caster.InstanceID);
        }
    }

    class ThermalGust : Components.SelfTargetedAOEs
    {
        public ThermalGust() : base(ActionID.MakeSpell(AID.ThermalGust), new AOEShapeRect(60, 2)) { }
    }

    class StormWithout : Components.SelfTargetedAOEs
    {
        public StormWithout() : base(ActionID.MakeSpell(AID.StormWithout), new AOEShapeDonut(10, 40)) { }
    }

    class StormWithin : Components.SelfTargetedAOEs
    {
        public StormWithin() : base(ActionID.MakeSpell(AID.StormWithin), new AOEShapeCircle(10)) { }
    }

    class AncientGlacier : Components.LocationTargetedAOEs
    {
        public AncientGlacier() : base(ActionID.MakeSpell(AID.AncientGlacierAOE), 6) { }
    }

    class Glaciation : Components.RaidwideCast
    {
        public Glaciation() : base(ActionID.MakeSpell(AID.Glaciation)) { }
    }

    // TODO: age of endless frost (cone angle, rotation direction...)
    class CE54NeverCryWolfStates : StateMachineBuilder
    {
        public CE54NeverCryWolfStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<IcePillar>()
                .ActivateOnEnter<PillarPierce>()
                .ActivateOnEnter<Shatter>()
                .ActivateOnEnter<BracingWind>()
                .ActivateOnEnter<LunarCry>()
                .ActivateOnEnter<ThermalGust>()
                .ActivateOnEnter<StormWithout>()
                .ActivateOnEnter<StormWithin>()
                .ActivateOnEnter<AncientGlacier>()
                .ActivateOnEnter<Glaciation>();
        }
    }

    public class CE54NeverCryWolf : BossModule
    {
        private List<Actor> _adds = new();

        public CE54NeverCryWolf(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-830, 190), 21))
        {
            _adds = Enemies(OID.Imaginifer);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            base.DrawEnemies(pcSlot, pc);
            Arena.Actors(_adds, ArenaColor.Enemy);
        }
    }
}
