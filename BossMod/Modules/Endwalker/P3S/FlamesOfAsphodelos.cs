using System;
using System.Numerics;

namespace BossMod.P3S
{
    using static BossModule;

    // state related to flames of asphodelos mechanic
    class FlamesOfAsphodelos : Component
    {
        private P3S _module;
        private float?[] _directions = new float?[3];

        public FlamesOfAsphodelos(P3S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (InAOE(_directions[1], actor.Position) || InAOE(_directions[0] != null ? _directions[0] : _directions[2], actor.Position))
            {
                hints.Add("GTFO from cone!");
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
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

        public override void OnCastStarted(Actor actor)
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

        public override void OnCastFinished(Actor actor)
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

        private void DrawZone(MiniArena arena, float? dir, uint color)
        {
            if (dir != null)
            {
                arena.ZoneIsoscelesTri(arena.WorldCenter, dir.Value, MathF.PI / 6, 50, color);
                arena.ZoneIsoscelesTri(arena.WorldCenter, dir.Value + MathF.PI, MathF.PI / 6, 50, color);
            }
        }

        private bool InAOE(float? dir, Vector3 pos)
        {
            if (dir == null)
                return false;

            var toPos = Vector3.Normalize(pos - _module.Arena.WorldCenter);
            var toDir = GeometryUtils.DirectionToVec3(dir.Value);
            return MathF.Abs(Vector3.Dot(toPos, toDir)) >= MathF.Cos(MathF.PI / 6);
        }
    }
}
