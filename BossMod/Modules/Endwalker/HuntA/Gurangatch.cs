using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.HuntA.Gurangatch
{
    public enum OID : uint
    {
        Boss = 0x361B, // R6.000, x1
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        LeftHammerSlammer = 27493, // Boss->self, 5.0s cast, range 30 180-degree cone
        RightHammerSlammer = 27494, // Boss->self, 5.0s cast, range 30 180-degree cone
        LeftHammerSecond = 27495, // Boss->self, 1.0s cast, range 30 180-degree cone
        RightHammerSecond = 27496, // Boss->self, 1.0s cast, range 30 180-degree cone
        OctupleSlammerLCW = 27497, // Boss->self, 9.0s cast, range 30 180-degree cone
        OctupleSlammerRCW = 27498, // Boss->self, 9.0s cast, range 30 180-degree cone
        OctupleSlammerLCCW = 27521, // Boss->self, 9.0s cast, range 30 180-degree cone
        OctupleSlammerRCCW = 27522, // Boss->self, 9.0s cast, range 30 180-degree cone
        OctupleSlammerRestL = 27499, // Boss->self, 1.0s cast, range 30 180-degree cone
        OctupleSlammerRestR = 27500, // Boss->self, 1.0s cast, range 30 180-degree cone
        WildCharge = 27511, // Boss->players, no cast, width 8 rect charge
        BoneShaker = 27512, // Boss->self, 4.0s cast, range 30 circle
    };

    class Slammer : Components.GenericAOEs
    {
        private int _remainingSlams = 0;
        private Angle _slamDir;
        private Angle _slamDirIncrement;

        private static AOEShapeCone _shape = new(30, 90.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_remainingSlams > 0)
                yield return new(_shape, module.PrimaryActor.Position, _slamDir); // TODO: activation
        }

        // TODO: this shouldn't be necessary...
        public override void Update(BossModule module)
        {
            if (module.PrimaryActor.CastInfo == null || !module.PrimaryActor.CastInfo.IsSpell())
                return;
            switch ((AID)module.PrimaryActor.CastInfo.Action.ID)
            {
                case AID.LeftHammerSlammer:
                case AID.OctupleSlammerLCW:
                case AID.OctupleSlammerLCCW:
                    _slamDir = module.PrimaryActor.Rotation + 90.Degrees();
                    break;
                case AID.RightHammerSlammer:
                case AID.OctupleSlammerRCW:
                case AID.OctupleSlammerRCCW:
                    _slamDir = module.PrimaryActor.Rotation - 90.Degrees();
                    break;
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaBackground(module, pcSlot, pc, arena);
            if (_remainingSlams > 0 && _slamDirIncrement.Rad != MathF.PI)
                arena.ZoneCone(module.PrimaryActor.Position, 0, _shape.Radius, _slamDir - _slamDirIncrement * 3 / 2, 45.Degrees(), ArenaColor.SafeFromAOE);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster != module.PrimaryActor)
                return;
            switch ((AID)spell.Action.ID)
            {
                case AID.LeftHammerSlammer:
                    _remainingSlams = 2;
                    _slamDirIncrement = 180.Degrees();
                    break;
                case AID.RightHammerSlammer:
                    _remainingSlams = 2;
                    _slamDirIncrement = 180.Degrees();
                    break;
                case AID.OctupleSlammerLCW:
                    _remainingSlams = 8;
                    _slamDirIncrement = 90.Degrees();
                    break;
                case AID.OctupleSlammerRCW:
                    _remainingSlams = 8;
                    _slamDirIncrement = 90.Degrees();
                    break;
                case AID.OctupleSlammerLCCW:
                    _remainingSlams = 8;
                    _slamDirIncrement = -90.Degrees();
                    break;
                case AID.OctupleSlammerRCCW:
                    _remainingSlams = 8;
                    _slamDirIncrement = -90.Degrees();
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster != module.PrimaryActor)
                return;
            switch ((AID)spell.Action.ID)
            {
                case AID.LeftHammerSlammer:
                case AID.RightHammerSlammer:
                case AID.LeftHammerSecond:
                case AID.RightHammerSecond:
                case AID.OctupleSlammerLCW:
                case AID.OctupleSlammerRCW:
                case AID.OctupleSlammerRestL:
                case AID.OctupleSlammerRestR:
                case AID.OctupleSlammerLCCW:
                case AID.OctupleSlammerRCCW:
                    _slamDir += _slamDirIncrement;
                    --_remainingSlams;
                    break;
            }
        }
    }

    class BoneShaker : Components.RaidwideCast
    {
        public BoneShaker() : base(ActionID.MakeSpell(AID.BoneShaker)) { }
    }

    class GurangatchStates : StateMachineBuilder
    {
        public GurangatchStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Slammer>()
                .ActivateOnEnter<BoneShaker>();
        }
    }

    public class Gurangatch : SimpleBossModule
    {
        public Gurangatch(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
