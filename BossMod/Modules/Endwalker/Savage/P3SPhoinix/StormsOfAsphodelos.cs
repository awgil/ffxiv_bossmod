using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P3SPhoinix
{
    // state related to storms of asphodelos mechanics
    class StormsOfAsphodelos : BossComponent
    {
        private AOEShapeCone _windsAOE = new(50, 30.Degrees());
        private AOEShapeCircle _beaconAOE = new(6);
        private List<Actor> _twisterTargets = new();
        private BitMask _tetherTargets;
        private BitMask _bossTargets;
        private BitMask _closeToTetherTarget;
        private BitMask _hitByMultipleAOEs;

        public override void Update(BossModule module)
        {
            _twisterTargets.Clear();
            _tetherTargets = _bossTargets = _closeToTetherTarget = _hitByMultipleAOEs = new();

            // we determine failing players, trying to take two reasonable tactics in account:
            // either two tanks immune and soak everything, or each player is hit by one mechanic
            // for now, we consider tether target to be a "tank"
            int[] aoesPerPlayer = new int[PartyState.MaxPartySize];

            foreach ((int i, var player) in module.Raid.WithSlot(true).WhereActor(x => x.Tether.Target == module.PrimaryActor.InstanceID))
            {
                _tetherTargets.Set(i);

                ++aoesPerPlayer[i];
                foreach ((int j, var other) in module.Raid.WithSlot().InRadiusExcluding(player, _beaconAOE.Radius))
                {
                    ++aoesPerPlayer[j];
                    _closeToTetherTarget.Set(j);
                }
            }

            foreach ((int i, var player) in module.Raid.WithSlot().SortedByRange(module.PrimaryActor.Position).Take(3))
            {
                _bossTargets.Set(i);
                foreach ((int j, var other) in FindPlayersInWinds(module, module.PrimaryActor, player))
                {
                    ++aoesPerPlayer[j];
                }
            }

            foreach (var twister in module.Enemies(OID.DarkblazeTwister))
            {
                var target = module.Raid.WithoutSlot().Closest(twister.Position);
                if (target == null)
                    continue; // there are no alive players - target list will be left empty

                _twisterTargets.Add(target);
                foreach ((int j, var other) in FindPlayersInWinds(module, twister, target))
                {
                    ++aoesPerPlayer[j];
                }
            }

            for (int i = 0; i < aoesPerPlayer.Length; ++i)
                if (aoesPerPlayer[i] > 1)
                    _hitByMultipleAOEs.Set(i);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (actor.Role == Role.Tank)
            {
                if (!_tetherTargets[slot])
                {
                    hints.Add("Intercept tether!");
                }
                if (_hitByMultipleAOEs[slot])
                {
                    hints.Add("Press invul!");
                }
            }
            else
            {
                if (_tetherTargets[slot])
                {
                    hints.Add("Pass the tether!");
                }
                if (_hitByMultipleAOEs[slot])
                {
                    hints.Add("GTFO from aoes!");
                }
            }
            if (_closeToTetherTarget[slot])
            {
                hints.Add("GTFO from tether!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach ((int i, var player) in module.Raid.WithSlot())
            {
                if (_tetherTargets[i])
                {
                    _beaconAOE.Draw(arena, player);
                }
                if (_bossTargets[i] && player.Position != module.PrimaryActor.Position)
                {
                    _windsAOE.Draw(arena, module.PrimaryActor.Position, Angle.FromDirection(player.Position - module.PrimaryActor.Position));
                }
            }

            foreach (var (twister, target) in module.Enemies(OID.DarkblazeTwister).Zip(_twisterTargets))
            {
                _windsAOE.Draw(arena, twister.Position, Angle.FromDirection(target.Position - twister.Position));
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var twister in module.Enemies(OID.DarkblazeTwister))
            {
                arena.Actor(twister, ArenaColor.Enemy, true);
            }

            foreach ((int i, var player) in module.Raid.WithSlot())
            {
                bool tethered = _tetherTargets[i];
                if (tethered)
                    arena.AddLine(module.PrimaryActor.Position, player.Position, player.Role == Role.Tank ? ArenaColor.Safe : ArenaColor.Danger);
                bool active = tethered || _bossTargets[i] || _twisterTargets.Contains(player);
                bool failing = (_hitByMultipleAOEs | _closeToTetherTarget)[i];
                arena.Actor(player, active ? ArenaColor.Danger : (failing ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric));
            }
        }

        private IEnumerable<(int, Actor)> FindPlayersInWinds(BossModule module, Actor origin, Actor target)
        {
            return module.Raid.WithSlot().InShape(_windsAOE, origin.Position, Angle.FromDirection(target.Position - origin.Position));
        }
    }
}
