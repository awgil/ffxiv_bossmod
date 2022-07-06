using ImGuiNET;
using System;
using System.Linq;

namespace BossMod
{
    class MNKActions : CommonActions
    {
        private MNKConfig _config;
        private MNKRotation.State _state;
        private MNKRotation.Strategy _strategy;
        private ActionID _nextBestSTAction = ActionID.MakeSpell(MNKRotation.AID.Bootshine);
        private ActionID _nextBestAOEAction = ActionID.MakeSpell(MNKRotation.AID.ArmOfTheDestroyer);

        public MNKActions(Autorotation autorot)
            : base(autorot, ActionID.MakeSpell(MNKRotation.AID.Bootshine))
        {
            _config = Service.Config.Get<MNKConfig>();
            _state = BuildState();
            _strategy = new();

            SmartQueueRegisterSpell(MNKRotation.AID.ArmsLength);
            SmartQueueRegister(CommonRotation.IDSprint);
            SmartQueueRegister(MNKRotation.IDStatPotion);
        }

        protected override void OnCastSucceeded(ActionID actionID, ulong targetID)
        {
            Log($"Cast {actionID} @ {targetID:X}, next-best={_nextBestSTAction}/{_nextBestAOEAction} [{_state}]");
        }

        protected override CommonRotation.State OnUpdate()
        {
            var currState = BuildState();
            LogStateChange(_state, currState);
            _state = currState;

            FillCommonStrategy(_strategy, MNKRotation.IDStatPotion);

            // cooldown execution
            _strategy.ExecuteArmsLength = SmartQueueActiveSpell(MNKRotation.AID.ArmsLength);

            var nextBestST = _config.FullRotation ? MNKRotation.GetNextBestAction(_state, _strategy, false) : ActionID.MakeSpell(MNKRotation.AID.Bootshine);
            var nextBestAOE = _config.FullRotation ? MNKRotation.GetNextBestAction(_state, _strategy, true) : ActionID.MakeSpell(MNKRotation.AID.ArmOfTheDestroyer);
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
                actionID = (MNKRotation.AID)actionID.ID switch
                {
                    MNKRotation.AID.Bootshine => _config.FullRotation ? _nextBestSTAction : actionID,
                    MNKRotation.AID.ArmOfTheDestroyer => _config.FullRotation ? _nextBestAOEAction : actionID,
                    _ => actionID
                };
            }
            ulong targetID = actionID.Type == ActionType.Spell ? (MNKRotation.AID)actionID.ID switch
            {
                _ => targets.MainTarget
            } : targets.MainTarget;
            return (actionID, targetID);
        }

        public override AIResult CalculateBestAction(Actor player, Actor primaryTarget)
        {
            // TODO: not all our aoe moves are radius 5 point-blank...
            bool useAOE = _state.UnlockedArmOfTheDestroyer && Autorot.PotentialTargetsInRangeFromPlayer(5).Count() > 2;
            var action = useAOE ? _nextBestAOEAction : _nextBestSTAction;
            return new() { Action = action, Target = primaryTarget, ReadyIn = Math.Max(ActionCooldown(action), _state.AnimationLock), Positional = Positional.Flank };
        }

        public override void DrawOverlay()
        {
            ImGui.TextUnformatted($"Next: {MNKRotation.ActionShortString(_nextBestSTAction)} / {MNKRotation.ActionShortString(_nextBestAOEAction)}");
            ImGui.TextUnformatted(_strategy.ToString());
            ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
        }

        private MNKRotation.State BuildState()
        {
            MNKRotation.State s = new();
            var player = Autorot.WorldState.Party.Player();
            if (player != null)
            {
                FillCommonState(s, player, MNKRotation.IDStatPotion);
                //s.Gauge = Service.JobGauges.Get<MNKGauge>().OathGauge;

                foreach (var status in player.Statuses)
                {
                    switch ((MNKRotation.SID)status.ID)
                    {
                        case MNKRotation.SID.OpoOpoForm:
                            s.Form = MNKRotation.Form.OpoOpo;
                            s.FormLeft = StatusDuration(status.ExpireAt);
                            break;
                        case MNKRotation.SID.RaptorForm:
                            s.Form = MNKRotation.Form.Raptor;
                            s.FormLeft = StatusDuration(status.ExpireAt);
                            break;
                        case MNKRotation.SID.CoeurlForm:
                            s.Form = MNKRotation.Form.Coeurl;
                            s.FormLeft = StatusDuration(status.ExpireAt);
                            break;
                    }
                }

                s.ArmsLengthCD = SpellCooldown(MNKRotation.AID.ArmsLength);
            }
            return s;
        }

        private void LogStateChange(MNKRotation.State prev, MNKRotation.State curr)
        {
            // do nothing if not in combat
            if (!(Autorot.WorldState.Party.Player()?.InCombat ?? false))
                return;

            // detect expired buffs
            if (curr.Form == MNKRotation.Form.None && prev.Form != MNKRotation.Form.None)
                Log($"Dropped form [{curr}]");
        }
    }
}
