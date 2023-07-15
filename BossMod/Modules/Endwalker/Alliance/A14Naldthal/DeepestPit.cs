using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A14Naldthal
{
    // TODO: we can use icon to determine targets much earlier, but how to clear them if they turn out to be fake?
    class DeepestPit : BossComponent
    {
        //private List<Actor> _targets = new();
        private List<Actor> _casters = new();

        private static float _radius = 6;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_casters.Any(c => actor.Position.InCircle(c.CastInfo!.LocXZ, _radius)))
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
                arena.ZoneCircle(c.CastInfo!.LocXZ, _radius, ArenaColor.Danger);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            //foreach (var t in _targets)
            //{
            //    arena.Actor(t, ArenaColor.Danger);
            //    arena.AddCircle(t.Position, _radius, ArenaColor.Danger);
            //}
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.DeepestPitFirst or AID.DeepestPitRest)
            {
                _casters.Add(caster);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.DeepestPitFirst or AID.DeepestPitRest)
            {
                _casters.Remove(caster);
                //if (_casters.Count == 0)
                //    _targets.Clear();
            }
        }

        //public override void OnEventIcon(BossModule module, ulong actorID, uint iconID)
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
