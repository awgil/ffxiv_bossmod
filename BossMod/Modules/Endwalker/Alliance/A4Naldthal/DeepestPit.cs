using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A4Naldthal
{
    // TODO: we can use icon to determine targets much earlier, but how to clear them if they turn out to be fake?
    class DeepestPit : BossModule.Component
    {
        //private List<Actor> _targets = new();
        private List<Actor> _casters = new();

        private static float _radius = 6;

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_casters.Any(c => GeometryUtils.PointInCircle(actor.Position - c.CastInfo!.Location, _radius)))
            {
                hints.Add("GTFO from puddle!");
            }
            //else if (_targets.InRadiusExcluding(actor, _radius).Any())
            //{
            //    hints.Add("GTFO from puddle target!");
            //}
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in _casters)
                arena.ZoneCircle(c.CastInfo!.Location, _radius, arena.ColorDanger);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            //foreach (var t in _targets)
            //{
            //    arena.Actor(t, arena.ColorDanger);
            //    arena.AddCircle(t.Position, _radius, arena.ColorDanger);
            //}
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.DeepestPitFirst) || actor.CastInfo!.IsSpell(AID.DeepestPitRest))
            {
                _casters.Add(actor);
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.DeepestPitFirst) || actor.CastInfo!.IsSpell(AID.DeepestPitRest))
            {
                _casters.Remove(actor);
                //if (_casters.Count == 0)
                //    _targets.Clear();
            }
        }

        //public override void OnEventIcon(BossModule module, uint actorID, uint iconID)
        //{
        //    if ((IconID)iconID == IconID.DeepestPitTarget)
        //    {
        //        var actor = module.WorldState.Actors.Find(actorID);
        //        if (actor != null)
        //            _targets.Add(actor);
        //    }
        //}
    }
}
