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
        private List<Actor> _twisters;
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

            // we determine failing players, trying to take two reasonable tactics in account:
            // either two tanks immune and soak everything, or each player is hit by one mechanic
            // for now, we consider tether target to be a "tank"
            int[] aoesPerPlayer = new int[PartyState.MaxSize];

            foreach ((int i, var player) in _module.Raid.WithSlot(true).WhereActor(x => x.Tether.Target == _module.PrimaryActor.InstanceID))
            {
                BitVector.SetVector64Bit(ref _tetherTargets, i);

                ++aoesPerPlayer[i];
                foreach ((int j, var other) in _module.Raid.WithSlot().InRadiusExcluding(player, _beaconRadius))
                {
                    ++aoesPerPlayer[j];
                    BitVector.SetVector64Bit(ref _closeToTetherTarget, j);
                }
            }

            float cosHalfAngle = MathF.Cos(_coneHalfAngle);
            foreach ((int i, var player) in _module.Raid.WithSlot().SortedByRange(_module.PrimaryActor.Position).Take(3))
            {
                BitVector.SetVector64Bit(ref _bossTargets, i);
                foreach ((int j, var other) in FindPlayersInWinds(_module.PrimaryActor.Position, player, cosHalfAngle))
                {
                    ++aoesPerPlayer[j];
                }
            }

            foreach (var twister in _twisters)
            {
                (var i, var player) = _module.Raid.WithSlot().SortedByRange(twister.Position).FirstOrDefault();
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

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
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

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            foreach ((int i, var player) in _module.Raid.WithSlot())
            {
                if (BitVector.IsVector64BitSet(_tetherTargets, i))
                {
                    arena.ZoneCircle(player.Position, _beaconRadius, arena.ColorAOE);
                }
                if (BitVector.IsVector64BitSet(_bossTargets, i) && player.Position != _module.PrimaryActor.Position)
                {
                    var offset = player.Position - _module.PrimaryActor.Position;
                    float phi = MathF.Atan2(offset.X, offset.Z);
                    arena.ZoneCone(_module.PrimaryActor.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, arena.ColorAOE);
                }
            }

            foreach ((var twister, int i) in _twisters.Zip(_twisterTargets))
            {
                var player = _module.Raid[i]; // not sure if twister could really have invalid target, but let's be safe...
                if (player == null || player.Position == twister.Position)
                    continue;

                var offset = player.Position - twister.Position;
                float phi = MathF.Atan2(offset.X, offset.Z);
                arena.ZoneCone(twister.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, arena.ColorAOE);
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var twister in _twisters)
            {
                arena.Actor(twister, arena.ColorEnemy);
            }

            foreach ((int i, var player) in _module.Raid.WithSlot())
            {
                bool active = BitVector.IsVector64BitSet(_tetherTargets | _bossTargets, i) || _twisterTargets.Contains(i);
                bool failing = BitVector.IsVector64BitSet(_hitByMultipleAOEs | _closeToTetherTarget, i);
                arena.Actor(player, active ? arena.ColorDanger : (failing ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric));
            }
        }

        private IEnumerable<(int, Actor)> FindPlayersInWinds(Vector3 origin, Actor target, float cosHalfAngle)
        {
            var dir = Vector3.Normalize(target.Position - origin);
            return _module.Raid.WithSlot().WhereActor(player => Vector3.Dot(dir, Vector3.Normalize(player.Position - origin)) >= cosHalfAngle);
        }
    }
}
