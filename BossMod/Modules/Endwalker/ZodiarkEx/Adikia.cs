using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.ZodiarkEx
{
    using static BossModule;

    // state related to adikia mechanic
    class Adikia : Component
    {
        private List<Actor> _casters = new();

        private static AOEShapeCircle _shape = new(21);

        public bool Done => _casters.Count == 0;

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_casters.Any(c => _shape.Check(actor.Position, c)))
                hints.Add("GTFO from side smash aoe!");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in _casters)
                _shape.Draw(arena, c);
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AdikiaL) || actor.CastInfo!.IsSpell(AID.AdikiaR))
                _casters.Add(actor);
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AdikiaL) || actor.CastInfo!.IsSpell(AID.AdikiaR))
                _casters.Remove(actor);
        }
    }
}
