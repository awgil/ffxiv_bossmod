using Dalamud.Game.ClientState.JobGauge.Types;
using ImGuiNET;
using System;
using System.Linq;

namespace BossMod.WAR
{
    class Actions : CommonActions
    {
        private WARConfig _config;
        private QuestLockCheck _lock;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;
        private ActionID _nextBestSTAction = ActionID.MakeSpell(AID.HeavySwing);
        private ActionID _nextBestAOEAction = ActionID.MakeSpell(AID.Overpower);
        private bool _justCast;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player)
        {
            _config = Service.Config.Get<WARConfig>();
            _lock = new(Definitions.QuestsPerLevel);
            _state = BuildState(autorot.WorldState.Actors.Find(player.TargetID));
            _strategy = new()
            {
                FirstChargeIn = 0.01f, // by default, always preserve 1 onslaught charge
                SecondChargeIn = 10000, // ... but don't preserve second
            };

            SmartQueueRegisterSpell(AID.Rampart);
            SmartQueueRegisterSpell(AID.Vengeance);
            SmartQueueRegisterSpell(AID.ThrillOfBattle);
            SmartQueueRegisterSpell(AID.Holmgang);
            SmartQueueRegisterSpell(AID.Equilibrium);
            SmartQueueRegisterSpell(AID.Reprisal);
            SmartQueueRegisterSpell(AID.ShakeItOff);
            SmartQueueRegisterSpell(AID.RawIntuition);
            SmartQueueRegisterSpell(AID.NascentFlash);
            SmartQueueRegisterSpell(AID.Bloodwhetting);
            SmartQueueRegisterSpell(AID.ArmsLength);
            SmartQueueRegisterSpell(AID.Provoke);
            SmartQueueRegisterSpell(AID.Shirk);
            SmartQueueRegisterSpell(AID.LowBlow);
            SmartQueueRegisterSpell(AID.Interject);
            SmartQueueRegister(CommonDefinitions.IDSprint);
            SmartQueueRegister(CommonDefinitions.IDPotionStr);
        }

