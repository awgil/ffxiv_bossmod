using Dalamud.Game.ClientState.JobGauge.Types;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    class SMNActions : CommonActions
    {
        private SMNConfig _config;
        private SMNRotation.State _state;
        private SMNRotation.Strategy _strategy;
        private ActionID _nextBestSTAction = ActionID.MakeSpell(SMNRotation.AID.Ruin1);
        private ActionID _nextBestAOEAction = ActionID.MakeSpell(SMNRotation.AID.Outburst);

        public SMNActions(Autorotation autorot, Actor player)
            : base(autorot, player)
        {
            _config = Service.Config.Get<SMNConfig>();
            _state = BuildState(autorot.WorldState.Actors.Find(player.TargetID));
            _strategy = new();

            SmartQueueRegisterSpell(SMNRotation.AID.RadiantAegis);
            SmartQueueRegisterSpell(SMNRotation.AID.Swiftcast);
            SmartQueueRegisterSpell(SMNRotation.AID.Surecast);
            SmartQueueRegisterSpell(SMNRotation.AID.Addle);
            SmartQueueRegister(CommonRotation.IDSprint);
            //SmartQueueRegister(SMNRotation.IDStatPotion);
        }

        protected override void OnCastSucceeded(ActorCastEvent ev)
        {
            Log($"Cast {ev.Action} @ {ev.MainTargetID:X}, next-best={_nextBestSTAction}/{_nextBestAOEAction} [{_state}]");
        }

        protected override CommonRotation.PlayerState OnUpdate(Actor? target, bool moving)
        {
            var currState = BuildState(target);
            LogStateChange(_state, currState);
            _state = currState;

            FillCommonStrategy(_strategy, SMNRotation.IDStatPotion);

            // cooldown execution
            _strategy.ExecuteRadiantAegis = SmartQueueActiveSpell(SMNRotation.AID.RadiantAegis);
            _strategy.ExecuteSwiftcast = SmartQueueActiveSpell(SMNRotation.AID.Swiftcast);
            _strategy.ExecuteSurecast = SmartQueueActiveSpell(SMNRotation.AID.Surecast);
            _strategy.ExecuteAddle = SmartQueueActiveSpell(SMNRotation.AID.Addle);

            var nextBestST = _config.FullRotation ? SMNRotation.GetNextBestAction(_state, _strategy, false, moving) : ActionID.MakeSpell(SMNRotation.AID.Ruin1);
            var nextBestAOE = _config.FullRotation ? SMNRotation.GetNextBestAction(_state, _strategy, true, moving) : ActionID.MakeSpell(SMNRotation.AID.Outburst);
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
                actionID = (SMNRotation.AID)actionID.ID switch
                {
                    SMNRotation.AID.Ruin1 => _config.FullRotation ? _nextBestSTAction : actionID,
                    SMNRotation.AID.Outburst => _config.FullRotation ? _nextBestAOEAction : actionID,
                    _ => actionID
                };
            }
            ulong targetID = actionID.Type == ActionType.Spell ? (SMNRotation.AID)actionID.ID switch
            {
                _ => targets.MainTarget
            } : targets.MainTarget;
            return (actionID, targetID);
        }

        public override AIResult CalculateBestAction(Actor player, Actor? primaryTarget, bool moving)
        {
            if (_strategy.Prepull && _state.UnlockedSummonCarbuncle && !_state.PetSummoned)
            {
                return new() { Action = ActionID.MakeSpell(SMNRotation.AID.SummonCarbuncle), Target = player };
            }
            else if (primaryTarget == null)
            {
                return new();
            }
            else if (primaryTarget.Type == ActorType.Enemy)
            {
                // TODO: proper implementation...
                bool useAOE = _state.UnlockedOutburst && Autorot.PotentialTargetsInRange(primaryTarget.Position, 5).Count() > 2;
                var action = SMNRotation.GetNextBestAction(_state, _strategy, useAOE, moving);
                return action ? new() { Action = action, Target = primaryTarget } : new();
            }
            else if (!moving && primaryTarget.IsDead)
            {
                return new() { Action = ActionID.MakeSpell(SmartResurrectAction()), Target = primaryTarget };
            }
            else if (primaryTarget.InCombat)
            {
                // TODO: this aoe/st heal selection is not very good...
                return new() { Action = _state.UnlockedPhysick ? ActionID.MakeSpell(SMNRotation.AID.Physick) : new(), Target = primaryTarget };
            }
            else
            {
                return new();
            }
        }

        public override void DrawOverlay()
        {
            ImGui.TextUnformatted($"Next: {SMNRotation.ActionShortString(_nextBestSTAction)} / {SMNRotation.ActionShortString(_nextBestAOEAction)}");
            ImGui.TextUnformatted(_strategy.ToString());
            ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
        }

        private SMNRotation.State BuildState(Actor? target)
        {
            SMNRotation.State s = new();
            FillCommonPlayerState(s, target, SMNRotation.IDStatPotion);

            s.PetSummoned = Autorot.WorldState.Actors.Any(a => a.Type == ActorType.Pet && a.OwnerID == Player.InstanceID);

            var gauge = Service.JobGauges.Get<SMNGauge>();
            s.IfritReady = gauge.IsIfritReady;
            s.TitanReady = gauge.IsTitanReady;
            s.GarudaReady = gauge.IsGarudaReady;
            s.Attunement = (SMNRotation.Attunement)(((int)gauge.AetherFlags >> 2) & 3);
            s.AttunementStacks = gauge.Attunement;
            s.AttunementLeft = gauge.AttunmentTimerRemaining * 0.001f;
            s.SummonLockLeft = gauge.SummonTimerRemaining * 0.001f;
            s.AetherflowStacks = gauge.AetherflowStacks;

            foreach (var status in Player.Statuses)
            {
                switch ((SMNRotation.SID)status.ID)
                {
                    case SMNRotation.SID.Swiftcast:
                        s.SwiftcastLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }

            //if (target != null)
            //{
            //    foreach (var status in target.Statuses)
            //    {
            //        switch ((SMNRotation.SID)status.ID)
            //        {
            //            case SMNRotation.SID.Thunder1:
            //                if (status.SourceID == Player.InstanceID)
            //                    s.TargetThunderLeft = StatusDuration(status.ExpireAt);
            //                break;
            //        }
            //    }
            //}

            return s;
        }

        private void LogStateChange(SMNRotation.State prev, SMNRotation.State curr)
        {
            // do nothing if not in combat
            if (!Player.InCombat)
                return;

            // detect expired buffs
            //if (curr.ElementalLeft == 0 && prev.ElementalLeft != 0 && prev.ElementalLeft < 1)
            //    Log($"Expired elemental [{curr}]");
        }

        private SMNRotation.AID SmartResurrectAction()
        {
            // 1. swiftcast, if ready and not up yet
            if (_state.UnlockedSwiftcast && _state.SwiftcastLeft <= 0 && _state.SwiftcastCD <= 0)
                return SMNRotation.AID.Swiftcast;

            return SMNRotation.AID.Resurrection;
        }
    }
}
