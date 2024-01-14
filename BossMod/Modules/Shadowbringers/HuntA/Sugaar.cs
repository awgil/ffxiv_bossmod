using System;
using System.Collections.Generic;
using System.Linq;


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
        private int _remainingHits = 0;
        private int _numbingnoise = 0;
        private int _tailsnap = 0;
        private Angle _RotationDir;
        private Angle _RotationDirIncrement;

        private static AOEShapeCone _shapeNumbingNoise = new(13, 60.Degrees());
        private static AOEShapeCone _shapeTailSnap = new(18, 60.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_remainingHits > 0 && _numbingnoise > 0)
                yield return new(_shapeNumbingNoise, module.PrimaryActor.Position, _RotationDir);
            if (_remainingHits > 0 && _tailsnap > 0)
                yield return new(_shapeTailSnap, module.PrimaryActor.Position, _RotationDir);
        }

        public override void Update(BossModule module)
        {
            if (module.PrimaryActor.CastInfo == null || !module.PrimaryActor.CastInfo.IsSpell())
                return;
            switch ((AID)module.PrimaryActor.CastInfo.Action.ID)
            {
                case AID.NumbingNoiseRotation:
                    _RotationDir = module.PrimaryActor.Rotation;
                    break;
                case AID.TailSnapRotation:
                    _RotationDir = module.PrimaryActor.Rotation+180.Degrees();
                    break;
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaBackground(module, pcSlot, pc, arena);
            if (_remainingHits > 1 && _RotationDirIncrement.Rad != MathF.PI && _numbingnoise == 1)
                arena.ZoneCone(module.PrimaryActor.Position, 0, _shapeNumbingNoise.Radius, _RotationDir + _RotationDirIncrement, 60.Degrees(), ArenaColor.Danger);
            if (_remainingHits > 1 && _RotationDirIncrement.Rad != MathF.PI && _tailsnap == 1)
                arena.ZoneCone(module.PrimaryActor.Position, 0, _shapeTailSnap.Radius, _RotationDir + _RotationDirIncrement, 60.Degrees(), ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster != module.PrimaryActor)
                return;
            switch ((AID)spell.Action.ID)
            {
                case AID.NumbingNoiseRotation:
                    _tailsnap = 0;
                    _numbingnoise = 1;
                    _remainingHits = 3;
                    _RotationDirIncrement = 120.Degrees();
                    break;
                case AID.NumbingNoiseDuringRotation:
                    _RotationDirIncrement = 120.Degrees();
                    break;
                case AID.TailSnapRotation:
                    _tailsnap = 1;
                    _numbingnoise = 0;
                    _remainingHits = 3;
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
   class RotationPull: Components.KnockbackFromCastTarget //TODO: pulls/attracts should probably have their own component to make this easier in future
    {
        private float PullDistance;
        private Angle Direction;
        private float DistanceToBoss;
        private int activeTailSnap;
        private int activeNumbingNoise;
        private string hint = "Use anti knockback ability or get pulled into danger zone!";
        public RotationPull() : base(ActionID.MakeSpell(AID.NumbingNoiseRotation),default) { }
        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
               var Boss = module.Enemies(OID.Boss).FirstOrDefault();
               var player = module.Raid.Player();
             if (Boss != null && player != null) 
                DistanceToBoss = (player.Position - Boss.Position).Length();
             if (Boss != null && player != null) 
                PullDistance = 30 - (Boss.HitboxRadius + player.HitboxRadius + (30 - DistanceToBoss));
              if (Boss != null && player != null) 
              Direction=Angle.FromDirection(player.Position - Boss.Position);
             if (activeNumbingNoise > 0 || activeTailSnap > 0 && PullDistance > 0 && PullDistance <= 25 && DistanceToBoss <= 30)
                yield return new(new(), PullDistance, default, null, Direction, Kind.TowardsOrigin);
            }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.NumbingNoiseRotation)
            activeNumbingNoise=1;
            if ((AID)spell.Action.ID == AID.TailSnapRotation)
            activeTailSnap = 1;
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.NumbingNoiseRotation)
            activeNumbingNoise = 0;
            if ((AID)spell.Action.ID == AID.TailSnapDuringRotation)
            activeTailSnap = 0;
        }
            public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
             if (activeNumbingNoise > 0 && DistanceToBoss >= 13 && DistanceToBoss <=30)
            {
                hints.Add(hint);
            }
            if (activeTailSnap > 0 && DistanceToBoss >= 18 && DistanceToBoss <=30)
            {
                hints.Add(hint);
            }
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

    public class Sugaar : SimpleBossModule
    {
        public Sugaar(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}