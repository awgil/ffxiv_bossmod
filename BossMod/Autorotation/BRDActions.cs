using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    class BRDActions : CommonActions
    {
        private BRDConfig _config;
        private BRDRotation.State _state;
        private BRDRotation.Strategy _strategy;
        private ActionID _nextBestSTAction = ActionID.MakeSpell(BRDRotation.AID.HeavyShot);
        private ActionID _nextBestAOEAction = ActionID.MakeSpell(BRDRotation.AID.QuickNock);

        public BRDActions(Autorotation autorot)
            : base(autorot, ActionID.MakeSpell(BRDRotation.AID.HeavyShot))
        {
            _config = Service.Config.Get<BRDConfig>();
            var player = autorot.WorldState.Party.Player();
            _state = player != null ? BuildState(player) : new();
            _strategy = new();

            SmartQueueRegisterSpell(BRDRotation.AID.ArmsLength);
            SmartQueueRegisterSpell(BRDRotation.AID.SecondWind);
            SmartQueueRegisterSpell(BRDRotation.AID.HeadGraze);
            SmartQueueRegister(CommonRotation.IDSprint);
            SmartQueueRegister(BRDRotation.IDStatPotion);
        }

        protected override void OnCastSucceeded(ActionID actionID, ulong targetID)
        {
            Log($"Cast {actionID} @ {targetID:X}, next-best={_nextBestSTAction}/{_nextBestAOEAction} [{_state}]");
        }

        protected override CommonRotation.State OnUpdate(Actor player)
        {
            var currState = BuildState(player);
            LogStateChange(_state, currState);
            _state = currState;

            FillCommonStrategy(_strategy, BRDRotation.IDStatPotion);

            // cooldown execution
            _strategy.ExecuteArmsLength = SmartQueueActiveSpell(BRDRotation.AID.ArmsLength);
            _strategy.ExecuteSecondWind = SmartQueueActiveSpell(BRDRotation.AID.SecondWind) && player.HP.Cur < player.HP.Max;
            _strategy.ExecuteHeadGraze = SmartQueueActiveSpell(BRDRotation.AID.HeadGraze);

            var nextBestST = _config.FullRotation ? BRDRotation.GetNextBestAction(_state, _strategy, false) : ActionID.MakeSpell(BRDRotation.AID.HeavyShot);
            var nextBestAOE = _config.FullRotation ? BRDRotation.GetNextBestAction(_state, _strategy, true) : ActionID.MakeSpell(BRDRotation.AID.QuickNock);
            if (_nextBestSTAction != nextBestST || _nextBestAOEAction != nextBestAOE)
            {
                Log($"Next-best changed: ST={_nextBestSTAction}->{nextBestST}, AOE={_nextBestAOEAction}->{nextBestAOE} [{_state}]");
                _nextBestSTAction = nextBestST;
                _nextBestAOEAction = nextBestAOE;
            }
            return _state;
        }

        protected override (ActionID, ulong) DoReplaceActionAndTarget(ActionID actionID, Targets targets)
        {
            if (actionID.Type == ActionType.Spell)
            {
                actionID = (BRDRotation.AID)actionID.ID switch
                {
                    BRDRotation.AID.HeavyShot => _config.FullRotation ? _nextBestSTAction : actionID,
                    BRDRotation.AID.QuickNock => _config.FullRotation ? _nextBestAOEAction : actionID,
                    _ => actionID
                };
            }
            ulong targetID = actionID.Type == ActionType.Spell ? (BRDRotation.AID)actionID.ID switch
            {
                _ => targets.MainTarget
            } : targets.MainTarget;
            return (actionID, targetID);
        }

        public override AIResult CalculateBestAction(Actor player, Actor? primaryTarget)
        {
            if (_strategy.Prepull && _state.UnlockedPeloton && _state.PelotonCD == 0 && _state.PelotonLeft == 0)
            {
                return new() { Action = ActionID.MakeSpell(BRDRotation.AID.Peloton), Target = player };
            }
            else if (primaryTarget != null)
            {
                // TODO: proper implementation, cone etc...
                bool useAOE = _state.UnlockedQuickNock && Autorot.PotentialTargetsInRange(primaryTarget.Position, 5).Count() > 1;
                var action = useAOE ? _nextBestAOEAction : _nextBestSTAction;
                return action ? new() { Action = action, Target = primaryTarget } : new();
            }
            else
            {
                return new();
            }
        }

        public override void DrawOverlay()
        {
            ImGui.TextUnformatted($"Next: {BRDRotation.ActionShortString(_nextBestSTAction)} / {BRDRotation.ActionShortString(_nextBestAOEAction)}");
            ImGui.TextUnformatted(_strategy.ToString());
            ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
        }

        private BRDRotation.State BuildState(Actor player)
        {
            BRDRotation.State s = new();
            FillCommonState(s, player, BRDRotation.IDStatPotion);

            //s.Chakra = Service.JobGauges.Get<BRDGauge>().Chakra;

            foreach (var status in player.Statuses)
            {
                switch ((BRDRotation.SID)status.ID)
                {
                    case BRDRotation.SID.StraightShotReady:
                        s.StraightShotLeft = StatusDuration(status.ExpireAt);
                        break;
                    case BRDRotation.SID.RagingStrikes:
                        s.RagingStrikesLeft = StatusDuration(status.ExpireAt);
                        break;
                    case BRDRotation.SID.Barrage:
                        s.BarrageLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }

            var target = Autorot.WorldState.Actors.Find(player.TargetID);
            if (target != null)
            {
                foreach (var status in target.Statuses)
                {
                    switch ((BRDRotation.SID)status.ID)
                    {
                        case BRDRotation.SID.VenomousBite:
                        case BRDRotation.SID.CausticBite:
                            if (status.SourceID == player.InstanceID)
                                s.TargetVenomousLeft = StatusDuration(status.ExpireAt);
                            break;
                        case BRDRotation.SID.Windbite:
                        case BRDRotation.SID.Stormbite:
                            if (status.SourceID == player.InstanceID)
                                s.TargetWindbiteLeft = StatusDuration(status.ExpireAt);
                            break;
                    }
                }
            }

            s.ArmsLengthCD = SpellCooldown(BRDRotation.AID.ArmsLength);
            s.SecondWindCD = SpellCooldown(BRDRotation.AID.SecondWind);
            s.HeadGrazeCD = SpellCooldown(BRDRotation.AID.HeadGraze);
            s.PelotonCD = SpellCooldown(BRDRotation.AID.Peloton);
            return s;
        }

        private void LogStateChange(BRDRotation.State prev, BRDRotation.State curr)
        {
            // do nothing if not in combat
            if (!(Autorot.WorldState.Party.Player()?.InCombat ?? false))
                return;

            // detect expired buffs
            //if (curr.Form == BRDRotation.Form.None && prev.Form != BRDRotation.Form.None)
            //    Log($"Dropped form [{curr}]");
        }
    }
}
