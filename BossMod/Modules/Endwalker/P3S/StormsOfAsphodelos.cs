using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to storms of asphodelos mechanics
    class StormsOfAsphodelos : Component
    {
        private AOEShapeCone _windsAOE = new(50, MathF.PI / 6);
        private AOEShapeCircle _beaconAOE = new(6);
        private List<Actor> _twisterTargets = new();
        private ulong _tetherTargets = 0;
        private ulong _bossTargets = 0;
        private ulong _closeToTetherTarget = 0;
        private ulong _hitByMultipleAOEs = 0;

        public override void Update(BossModule module)
        {
            _twisterTargets.Clear();
            _tetherTargets = _bossTargets = _closeToTetherTarget = _hitByMultipleAOEs = 0;

            // we determine failing players, trying to take two reasonable tactics in account:
            // either two tanks immune and soak everything, or each player is hit by one mechanic
            // for now, we consider tether target to be a "tank"
            int[] aoesPerPlayer = new int[PartyState.MaxSize];

            foreach ((int i, var player) in module.Raid.WithSlot(true).WhereActor(x => x.Tether.Target == module.PrimaryActor.InstanceID))
            {
                BitVector.SetVector64Bit(ref _tetherTargets, i);

                ++aoesPerPlayer[i];
                foreach ((int j, var other) in module.Raid.WithSlot().InRadiusExcluding(player, _beaconAOE.Radius))
                {
                    ++aoesPerPlayer[j];
                    BitVector.SetVector64Bit(ref _closeToTetherTarget, j);
                }
            }

            foreach ((int i, var player) in module.Raid.WithSlot().SortedByRange(module.PrimaryActor.Position).Take(3))
            {
                BitVector.SetVector64Bit(ref _bossTargets, i);
                foreach ((int j, var other) in FindPlayersInWinds(module, module.PrimaryActor, player))
                {
                    ++aoesPerPlayer[j];
                }
            }

            foreach (var twister in module.Enemies(OID.DarkblazeTwister))
            {
                var target = module.Raid.WithoutSlot().MinBy(a => (a.Position - twister.Position).LengthSquared());
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
                    BitVector.SetVector64Bit(ref _hitByMultipleAOEs, i);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            bool tethered = BitVector.IsVector64BitSet(_tetherTargets, slot);
            bool hitByMultipleAOEs = BitVector.IsVector64BitSet(_hitByMultipleAOEs, slot);
            if (actor.Role == Role.Tank)
            {
                if (!tethered)
                {
                    hints.Add("Intercept tether!");
                }
                if (hitByMultipleAOEs)
                {
                    hints.Add("Press invul!");
                }
            }
            else
            {
                if (tethered)
                {
                    hints.Add("Pass the tether!");
                }
                if (hitByMultipleAOEs)
                {
                    hints.Add("GTFO from aoes!");
                }
            }
            if (BitVector.IsVector64BitSet(_closeToTetherTarget, slot))
            {
                hints.Add("GTFO from tether!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach ((int i, var player) in module.Raid.WithSlot())
            {
                if (BitVector.IsVector64BitSet(_tetherTargets, i))
                {
                    _beaconAOE.Draw(arena, player);
                }
                if (BitVector.IsVector64BitSet(_bossTargets, i) && player.Position != module.PrimaryActor.Position)
                {
                    _windsAOE.Draw(arena, module.PrimaryActor.Position, GeometryUtils.DirectionFromVec3(player.Position - module.PrimaryActor.Position));
                }
            }

            foreach (var (twister, target) in module.Enemies(OID.DarkblazeTwister).Zip(_twisterTargets))
            {
                _windsAOE.Draw(arena, twister.Position, GeometryUtils.DirectionFromVec3(target.Position - twister.Position));
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var twister in module.Enemies(OID.DarkblazeTwister))
            {
                arena.Actor(twister, arena.ColorEnemy);
            }

            foreach ((int i, var player) in module.Raid.WithSlot())
            {
                bool tethered = BitVector.IsVector64BitSet(_tetherTargets, i);
                if (tethered)
                    arena.AddLine(module.PrimaryActor.Position, player.Position, player.Role == Role.Tank ? arena.ColorSafe : arena.ColorDanger);
                bool active = tethered || BitVector.IsVector64BitSet(_bossTargets, i) || _twisterTargets.Contains(player);
                bool failing = BitVector.IsVector64BitSet(_hitByMultipleAOEs | _closeToTetherTarget, i);
                arena.Actor(player, active ? arena.ColorDanger : (failing ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric));
            }
        }

        private IEnumerable<(int, Actor)> FindPlayersInWinds(BossModule module, Actor origin, Actor target)
        {
            return module.Raid.WithSlot().InShape(_windsAOE, origin.Position, GeometryUtils.DirectionFromVec3(target.Position - origin.Position));
        }
    }
}
