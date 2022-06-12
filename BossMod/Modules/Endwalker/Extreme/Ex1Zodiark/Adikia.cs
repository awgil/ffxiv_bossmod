using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex1Zodiark
{
    // state related to adikia mechanic
    class Adikia : BossModule.Component
    {
        private List<Actor> _casters = new();

        private static AOEShapeCircle _shape = new(21);

        public bool Done => _casters.Count == 0;

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_casters.Any(c => _shape.Check(actor.Position, c)))
                hints.Add("GTFO from side smash aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in _casters)
                _shape.Draw(arena, c);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AdikiaL) || actor.CastInfo!.IsSpell(AID.AdikiaR))
                _casters.Add(actor);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AdikiaL) || actor.CastInfo!.IsSpell(AID.AdikiaR))
                _casters.Remove(actor);
        }
    }
}
