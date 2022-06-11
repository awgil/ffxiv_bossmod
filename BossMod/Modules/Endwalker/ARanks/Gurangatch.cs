using System;

namespace BossMod.Endwalker.ARanks.Gurangatch
{
    public enum OID : uint
    {
        Boss = 0x361B,
    };

    public enum AID : uint
    {
        AutoAttack = 870,
        LeftHammerSlammer = 27493,
        RightHammerSlammer = 27494,
        LeftHammerSecond = 27495,
        RightHammerSecond = 27496,
        OctupleSlammerLCW = 27497,
        OctupleSlammerRCW = 27498,
        OctupleSlammerRestL = 27499,
        OctupleSlammerRestR = 27500,
        // WildCharge = 27511? TODO never seen...
        BoneShaker = 27512,
        OctupleSlammerLCCW = 27521,
        OctupleSlammerRCCW = 27522,
    }

    public class Mechanics : BossModule.Component
    {
        private AOEShapeCone _slammer = new(30, Angle.Radians(MathF.PI / 2));
        private int _remainingSlams = 0;
        private Angle _slamDir;
        private Angle _slamDirIncrement;

        public override void Update(BossModule module)
        {
            if (module.PrimaryActor.CastInfo == null || !module.PrimaryActor.CastInfo.IsSpell())
                return;
            switch ((AID)module.PrimaryActor.CastInfo.Action.ID)
            {
                case AID.LeftHammerSlammer:
                case AID.OctupleSlammerLCW:
                case AID.OctupleSlammerLCCW:
                    _slamDir = module.PrimaryActor.Rotation + Angle.Radians(MathF.PI / 2);
                    break;
                case AID.RightHammerSlammer:
                case AID.OctupleSlammerRCW:
                case AID.OctupleSlammerRCCW:
                    _slamDir = module.PrimaryActor.Rotation - Angle.Radians(MathF.PI / 2);
                    break;
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_remainingSlams > 0 && _slammer.Check(actor.Position, module.PrimaryActor.Position, _slamDir))
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.BoneShaker => "Raidwide",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_remainingSlams <= 0)
                return;

            _slammer.Draw(arena, module.PrimaryActor.Position, _slamDir);
            if (_slamDirIncrement.Rad != MathF.PI)
                arena.ZoneCone(module.PrimaryActor.Position, 0, _slammer.Radius, _slamDir - _slamDirIncrement * 3 / 2, Angle.Radians(MathF.PI / 4), arena.ColorSafeFromAOE);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor != module.PrimaryActor || !actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.LeftHammerSlammer:
                    _remainingSlams = 2;
                    _slamDirIncrement = Angle.Radians(MathF.PI);
                    break;
                case AID.RightHammerSlammer:
                    _remainingSlams = 2;
                    _slamDirIncrement = Angle.Radians(MathF.PI);
                    break;
                case AID.OctupleSlammerLCW:
                    _remainingSlams = 8;
                    _slamDirIncrement = Angle.Radians(MathF.PI / 2);
                    break;
                case AID.OctupleSlammerRCW:
                    _remainingSlams = 8;
                    _slamDirIncrement = Angle.Radians(MathF.PI / 2);
                    break;
                case AID.OctupleSlammerLCCW:
                    _remainingSlams = 8;
                    _slamDirIncrement = -Angle.Radians(MathF.PI / 2);
                    break;
                case AID.OctupleSlammerRCCW:
                    _remainingSlams = 8;
                    _slamDirIncrement = -Angle.Radians(MathF.PI / 2);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor != module.PrimaryActor || !actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
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

    public class Gurangatch : SimpleBossModule
    {
        public Gurangatch(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            BuildStateMachine<Mechanics>();
        }
    }
}
