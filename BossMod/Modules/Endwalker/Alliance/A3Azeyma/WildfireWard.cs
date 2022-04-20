using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Alliance.A3Azeyma
{
    class WildfireWard : BossModule.Component
    {
        private bool _active;
        private List<Actor> _glimpse = new();

        private static Vector3[] _tri = { new(-750, 0, -762), new(-760.392f, 0, -744), new(-739.608f, 0, -744) };
        private static float _knockbackDistance = 15;

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (!_active)
                return;
            if (!GeometryUtils.PointInTri(actor.Position, _tri[0], _tri[1], _tri[2]))
                hints.Add("Go to safe zone!");
            var adjPos = AdjustedPosition(actor);
            if (adjPos != actor.Position && !GeometryUtils.PointInTri(adjPos, _tri[0], _tri[1], _tri[2]))
                hints.Add("About to be knocked into fire!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_active)
                arena.ZoneTri(_tri[0], _tri[1], _tri[2], arena.ColorSafeFromAOE);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var adjPos = AdjustedPosition(pc);
            if (adjPos != pc.Position)
            {
                arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                arena.Actor(adjPos, 0, arena.ColorDanger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.WildfireWard))
                _active = true;
            else if (actor.CastInfo!.IsSpell(AID.IlluminatingGlimpse))
                _glimpse.Add(actor);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.IlluminatingGlimpse))
            {
                _glimpse.Remove(actor);
                if (_glimpse.Count == 0)
                    _active = false;
            }
        }

        private Vector3 AdjustedPosition(Actor actor)
        {
            var glimpse = _glimpse.FirstOrDefault();
            if (glimpse == null)
                return actor.Position;

            var dir = GeometryUtils.DirectionToVec3(glimpse.Rotation);
            var normal = new Vector3(dir.Z, 0, -dir.X);
            return actor.Position + normal * _knockbackDistance;
        }
    }
}
