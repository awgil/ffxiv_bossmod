using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P2IntermissionOrder : BossComponent
    {
        public int[] PlayerOrder = new int[PartyState.MaxPartySize];

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (PlayerOrder[slot] > 0)
                hints.Add($"Order: {PlayerOrder[slot]}", false);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID >= 79 && iconID <= 86)
            {
                int slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    PlayerOrder[slot] = (int)iconID - 78;
            }
        }
    }

    class P2IntermissionHawkBlaster : Components.GenericAOEs
    {
        private Angle _blasterStartingDirection;

        private static float _blasterOffset = 14;
        private static AOEShapeCircle _blasterShape = new(10);

        public P2IntermissionHawkBlaster() : base(ActionID.MakeSpell(AID.HawkBlasterIntermission)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in FutureBlasterCenters(module))
                yield return new(_blasterShape, c, risky: false);
            foreach (var c in ImminentBlasterCenters(module))
                yield return new(_blasterShape, c, color: ArenaColor.Danger);
        }

        // TODO: reconsider
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (movementHints != null && SafeSpotHint(module, slot) is var safespot && safespot != null)
                movementHints.Add(actor.Position, safespot.Value, ArenaColor.Safe);
        }

        // TODO: reconsider
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (SafeSpotHint(module, pcSlot) is var safespot && safespot != null)
                arena.AddCircle(safespot.Value, 1, ArenaColor.Safe);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == WatchedAction)
            {
                if (NumCasts == 0)
                {
                    var offset = spell.TargetXZ - module.Bounds.Center;
                    // a bit of a hack: most strats (lpdu etc) select a half between W and NE inclusive to the 'first' group; ensure 'starting' direction is one of these
                    bool invert = Math.Abs(offset.Z) < 2 ? offset.X > 0 : offset.Z > 0;
                    if (invert)
                        offset = -offset;
                    _blasterStartingDirection = Angle.FromDirection(offset);
                }
                ++NumCasts;
            }
        }

        // 0,1,2,3 - offset aoes, 4 - center aoe
        private int NextBlasterIndex => NumCasts switch
        {
            0 or 1 => 0,
            2 or 3 => 1,
            4 or 5 => 2,
            6 or 7 => 3,
            8 => 4,
            9 or 10 => 5,
            11 or 12 => 6,
            13 or 14 => 7,
            15 or 16 => 8,
            17 => 9,
            _ => 10
        };

        private IEnumerable<WPos> BlasterCenters(BossModule module, int index)
        {
            switch (index)
            {
                case 0: case 1: case 2: case 3:
                    {
                        var dir = (_blasterStartingDirection - index * 45.Degrees()).ToDirection();
                        yield return module.Bounds.Center + _blasterOffset * dir;
                        yield return module.Bounds.Center - _blasterOffset * dir;
                    }
                    break;
                case 5: case 6: case 7: case 8:
                    {
                        var dir = (_blasterStartingDirection - (index - 5) * 45.Degrees()).ToDirection();
                        yield return module.Bounds.Center + _blasterOffset * dir;
                        yield return module.Bounds.Center - _blasterOffset * dir;
                    }
                    break;
                case 4: case 9:
                    yield return module.Bounds.Center;
                    break;
            }
        }

        private IEnumerable<WPos> ImminentBlasterCenters(BossModule module) => NumCasts > 0 ? BlasterCenters(module, NextBlasterIndex) : Enumerable.Empty<WPos>();
        private IEnumerable<WPos> FutureBlasterCenters(BossModule module) => NumCasts > 0 ? BlasterCenters(module, NextBlasterIndex + 1) : Enumerable.Empty<WPos>();

        // TODO: reconsider
        private WPos? SafeSpotHint(BossModule module, int slot)
        {
            //var safespots = NextBlasterIndex switch
            //{
            //    1 or 2 or 3 or 4 => BlasterCenters(module, NextBlasterIndex - 1),
            //    5 => BlasterCenters(module, 3),
            //    6 or 7 or 8 => BlasterCenters(module, NextBlasterIndex - 1),
            //    _ => Enumerable.Empty<WPos>()
            //};
            if (NextBlasterIndex != 1)
                return null;

            var strategy = Service.Config.Get<TEAConfig>().P2IntermissionHints;
            if (strategy == TEAConfig.P2Intermission.None)
                return null;

            bool invert = strategy == TEAConfig.P2Intermission.FirstForOddPairs && (module.FindComponent<P2IntermissionOrder>()?.PlayerOrder[slot] is 3 or 4 or 7 or 8);
            var offset = _blasterOffset * _blasterStartingDirection.ToDirection();
            return module.Bounds.Center + (invert ? -offset : offset);
        }
    }

    class P2IntermissionKnockbacks : Components.GenericBaitAway
    {
        private enum State { Teleport, Alpha, Blasty }

        private P2IntermissionOrder? _order;
        private State _nextState;
        private Actor? _chaser;
        private WPos _prevPos;
        private DateTime _nextHit;

        private static AOEShapeCone _shapeAlpha = new(30, 45.Degrees());
        private static AOEShapeRect _shapeBlasty = new(55, 5);

        public override void Init(BossModule module)
        {
            _order = module.FindComponent<P2IntermissionOrder>();
            _chaser = module.Enemies(OID.CruiseChaser).FirstOrDefault();
            _prevPos = _chaser?.Position ?? default;
            _nextHit = module.WorldState.CurrentTime.AddSeconds(7.4f); // assumed to be created at first hawk blaster aoe; hawk blasters happen every ~2.2s
        }

        public override void Update(BossModule module)
        {
            if (_nextState == State.Teleport && _chaser != null && _chaser.Position != _prevPos)
            {
                _nextState = State.Alpha;
                _prevPos = _chaser.Position;
                SetNextBaiter(module, NumCasts + 1, _shapeAlpha);
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var playerOrder = _order != null ? _order.PlayerOrder[slot] : 0;
            if (playerOrder > NumCasts)
            {
                var hitIn = Math.Max(0, (float)(_nextHit - module.WorldState.CurrentTime).TotalSeconds);
                var hitIndex = NumCasts + 1;
                while (playerOrder > hitIndex)
                {
                    hitIn += (hitIndex & 1) != 0 ? 1.5f : 3.2f;
                    ++hitIndex;
                }
                if (hitIn < 5)
                {
                    var action = actor.Class.GetClassCategory() is ClassCategory.Healer or ClassCategory.Caster ? ActionID.MakeSpell(WHM.AID.Surecast) : ActionID.MakeSpell(WAR.AID.ArmsLength);
                    hints.PlannedActions.Add((action, actor, hitIn, false));
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.AlphaSword:
                    ++NumCasts;
                    _nextState = State.Blasty;
                    SetNextBaiter(module, NumCasts + 1, _shapeBlasty);
                    _nextHit = module.WorldState.CurrentTime.AddSeconds(1.5f);
                    break;
                case AID.SuperBlasstyCharge:
                    ++NumCasts;
                    _nextState = State.Teleport;
                    CurrentBaits.Clear();
                    _nextHit = module.WorldState.CurrentTime.AddSeconds(3.2f);
                    break;

            }
        }

        private void SetNextBaiter(BossModule module, int order, AOEShape shape)
        {
            CurrentBaits.Clear();
            int slot = _order != null ? Array.IndexOf(_order.PlayerOrder, order) : -1;
            var target = module.Raid[slot];
            if (_chaser != null && target != null)
                CurrentBaits.Add(new(_chaser, target, shape));
        }
    }
}
