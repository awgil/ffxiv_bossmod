using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE63WornToShadow
{
    public enum OID : uint
    {
        Boss = 0x31D0, // R7.500, x1
        Helper = 0x233C, // R0.500, x18
        AlkonostsShadow = 0x31D1, // R3.750-7.500, spawn during fight
        VorticalOrb1 = 0x3239, // R0.500, spawn during fight
        VorticalOrb2 = 0x323A, // R0.500, spawn during fight
        VorticalOrb3 = 0x31D2, // R0.500, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 6499, // Boss->player, no cast, single-target
        Stormcall = 24121, // Boss->self, 5.0s cast, single-target, visual
        Explosion = 24122, // VorticalOrb1/VorticalOrb2/VorticalOrb3->self, 1.5s cast, range 35 circle aoe
        BladedBeak = 24123, // Boss->player, 5.0s cast, single-target, tankbuster
        NihilitysSong = 24124, // Boss->self, 5.0s cast, single-target, visual
        NihilitysSongAOE = 24125, // Helper->self, no cast, ???, raidwide
        Fantod = 24126, // Boss->self, 2.0s cast, single-target, visual
        FantodAOE = 24127, // Helper->location, 3.0s cast, range 3 circle puddle
        Foreshadowing = 24128, // Boss->self, 5.0s cast, single-target, visual
        ShadowsCast = 24129, // AlkonostsShadow->Boss, 5.0s cast, single-target, visual (applies transfiguration status to caster and stores casted spell)
        FrigidPulse = 24130, // Boss->self, 5.0s cast, range 8-25 donut
        PainStorm = 24131, // Boss->self, 5.0s cast, range 36 130-degree cone aoe
        PainfulGust = 24132, // Boss->self, 5.0s cast, range 20 circle aoe
        ForeshadowingPulse = 24133, // AlkonostsShadow->self, 5.0s cast, range 8-25 donut
        ForeshadowingStorm = 24134, // AlkonostsShadow->self, 5.0s cast, range 36 130-degree cone aoe
        ForeshadowingGust = 24135, // AlkonostsShadow->self, 5.0s cast, range 20 circle aoe
    };

    public enum SID : uint
    {
        OrbMovement = 2234, // none->VorticalOrb1/VorticalOrb2/VorticalOrb3, extra=0x1E (fast)/0x49 (slow)
        Transfiguration = 705, // AlkonostsShadow->AlkonostsShadow, extra=0x1A4
    };

    class Stormcall : Components.GenericAOEs
    {
        private List<(Actor source, WPos dest, DateTime activation)> _sources = new();
        private static AOEShapeCircle _shape = new(35);

        public Stormcall() : base(ActionID.MakeSpell(AID.Explosion)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _sources.Take(2).Select(e => new AOEInstance(_shape, e.dest, activation: e.activation));
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.OrbMovement)
            {
                var dest = module.Bounds.Center + 29 * (actor.Position - module.Bounds.Center).Normalized();
                _sources.Add((actor, dest, module.WorldState.CurrentTime.AddSeconds(status.Extra == 0x1E ? 9.7 : 19.9)));
                _sources.SortBy(e => e.activation);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction && _sources.FindIndex(e => e.source == caster) is var index && index >= 0)
                _sources[index] = (caster, caster.Position, spell.FinishAt);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _sources.RemoveAll(e => e.source == caster);
        }
    }

    class BladedBeak : Components.SingleTargetCast
    {
        public BladedBeak() : base(ActionID.MakeSpell(AID.BladedBeak)) { }
    }

    class NihilitysSong : Components.RaidwideCast
    {
        public NihilitysSong() : base(ActionID.MakeSpell(AID.NihilitysSong)) { }
    }

    class Fantod : Components.LocationTargetedAOEs
    {
        public Fantod() : base(ActionID.MakeSpell(AID.FantodAOE), 3) { }
    }

    class FrigidPulse : Components.SelfTargetedAOEs
    {
        public FrigidPulse() : base(ActionID.MakeSpell(AID.FrigidPulse), new AOEShapeDonut(8, 25)) { }
    }

    class PainStorm : Components.SelfTargetedAOEs
    {
        public PainStorm() : base(ActionID.MakeSpell(AID.PainStorm), new AOEShapeCone(36, 65.Degrees())) { }
    }

    class PainfulGust : Components.SelfTargetedAOEs
    {
        public PainfulGust() : base(ActionID.MakeSpell(AID.PainfulGust), new AOEShapeCircle(20)) { }
    }

    // TODO: prediction for foreshadowing?..
    class ForeshadowingPulse : Components.SelfTargetedAOEs
    {
        public ForeshadowingPulse() : base(ActionID.MakeSpell(AID.ForeshadowingPulse), new AOEShapeDonut(8, 25)) { }
    }

    class ForeshadowingStorm : Components.SelfTargetedAOEs
    {
        public ForeshadowingStorm() : base(ActionID.MakeSpell(AID.ForeshadowingStorm), new AOEShapeCone(36, 65.Degrees())) { }
    }

    class ForeshadowingGust : Components.SelfTargetedAOEs
    {
        public ForeshadowingGust() : base(ActionID.MakeSpell(AID.ForeshadowingGust), new AOEShapeCircle(20)) { }
    }

    class CE63WornToShadowStates : StateMachineBuilder
    {
        public CE63WornToShadowStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Stormcall>()
                .ActivateOnEnter<BladedBeak>()
                .ActivateOnEnter<NihilitysSong>()
                .ActivateOnEnter<Fantod>()
                .ActivateOnEnter<FrigidPulse>()
                .ActivateOnEnter<PainStorm>()
                .ActivateOnEnter<PainfulGust>()
                .ActivateOnEnter<ForeshadowingPulse>()
                .ActivateOnEnter<ForeshadowingStorm>()
                .ActivateOnEnter<ForeshadowingGust>();
        }
    }

    public class CE63WornToShadow : BossModule
    {
        public CE63WornToShadow(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-480, -690), 30)) { }
    }
}
