using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.P3S
{
    using static BossModule;

    // state related to storms of asphodelos mechanics
    class StormsOfAsphodelos : Component
    {
        private P3S _module;
        private List<WorldState.Actor> _twisters;
        private List<int> _twisterTargets = new();
        private ulong _tetherTargets = 0;
        private ulong _bossTargets = 0;
        private ulong _closeToTetherTarget = 0;
        private ulong _hitByMultipleAOEs = 0;

        private static float _coneHalfAngle = MathF.PI / 6; // not sure about this!!!
        private static float _beaconRadius = 6;

        public StormsOfAsphodelos(P3S module)
        {
            _module = module;
            _twisters = module.Enemies(OID.DarkblazeTwister);
        }

        public override void Update()
        {
            _twisterTargets.Clear();
            _tetherTargets = _bossTargets = _closeToTetherTarget = _hitByMultipleAOEs = 0;
            var boss = _module.Boss();
            if (boss == null)
                return;

            // we determine failing players, trying to take two reasonable tactics in account:
            // either two tanks immune and soak everything, or each player is hit by one mechanic
            // for now, we consider tether target to be a "tank"
            int[] aoesPerPlayer = new int[_module.RaidMembers.Length];

            foreach ((int i, var player) in _module.RaidMembers.WithSlot(true).WhereActor(x => x.Tether.Target == boss.InstanceID))
            {
                BitVector.SetVector64Bit(ref _tetherTargets, i);

                ++aoesPerPlayer[i];
                foreach ((int j, var other) in _module.RaidMembers.WithSlot().InRadiusExcluding(player, _beaconRadius))
                {
                    ++aoesPerPlayer[j];
                    BitVector.SetVector64Bit(ref _closeToTetherTarget, j);
                }
            }

            float cosHalfAngle = MathF.Cos(_coneHalfAngle);
            foreach ((int i, var player) in _module.RaidMembers.WithSlot().SortedByRange(boss.Position).Take(3))
            {
                BitVector.SetVector64Bit(ref _bossTargets, i);
                foreach ((int j, var other) in FindPlayersInWinds(boss.Position, player, cosHalfAngle))
                {
                    ++aoesPerPlayer[j];
                }
            }

            foreach (var twister in _twisters)
            {
                (var i, var player) = _module.RaidMembers.WithSlot().SortedByRange(twister.Position).FirstOrDefault();
                if (player == null)
                {
                    _twisterTargets.Add(-1);
                    continue;
                }

                _twisterTargets.Add(i);
                foreach ((int j, var other) in FindPlayersInWinds(twister.Position, player, cosHalfAngle))
                {
                    ++aoesPerPlayer[j];
                }
            }

            for (int i = 0; i < aoesPerPlayer.Length; ++i)
                if (aoesPerPlayer[i] > 1)
                    BitVector.SetVector64Bit(ref _hitByMultipleAOEs, i);
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
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

        public override void DrawArenaBackground(MiniArena arena)
        {
            var boss = _module.Boss();
            if (boss == null)
                return;

            foreach ((int i, var player) in _module.RaidMembers.WithSlot())
            {
                if (BitVector.IsVector64BitSet(_tetherTargets, i))
                {
                    arena.ZoneCircle(player.Position, _beaconRadius, arena.ColorAOE);
                }
                if (BitVector.IsVector64BitSet(_bossTargets, i) && player.Position != boss.Position)
                {
                    var offset = player.Position - boss.Position;
                    float phi = MathF.Atan2(offset.X, offset.Z);
                    arena.ZoneCone(boss.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, arena.ColorAOE);
                }
            }

            foreach ((var twister, int i) in _twisters.Zip(_twisterTargets))
            {
                var player = _module.RaidMember(i); // not sure if twister could really have invalid target, but let's be safe...
                if (player == null || player.Position == twister.Position)
                    continue;

                var offset = player.Position - twister.Position;
                float phi = MathF.Atan2(offset.X, offset.Z);
                arena.ZoneCone(twister.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, arena.ColorAOE);
            }
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            foreach (var twister in _twisters)
            {
                arena.Actor(twister, arena.ColorEnemy);
            }

            foreach ((int i, var player) in _module.RaidMembers.WithSlot())
            {
                bool active = BitVector.IsVector64BitSet(_tetherTargets | _bossTargets, i) || _twisterTargets.Contains(i);
                bool failing = BitVector.IsVector64BitSet(_hitByMultipleAOEs | _closeToTetherTarget, i);
                arena.Actor(player, active ? arena.ColorDanger : (failing ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric));
            }
        }

        private IEnumerable<(int, WorldState.Actor)> FindPlayersInWinds(Vector3 origin, WorldState.Actor target, float cosHalfAngle)
        {
            var dir = Vector3.Normalize(target.Position - origin);
            return _module.RaidMembers.WithSlot().WhereActor(player => Vector3.Dot(dir, Vector3.Normalize(player.Position - origin)) >= cosHalfAngle);
        }
    }
}
