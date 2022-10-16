using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Components
{
    // generic gaze component, allows customized 'eye' position
    public abstract class GenericGaze : CastCounter
    {
        private static float _eyeOuterH = 10;
        private static float _eyeOuterV = 6;
        private static float _eyeInnerR = 4;
        private static float _eyeOuterR = (_eyeOuterH * _eyeOuterH + _eyeOuterV * _eyeOuterV) / (2 * _eyeOuterV);
        private static float _eyeOffsetV = _eyeOuterR - _eyeOuterV;
        private static float _eyeHalfAngle = MathF.Asin(_eyeOuterH / _eyeOuterR);

        public abstract IEnumerable<(WPos pos, DateTime activation)> EyePositions(BossModule module);

        public GenericGaze(ActionID aid = new()) : base(aid) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (EyePositions(module).Any(eye => HitByEye(actor, eye.pos)))
                hints.Add("Turn away from gaze!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var eye in EyePositions(module))
                hints.ForbiddenDirections.Add((Angle.FromDirection(eye.pos - actor.Position), 45.Degrees(), eye.activation));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var eye in EyePositions(module))
            {
                var eyeCenter = IndicatorScreenPos(module, eye.pos);
                var dl = ImGui.GetWindowDrawList();
                dl.PathArcTo(eyeCenter - new Vector2(0, _eyeOffsetV), _eyeOuterR, MathF.PI / 2 + _eyeHalfAngle, MathF.PI / 2 - _eyeHalfAngle);
                dl.PathArcTo(eyeCenter + new Vector2(0, _eyeOffsetV), _eyeOuterR, -MathF.PI / 2 + _eyeHalfAngle, -MathF.PI / 2 - _eyeHalfAngle);
                dl.PathFillConvex(HitByEye(pc, eye.pos) ? ArenaColor.Enemy : ArenaColor.PC);
                dl.AddCircleFilled(eyeCenter, _eyeInnerR, ArenaColor.Border);
            }
        }

        private bool HitByEye(Actor actor, WPos eye)
        {
            return actor.Rotation.ToDirection().Dot((eye - actor.Position).Normalized()) >= 0.707107f; // 45-degree
        }

        private Vector2 IndicatorScreenPos(BossModule module, WPos eye)
        {
            if (module.Bounds.Contains(eye))
            {
                return module.Arena.WorldPositionToScreenPosition(eye);
            }
            else
            {
                var dir = (eye - module.Bounds.Center).Normalized();
                return module.Arena.ScreenCenter + module.Arena.RotatedCoords(dir.ToVec2()) * (module.Arena.ScreenHalfSize + module.Arena.ScreenMarginSize / 2);
            }
        }
    }

    // gaze that happens on cast end
    public class CastGaze : GenericGaze
    {
        private List<Actor> _casters = new();

        public CastGaze(ActionID aid) : base(aid) { }

        public override IEnumerable<(WPos pos, DateTime activation)> EyePositions(BossModule module) => _casters.Select(c => (c.Position, c.CastInfo!.FinishAt));

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Remove(caster);
        }
    }
}
