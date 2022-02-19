using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.P3S
{
    using static BossModule;

    // state related to brightened fire mechanic
    // this helper relies on waymarks 1-4, and assumes they don't change during fight - this is of course quite an assumption, but whatever...
    class BrightenedFire : CommonComponents.CastCounter
    {
        private P3S _module;
        private List<WorldState.Actor> _darkenedFires;
        private int[] _playerOrder = new int[8]; // 0 if unknown, 1-8 otherwise

        private static float _aoeRange = 7;

        public BrightenedFire(P3S module)
            : base(ActionID.MakeSpell(AID.BrightenedFireAOE))
        {
            _module = module;
            _darkenedFires = module.Enemies(OID.DarkenedFire);
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_playerOrder[slot] <= NumCasts)
                return;

            var pos = PositionForOrder(_playerOrder[slot]);
            if (!GeometryUtils.PointInCircle(actor.Position - pos, 5))
            {
                hints.Add($"Get to correct position {_playerOrder[slot]}!");
            }

            int numHitAdds = _darkenedFires.InRadius(actor.Position, _aoeRange).Count();
            if (numHitAdds < 2)
            {
                hints.Add("Get closer to adds!");
            }
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            var pc = _module.Player();
            if (pc == null || _playerOrder[_module.Raid.PlayerSlot] <= NumCasts)
                return;

            var pos = PositionForOrder(_playerOrder[_module.Raid.PlayerSlot]);
            arena.AddCircle(pos, 1, arena.ColorSafe);

            // draw all adds
            int addIndex = 0;
            foreach (var fire in _darkenedFires.SortedByRange(pos))
            {
                arena.Actor(fire, addIndex++ < 2 ? arena.ColorDanger : arena.ColorPlayerGeneric);
            }

            // draw range circle
            arena.AddCircle(pc.Position, _aoeRange, arena.ColorDanger);
        }

        public override void OnEventIcon(uint actorID, uint iconID)
        {
            if (iconID >= 268 && iconID <= 275)
            {
                int slot = _module.Raid.FindSlot(actorID);
                if (slot >= 0)
                    _playerOrder[slot] = (int)iconID - 267;
            }
        }

        private Vector3 PositionForOrder(int order)
        {
            // TODO: consider how this can be improved...
            var markID = (WorldState.Waymark)((int)WorldState.Waymark.N1 + (order - 1) % 4);
            return _module.WorldState.GetWaymark(markID) ?? _module.Arena.WorldCenter;
        }
    }
}
