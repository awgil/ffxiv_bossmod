using System;
using System.Numerics;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to flames of asphodelos mechanic
    class FlamesOfAsphodelos : Component
    {
        private Angle?[] _directions = new Angle?[3];

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (InAOE(module, _directions[1], actor.Position) || InAOE(module, _directions[0] != null ? _directions[0] : _directions[2], actor.Position))
            {
                hints.Add("GTFO from cone!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_directions[0] != null)
            {
                DrawZone(arena, _directions[0], arena.ColorDanger);
                DrawZone(arena, _directions[1], arena.ColorAOE);
            }
            else if (_directions[1] != null)
            {
                DrawZone(arena, _directions[1], arena.ColorDanger);
                DrawZone(arena, _directions[2], arena.ColorAOE);
            }
            else
            {
                DrawZone(arena, _directions[2], arena.ColorDanger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.FlamesOfAsphodelosAOE1:
                    _directions[0] = actor.Rotation;
                    break;
                case AID.FlamesOfAsphodelosAOE2:
                    _directions[1] = actor.Rotation;
                    break;
                case AID.FlamesOfAsphodelosAOE3:
                    _directions[2] = actor.Rotation;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.FlamesOfAsphodelosAOE1:
                    _directions[0] = null;
                    break;
                case AID.FlamesOfAsphodelosAOE2:
                    _directions[1] = null;
                    break;
                case AID.FlamesOfAsphodelosAOE3:
                    _directions[2] = null;
                    break;
            }
        }

        private void DrawZone(MiniArena arena, Angle? dir, uint color)
        {
            if (dir != null)
            {
                arena.ZoneIsoscelesTri(arena.WorldCenter, dir.Value, Angle.Radians(MathF.PI / 6), 50, color);
                arena.ZoneIsoscelesTri(arena.WorldCenter, dir.Value + Angle.Radians(MathF.PI), Angle.Radians(MathF.PI / 6), 50, color);
            }
        }

        private bool InAOE(BossModule module, Angle? dir, Vector3 pos)
        {
            if (dir == null)
                return false;

            var toPos = Vector3.Normalize(pos - module.Arena.WorldCenter);
            var toDir = dir.Value.ToDirection();
            return MathF.Abs(Vector3.Dot(toPos, toDir)) >= MathF.Cos(MathF.PI / 6);
        }
    }
}
