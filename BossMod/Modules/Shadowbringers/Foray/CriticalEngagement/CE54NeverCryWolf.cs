using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE54NeverCryWolf
{
    public enum OID : uint
    {
        Boss = 0x319C, // R9.996, x1
        Helper = 0x233C, // R0.500, x18
        IceSprite = 0x319D, // R0.800, spawn during fight
        Icicle = 0x319E, // R3.000, spawn during fight
        Imaginifer = 0x319F, // R0.500, spawn during fight
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
        AgeOfEndlessFrostFirstCW = 23590, // Boss->self, 5.0s cast, single-target, visual
        AgeOfEndlessFrostFirstCCW = 23591, // Boss->self, 5.0s cast, single-target, visual
        AgeOfEndlessFrostFirstAOE = 23592, // Helper->self, 5.0s cast, range 40 20-degree cone
        AgeOfEndlessFrostRest = 22883, // Boss->self, no cast, single-target
        AgeOfEndlessFrostRestAOE = 23593, // Helper->self, 0.5s cast, range 40 20-degree cone

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

    class BracingWind : Components.KnockbackFromCastTarget
    {
        public BracingWind() : base(ActionID.MakeSpell(AID.BracingWind), 40, false, 1, new AOEShapeRect(60, 6), Kind.DirForward) { }
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

    class AgeOfEndlessFrost : Components.GenericAOEs
    {
        private Angle _increment;
        private List<Angle> _angles = new();
        private DateTime _nextActivation;

        private static AOEShapeCone _shape = new(40, 10.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _angles.Select(a => new AOEInstance(_shape, module.PrimaryActor.Position, a, _nextActivation));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.AgeOfEndlessFrostFirstCW:
                    _increment = -40.Degrees();
                    _nextActivation = spell.NPCFinishAt;
                    break;
                case AID.AgeOfEndlessFrostFirstCCW:
                    _increment = 40.Degrees();
                    _nextActivation = spell.NPCFinishAt;
                    break;
                case AID.AgeOfEndlessFrostFirstAOE:
                    NumCasts = 0;
                    _angles.Add(spell.Rotation);
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.AgeOfEndlessFrostFirstCCW or AID.AgeOfEndlessFrostFirstCW or AID.AgeOfEndlessFrostRest)
            {
                if (NumCasts == 0)
                {
                    _nextActivation = module.WorldState.CurrentTime.AddSeconds(2.6);
                }
                else if (NumCasts < 6)
                {
                    _nextActivation = module.WorldState.CurrentTime.AddSeconds(2.1);
                }
                else
                {
                    _angles.Clear();
                }

                ++NumCasts;
                for (int i = 0; i < _angles.Count; ++i)
                    _angles[i] += _increment;
            }
        }
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
                .ActivateOnEnter<AgeOfEndlessFrost>()
                .ActivateOnEnter<StormWithout>()
                .ActivateOnEnter<StormWithin>()
                .ActivateOnEnter<AncientGlacier>()
                .ActivateOnEnter<Glaciation>();
        }
    }

    [ModuleInfo(CFCID = 778, DynamicEventID = 25)]
    public class CE54NeverCryWolf : BossModule
    {
        private IReadOnlyList<Actor> _adds;

        public CE54NeverCryWolf(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-830, 190), 24))
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