        protected override void OnCastSucceeded(ActorCastEvent ev)
        {
            string comment = "";
            if (ev.Action.Type == ActionType.Spell)
            {
                switch ((AID)ev.Action.ID)
                {
                    case AID.HeavySwing:
                        if (_state.ComboLastMove == AID.HeavySwing || _state.ComboLastMove == AID.Maim)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        break;
                    case AID.Maim:
                        if (_state.ComboLastMove != AID.HeavySwing)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.Gauge > 90)
                            comment += $", mistake=overcap-gauge";
                        break;
                    case AID.StormPath:
                        if (_state.ComboLastMove != AID.Maim)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.Gauge > 80)
                            comment += $", mistake=overcap-gauge";
                        if (_state.SurgingTempestLeft <= 0)
                            comment += $", mistake=no-st";
                        break;
                    case AID.StormEye:
                        if (_state.ComboLastMove != AID.Maim)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.Gauge > 90)
                            comment += $", mistake=overcap-gauge";
                        if (_state.SurgingTempestLeft > 30)
                            comment += $", mistake=overcap-st";
                        break;
                    case AID.FellCleave:
                        comment += _state.InnerReleaseStacks > 0 ? ", spent IR stack" : ", spent gauge";
                        if (_state.CD(CDGroup.Infuriate) < 5)
                            comment += $", mistake=overcap-infuriate";
                        if (_state.SurgingTempestLeft <= 0)
                            comment += $", mistake=no-st";
                        break;
                    case AID.InnerChaos:
                        if (_state.CD(CDGroup.Infuriate) < 5)
                            comment += $", mistake=overcap-infuriate";
                        if (_state.SurgingTempestLeft <= 0)
                            comment += $", mistake=no-st";
                        break;
                    case AID.Overpower:
                        if (_state.ComboLastMove == AID.Overpower)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        break;
                    case AID.MythrilTempest:
                        if (_state.ComboLastMove != AID.Overpower)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.Gauge > 80)
                            comment += $", mistake=overcap-gauge";
                        break;
                    case AID.Infuriate:
                        if (_state.Gauge > 50)
                            comment += $", mistake=overcap-gauge";
                        if (_state.NascentChaosLeft > 0)
                            comment += $", mistake=overwrite-nc";
                        break;
                    case AID.Onslaught:
                        // note: onslaught without ST is not really a mistake...
                        break;
                    case AID.Upheaval:
                        if (_state.SurgingTempestLeft <= 0)
                            comment += $", mistake=no-st";
                        break;
                    case AID.InnerRelease:
                        if (_state.SurgingTempestLeft > 50)
                            comment += $", mistake=overcap-st";
                        break;
                    case AID.Tomahawk:
                        break;
                }
            }
            Log($"Cast {ev.Action} @ {ev.MainTargetID:X}, next-best={_nextBestSTAction}/{_nextBestAOEAction}{comment} [{_state}]");
            _justCast = true;
        }

        protected override CommonRotation.PlayerState OnUpdate(Actor? target, bool moving)
        {
            var currState = BuildState(target);
            LogStateChange(_state, currState);
            _state = currState;

            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);

            // cooldown execution
            _strategy.ExecuteRampart = SmartQueueActiveSpell(AID.Rampart);
            _strategy.ExecuteVengeance = SmartQueueActiveSpell(AID.Vengeance);
            _strategy.ExecuteThrillOfBattle = SmartQueueActiveSpell(AID.ThrillOfBattle);
            _strategy.ExecuteHolmgang = SmartQueueActiveSpell(AID.Holmgang);
            _strategy.ExecuteEquilibrium = SmartQueueActiveSpell(AID.Equilibrium) && Player.HP.Cur < Player.HP.Max;
            _strategy.ExecuteReprisal = SmartQueueActiveSpell(AID.Reprisal) && AllowReprisal();
            _strategy.ExecuteShakeItOff = SmartQueueActiveSpell(AID.ShakeItOff); // TODO: check that raid is in range?...
            _strategy.ExecuteBloodwhetting = SmartQueueActiveSpell(AID.RawIntuition) || SmartQueueActiveSpell(AID.Bloodwhetting); // TODO: consider auto-use?..
            _strategy.ExecuteNascentFlash = SmartQueueActiveSpell(AID.NascentFlash);
            _strategy.ExecuteArmsLength = SmartQueueActiveSpell(AID.ArmsLength);
            _strategy.ExecuteProvoke = SmartQueueActiveSpell(AID.Provoke); // TODO: check that not MT already
            _strategy.ExecuteShirk = SmartQueueActiveSpell(AID.Shirk); // TODO: check that hate is close to MT...
            _strategy.ExecuteLowBlow = SmartQueueActiveSpell(AID.LowBlow);
            _strategy.ExecuteInterject = SmartQueueActiveSpell(AID.Interject) && AllowInterject(target);

            var nextBestST = _config.FullRotation ? Rotation.GetNextBestAction(_state, _strategy, false) : ActionID.MakeSpell(AID.HeavySwing);
            var nextBestAOE = _config.FullRotation ? Rotation.GetNextBestAction(_state, _strategy, true) : ActionID.MakeSpell(AID.Overpower);
            if (_nextBestSTAction != nextBestST || _nextBestAOEAction != nextBestAOE)
            {
                Log($"Next-best changed: ST={_nextBestSTAction}->{nextBestST}, AOE={_nextBestAOEAction}->{nextBestAOE} [{_state}]");
                _nextBestSTAction = nextBestST;
                _nextBestAOEAction = nextBestAOE;
                _justCast = false;
            }

            if (_justCast)
            {
                Log($"First update after cast [{_state}]");
                _justCast = false;
            }

            return _state;
        }

        protected override (ActionID, ulong) DoReplaceActionAndTarget(ActionID actionID, Targets targets)
        {
            if (actionID.Type == ActionType.Spell)
            {
                actionID = (AID)actionID.ID switch
                {
                    AID.HeavySwing => _config.FullRotation ? _nextBestSTAction : actionID,
                    AID.Overpower => _config.FullRotation ? _nextBestAOEAction : actionID,
                    AID.Maim => _config.STCombos ? ActionID.MakeSpell(Rotation.GetNextMaimComboAction(_state.ComboLastMove)) : actionID,
                    AID.StormEye => _config.STCombos ? ActionID.MakeSpell(Rotation.GetNextSTComboAction(_state.ComboLastMove, AID.StormEye)) : actionID,
                    AID.StormPath => _config.STCombos ? ActionID.MakeSpell(Rotation.GetNextSTComboAction(_state.ComboLastMove, AID.StormPath)) : actionID,
                    AID.MythrilTempest => _config.AOECombos ? ActionID.MakeSpell(Rotation.GetNextAOEComboAction(_state.ComboLastMove)) : actionID,
                    _ => actionID
                };
            }
            ulong targetID = actionID.Type == ActionType.Spell ? (AID)actionID.ID switch
            {
                AID.NascentFlash or AID.Shirk => SmartTargetCoTank(actionID, targets, _config.SmartNascentFlashShirkTarget)?.InstanceID ?? targets.MainTarget,
                AID.Provoke => SmartTargetHostile(actionID, targets, _config.ProvokeMouseover)?.InstanceID ?? targets.MainTarget,
                AID.Holmgang => _config.HolmgangSelf ? Player.InstanceID : targets.MainTarget,
                _ => targets.MainTarget
            } : targets.MainTarget;
            return (actionID, targetID);
        }

        public override AIResult CalculateBestAction(Actor player, Actor? primaryTarget, bool moving)
        {
            if (primaryTarget?.Type != ActorType.Enemy)
                return new();
            // TODO: proper implementation...
            return new() { Action = _nextBestSTAction, Target = primaryTarget };
        }

        public override void DrawOverlay()
        {
            ImGui.TextUnformatted($"Next: {Rotation.ActionShortString(_nextBestSTAction)} / {Rotation.ActionShortString(_nextBestAOEAction)}");
            ImGui.TextUnformatted(_strategy.ToString());
            ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
        }

        private Rotation.State BuildState(Actor? target)
        {
            Rotation.State s = new();
            FillCommonPlayerState(s, target, CommonDefinitions.IDPotionStr);
            s.Level = _lock.AdjustLevel(s.Level);

            s.Gauge = Service.JobGauges.Get<WARGauge>().BeastGauge;

            foreach (var status in Player.Statuses)
            {
                switch ((SID)status.ID)
                {
                    case SID.SurgingTempest:
                        s.SurgingTempestLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.NascentChaos:
                        s.NascentChaosLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.Berserk:
                    case SID.InnerRelease:
                        s.InnerReleaseLeft = StatusDuration(status.ExpireAt);
                        s.InnerReleaseStacks = status.Extra & 0xFF;
                        break;
                    case SID.PrimalRend:
                        s.PrimalRendLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }

            return s;
        }

        private void LogStateChange(Rotation.State prev, Rotation.State curr)
        {
            // do nothing if not in combat
            if (!Player.InCombat)
                return;

            // detect expired buffs
            if (curr.InnerReleaseLeft == 0 && prev.InnerReleaseLeft != 0 && prev.InnerReleaseLeft < 1)
                Log($"Expired IR [{curr}]");
            if (curr.NascentChaosLeft == 0 && prev.NascentChaosLeft != 0 && prev.NascentChaosLeft < 1)
                Log($"Expired NC [{curr}]");
            if (curr.PrimalRendLeft == 0 && prev.PrimalRendLeft != 0 && prev.PrimalRendLeft < 1)
                Log($"Expired PR [{curr}]");
            if (curr.SurgingTempestLeft == 0 && prev.SurgingTempestLeft != 0 && prev.SurgingTempestLeft < 1)
                Log($"Expired ST [{curr}]");
            if (curr.ComboTimeLeft == 0 && prev.ComboTimeLeft != 0 && prev.ComboTimeLeft < 1)
                Log($"Expired combo [{curr}]");
        }

        // check whether any targetable enemies are in reprisal range (TODO: consider checking only target?..)
        private bool AllowReprisal()
        {
            return Autorot.PotentialTargetsInRangeFromPlayer(5).Any();
        }

        private bool AllowInterject(Actor? target)
        {
            return target?.CastInfo?.Interruptible ?? false;
        }
    }
}
