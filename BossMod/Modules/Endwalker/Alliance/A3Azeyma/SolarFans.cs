using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Alliance.A3Azeyma
{
    class SolarFans : BossModule.Component
    {
        private List<(Actor, AOEShapeRect)> _start = new();
        private bool _rhythmActive;
        private List<Actor> _finish = new();

        private static float _flightRadiusInner = 20; // TODO: check whether this is correct
        private static float _flightRadiusOuter = 30;
        private static AOEShapeCircle _aoeFinish = new(25);

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_start.Any(e => e.Item2.Check(actor.Position, e.Item1)) ||
                _rhythmActive && module.Enemies(OID.WardensFlame).Any(flame => ActorInRhythmAOE(module.Arena.WorldCenter, flame, actor)) ||
                _finish.Any(e => _aoeFinish.Check(actor.Position, e)))
            {
                hints.Add("GTFO from aoe!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (fan, shape) in _start)
            {
                shape.Draw(arena, fan);
            }

            if (_rhythmActive)
            {
                foreach (var flame in module.Enemies(OID.WardensFlame))
                {
                    var dir = Angle.FromDirection(flame.Position - arena.WorldCenter) + Angle.Radians(MathF.PI / 4);
                    arena.ZoneCone(arena.WorldCenter, _flightRadiusInner, _flightRadiusOuter, dir, Angle.Radians(MathF.PI / 4), arena.ColorAOE);
                }
            }

            foreach (var finish in _finish)
            {
                _aoeFinish.Draw(arena, finish);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.SolarFansAOE:
                    AOEShapeRect shape = new(0, 5);
                    shape.SetEndPointFromCastLocation(actor);
                    _start.Add((actor, shape));
                    break;
                case AID.RadiantFlourish:
                    _rhythmActive = false;
                    _finish.Add(actor);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.SolarFansAOE:
                    _start.RemoveAll(e => e.Item1 == actor);
                    _rhythmActive = true;
                    break;
                case AID.RadiantFlourish:
                    _finish.Remove(actor);
                    break;
            }
        }

        private bool ActorInRhythmAOE(Vector3 center, Actor flame, Actor player)
        {
            var playerOffet = player.Position - center;
            return !GeometryUtils.PointInCircle(playerOffet, _flightRadiusInner) && GeometryUtils.PointInCone(playerOffet, Angle.FromDirection(flame.Position - center) + Angle.Radians(MathF.PI / 4), Angle.Radians(MathF.PI / 4));
        }
    }
}
