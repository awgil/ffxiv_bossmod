using System;
using System.Linq;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to act 4 wreath of thorns
    // note: we assume that aoes are popped in waymark order...
    // TODO: show concrete position hint for blue breaks (e.g. assume dark move 1/8 CW and blue move 3/8 CW)
    class WreathOfThorns4 : Component
    {
        private P4S _module;
        private IconID[] _playerIcons = new IconID[8];
        private WorldState.Actor?[] _playerTetherSource = new WorldState.Actor?[8];
        private int _doneTowers = 0;
        private int _doneAOEs = 0;

        private static float _waterExplosionRange = 10;

        public WreathOfThorns4(P4S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_doneTowers < 4)
            {
                if (_playerIcons[slot] == IconID.AkanthaiWater)
                {
                    hints.Add("Break tether!");
                    if (_module.Raid.WithoutSlot().InRadiusExcluding(actor, _waterExplosionRange).Any())
                    {
                        hints.Add("GTFO from others!");
                    }
                }
                else
                {
                    var soakedTower = _playerTetherSource.Zip(_playerIcons).Where(si => si.Item1 != null && si.Item2 == IconID.AkanthaiWater).Select(si => si.Item1!).InRadius(actor.Position, P4S.WreathTowerRadius).FirstOrDefault();
                    hints.Add("Soak the tower!", soakedTower == null);
                }
            }
            else
            {
                var nextAOE = NextAOE();
                if (nextAOE != null)
                {
                    if (nextAOE.Tether.Target == actor.InstanceID)
                    {
                        hints.Add("Break tether!");
                    }
                    if (GeometryUtils.PointInCircle(actor.Position - nextAOE.Position, P4S.WreathAOERadius))
                    {
                        hints.Add("GTFO from AOE!");
                    }
                }
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            if (_doneTowers < 4)
                return;

            var nextAOE = NextAOE();
            if (nextAOE != null)
                arena.ZoneCircle(nextAOE.Position, P4S.WreathAOERadius, arena.ColorAOE);
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            var pc = _module.Player();
            foreach (((var player, var icon), var source) in _module.Raid.Members.Zip(_playerIcons).Zip(_playerTetherSource))
            {
                arena.Actor(player, arena.ColorPlayerGeneric);
                if (player == null || source == null)
                    continue;

                // tower soak & explosion
                if (icon == IconID.AkanthaiWater)
                {
                    arena.AddCircle(source.Position, P4S.WreathTowerRadius, arena.ColorSafe);
                    arena.AddCircle(player.Position, _waterExplosionRange, arena.ColorDanger);
                }

                // tether
                if (player == pc)
                {
                    arena.AddLine(player.Position, source.Position, icon == IconID.AkanthaiWater ? 0xffff8000 : 0xffff00ff);
                }
            }
        }

        public override void OnTethered(WorldState.Actor actor)
        {
            if (actor.OID == (uint)OID.Helper)
            {
                var slot = _module.Raid.FindSlot(actor.Tether.Target);
                if (slot >= 0)
                    _playerTetherSource[slot] = actor;
            }
        }

        public override void OnUntethered(WorldState.Actor actor)
        {
            if (actor.OID == (uint)OID.Helper)
            {
                var slot = _module.Raid.FindSlot(actor.Tether.Target);
                if (slot >= 0)
                    _playerTetherSource[slot] = null;
            }
        }

        public override void OnCastFinished(WorldState.Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                ++_doneTowers;
            if (actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE))
                ++_doneAOEs;
        }

        public override void OnEventIcon(uint actorID, uint iconID)
        {
            var slot = _module.Raid.FindSlot(actorID);
            if (slot >= 0)
                _playerIcons[slot] = (IconID)iconID;
        }

        private WorldState.Actor? NextAOE()
        {
            // note: this is quite a hack to be honest...
            var pos = _doneAOEs < 4 ? _module.WorldState.GetWaymark((WorldState.Waymark)((int)WorldState.Waymark.N1 + _doneAOEs)) : null;
            return pos != null ? _playerTetherSource.Zip(_playerIcons).Where(si => si.Item1 != null && si.Item2 == IconID.AkanthaiDark).Select(si => si.Item1!).MinBy(s => (s.Position - pos.Value).LengthSquared()) : null;
        }
    }
}
