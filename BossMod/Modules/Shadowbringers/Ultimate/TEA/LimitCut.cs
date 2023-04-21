using System;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class LimitCut : Components.GenericBaitAway
    {
        private enum State { Teleport, Alpha, Blasty }

        public int[] PlayerOrder = new int[PartyState.MaxPartySize];
        private float _alphaDelay;
        private State _nextState;
        private Actor? _chaser;
        private WPos _prevPos;
        private DateTime _nextHit;

        private static AOEShapeCone _shapeAlpha = new(30, 45.Degrees());
        private static AOEShapeRect _shapeBlasty = new(55, 5);

        public LimitCut(float alphaDelay)
        {
            _alphaDelay = alphaDelay;
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

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (PlayerOrder[slot] > 0)
                hints.Add($"Order: {PlayerOrder[slot]}", false);
            base.AddHints(module, slot, actor, hints, movementHints);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (PlayerOrder[slot] > NumCasts)
            {
                var hitIn = Math.Max(0, (float)(_nextHit - module.WorldState.CurrentTime).TotalSeconds);
                var hitIndex = NumCasts + 1;
                while (PlayerOrder[slot] > hitIndex)
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

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID >= 79 && iconID <= 86)
            {
                int slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    PlayerOrder[slot] = (int)iconID - 78;

                if (_chaser == null)
                {
                    // initialize baits on first icon; note that icons appear over ~300ms
                    _chaser = ((TEA)module).CruiseChaser();
                    _prevPos = _chaser?.Position ?? default;
                    _nextHit = module.WorldState.CurrentTime.AddSeconds(9.5f);
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.AlphaSwordP2:
                    ++NumCasts;
                    _nextState = State.Blasty;
                    SetNextBaiter(module, NumCasts + 1, _shapeBlasty);
                    _nextHit = module.WorldState.CurrentTime.AddSeconds(1.5f);
                    break;
                case AID.SuperBlasstyChargeP2:
                case AID.SuperBlasstyChargeP3:
                    ++NumCasts;
                    _nextState = State.Teleport;
                    CurrentBaits.Clear();
                    _nextHit = module.WorldState.CurrentTime.AddSeconds(_alphaDelay);
                    break;

            }
        }

        private void SetNextBaiter(BossModule module, int order, AOEShape shape)
        {
            CurrentBaits.Clear();
            var target = module.Raid[Array.IndexOf(PlayerOrder, order)];
            if (_chaser != null && target != null)
                CurrentBaits.Add(new(_chaser, target, shape));
        }
    }
}
