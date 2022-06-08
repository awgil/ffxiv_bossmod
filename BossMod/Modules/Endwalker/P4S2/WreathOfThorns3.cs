using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P4S2
{
    using static BossModule;

    // state related to act 3 wreath of thorns
    // note: there should be four tethered helpers on activation
    class WreathOfThorns3 : Component
    {
        public enum State { RangedTowers, Knockback, MeleeTowers, Done }

        public State CurState { get; private set; } = State.RangedTowers;
        public int NumJumps { get; private set; } = 0;
        public int NumCones { get; private set; } = 0;
        private AOEShapeCone _coneAOE = new(50, MathF.PI / 4); // not sure about half-width...
        private List<Actor> _relevantHelpers = new(); // 4 towers -> knockback -> 4 towers
        private Actor? _jumpTarget = null; // either predicted (if jump is imminent) or last actual (if cones are imminent)
        private BitMask _coneTargets;
        private BitMask _playersInAOE;

        private IEnumerable<Actor> _rangedTowers => _relevantHelpers.Take(4);
        private IEnumerable<Actor> _knockbackThorn => _relevantHelpers.Skip(4).Take(1);
        private IEnumerable<Actor> _meleeTowers => _relevantHelpers.Skip(5);

        private static float _jumpAOERadius = 10;

        public override void Update(BossModule module)
        {
            _coneTargets = _playersInAOE = new();
            if (NumCones == NumJumps)
            {
                _jumpTarget = module.Raid.WithoutSlot().SortedByRange(module.PrimaryActor.Position).LastOrDefault();
                _playersInAOE = _jumpTarget != null ? module.Raid.WithSlot().InRadiusExcluding(_jumpTarget, _jumpAOERadius).Mask() : new();
            }
            else
            {
                foreach ((int i, var player) in module.Raid.WithSlot().SortedByRange(module.PrimaryActor.Position).Take(3))
                {
                    _coneTargets.Set(i);
                    if (player.Position != module.PrimaryActor.Position)
                    {
                        var direction = Vector3.Normalize(player.Position - module.PrimaryActor.Position);
                        _playersInAOE |= module.Raid.WithSlot().Exclude(i).WhereActor(p => GeometryUtils.PointInCone(p.Position - module.PrimaryActor.Position, direction, _coneAOE.HalfAngle)).Mask();
                    }
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (CurState != State.Done)
            {
                // TODO: consider raid comps with 3+ melee or ranged...
                bool shouldSoakTower = CurState == State.RangedTowers
                    ? (actor.Role == Role.Ranged || actor.Role == Role.Healer)
                    : (actor.Role == Role.Melee || actor.Role == Role.Tank);
                var soakedTower = (CurState == State.RangedTowers ? _rangedTowers : _meleeTowers).InRadius(actor.Position, P4S2.WreathTowerRadius).FirstOrDefault();
                if (shouldSoakTower)
                {
                    hints.Add("Soak the tower!", soakedTower == null);
                }
                else if (soakedTower != null)
                {
                    hints.Add("GTFO from tower!");
                }
            }

            if (_playersInAOE[slot])
            {
                hints.Add("GTFO from aoe!");
            }
            if (NumCones == NumJumps && actor == _jumpTarget && _playersInAOE.Any())
            {
                hints.Add("GTFO from raid!");
            }
            if (NumCones != NumJumps && actor == _jumpTarget && _coneTargets[slot])
            {
                hints.Add("GTFO from boss!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_coneTargets.Any())
            {
                foreach ((_, var player) in module.Raid.WithSlot().IncludedInMask(_coneTargets))
                {
                    _coneAOE.Draw(arena, module.PrimaryActor.Position, GeometryUtils.DirectionFromVec3(player.Position - module.PrimaryActor.Position));
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach ((int i, var player) in module.Raid.WithSlot())
                arena.Actor(player, _playersInAOE[i] ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);

            if (CurState != State.Done)
            {
                foreach (var tower in (CurState == State.RangedTowers ? _rangedTowers : _meleeTowers))
                    arena.AddCircle(tower.Position, P4S2.WreathTowerRadius, arena.ColorSafe);
            }

            if (NumCones != NumJumps)
            {
                foreach ((_, var player) in module.Raid.WithSlot().IncludedInMask(_coneTargets))
                    arena.Actor(player, arena.ColorDanger);
                arena.Actor(_jumpTarget, arena.ColorVulnerable);
            }
            else if (_jumpTarget != null)
            {
                arena.Actor(_jumpTarget, arena.ColorDanger);
                arena.AddCircle(_jumpTarget.Position, _jumpAOERadius, arena.ColorDanger);
            }
        }

        public override void OnTethered(BossModule module, Actor actor)
        {
            if (actor.OID == (uint)OID.Helper && actor.Tether.ID == (uint)TetherID.WreathOfThorns)
                _relevantHelpers.Add(actor);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (CurState == State.RangedTowers && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                CurState = State.Knockback;
            else if (CurState == State.Knockback && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeKnockback))
                CurState = State.MeleeTowers;
            else if (CurState == State.MeleeTowers && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                CurState = State.Done;
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.KothornosKickJump))
            {
                ++NumJumps;
                _jumpTarget = module.WorldState.Actors.Find(info.MainTargetID);
            }
            else if (info.IsSpell(AID.KothornosQuake1) || info.IsSpell(AID.KothornosQuake2))
            {
                ++NumCones;
            }
        }
    }
}
