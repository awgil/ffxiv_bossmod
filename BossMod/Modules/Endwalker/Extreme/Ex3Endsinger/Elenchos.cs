using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex3Endsigner
{
    class Elenchos : BossComponent
    {
        private Actor? _center;
        private List<Actor> _sides = new();

        private static AOEShapeRect _aoeCenter = new(40, 7);
        private static AOEShapeRect _aoeSides = new(40, 6.5f, 40);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_aoeCenter.Check(actor.Position, _center) || _sides.Any(s => _aoeSides.Check(actor.Position, s)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _aoeCenter.Draw(arena, _center);
            foreach (var s in _sides)
                _aoeSides.Draw(arena, s);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.ElenchosCenter))
                _center = actor;
            else if (actor.CastInfo!.IsSpell(AID.ElenchosSidesAOE))
                _sides.Add(actor);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (_center == actor)
                _center = null;
            else
                _sides.Remove(actor);
        }
    }
}
