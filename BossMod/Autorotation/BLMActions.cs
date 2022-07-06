using Dalamud.Game.ClientState.JobGauge.Types;
using ImGuiNET;
using System;
using System.Linq;

namespace BossMod
{
    class BLMActions : CommonActions
    {
        private BLMConfig _config;
        private BLMRotation.State _state;
        private BLMRotation.Strategy _strategy;
        private ActionID _nextBestSTAction = ActionID.MakeSpell(BLMRotation.AID.Blizzard1);
        private ActionID _nextBestAOEAction = ActionID.MakeSpell(BLMRotation.AID.Blizzard2);

        // TODO: this should be done by framework (speculative effect application for actions that we didn't get EffectResult for yet)
        private DateTime _lastThunderSpeculation;
        private ulong _lastThunderTarget;

        public BLMActions(Autorotation autorot)
            : base(autorot, ActionID.MakeSpell(BLMRotation.AID.Blizzard1))
        {
            _config = Service.Config.Get<BLMConfig>();
            _state = BuildState();
            _strategy = new();

            SmartQueueRegisterSpell(BLMRotation.AID.Swiftcast);
            SmartQueueRegisterSpell(BLMRotation.AID.Surecast);
            SmartQueueRegister(CommonRotation.IDSprint);
            //SmartQueueRegister(BLMRotation.IDStatPotion);
        }

        protected override void OnCastSucceeded(ActionID actionID, ulong targetID)
        {
            Log($"Cast {actionID} @ {targetID:X}, next-best={_nextBestSTAction}/{_nextBestAOEAction} [{_state}]");
            // hack
            if (actionID.Type == ActionType.Spell && (BLMRotation.AID)actionID.ID is BLMRotation.AID.Thunder1)
            {
                _lastThunderSpeculation = Autorot.WorldState.CurrentTime.AddMilliseconds(1000);
                _lastThunderTarget = targetID;
            }
        }

        protected override CommonRotation.State OnUpdate()
        {
            var currState = BuildState();
            LogStateChange(_state, currState);
            _state = currState;

            FillCommonStrategy(_strategy, BLMRotation.IDStatPotion);

            // cooldown execution
            _strategy.ExecuteSwiftcast = SmartQueueActiveSpell(BLMRotation.AID.Swiftcast);
            _strategy.ExecuteSurecast = SmartQueueActiveSpell(BLMRotation.AID.Surecast);

            var nextBestST = _config.FullRotation ? BLMRotation.GetNextBestAction(_state, _strategy, false) : ActionID.MakeSpell(BLMRotation.AID.Blizzard1);
            var nextBestAOE = _config.FullRotation ? BLMRotation.GetNextBestAction(_state, _strategy, true) : ActionID.MakeSpell(BLMRotation.AID.Blizzard2);
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
                actionID = (BLMRotation.AID)actionID.ID switch
                {
                    BLMRotation.AID.Blizzard1 => _config.FullRotation ? _nextBestSTAction : actionID,
                    BLMRotation.AID.Blizzard2 => _config.FullRotation ? _nextBestAOEAction : actionID,
                    _ => actionID
                };
            }
            ulong targetID = actionID.Type == ActionType.Spell ? (BLMRotation.AID)actionID.ID switch
            {
                _ => targets.MainTarget
            } : targets.MainTarget;
            return (actionID, targetID);
        }

        public override AIResult CalculateBestAction(Actor player, Actor primaryTarget)
        {
            // TODO: proper implementation...
            bool useAOE = _state.UnlockedBlizzard2 && Autorot.PotentialTargetsInRange(primaryTarget.Position, 5).Count() > 2;
            var action = useAOE ? _nextBestAOEAction : _nextBestSTAction;
            return new() { Action = action, Target = primaryTarget, ReadyIn = Math.Max(ActionCooldown(action), _state.AnimationLock) };
        }

        public override void DrawOverlay()
        {
            ImGui.TextUnformatted($"Next: {BLMRotation.ActionShortString(_nextBestSTAction)} / {BLMRotation.ActionShortString(_nextBestAOEAction)}");
            ImGui.TextUnformatted(_strategy.ToString());
            ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
        }

        private BLMRotation.State BuildState()
        {
            BLMRotation.State s = new();
            var player = Autorot.WorldState.Party.Player();
            if (player != null)
            {
                FillCommonState(s, player, BLMRotation.IDStatPotion);
                var gauge = Service.JobGauges.Get<BLMGauge>();
                s.ElementalLevel = gauge.InAstralFire ? gauge.AstralFireStacks : -gauge.UmbralIceStacks;
                s.ElementalLeft = gauge.ElementTimeRemaining * 0.001f;

                //foreach (var status in player.Statuses)
                //{
                //    switch ((BLMRotation.SID)status.ID)
                //    {
                //        case BLMRotation.SID.Swiftcast:
                //            s.SwiftcastLeft = StatusDuration(status.ExpireAt);
                //            break;
                //        case BLMRotation.SID.ThinAir:
                //            s.ThinAirLeft = StatusDuration(status.ExpireAt);
                //            break;
                //        case BLMRotation.SID.Freecure:
                //            s.FreecureLeft = StatusDuration(status.ExpireAt);
                //            break;
                //        case BLMRotation.SID.Medica2:
                //            if (status.SourceID == player.InstanceID)
                //                s.MedicaLeft = StatusDuration(status.ExpireAt);
                //            break;
                //    }
                //}

                var target = Autorot.WorldState.Actors.Find(player.TargetID);
                if (target != null)
                {
                    foreach (var status in target.Statuses)
                    {
                        switch ((BLMRotation.SID)status.ID)
                        {
                            case BLMRotation.SID.Thunder1:
                                if (status.SourceID == player.InstanceID)
                                    s.TargetThunderLeft = StatusDuration(status.ExpireAt);
                                break;
                        }
                    }
                }
                if (s.TargetThunderLeft == 0 && _lastThunderSpeculation > Autorot.WorldState.CurrentTime && _lastThunderTarget == player.TargetID)
                {
                    s.TargetThunderLeft = 21;
                }

                s.TransposeCD = SpellCooldown(BLMRotation.AID.Transpose);
            }
            return s;
        }

        private void LogStateChange(BLMRotation.State prev, BLMRotation.State curr)
        {
            // do nothing if not in combat
            if (!(Autorot.WorldState.Party.Player()?.InCombat ?? false))
                return;

            // detect expired buffs
            if (curr.ElementalLeft == 0 && prev.ElementalLeft != 0 && prev.ElementalLeft < 1)
                Log($"Expired elemental [{curr}]");
        }
    }
}
