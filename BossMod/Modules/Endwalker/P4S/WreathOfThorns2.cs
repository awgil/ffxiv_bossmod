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

        public State CurState { get; private set; } = State.DarkDesign;
        private P4S _module;
        private List<WorldState.Actor> _relevantHelpers = new(); // 2 aoes -> 8 towers -> 2 aoes
        private (WorldState.Actor?, WorldState.Actor?) _darkTH; // first is one having tether
        private (WorldState.Actor?, WorldState.Actor?) _fireTH;
        private (WorldState.Actor?, WorldState.Actor?) _fireDD;
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

            bool isTowerSoaker = actor == _darkTH.Item1 || actor == _darkTH.Item2;
            if (CurState == State.DarkDesign)
            {
                if (!isTowerSoaker)
                {
                    hints.Add("Stay in center", false);
                }
                else if (_darkTH.Item1!.Tether.ID != 0)
                {
                    hints.Add("Break tether!");
                }
            }
            else
            {
                var curFirePair = CurState == State.FirstSet ? _fireTH : _fireDD;
                bool curFirePairUnbroken = curFirePair.Item1 != null && curFirePair.Item1.Tether.ID != 0;
                bool isFromCurrentPair = actor == curFirePair.Item1 || actor == curFirePair.Item2;
                if (isFromCurrentPair && curFirePairUnbroken)
                {
                    hints.Add("Break tether!");
                }

                var relevantHelpers = CurState == State.FirstSet ? _firstSet : _secondSet;
                if (relevantHelpers.Where(IsAOE).InRadius(actor.Position, P4S.WreathAOERadius).Any())
                {
                    hints.Add("GTFO from AOE!");
                }

                var soakedTower = relevantHelpers.Where(IsTower).InRadius(actor.Position, P4S.WreathTowerRadius).FirstOrDefault();
                if (isTowerSoaker)
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

                    if (!isFromCurrentPair && curFirePairUnbroken)
                    {
                        bool nearFire = GeometryUtils.PointInCircle(actor.Position - curFirePair.Item1!.Position, _fireExplosionRadius) || GeometryUtils.PointInCircle(actor.Position - curFirePair.Item2!.Position, _fireExplosionRadius);
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
            var pc = _module.Player();
            if (pc == null || CurState == State.Done)
                return;

            // draw players and their tethers
            foreach ((int i, var player) in _module.Raid.WithSlot())
            {
                arena.Actor(player, arena.ColorPlayerGeneric);
                var tetherTarget = player.Tether.Target != 0 ? _module.WorldState.FindActor(player.Tether.Target) : null;
                if (tetherTarget != null)
                {
                    bool breaking = player.Tether.ID == (uint)TetherID.WreathOfThorns;
                    arena.AddLine(player.Position, tetherTarget.Position, breaking ? arena.ColorDanger : arena.ColorSafe);
                }
            }

            // draw towers for designated tower soakers
            bool isTowerSoaker = pc == _darkTH.Item1 || pc == _darkTH.Item2;
            if (isTowerSoaker)
                foreach (var tower in (CurState == State.SecondSet ? _secondSet : _firstSet).Where(IsTower))
                    arena.AddCircle(tower.Position, P4S.WreathTowerRadius, CurState == State.DarkDesign ?  arena.ColorDanger : arena.ColorSafe);

            // draw circles around next imminent fire explosion
            if (CurState != State.DarkDesign)
            {
                var curFirePair = CurState == State.FirstSet ? _fireTH : _fireDD;
                bool curFirePairUnbroken = curFirePair.Item1 != null && curFirePair.Item1.Tether.ID != 0;
                if (curFirePairUnbroken)
                {
                    arena.AddCircle(curFirePair.Item1!.Position, _fireExplosionRadius, isTowerSoaker ? arena.ColorDanger : arena.ColorSafe);
                    arena.AddCircle(curFirePair.Item2!.Position, _fireExplosionRadius, isTowerSoaker ? arena.ColorDanger : arena.ColorSafe);
                }
            }
        }

        public override void OnTethered(WorldState.Actor actor)
        {
            if (actor.OID == (uint)OID.Helper && actor.Tether.ID == (uint)TetherID.WreathOfThorns)
            {
                _relevantHelpers.Add(actor);
            }
            else if (actor.Type == WorldState.ActorType.Player)
            {
                PlayerTetherOrIconAssigned(_module.Raid.FindSlot(actor.InstanceID), actor);
            }
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
            var slot = _module.Raid.FindSlot(actorID);
            if (slot == -1)
                return;

            _playerIcons[slot] = (IconID)iconID;
            PlayerTetherOrIconAssigned(slot, _module.Raid[slot]!);
        }

        private void PlayerTetherOrIconAssigned(int slot, WorldState.Actor actor)
        {
            if (slot == -1 || _playerIcons[slot] == IconID.None || actor.Tether.Target == 0)
                return; // icon or tether not assigned yet

            var tetherTarget = _module.WorldState.FindActor(actor.Tether.Target);
            if (tetherTarget == null)
                return; // weird

            if (_playerIcons[slot] == IconID.AkanthaiDark)
            {
                _darkTH = (actor, tetherTarget);
            }
            else if (_playerIcons[slot] == IconID.AkanthaiFire)
            {
                if (actor.Role == Role.Tank || actor.Role == Role.Healer)
                    _fireTH = (actor, tetherTarget);
                else
                    _fireDD = (actor, tetherTarget);
            }
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
    }
}
