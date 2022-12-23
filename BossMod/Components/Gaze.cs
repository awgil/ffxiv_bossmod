using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Components
{
    // generic gaze/weakpoint component, allows customized 'eye' position
    public abstract class GenericGaze : CastCounter
    {
        public struct Eye
        {
            public WPos Position;
            public DateTime Activation;
            public Angle Forward; // if non-zero, treat specified side as 'forward' for hit calculations

            public Eye(WPos position, DateTime activation = new(), Angle forward = new())
            {
                Position = position;
                Activation = activation;
                Forward = forward;
            }
        }

        public bool Inverted; // if inverted, player should face eyes instead of averting

        private static float _eyeOuterH = 10;
        private static float _eyeOuterV = 6;
        private static float _eyeInnerR = 4;
        private static float _eyeOuterR = (_eyeOuterH * _eyeOuterH + _eyeOuterV * _eyeOuterV) / (2 * _eyeOuterV);
        private static float _eyeOffsetV = _eyeOuterR - _eyeOuterV;
        private static float _eyeHalfAngle = MathF.Asin(_eyeOuterH / _eyeOuterR);

        public abstract IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor);

        public GenericGaze(ActionID aid = new(), bool inverted = false) : base(aid)
        {
            Inverted = inverted;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveEyes(module, slot, actor).Any(eye => HitByEye(actor, eye) != Inverted))
                hints.Add(Inverted ? "Face the eye!" : "Turn away from gaze!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (Inverted)
            {
                foreach (var eye in ActiveEyes(module, slot, actor))
                    hints.ForbiddenDirections.Add((Angle.FromDirection(actor.Position - eye.Position) - eye.Forward, 135.Degrees(), eye.Activation));
            }
            else
            {
                foreach (var eye in ActiveEyes(module, slot, actor))
                    hints.ForbiddenDirections.Add((Angle.FromDirection(eye.Position - actor.Position) - eye.Forward, 45.Degrees(), eye.Activation));
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var eye in ActiveEyes(module, pcSlot, pc))
            {
                bool danger = HitByEye(pc, eye) != Inverted;
                var eyeCenter = IndicatorScreenPos(module, eye.Position);
                var dl = ImGui.GetWindowDrawList();
                dl.PathArcTo(eyeCenter - new Vector2(0, _eyeOffsetV), _eyeOuterR, MathF.PI / 2 + _eyeHalfAngle, MathF.PI / 2 - _eyeHalfAngle);
                dl.PathArcTo(eyeCenter + new Vector2(0, _eyeOffsetV), _eyeOuterR, -MathF.PI / 2 + _eyeHalfAngle, -MathF.PI / 2 - _eyeHalfAngle);
                dl.PathFillConvex(danger ? ArenaColor.Enemy : ArenaColor.PC);
                dl.AddCircleFilled(eyeCenter, _eyeInnerR, ArenaColor.Border);

                if (Inverted)
                    arena.PathArcTo(pc.Position, 1, (pc.Rotation + eye.Forward + 45.Degrees()).Rad, (pc.Rotation + eye.Forward + 315.Degrees()).Rad);
                else
                    arena.PathArcTo(pc.Position, 1, (pc.Rotation + eye.Forward - 45.Degrees()).Rad, (pc.Rotation + eye.Forward + 45.Degrees()).Rad);
                arena.PathStroke(false, ArenaColor.Enemy);
            }
        }

        private bool HitByEye(Actor actor, Eye eye)
        {
            return (actor.Rotation + eye.Forward).ToDirection().Dot((eye.Position - actor.Position).Normalized()) >= 0.707107f; // 45-degree
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

        public CastGaze(ActionID aid, bool inverted = false) : base(aid, inverted) { }

        public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor) => _casters.Select(c => new Eye(c.Position, c.CastInfo!.FinishAt));

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

    // cast weakpoint component: a number of casts (with supposedly non-intersecting shapes), player should face specific side determined by active status to the caster for aoe he's in
    public class CastWeakpoint : GenericGaze
    {
        public AOEShape Shape;
        public uint[] Statuses; // 4 elements: fwd, left, back, right
        private List<Actor> _casters = new();
        private Dictionary<ulong, Angle> _playerWeakpoints = new();

        public CastWeakpoint(ActionID aid, AOEShape shape, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight) : base(aid, true)
        {
            Shape = shape;
            Statuses = new[] { statusForward, statusLeft, statusBackward, statusRight };
        }

        public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor)
        {
            // if there are multiple casters, take one that finishes first
            var caster = _casters.Where(a => Shape.Check(actor.Position, a.Position, a.CastInfo!.Rotation)).MinBy(a => a.CastInfo!.FinishAt);
            Angle angle;
            if (caster != null && _playerWeakpoints.TryGetValue(actor.InstanceID, out angle))
                yield return new(caster.Position, caster.CastInfo!.FinishAt, angle);
        }

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

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var statusKind = Array.IndexOf(Statuses, status.ID);
            if (statusKind >= 0)
                _playerWeakpoints[actor.InstanceID] = statusKind * 90.Degrees();
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            var statusKind = Array.IndexOf(Statuses, status.ID);
            if (statusKind >= 0)
                _playerWeakpoints.Remove(actor.InstanceID);
        }
    }
}
