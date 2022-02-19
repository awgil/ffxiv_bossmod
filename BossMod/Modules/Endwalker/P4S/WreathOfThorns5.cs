using System.Collections.Generic;
using System.Linq;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to act 5 (finale) wreath of thorns
    class WreathOfThorns5 : Component
    {
        private P4S _module;
        private List<uint> _playersOrder = new();
        private List<WorldState.Actor> _towersOrder = new();
        private int _castsDone = 0;

        private static float _impulseAOERadius = 5;

        public WreathOfThorns5(P4S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            int order = _playersOrder.IndexOf(actor.InstanceID);
            if (order >= 0)
            {
                hints.Add($"Order: {order}", false);

                if (order >= _castsDone && order < _towersOrder.Count)
                {
                    hints.Add("Soak tower!", !GeometryUtils.PointInCircle(actor.Position - _towersOrder[order].Position, P4S.WreathTowerRadius));
                }
            }

            if (_playersOrder.Count < 8)
            {
                hints.Add("Spread!", _module.Raid.WithoutSlot().InRadiusExcluding(actor, _impulseAOERadius).Any());
            }
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            var pc = _module.Player();
            if (pc == null)
                return;

            int order = _playersOrder.IndexOf(pc.InstanceID);
            if (order >= _castsDone && order < _towersOrder.Count)
                arena.AddCircle(_towersOrder[order].Position, P4S.WreathTowerRadius, arena.ColorSafe);

            if (_playersOrder.Count < 8)
            {
                arena.AddCircle(pc.Position, _impulseAOERadius, arena.ColorDanger);
                foreach (var player in _module.Raid.WithoutSlot().Exclude(pc))
                    arena.Actor(player, GeometryUtils.PointInCircle(player.Position - pc.Position, _impulseAOERadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }
        }

        public override void OnTethered(WorldState.Actor actor)
        {
            if (actor.OID == (uint)OID.Helper)
                _towersOrder.Add(actor);
        }

        public override void OnEventCast(WorldState.CastResult info)
        {
            if (info.IsSpell(AID.FleetingImpulseAOE))
                _playersOrder.Add(info.MainTargetID);
            else if (info.IsSpell(AID.AkanthaiExplodeTower))
                ++_castsDone;
        }
    }
}
