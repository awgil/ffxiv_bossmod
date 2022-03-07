using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to act 2 wreath of thorns
    // note: we assume that (1) dark targets soak all towers, (2) first fire to be broken is tank-healer pair (since their debuff is slightly shorter)
    class WreathOfThorns2 : Component
    {
        public enum State { DarkDesign, FirstSet, SecondSet, Done }

        public State CurState { get; private set; } = State.DarkDesign;
        private P4S _module;
        private List<Actor> _relevantHelpers = new(); // 2 aoes -> 8 towers -> 2 aoes
        private (Actor?, Actor?) _darkTH; // first is one having tether
        private (Actor?, Actor?) _fireTH;
        private (Actor?, Actor?) _fireDD;
        private IconID[] _playerIcons = new IconID[8];
        private int _numAOECasts = 0;

        private IEnumerable<Actor> _firstSet => _relevantHelpers.Take(4);
        private IEnumerable<Actor> _secondSet => _relevantHelpers.Skip(4);

        private static float _fireExplosionRadius = 6;

        public WreathOfThorns2(P4S module)
        {
            _module = module;
            // note: there should be four tethered helpers on activation
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            bool isTowerSoaker = actor == _darkTH.Item1 || actor == _darkTH.Item2;
            if (CurState == State.DarkDesign)
            {
                if (!isTowerSoaker)
                {
                    hints.Add("Stay in center", false);
                }
                else if (_darkTH.Item1!.Tether.ID != 0) // tether not broken yet
                {
                    hints.Add("Break tether!");
                }
            }
            else
            {
                var curFirePair = (_fireTH.Item1 != null && _fireTH.Item1.Tether.ID != 0) ? _fireTH : ((_fireDD.Item1 != null && _fireDD.Item1.Tether.ID != 0) ? _fireDD : (null, null));
                bool isFromCurrentPair = actor == curFirePair.Item1 || actor == curFirePair.Item2;
                if (isFromCurrentPair)
                {
                    hints.Add("Break tether!");
                }
                else if (curFirePair.Item1 != null && !isTowerSoaker)
                {
                    bool nearFire = GeometryUtils.PointInCircle(actor.Position - curFirePair.Item1!.Position, _fireExplosionRadius) || GeometryUtils.PointInCircle(actor.Position - curFirePair.Item2!.Position, _fireExplosionRadius);
                    hints.Add("Stack with breaking tether!", !nearFire);
                }

                if (CurState != State.Done)
                {
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
                    else if (soakedTower != null)
                    {
                        hints.Add("GTFO from tower!");
                    }
                }
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (CurState == State.Done)
                return;

            foreach (var aoe in (CurState == State.SecondSet ? _secondSet : _firstSet).Where(IsAOE))
                arena.ZoneCircle(aoe.Position, P4S.WreathAOERadius, arena.ColorAOE);
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            // draw players
            foreach (var player in _module.Raid.WithoutSlot().Exclude(pc))
                arena.Actor(player, arena.ColorPlayerGeneric);

            // draw pc's tether
            var pcPartner = pc.Tether.Target != 0
                ? _module.WorldState.Actors.Find(pc.Tether.Target)
                : _module.Raid.WithoutSlot().FirstOrDefault(p => p.Tether.Target == pc.InstanceID);
            if (pcPartner != null)
            {
                var tetherColor = _playerIcons[pcSlot] switch {
                    IconID.AkanthaiFire => 0xff00ffff,
                    IconID.AkanthaiWind => 0xff00ff00,
                    _ => 0xffff00ff
                };
                arena.AddLine(pc.Position, pcPartner.Position, tetherColor);
            }

            // draw towers for designated tower soakers
            bool isTowerSoaker = pc == _darkTH.Item1 || pc == _darkTH.Item2;
            if (isTowerSoaker && CurState != State.Done)
                foreach (var tower in (CurState == State.SecondSet ? _secondSet : _firstSet).Where(IsTower))
                    arena.AddCircle(tower.Position, P4S.WreathTowerRadius, CurState == State.DarkDesign ?  arena.ColorDanger : arena.ColorSafe);

            // draw circles around next imminent fire explosion
            if (CurState != State.DarkDesign)
            {
                var curFirePair = (_fireTH.Item1 != null && _fireTH.Item1.Tether.ID != 0) ? _fireTH : ((_fireDD.Item1 != null && _fireDD.Item1.Tether.ID != 0) ? _fireDD : (null, null));
                if (curFirePair.Item1 != null)
                {
                    arena.AddCircle(curFirePair.Item1!.Position, _fireExplosionRadius, isTowerSoaker ? arena.ColorDanger : arena.ColorSafe);
                    arena.AddCircle(curFirePair.Item2!.Position, _fireExplosionRadius, isTowerSoaker ? arena.ColorDanger : arena.ColorSafe);
                }
            }
        }

        public override void OnTethered(Actor actor)
        {
            if (actor.OID == (uint)OID.Helper && actor.Tether.ID == (uint)TetherID.WreathOfThorns)
            {
                _relevantHelpers.Add(actor);
            }
            else if (actor.Type == ActorType.Player)
            {
                PlayerTetherOrIconAssigned(_module.Raid.FindSlot(actor.InstanceID), actor);
            }
        }

        public override void OnCastFinished(Actor actor)
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

        private void PlayerTetherOrIconAssigned(int slot, Actor actor)
        {
            if (slot == -1 || _playerIcons[slot] == IconID.None || actor.Tether.Target == 0)
                return; // icon or tether not assigned yet

            var tetherTarget = _module.WorldState.Actors.Find(actor.Tether.Target);
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

        private bool IsTower(Actor actor)
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

        private bool IsAOE(Actor actor)
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
