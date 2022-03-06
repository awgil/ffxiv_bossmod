using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to curtain call mechanic
    // TODO: unhardcode relative order in pairs, currently tanks/healers pop first...
    class CurtainCall : Component
    {
        private P4S _module;
        private int[] _playerOrder = new int[8];
        private List<WorldState.Actor>? _playersInBreakOrder;
        private int _numCasts = 0;

        public CurtainCall(P4S module)
        {
            _module = module;
        }

        public override void Update()
        {
            if (_playersInBreakOrder == null)
            {
                _playersInBreakOrder = _module.Raid.Members.Zip(_playerOrder).Where(po => po.Item1 != null && po.Item2 != 0).OrderBy(po => po.Item2).Select(po => po.Item1!).ToList();
            }
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_playerOrder[slot] > _numCasts)
            {
                var relOrder = _playerOrder[slot] - _numCasts;
                hints.Add($"Tether break order: {relOrder}", relOrder == 1);
            }
        }

        public override void AddGlobalHints(GlobalHints hints)
        {
            if (_playersInBreakOrder != null)
                hints.Add($"Order: {string.Join(" -> ", _playersInBreakOrder.Skip(_numCasts).Select(OrderTextForPlayer))}");
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            var pc = _module.Player();
            if (pc == null)
                return;

            // draw other players
            foreach ((int slot, var player) in _module.Raid.WithSlot().Exclude(pc))
                arena.Actor(player, _playerOrder[slot] == _numCasts + 1 ? arena.ColorDanger : arena.ColorPlayerGeneric);

            // tether
            var tetherTarget = pc.Tether.Target != 0 ? _module.WorldState.FindActor(pc.Tether.Target) : null;
            if (tetherTarget != null)
                arena.AddLine(pc.Position, tetherTarget.Position, pc.Tether.ID == (uint)TetherID.WreathOfThorns ? arena.ColorDanger : arena.ColorSafe);
        }

        public override void OnStatusGain(WorldState.Actor actor, int index)
        {
            if (actor.Statuses[index].ID == (uint)SID.Thornpricked)
            {
                int slot = _module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    _playerOrder[slot] = 2 * (int)((actor.Statuses[index].ExpireAt - _module.WorldState.CurrentTime).TotalSeconds / 10); // 2/4/6/8
                    if (actor.Role == Role.Tank || actor.Role == Role.Healer)
                        --_playerOrder[slot]; // TODO: this should be configurable (DD first vs tank first)
                    _playersInBreakOrder = null;
                }
            }
        }

        public override void OnStatusLose(WorldState.Actor actor, int index)
        {
            if (actor.Statuses[index].ID == (uint)SID.Thornpricked)
                ++_numCasts;
        }

        private string OrderTextForPlayer(WorldState.Actor player)
        {
            //return player.Name;
            var status = player.FindStatus((uint)SID.Thornpricked);
            var remaining = status != null ? (status.Value.ExpireAt - _module.WorldState.CurrentTime).TotalSeconds : 0;
            return $"{player.Name} ({remaining:f1}s)";
        }
    }
}
