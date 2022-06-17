using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex3Endsigner
{
    class Endsong : BossComponent
    {
        private List<Actor> _active = new();

        private static AOEShapeCircle _aoe = new(15);

        public Endsong()
        {
            Tether(TetherID.EndsongFirst, (_, source, _) => _active.Add(source));
            Tether(TetherID.EndsongNext, (_, source, _) => _active.Add(source));
            Untether(TetherID.EndsongFirst, (_, source, _) => _active.Remove(source));
            Untether(TetherID.EndsongNext, (_, source, _) => _active.Remove(source));
        }

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
    }
}
