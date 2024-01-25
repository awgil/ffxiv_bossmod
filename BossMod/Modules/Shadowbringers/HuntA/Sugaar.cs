// CONTRIB: made by malediktus, not checked
using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.HuntA.Sugaar
{
    public enum OID : uint
    {
        Boss = 0x2875,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        NumbingNoise = 18015, // Boss->self (front), 5.0s cast, range 13, 120 degree cone
        TailSnap = 18016, // Boss->self (behind), 5.0s cast, range 18, 120 degree cone
        BodySlam = 18018, // Boss->self, 5.0s cast, range 11 circle
        NumbingNoiseRotation = 18100, // rotation, pulls player in from 30 by max 25 units between hitboxes, 3 attacks of NumbingNoise2
        NumbingNoiseDuringRotation = 18098, // during rotation, 0.3s cast time, 13 range, 120 degree cone
        TailSnapRotation = 18101, // rotation, pulls player in from 30 by max 25 units between hitboxes, 3 attacks of TailSnap2
        TailSnapDuringRotation = 18099, // Boss->self (behind), 0.3s cast, range 18, 120 degree cone
    }

    class NumbingNoise : Components.SelfTargetedAOEs
    {
        public NumbingNoise() : base(ActionID.MakeSpell(AID.NumbingNoise), new AOEShapeCone(13,60.Degrees())) { } 
    }

    class TailSnap : Components.SelfTargetedAOEs
    {
        public TailSnap() : base(ActionID.MakeSpell(AID.TailSnap), new AOEShapeCone(18,60.Degrees())) { }
    }

    class Rotation : Components.GenericAOEs
    {
        private int _remainingHits;
        private bool _numbingnoise;
        private bool _tailsnap;
        private Angle _RotationDir;
        private Angle _RotationDirIncrement;

        private static AOEShapeCone _shapeNumbingNoise = new(13, 60.Degrees());
        private static AOEShapeCone _shapeTailSnap = new(18, 60.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_remainingHits > 0 && _numbingnoise)
                yield return new(_shapeNumbingNoise, module.PrimaryActor.Position, _RotationDir);
            if (_remainingHits > 0 && _tailsnap )
                yield return new(_shapeTailSnap, module.PrimaryActor.Position, _RotationDir);
            if (_remainingHits > 1 && _RotationDirIncrement.Rad != MathF.PI && _numbingnoise)
                yield return new(_shapeNumbingNoise, module.PrimaryActor.Position, _RotationDir + _RotationDirIncrement, default, ArenaColor.Danger);
            if (_remainingHits > 1 && _RotationDirIncrement.Rad != MathF.PI && _tailsnap)
                yield return new(_shapeTailSnap, module.PrimaryActor.Position, _RotationDir + _RotationDirIncrement, default, ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster != module.PrimaryActor)
                return;
            switch ((AID)spell.Action.ID)
            {
                case AID.NumbingNoiseRotation:
                    _tailsnap = false;
                    _numbingnoise = true;
                    _remainingHits = 3;
                    _RotationDir = module.PrimaryActor.Rotation;
                    _RotationDirIncrement = 120.Degrees();
                    break;
                case AID.NumbingNoiseDuringRotation:
                    _RotationDirIncrement = 120.Degrees();
                    break;
                case AID.TailSnapRotation:
                    _tailsnap = true;
                    _numbingnoise = false;
                    _remainingHits = 3;
                    _RotationDir = module.PrimaryActor.Rotation+180.Degrees();
                    _RotationDirIncrement = -120.Degrees();
                    break;
                case AID.TailSnapDuringRotation:
                    _RotationDirIncrement = -120.Degrees();
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster != module.PrimaryActor)
                return;
            switch ((AID)spell.Action.ID)
            {
                case AID.NumbingNoiseDuringRotation:
                case AID.TailSnapDuringRotation:
                    _RotationDir += _RotationDirIncrement;
                    --_remainingHits;
                    break;
            }
        }
    }

    class BodySlam : Components.SelfTargetedAOEs
    {
        public BodySlam() : base(ActionID.MakeSpell(AID.BodySlam), new AOEShapeCircle(11)) { }
    }

    class RotationPull: Components.Knockback //TODO: pulls/attracts should probably have their own component to make this easier in future
    {
        private float PullDistance;
        private Angle Direction;
        private float DistanceToBoss;
        private bool activeTailSnap;
        private bool activeNumbingNoise;
        private readonly string hint = "Use anti knockback ability or get pulled into danger zone!";

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            var player = module.Raid.Player();
            if (player != null)
                DistanceToBoss = (player.Position - module.PrimaryActor.Position).Length();
            if (player != null)
                PullDistance = 30 - (module.PrimaryActor.HitboxRadius + player.HitboxRadius + (30 - DistanceToBoss));
            if (player != null)
                Direction = Angle.FromDirection(player.Position - module.PrimaryActor.Position);
            if (activeNumbingNoise || activeTailSnap && PullDistance > 0 && PullDistance <= 25 && DistanceToBoss <= 30)
                yield return new(new(), PullDistance, default, null, Direction, Kind.TowardsOrigin);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.NumbingNoiseRotation)
                activeNumbingNoise = true;
            if ((AID)spell.Action.ID == AID.TailSnapRotation)
                activeTailSnap = true;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastFinished(module, caster, spell);
            if ((AID)spell.Action.ID == AID.NumbingNoiseRotation)
                activeNumbingNoise = false;
            if ((AID)spell.Action.ID == AID.TailSnapDuringRotation)
                activeTailSnap = false;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (activeNumbingNoise && DistanceToBoss >= 13 && DistanceToBoss <=30)
                hints.Add(hint);
            if (activeTailSnap && DistanceToBoss >= 18 && DistanceToBoss <=30)
                hints.Add(hint);
        }
    }

    class SugaarStates : StateMachineBuilder
    {
        public SugaarStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<NumbingNoise>()
                .ActivateOnEnter<TailSnap>()
                .ActivateOnEnter<RotationPull>()
                .ActivateOnEnter<Rotation>()
                .ActivateOnEnter<BodySlam>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 125)]
    public class Sugaar(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) {}
}
