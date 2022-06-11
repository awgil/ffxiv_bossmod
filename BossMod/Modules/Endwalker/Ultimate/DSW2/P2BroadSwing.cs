using System;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P2BroadSwing : CommonComponents.CastCounter
    {
        private Actor? _caster;
        private bool _lr;

        private static AOEShapeCone _aoe = new(40, Angle.Radians(MathF.PI / 3));

        public P2BroadSwing() : base(ActionID.MakeSpell(AID.BroadSwingAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_caster == null)
                return;
            bool inAOE = NumCasts switch
            {
                0 => _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(0)) || _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(1)),
                1 => _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(1)) || _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(2)),
                2 => _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(2)),
                _ => false
            };
            if (inAOE)
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_caster == null)
                return;
            switch (NumCasts)
            {
                case 0:
                    DrawZone(arena, _caster, 0, arena.ColorDanger);
                    DrawZone(arena, _caster, 1, arena.ColorAOE);
                    break;
                case 1:
                    DrawZone(arena, _caster, 1, arena.ColorDanger);
                    DrawZone(arena, _caster, 2, arena.ColorAOE);
                    break;
                case 2:
                    DrawZone(arena, _caster, 2, arena.ColorDanger);
                    break;
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.BroadSwingRL:
                    _caster = actor;
                    _lr = false;
                    break;
                case AID.BroadSwingLR:
                    _caster = actor;
                    _lr = true;
                    break;
            }
        }

        private Angle AngleOffset(int order)
        {
            var dir = order switch
            {
                0 => -MathF.PI / 3,
                1 => +MathF.PI / 3,
                _ => MathF.PI
            };
            return Angle.Radians(_lr ? -dir : dir);
        }

        private void DrawZone(MiniArena arena, Actor caster, int order, uint color)
        {
            var dir = caster.Rotation + AngleOffset(order);
            arena.ZoneCone(caster.Position, 0, _aoe.Radius, dir, _aoe.HalfAngle, color);
        }
    }
}
