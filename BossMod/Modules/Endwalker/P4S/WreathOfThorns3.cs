using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to act 3 wreath of thorns
    class WreathOfThorns3 : Component
    {
        public enum State { RangedTowers, Knockback, MeleeTowers, Done }

        public State CurState { get; private set; } = State.RangedTowers;
        public int NumJumps { get; private set; } = 0;
        public int NumCones { get; private set; } = 0;
        private P4S _module;
        private List<WorldState.Actor> _relevantHelpers = new(); // 4 towers -> knockback -> 4 towers
        private WorldState.Actor? _jumpTarget = null; // either predicted (if jump is imminent) or last actual (if cones are imminent)
        private ulong _coneTargets = 0;
        private ulong _playersInAOE = 0;

        private IEnumerable<WorldState.Actor> _rangedTowers => _relevantHelpers.Take(4);
        private IEnumerable<WorldState.Actor> _knockbackThorn => _relevantHelpers.Skip(4).Take(1);
        private IEnumerable<WorldState.Actor> _meleeTowers => _relevantHelpers.Skip(5);

        private static float _jumpAOERadius = 10;
        private static float _coneHalfAngle = MathF.PI / 4; // not sure about this...

        public WreathOfThorns3(P4S module)
        {
            _module = module;
            // note: there should be four tethered helpers on activation
        }

        public override void Update()
        {
            _coneTargets = _playersInAOE = 0;
            var boss = _module.Boss2();
            if (boss == null)
                return;

            if (NumCones == NumJumps)
            {
                _jumpTarget = _module.RaidMembers.WithoutSlot().SortedByRange(boss.Position).LastOrDefault();
                _playersInAOE = _jumpTarget != null ? _module.RaidMembers.WithSlot().InRadiusExcluding(_jumpTarget, _jumpAOERadius).Mask() : 0;
            }
            else
            {
                foreach ((int i, var player) in _module.RaidMembers.WithSlot().SortedByRange(boss.Position).Take(3))
                {
                    BitVector.SetVector64Bit(ref _coneTargets, i);
                    if (player.Position != boss.Position)
                    {
                        var direction = Vector3.Normalize(player.Position - boss.Position);
                        _playersInAOE |= _module.RaidMembers.WithSlot().Exclude(i).WhereActor(p => GeometryUtils.PointInCone(p.Position - boss.Position, direction, _coneHalfAngle)).Mask();
                    }
                }
            }
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (CurState != State.Done)
            {
                // TODO: consider raid comps with 3+ melee or ranged...
                bool shouldSoakTower = CurState == State.RangedTowers
                    ? (actor.Role == Role.Ranged || actor.Role == Role.Healer)
                    : (actor.Role == Role.Melee || actor.Role == Role.Tank);
                var soakedTower = (CurState == State.RangedTowers ? _rangedTowers : _meleeTowers).InRadius(actor.Position, P4S.WreathTowerRadius).FirstOrDefault();
                if (shouldSoakTower)
                {
                    hints.Add("Soak the tower!", soakedTower == null);
                }
                else if (soakedTower != null)
                {
                    hints.Add("GTFO from tower!");
                }
            }

            if (BitVector.IsVector64BitSet(_playersInAOE, slot))
            {
                hints.Add("GTFO from aoe!");
            }
            if (NumCones == NumJumps && actor == _jumpTarget && _playersInAOE != 0)
            {
                hints.Add("GTFO from raid!");
            }
            if (NumCones != NumJumps && actor == _jumpTarget && BitVector.IsVector64BitSet(_coneTargets, slot))
            {
                hints.Add("GTFO from boss!");
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            var boss = _module.Boss2();
            if (_coneTargets != 0 && boss != null)
            {
                foreach ((_, var player) in _module.RaidMembers.WithSlot().IncludedInMask(_coneTargets).WhereActor(x => boss.Position != x.Position))
                {
                    var offset = player.Position - boss.Position;
                    float phi = MathF.Atan2(offset.X, offset.Z);
                    arena.ZoneCone(boss.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, arena.ColorAOE);
                }
            }
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            foreach ((int i, var player) in _module.RaidMembers.WithSlot())
                arena.Actor(player, BitVector.IsVector64BitSet(_playersInAOE, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);

            if (CurState != State.Done)
            {
                foreach (var tower in (CurState == State.RangedTowers ? _rangedTowers : _meleeTowers))
                    arena.AddCircle(tower.Position, P4S.WreathTowerRadius, arena.ColorSafe);
            }

            if (NumCones != NumJumps)
            {
                foreach ((_, var player) in _module.RaidMembers.WithSlot().IncludedInMask(_coneTargets))
                    arena.Actor(player, arena.ColorDanger);
                arena.Actor(_jumpTarget, arena.ColorVulnerable);
            }
            else if (_jumpTarget != null)
            {
                arena.Actor(_jumpTarget, arena.ColorDanger);
                arena.AddCircle(_jumpTarget.Position, _jumpAOERadius, arena.ColorDanger);
            }
        }

        public override void OnTethered(WorldState.Actor actor)
        {
            if (actor.OID == (uint)OID.Helper && actor.Tether.ID == (uint)TetherID.WreathOfThorns)
                _relevantHelpers.Add(actor);
        }

        public override void OnCastFinished(WorldState.Actor actor)
        {
            if (CurState == State.RangedTowers && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                CurState = State.Knockback;
            else if (CurState == State.Knockback && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeKnockback))
                CurState = State.MeleeTowers;
            else if (CurState == State.MeleeTowers && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                CurState = State.Done;
        }

        public override void OnEventCast(WorldState.CastResult info)
        {
            if (info.IsSpell(AID.KothornosKickJump))
            {
                ++NumJumps;
                _jumpTarget = _module.WorldState.FindActor(info.MainTargetID);
            }
            else if (info.IsSpell(AID.KothornosQuake1) || info.IsSpell(AID.KothornosQuake2))
            {
                ++NumCones;
            }
        }
    }
}
