using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to act 2 wreath of thorns
    // note: we assume that (1) dark targets soak all towers, (2) first fire to be broken is tank-healer pair (since their debuff is slightly shorter)
    // theoretically second set of towers could be soaked by first fire pair, but whatever...
    class WreathOfThorns2 : Component
    {
        public enum State { DarkDesign, FirstSet, SecondSet, Done }
        public enum TetherType { None, Wind, FireDD, FireTH, Dark } // sorted in break priority order

        public State CurState { get; private set; } = State.DarkDesign;
        private P4S _module;
        private List<WorldState.Actor> _relevantHelpers = new(); // 2 aoes -> 8 towers -> 2 aoes
        private IconID[] _playerIcons = new IconID[8];
        private int _numAOECasts = 0;

        private IEnumerable<WorldState.Actor> _firstSet => _relevantHelpers.Take(4);
        private IEnumerable<WorldState.Actor> _secondSet => _relevantHelpers.Skip(4);

        private static float _fireExplosionRadius = 6;

        public WreathOfThorns2(P4S module)
        {
            _module = module;
            // note: there should be four tethered helpers on activation
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (CurState == State.Done)
                return;

            if (CurState == State.DarkDesign)
            {
                var darkSource = _module.RaidMembers.WithSlot().WhereSlot(i => _playerIcons[i] == IconID.AkanthaiDark).FirstOrDefault().Item2;
                if (actor == darkSource || actor.InstanceID == darkSource?.Tether.Target)
                {
                    hints.Add("Break tether!");
                }
                else
                {
                    hints.Add("Stay in center", false);
                }
            }
            else
            {
                (var nextTetherSource, var nextTetherType) = ActiveTethers().MaxBy(pt => pt.Item2);
                bool isNextTether = actor == nextTetherSource || actor.InstanceID == nextTetherSource?.Tether.Target;
                if (isNextTether)
                {
                    hints.Add("Break tether!");
                }

                var relevantHelpers = CurState == State.FirstSet ? _firstSet : _secondSet;
                if (relevantHelpers.Where(IsAOE).InRadius(actor.Position, P4S.WreathAOERadius).Any())
                {
                    hints.Add("GTFO from AOE!");
                }

                var soakedTower = relevantHelpers.Where(IsTower).InRadius(actor.Position, P4S.WreathTowerRadius).FirstOrDefault();
                if (_playerIcons[slot] != IconID.AkanthaiWind && _playerIcons[slot] != IconID.AkanthaiFire)
                {
                    // note: we're assuming that players with 'dark' soak all towers
                    hints.Add("Soak the tower!", soakedTower == null);
                }
                else
                {
                    if (soakedTower != null)
                    {
                        hints.Add("GTFO from tower!");
                    }

                    if (!isNextTether && nextTetherSource != null && (nextTetherType == TetherType.FireDD || nextTetherType == TetherType.FireTH))
                    {
                        bool nearFire = GeometryUtils.PointInCircle(nextTetherSource.Position - actor.Position, _fireExplosionRadius);
                        if (!nearFire)
                        {
                            var nextTetherTarget = _module.WorldState.FindActor(nextTetherSource.Tether.Target);
                            nearFire = nextTetherTarget != null && GeometryUtils.PointInCircle(nextTetherTarget.Position - actor.Position, _fireExplosionRadius);
                        }
                        hints.Add("Stack with breaking tether!", !nearFire);
                    }
                }
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            if (CurState == State.Done)
                return;

            foreach (var aoe in (CurState == State.SecondSet ? _secondSet : _firstSet).Where(IsAOE))
                arena.ZoneCircle(aoe.Position, P4S.WreathAOERadius, arena.ColorAOE);
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            if (CurState == State.Done)
                return;

            foreach (var tower in (CurState == State.SecondSet ? _secondSet : _firstSet).Where(IsTower))
                arena.AddCircle(tower.Position, P4S.WreathTowerRadius, arena.ColorSafe);

            foreach ((int i, var player) in _module.RaidMembers.WithSlot())
            {
                arena.Actor(player, arena.ColorPlayerGeneric);
                var tetherTarget = player.Tether.Target != 0 ? _module.WorldState.FindActor(player.Tether.Target) : null;
                if (tetherTarget != null)
                {
                    bool breaking = player.Tether.ID == (uint)TetherID.WreathOfThorns;
                    arena.AddLine(player.Position, tetherTarget.Position, breaking ? arena.ColorDanger : arena.ColorSafe);
                    if (breaking && _playerIcons[i] == IconID.AkanthaiFire)
                    {
                        arena.AddCircle(player.Position, _fireExplosionRadius, arena.ColorDanger);
                        arena.AddCircle(tetherTarget.Position, _fireExplosionRadius, arena.ColorDanger);
                    }
                }
            }
        }

        public override void OnTethered(WorldState.Actor actor)
        {
            if (actor.OID == (uint)OID.Helper && actor.Tether.ID == (uint)TetherID.WreathOfThorns)
                _relevantHelpers.Add(actor);
        }

        public override void OnCastFinished(WorldState.Actor actor)
        {
            if (CurState == State.DarkDesign && actor.CastInfo!.IsSpell(AID.DarkDesign))
                CurState = State.FirstSet;
            else if (CurState == State.FirstSet && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE) && ++_numAOECasts >= 2)
                CurState = State.SecondSet;
            else if (CurState == State.SecondSet && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE) && ++_numAOECasts >= 4)
                CurState = State.Done;
        }

        public override void OnEventIcon(uint actorID, uint iconID)
        {
            var slot = _module.RaidMembers.FindSlot(actorID);
            if (slot >= 0)
                _playerIcons[slot] = (IconID)iconID;
        }

        private bool IsTower(WorldState.Actor actor)
        {
            if (actor.Position.X < 90)
                return actor.Position.Z > 100;
            else if (actor.Position.Z < 90)
                return actor.Position.X < 100;
            else if (actor.Position.X > 110)
                return actor.Position.Z < 100;
            else if (actor.Position.Z > 110)
                return actor.Position.X > 100;
            else
                return false;
        }

        private bool IsAOE(WorldState.Actor actor)
        {
            if (actor.Position.X < 90)
                return actor.Position.Z < 100;
            else if (actor.Position.Z < 90)
                return actor.Position.X > 100;
            else if (actor.Position.X > 110)
                return actor.Position.Z > 100;
            else if (actor.Position.Z > 110)
                return actor.Position.X < 100;
            else
                return false;
        }

        private TetherType TetherPriority(WorldState.Actor player, IconID icon)
        {
            return icon switch
            {
                IconID.AkanthaiDark => TetherType.Dark,
                IconID.AkanthaiWind => TetherType.Wind,
                IconID.AkanthaiFire => (player.Role == Role.Tank || player.Role == Role.Healer) ? TetherType.FireTH : TetherType.FireDD,
                _ => TetherType.None
            };
        }

        private IEnumerable<(WorldState.Actor, TetherType)> ActiveTethers()
        {
            return _module.RaidMembers.WithSlot()
                .Select(ip => (ip.Item2, TetherPriority(ip.Item2, _playerIcons[ip.Item1])))
                .Where(it => it.Item2 != TetherType.None);
        }
    }
}
