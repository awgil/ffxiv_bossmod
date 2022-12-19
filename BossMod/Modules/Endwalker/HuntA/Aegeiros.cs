using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.HuntA.Aegeiros
{
    public enum OID : uint
    {
        Boss = 0x3671, // R7.500, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        Leafstorm = 27708, // Boss->self, 6.0s cast, range 10 circle
        Rimestorm = 27709, // Boss->self, 1.0s cast, range 40 180-degree cone
        Snowball = 27710, // Boss->location, 3.0s cast, range 8 circle
        Canopy = 27711, // Boss->players, no cast, range 12 ?-degree cone cleave
        BackhandBlow = 27712, // Boss->self, 3.0s cast, range 12 120-degree cone
    };

    class LeafstormRimestorm : Components.GenericAOEs
    {
        private DateTime _rimestormExpected;
        private static AOEShapeCircle _leafstorm = new(10);
        private static AOEShapeCone _rimestorm = new(40, 90.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.Leafstorm) ?? false)
                yield return new(_leafstorm, module.PrimaryActor.Position, module.PrimaryActor.CastInfo!.Rotation, module.PrimaryActor.CastInfo.FinishAt);

            if (module.PrimaryActor.CastInfo?.IsSpell(AID.Rimestorm) ?? false)
                yield return new(_rimestorm, module.PrimaryActor.Position, module.PrimaryActor.CastInfo!.Rotation, module.PrimaryActor.CastInfo.FinishAt);
            else if (_rimestormExpected != new DateTime())
                yield return new(_rimestorm, module.PrimaryActor.Position, module.PrimaryActor.CastInfo?.Rotation ?? module.PrimaryActor.Rotation, _rimestormExpected);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster == module.PrimaryActor && (AID)spell.Action.ID == AID.Leafstorm)
                _rimestormExpected = module.WorldState.CurrentTime.AddSeconds(9.6f);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster == module.PrimaryActor && (AID)spell.Action.ID == AID.Rimestorm)
                _rimestormExpected = new();
        }
    }

    class Snowball : Components.LocationTargetedAOEs
    {
        public Snowball() : base(ActionID.MakeSpell(AID.Snowball), 8) { }
    }

    class Canopy : Components.Cleave
    {
        public Canopy() : base(ActionID.MakeSpell(AID.Canopy), new AOEShapeCone(12, 60.Degrees()), activeWhileCasting: false) { } // TODO: verify angle
    }

    class BackhandBlow : Components.SelfTargetedAOEs
    {
        public BackhandBlow() : base(ActionID.MakeSpell(AID.BackhandBlow), new AOEShapeCone(12, 60.Degrees())) { }
    }

    class AegeirosStates : StateMachineBuilder
    {
        public AegeirosStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<LeafstormRimestorm>()
                .ActivateOnEnter<Snowball>()
                .ActivateOnEnter<Canopy>()
                .ActivateOnEnter<BackhandBlow>();
        }
    }

    public class Aegeiros : SimpleBossModule
    {
        public Aegeiros(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
