using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.P4S2
{
    using static BossModule;

    // state related to act 5 (finale) wreath of thorns
    class WreathOfThorns5 : Component
    {
        private P4S2 _module;
        private List<uint> _playersOrder = new();
        private List<Actor> _towersOrder = new();
        private int _castsDone = 0;

        private static float _impulseAOERadius = 5;

        public WreathOfThorns5(P4S2 module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            int order = _playersOrder.IndexOf(actor.InstanceID);
            if (order >= 0)
            {
                hints.Add($"Order: {order + 1}", false);

                if (order >= _castsDone && order < _towersOrder.Count)
                {
                    hints.Add("Soak tower!", !GeometryUtils.PointInCircle(actor.Position - _towersOrder[order].Position, P4S2.WreathTowerRadius));
                }
            }

            if (_playersOrder.Count < 8)
            {
                hints.Add("Spread!", _module.Raid.WithoutSlot().InRadiusExcluding(actor, _impulseAOERadius).Any());
            }
        }

        public override void AddGlobalHints(GlobalHints hints)
        {
            hints.Add($"Order: {string.Join(" -> ", _playersOrder.Skip(_castsDone).Select(id => _module.WorldState.Actors.Find(id)?.Name ?? "???"))}");
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            int order = _playersOrder.IndexOf(pc.InstanceID);
            if (order >= _castsDone && order < _towersOrder.Count)
                arena.AddCircle(_towersOrder[order].Position, P4S2.WreathTowerRadius, arena.ColorSafe);

            var pcTetherTarget = pc.Tether.Target != 0 ? _module.WorldState.Actors.Find(pc.Tether.Target) : null;
            if (pcTetherTarget != null)
            {
                arena.AddLine(pc.Position, pcTetherTarget.Position, pc.Tether.ID == (uint)TetherID.WreathOfThorns ? arena.ColorDanger : arena.ColorSafe);
            }

            if (_playersOrder.Count < 8)
            {
                arena.AddCircle(pc.Position, _impulseAOERadius, arena.ColorDanger);
                foreach (var player in _module.Raid.WithoutSlot().Exclude(pc))
                    arena.Actor(player, GeometryUtils.PointInCircle(player.Position - pc.Position, _impulseAOERadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }
        }

        public override void OnTethered(Actor actor)
        {
            if (actor.OID == (uint)OID.Helper)
                _towersOrder.Add(actor);
        }

        public override void OnEventCast(CastEvent info)
        {
            if (info.IsSpell(AID.FleetingImpulseAOE))
                _playersOrder.Add(info.MainTargetID);
            else if (info.IsSpell(AID.AkanthaiExplodeTower))
                ++_castsDone;
        }
    }
}
