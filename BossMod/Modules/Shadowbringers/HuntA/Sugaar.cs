// CONTRIB: made by malediktus, verified
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.HuntA.Sugaar
{
    public enum OID : uint
    {
        Boss = 0x2875, // R5.500, x1
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        BodySlam = 18018, // Boss->self, 5.0s cast, range 11 circle

        NumbingNoise = 18015, // Boss->self (front), 5.0s cast, range 13, 120 degree cone
        NumbingNoiseRotating = 18098, // during rotation, 0.3s cast time, 13 range, 120 degree cone
        NumbingNoiseAttract = 18100, // rotation, pulls player in from 30 by max 25 units between hitboxes, 3 attacks of NumbingNoiseRotating

        TailSnap = 18016, // Boss->self (behind), 5.0s cast, range 18, 120 degree cone
        TailSnapRotating = 18099, // Boss->self (behind), 0.3s cast, range 18, 120 degree cone
        TailSnapAttract = 18101, // rotation, pulls player in from 30 by max 25 units between hitboxes, 3 attacks of TailSnapRotating
    }

    public enum IconID : uint
    {
        RotateCW = 167, // Boss
        RotateCCW = 168, // Boss
    };

    class BodySlam : Components.SelfTargetedAOEs
    {
        public BodySlam() : base(ActionID.MakeSpell(AID.BodySlam), new AOEShapeCircle(11)) { }
    }

    class NumbingNoise : Components.SelfTargetedAOEs
    {
        public NumbingNoise() : base(ActionID.MakeSpell(AID.NumbingNoise), new AOEShapeCone(13, 60.Degrees())) { }
    }

    class TailSnap : Components.SelfTargetedAOEs
    {
        public TailSnap() : base(ActionID.MakeSpell(AID.TailSnap), new AOEShapeCone(18, 60.Degrees())) { }
    }

    class NumbingNoiseTailSnapRotating : Components.GenericRotatingAOE
    {
        private Angle _nextRotation;
        private Angle _nextIncrement;
        private AOEShapeCone? _nextShape;

        private static AOEShapeCone _shapeNumbingNoise = new(13, 60.Degrees());
        private static AOEShapeCone _shapeTailSnap = new(18, 60.Degrees());

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var increment = (IconID)iconID switch
            {
                IconID.RotateCW => -120.Degrees(),
                IconID.RotateCCW => 120.Degrees(),
                _ => default
            };
            if (increment != default)
            {
                _nextIncrement = increment;
                InitIfReady(module, actor);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            (AOEShapeCone? shape, Angle rotation) = (AID)spell.Action.ID switch
            {
                AID.NumbingNoiseAttract => (_shapeNumbingNoise, spell.Rotation),
                AID.TailSnapAttract => (_shapeTailSnap, spell.Rotation + 180.Degrees()),
                _ => (null, default)
            };
            if (shape != null)
            {
                _nextShape = shape;
                _nextRotation = rotation;
                InitIfReady(module, caster);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NumbingNoiseRotating or AID.TailSnapRotating)
                AdvanceSequence(caster.Position, spell.Rotation, module.WorldState.CurrentTime);
        }

        private void InitIfReady(BossModule module, Actor source)
        {
            if (_nextShape != null && _nextIncrement != default)
            {
                Sequences.Add(new(_nextShape, source.Position, _nextRotation, _nextIncrement, module.WorldState.CurrentTime.AddSeconds(6.1f), 2.8f, 3));
                _nextRotation = default;
                _nextIncrement = default;
                _nextShape = null;
            }
        }
    }

    class NumbingNoiseTailSnapAttract : Components.Knockback
    {
        private NumbingNoiseTailSnapRotating? _rotating;
        private DateTime _activation;

        private static AOEShapeCircle _shape = new(30);

        public override void Init(BossModule module) => _rotating = module.FindComponent<NumbingNoiseTailSnapRotating>();

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (_activation != default)
                yield return new(module.PrimaryActor.Position, 25, _activation, _shape, default, Kind.TowardsOrigin, module.PrimaryActor.HitboxRadius + actor.HitboxRadius);
        }

        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => _rotating?.ActiveAOEs(module, slot, actor).Any(aoe => aoe.Check(pos)) ?? false;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster == module.PrimaryActor && (AID)spell.Action.ID is AID.NumbingNoiseAttract or AID.TailSnapAttract)
                _activation = spell.FinishAt;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster == module.PrimaryActor && (AID)spell.Action.ID is AID.NumbingNoiseAttract or AID.TailSnapAttract)
                _activation = default;
        }
    }

    class SugaarStates : StateMachineBuilder
    {
        public SugaarStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<BodySlam>()
                .ActivateOnEnter<NumbingNoise>()
                .ActivateOnEnter<TailSnap>()
                .ActivateOnEnter<NumbingNoiseTailSnapRotating>()
                .ActivateOnEnter<NumbingNoiseTailSnapAttract>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 125)]
    public class Sugaar(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) {}
}
