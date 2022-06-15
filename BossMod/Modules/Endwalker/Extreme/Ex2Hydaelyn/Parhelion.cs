using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn
{
    class Parhelion : BossComponent
    {
        private List<Actor> _completedParhelions = new();
        private bool _subparhelions;

        private static AOEShapeRect _beacon = new(45, 3);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveParhelions(module).Any(p => _beacon.Check(actor.Position, p)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in ActiveParhelions(module))
                _beacon.Draw(arena, p);
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.BeaconParhelion))
            {
                var caster = module.WorldState.Actors.Find(info.CasterID);
                if (caster != null)
                {
                    _completedParhelions.Add(caster);
                    _subparhelions = _completedParhelions.Count >= 15;
                }
            }
            else if (info.IsSpell(AID.BeaconSubparhelion))
            {
                _completedParhelions.RemoveAll(p => p.InstanceID == info.CasterID);
            }
        }

        private IEnumerable<Actor> ActiveParhelions(BossModule module)
        {
            if (_subparhelions)
                return _completedParhelions.Take(10);
            else
                return module.Enemies(OID.Parhelion).Where(p => p.CastInfo != null);
        }
    }
}
