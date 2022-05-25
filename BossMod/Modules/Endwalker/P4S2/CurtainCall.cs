using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.P4S2
{
    using static BossModule;

    // state related to curtain call mechanic
    class CurtainCall : Component
    {
        private int[] _playerOrder = new int[8];
        private List<Actor>? _playersInBreakOrder;
        private int _numCasts = 0;

        public override void Update(BossModule module)
        {
            if (_playersInBreakOrder == null)
            {
                _playersInBreakOrder = module.Raid.Members.Zip(_playerOrder).Where(po => po.Item1 != null && po.Item2 != 0).OrderBy(po => po.Item2).Select(po => po.Item1!).ToList();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_playerOrder[slot] > _numCasts)
            {
                var relOrder = _playerOrder[slot] - _numCasts;
                hints.Add($"Tether break order: {relOrder}", relOrder == 1);
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_playersInBreakOrder != null)
                hints.Add($"Order: {string.Join(" -> ", _playersInBreakOrder.Skip(_numCasts).Select(a => OrderTextForPlayer(module, a)))}");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            // draw other players
            foreach ((int slot, var player) in module.Raid.WithSlot().Exclude(pc))
                arena.Actor(player, _playerOrder[slot] == _numCasts + 1 ? arena.ColorDanger : arena.ColorPlayerGeneric);

            // tether
            var tetherTarget = pc.Tether.Target != 0 ? module.WorldState.Actors.Find(pc.Tether.Target) : null;
            if (tetherTarget != null)
                arena.AddLine(pc.Position, tetherTarget.Position, pc.Tether.ID == (uint)TetherID.WreathOfThorns ? arena.ColorDanger : arena.ColorSafe);
        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            if (actor.Statuses[index].ID == (uint)SID.Thornpricked)
            {
                int slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    _playerOrder[slot] = 2 * (int)((actor.Statuses[index].ExpireAt - module.WorldState.CurrentTime).TotalSeconds / 10); // 2/4/6/8
                    bool ddFirst = Service.Config.Get<P4S2Config>().CurtainCallDDFirst;
                    if (ddFirst != actor.Role is Role.Tank or Role.Healer)
                        --_playerOrder[slot];
                    _playersInBreakOrder = null;
                }
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, int index)
        {
            if (actor.Statuses[index].ID == (uint)SID.Thornpricked)
                ++_numCasts;
        }

        private string OrderTextForPlayer(BossModule module, Actor player)
        {
            //return player.Name;
            var status = player.FindStatus((uint)SID.Thornpricked);
            var remaining = status != null ? (status.Value.ExpireAt - module.WorldState.CurrentTime).TotalSeconds : 0;
            return $"{player.Name} ({remaining:f1}s)";
        }
    }
}
