using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex3Endsigner
{
    class Endsong : BossComponent
    {
        private List<Actor> _active = new();

        private static AOEShapeCircle _aoe = new(15);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_active.Any(a => _aoe.Check(actor.Position, a)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var a in _active)
                _aoe.Draw(arena, a);
        }

        public override void OnTethered(BossModule module, Actor actor)
        {
            if ((TetherID)actor.Tether.ID is TetherID.EndsongFirst or TetherID.EndsongNext)
                _active.Add(actor);
        }

        public override void OnUntethered(BossModule module, Actor actor)
        {
            _active.Remove(actor);
        }
    }
}
