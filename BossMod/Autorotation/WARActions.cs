using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using ImGuiNET;
using System;
using System.Linq;
using System.Text;

namespace BossMod
{
    class WARActions : CommonActions
    {
        // after pressing a cooldown button, it will stay 'queued' for a few seconds - this is to limit the effect of e.g. misclick while ability has long cooldown
        private struct SmartQueueEntry
        {
            private DateTime _queuedAt;

            public bool Active => (DateTime.Now - _queuedAt).TotalSeconds < 3;
            public void Activate() => _queuedAt = DateTime.Now;
            public void Deactivate() => _queuedAt = new();
        }

        private WARConfig _config;
        private WARRotation.State _state;
        private WARRotation.Strategy _strategy;
        private ActionID _nextBestAction = ActionID.MakeSpell(WARRotation.AID.HeavySwing);
        private uint _forceMovementFlags = 1; // 0 = force-disable, 3 = force-enable, other = whatever planner says
        private SmartQueueEntry _qRampart;
        private SmartQueueEntry _qVengeance;
        private SmartQueueEntry _qThrillOfBattle;
        private SmartQueueEntry _qHolmgang;
        private SmartQueueEntry _qEquilibrium;
        private SmartQueueEntry _qReprisal;
        private SmartQueueEntry _qShakeItOff;
        private SmartQueueEntry _qBloodwhetting;
        private SmartQueueEntry _qNascentFlash;
        private SmartQueueEntry _qArmsLength;
        private SmartQueueEntry _qProvoke;
        private SmartQueueEntry _qShirk;
        private SmartQueueEntry _qSprint;
        private SmartQueueEntry _qPotion;

        public WARActions(AutorotationConfig config, BossModuleManager bossmods)
            : base(config, bossmods)
        {
            _config = config.Get<WARConfig>();
            _state = BuildState(WARRotation.AID.None, 0, 0, 0.1f);
            _strategy = new()
            {
                FirstChargeIn = 0.01f, // by default, always preserve 1 onslaught charge
                SecondChargeIn = 10000, // ... but don't preserve second
            };
        }

        public override void CastSucceeded(ActionID actionID)
        {
            string comment = "";
            if (actionID.Type == ActionType.Spell)
            {
                switch ((WARRotation.AID)actionID.ID)
                {
                    case WARRotation.AID.HeavySwing:
                        if (_state.ComboLastMove == WARRotation.AID.HeavySwing || _state.ComboLastMove == WARRotation.AID.Maim)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.InnerReleaseStacks > 0)
                            comment += $", mistake=wasted-ir-stack";
                        break;
                    case WARRotation.AID.Maim:
                        if (_state.ComboLastMove != WARRotation.AID.HeavySwing)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.InnerReleaseStacks > 0)
                            comment += $", mistake=wasted-ir-stack";
                        if (_state.Gauge > 90)
                            comment += $", mistake=overcap-gauge";
                        break;
                    case WARRotation.AID.StormPath:
                        if (_state.ComboLastMove != WARRotation.AID.Maim)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.InnerReleaseStacks > 0)
                            comment += $", mistake=wasted-ir-stack";
                        if (_state.Gauge > 80)
                            comment += $", mistake=overcap-gauge";
                        if (_state.SurgingTempestLeft <= 0)
                            comment += $", mistake=no-st";
                        break;
                    case WARRotation.AID.StormEye:
                        if (_state.ComboLastMove != WARRotation.AID.Maim)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.InnerReleaseStacks > 0)
                            comment += $", mistake=wasted-ir-stack";
                        if (_state.Gauge > 90)
                            comment += $", mistake=overcap-gauge";
                        if (_state.SurgingTempestLeft > 30)
                            comment += $", mistake=overcap-st";
                        break;
                    case WARRotation.AID.FellCleave:
                        comment += _state.InnerReleaseStacks > 0 ? ", spent IR stack" : ", spent gauge";
                        if (_state.InfuriateCD < 5)
                            comment += $", mistake=overcap-infuriate";
                        if (_state.SurgingTempestLeft <= 0)
                            comment += $", mistake=no-st";
                        break;
                    case WARRotation.AID.InnerChaos:
                        if (_state.InnerReleaseStacks > 0)
                            comment += $", mistake=wasted-ir-stack";
                        if (_state.InfuriateCD < 5)
                            comment += $", mistake=overcap-infuriate";
                        if (_state.SurgingTempestLeft <= 0)
                            comment += $", mistake=no-st";
                        break;
                    case WARRotation.AID.Overpower:
                        if (_state.ComboLastMove == WARRotation.AID.Overpower)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.InnerReleaseStacks > 0)
                            comment += $", mistake=wasted-ir-stack";
                        break;
                    case WARRotation.AID.MythrilTempest:
                        if (_state.ComboLastMove != WARRotation.AID.Overpower)
                            comment += $", mistake=wrong-combo({_state.ComboLastMove})";
                        if (_state.InnerReleaseStacks > 0)
                            comment += $", mistake=wasted-ir-stack";
                        if (_state.Gauge > 80)
                            comment += $", mistake=overcap-gauge";
                        break;
                    case WARRotation.AID.Infuriate:
                        if (_state.Gauge > 50)
                            comment += $", mistake=overcap-gauge";
                        if (_state.NascentChaosLeft > 0)
                            comment += $", mistake=overwrite-nc";
                        if (_state.InnerReleaseStacks > 0)
                            comment += $", mistake=infuriate-under-ir";
                        break;
                    case WARRotation.AID.Onslaught:
                        // note: onslaught without ST is not really a mistake...
                        break;
                    case WARRotation.AID.Upheaval:
                        if (_state.SurgingTempestLeft <= 0)
                            comment += $", mistake=no-st";
                        break;
                    case WARRotation.AID.InnerRelease:
                        if (_state.SurgingTempestLeft <= 0)
                            comment += $", mistake=no-st";
                        if (_state.NascentChaosLeft > 0)
                            comment += $", mistake=ir-under-nc";
                        if (_state.SurgingTempestLeft > 50)
                            comment += $", mistake=overcap-st";
                        break;
                    case WARRotation.AID.Tomahawk:
                        if (_state.InnerReleaseStacks > 0)
                            comment += $", mistake=wasted-ir-stack";
                        break;
                }
            }
            Log($"Cast {actionID}, next-best={_nextBestAction}{comment} [{StateString(_state)}]");
        }

        public override void Update(uint comboLastAction, float comboTimeLeft, float animLock, float animLockDelay)
        {
            var currState = BuildState((WARRotation.AID)comboLastAction, comboTimeLeft, animLock, animLockDelay);
            LogStateChange(_state, currState);
            _state = currState;

            _strategy.Potion = _qPotion.Active ? WARRotation.Strategy.PotionUse.Immediate : _config.PotionUse;
            if (_strategy.Potion != WARRotation.Strategy.PotionUse.Manual && !HavePotions()) // don't try to use potions if player doesn't have any
                _strategy.Potion = WARRotation.Strategy.PotionUse.Manual;

            _strategy.RaidBuffsIn = _bossmods.RaidCooldowns.NextDamageBuffIn(_bossmods.WorldState.CurrentTime);
            if (_forceMovementFlags == 0)
                _strategy.PositionLockIn = 0;
            else if (_forceMovementFlags == 3 || _bossmods.ActiveModule == null)
                _strategy.PositionLockIn = 10000;
            else
                _strategy.PositionLockIn = _bossmods.ActiveModule.StateMachine.EstimateTimeToNextPositioning();
            _strategy.FightEndIn = _bossmods.ActiveModule != null ? _bossmods.ActiveModule.StateMachine.EstimateTimeToNextDowntime() : 0;

            // cooldown management (TODO: planning)
            _strategy.ExecuteRampart = _qRampart.Active;
            _strategy.ExecuteVengeance = _qVengeance.Active;
            _strategy.ExecuteThrillOfBattle = _qThrillOfBattle.Active;
            _strategy.ExecuteHolmgang = _qHolmgang.Active;
            _strategy.ExecuteEquilibrium = _qEquilibrium.Active && Service.ClientState.LocalPlayer?.CurrentHp < Service.ClientState.LocalPlayer?.MaxHp;
            _strategy.ExecuteReprisal = _qReprisal.Active; // TODO: check that at least one enemy is in range!
            _strategy.ExecuteShakeItOff = _qShakeItOff.Active; // TODO: check that raid is in range?...
            _strategy.ExecuteBloodwhetting = _qBloodwhetting.Active; // TODO: consider auto-use?..
            _strategy.ExecuteNascentFlash = _qNascentFlash.Active;
            _strategy.ExecuteArmsLength = _qArmsLength.Active;
            _strategy.ExecuteProvoke = _qProvoke.Active; // TODO: check that not MT already
            _strategy.ExecuteShirk = _qShirk.Active; // TODO: check that hate is close to MT...
            _strategy.ExecuteSprint = _qSprint.Active;

            var nextBest = _config.FullSTRotation ? WARRotation.GetNextBestAction(_state, _strategy) : ActionID.MakeSpell(WARRotation.AID.HeavySwing);
            if (nextBest != _nextBestAction)
                Log($"Next-best changed from {_nextBestAction} to {nextBest} [{StateString(_state)}]");
            _nextBestAction = nextBest;
        }

        public override (ActionID, uint) ReplaceActionAndTarget(ActionID actionID, uint targetID)
        {
            var actionAdj = actionID.Type switch
            {
                ActionType.Spell => (WARRotation.AID)actionID.ID switch
                {
                    WARRotation.AID.HeavySwing => _config.FullSTRotation ? _nextBestAction : actionID,
                    WARRotation.AID.Maim => _config.STCombos ? ActionID.MakeSpell(WARRotation.GetNextMaimComboAction(_state.ComboLastMove)) : actionID,
                    WARRotation.AID.StormEye => _config.STCombos ? ActionID.MakeSpell(WARRotation.GetNextSTComboAction(_state.ComboLastMove, WARRotation.AID.StormEye)) : actionID,
                    WARRotation.AID.StormPath => _config.STCombos ? ActionID.MakeSpell(WARRotation.GetNextSTComboAction(_state.ComboLastMove, WARRotation.AID.StormPath)) : actionID,
                    WARRotation.AID.MythrilTempest => _config.AOECombos ? ActionID.MakeSpell(WARRotation.GetNextAOEComboAction(_state.ComboLastMove)) : actionID,
                    WARRotation.AID.Rampart => SmartQueue(actionID, ref _qRampart),
                    WARRotation.AID.Vengeance => SmartQueue(actionID, ref _qVengeance),
                    WARRotation.AID.ThrillOfBattle => SmartQueue(actionID, ref _qThrillOfBattle),
                    WARRotation.AID.Holmgang => SmartQueue(actionID, ref _qHolmgang),
                    WARRotation.AID.Equilibrium => SmartQueue(actionID, ref _qEquilibrium),
                    WARRotation.AID.Reprisal => SmartQueue(actionID, ref _qReprisal),
                    WARRotation.AID.ShakeItOff => SmartQueue(actionID, ref _qShakeItOff),
                    WARRotation.AID.Bloodwhetting or WARRotation.AID.RawIntuition => SmartQueue(actionID, ref _qBloodwhetting),
                    WARRotation.AID.NascentFlash => SmartQueue(actionID, ref _qNascentFlash),
                    WARRotation.AID.ArmsLength => SmartQueue(actionID, ref _qArmsLength),
                    WARRotation.AID.Provoke => SmartQueue(actionID, ref _qProvoke),
                    WARRotation.AID.Shirk => SmartQueue(actionID, ref _qShirk),
                    _ => actionID
                },
                ActionType.General => actionID == WARRotation.IDSprint ? SmartQueue(actionID, ref _qSprint) : actionID,
                ActionType.Item => actionID == WARRotation.IDStatPotion ? SmartQueue(actionID, ref _qPotion) : actionID,
                _ => actionID
            };
            var targetAdj = actionID.Type == ActionType.Spell ? (WARRotation.AID)actionID.ID switch
            {
                WARRotation.AID.NascentFlash => _config.SmartNascentFlashTarget ? SmartTargetCoTank(targetID, ref _qNascentFlash) : targetID,
                WARRotation.AID.Shirk => _config.SmartShirkTarget ? SmartTargetCoTank(targetID, ref _qShirk) : targetID,
                WARRotation.AID.Holmgang => _config.HolmgangSelf ? Service.ClientState.LocalPlayer?.ObjectId ?? targetID : targetID,
                _ => targetID
            } : targetID;
            return (actionAdj, targetAdj);
        }

        public override void DrawOverlay()
        {
            var switchToDefault = _forceMovementFlags == 0;
            if (ImGui.CheckboxFlags("Enable movement", ref _forceMovementFlags, 3) && switchToDefault)
                _forceMovementFlags = 1;
            ImGui.Text($"Next: {_nextBestAction}, SmartQueue:{SmartQueueString(_strategy)}");
            ImGui.Text($"GCD={_state.GCD:f3}, Lock={_state.AnimationLock:f3}, RBLeft={_state.RaidBuffsLeft:f2}");
            ImGui.Text($"FightEnd={_strategy.FightEndIn:f3}, PosLock={_strategy.PositionLockIn:f3}, RBIn={_strategy.RaidBuffsIn:f2}");
        }

        private WARRotation.State BuildState(WARRotation.AID comboLastAction, float comboTimeLeft, float animLock, float animLockDelay)
        {
            WARRotation.State s = new();
            if (Service.ClientState.LocalPlayer != null)
            {
                s.Level = Service.ClientState.LocalPlayer.Level;
                s.AnimationLock = animLock;
                s.AnimationLockDelay = animLockDelay;
                s.ComboTimeLeft = comboTimeLeft;
                s.ComboLastMove = comboLastAction;
                s.Gauge = Service.JobGauges.Get<WARGauge>().BeastGauge;

                foreach (var status in Service.ClientState.LocalPlayer.StatusList)
                {
                    if (IsDamageBuff(status.StatusId))
                    {
                        s.RaidBuffsLeft = MathF.Max(s.RaidBuffsLeft, StatusDuration(status.RemainingTime));
                    }

                    switch ((WARRotation.SID)status.StatusId)
                    {
                        case WARRotation.SID.SurgingTempest:
                            s.SurgingTempestLeft = StatusDuration(status.RemainingTime);
                            break;
                        case WARRotation.SID.NascentChaos:
                            s.NascentChaosLeft = StatusDuration(status.RemainingTime);
                            break;
                        case WARRotation.SID.InnerRelease:
                            s.InnerReleaseLeft = StatusDuration(status.RemainingTime);
                            s.InnerReleaseStacks = status.StackCount;
                            break;
                        case WARRotation.SID.PrimalRend:
                            s.PrimalRendLeft = StatusDuration(status.RemainingTime);
                            break;
                    }
                }

                s.GCD = SpellCooldown(WARRotation.AID.HeavySwing);
                s.InfuriateCD = SpellCooldown(WARRotation.AID.Infuriate);
                s.UpheavalCD = SpellCooldown(WARRotation.AID.Upheaval);
                s.InnerReleaseCD = SpellCooldown(WARRotation.AID.InnerRelease);
                s.OnslaughtCD = SpellCooldown(WARRotation.AID.Onslaught);
                s.RampartCD = SpellCooldown(WARRotation.AID.Rampart);
                s.VengeanceCD = SpellCooldown(WARRotation.AID.Vengeance);
                s.ThrillOfBattleCD = SpellCooldown(WARRotation.AID.ThrillOfBattle);
                s.HolmgangCD = SpellCooldown(WARRotation.AID.Holmgang);
                s.EquilibriumCD = SpellCooldown(WARRotation.AID.Equilibrium);
                s.ReprisalCD = SpellCooldown(WARRotation.AID.Reprisal);
                s.ShakeItOffCD = SpellCooldown(WARRotation.AID.ShakeItOff);
                s.BloodwhettingCD = SpellCooldown(WARRotation.AID.Bloodwhetting);
                s.ArmsLengthCD = SpellCooldown(WARRotation.AID.ArmsLength);
                s.ProvokeCD = SpellCooldown(WARRotation.AID.Provoke);
                s.ShirkCD = SpellCooldown(WARRotation.AID.Shirk);
                s.SprintCD = ActionCooldown(WARRotation.IDSprint);
                s.PotionCD = ActionCooldown(WARRotation.IDStatPotion);
            }
            return s;
        }

        private unsafe bool HavePotions()
        {
            return FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(WARRotation.IDStatPotion.ID % 1000000, true, false, false) > 0;
        }

        private void LogStateChange(WARRotation.State prev, WARRotation.State curr)
        {
            // do nothing if not in combat
            if (Service.ClientState.LocalPlayer == null || !Service.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat))
                return;

            // detect expired buffs
            if (curr.InnerReleaseLeft == 0 && prev.InnerReleaseLeft != 0 && prev.InnerReleaseLeft < 1)
                Log($"Expired IR [{StateString(curr)}]");
            if (curr.NascentChaosLeft == 0 && prev.NascentChaosLeft != 0 && prev.NascentChaosLeft < 1)
                Log($"Expired NC [{StateString(curr)}]");
            if (curr.PrimalRendLeft == 0 && prev.PrimalRendLeft != 0 && prev.PrimalRendLeft < 1)
                Log($"Expired PR [{StateString(curr)}]");
            if (curr.SurgingTempestLeft == 0 && prev.SurgingTempestLeft != 0 && prev.SurgingTempestLeft < 1)
                Log($"Expired ST [{StateString(curr)}]");
            if (curr.ComboTimeLeft == 0 && prev.ComboTimeLeft != 0 && prev.ComboTimeLeft < 1)
                Log($"Expired combo [{StateString(curr)}]");
        }

        private static string StateString(WARRotation.State s)
        {
            return $"g={s.Gauge}, RB={s.RaidBuffsLeft:f1}, ST={s.SurgingTempestLeft:f1}, NC={s.NascentChaosLeft:f1}, PR={s.PrimalRendLeft:f1}, IR={s.InnerReleaseStacks}/{s.InnerReleaseLeft:f1}, IRCD={s.InnerReleaseCD:f1}, InfCD={s.InfuriateCD:f1}, UphCD={s.UpheavalCD:f1}, OnsCD={s.OnslaughtCD:f1}, PotCD={s.PotionCD:f1}, GCD={s.GCD:f3}, ALock={s.AnimationLock:f3}, ALockDelay={s.AnimationLockDelay:f3}, lvl={s.Level}";
        }

        private static string SmartQueueString(WARRotation.Strategy strategy)
        {
            var sb = new StringBuilder();
            if (strategy.ExecuteProvoke)
                sb.Append(" Provoke");
            if (strategy.ExecuteShirk)
                sb.Append(" Shirk");
            if (strategy.ExecuteHolmgang)
                sb.Append(" Holmgang");
            if (strategy.ExecuteArmsLength)
                sb.Append(" ArmsLength");
            if (strategy.ExecuteShakeItOff)
                sb.Append(" ShakeItOff");
            if (strategy.ExecuteVengeance)
                sb.Append(" Vengeance");
            if (strategy.ExecuteRampart)
                sb.Append(" Rampart");
            if (strategy.ExecuteThrillOfBattle)
                sb.Append(" ThrillOfBattle");
            if (strategy.ExecuteEquilibrium)
                sb.Append(" Equilibrium");
            if (strategy.ExecuteReprisal)
                sb.Append(" Reprisal");
            if (strategy.ExecuteBloodwhetting)
                sb.Append(" Bloodwhetting");
            if (strategy.ExecuteNascentFlash)
                sb.Append(" NascentFlash");
            if (strategy.ExecuteSprint)
                sb.Append(" Sprint");
            return sb.ToString();
        }

        private ActionID SmartQueue(ActionID action, ref SmartQueueEntry q)
        {
            if (!_config.SmartCooldownQueueing || (_state.GCD <= 0 && _state.AnimationLock <= 0))
            {
                // smart queueing is disabled, or we're pressing action when not otherwise busy (e.g. during downtime), so execute it immediately
                return action;
            }
            else
            {
                // perform smart queueing and return next-best ST (?) action
                // TODO: consider smart queueing when spamming AOE rotation...
                Log($"Smart-queueing {action}");
                q.Activate();
                return _nextBestAction;
            }
        }

        // shirk/nascent flash smart targeting: target if friendly > mouseover if friendly > other tank
        private uint SmartTargetCoTank(uint targetID, ref SmartQueueEntry q)
        {
            var target = _bossmods.WorldState.Actors.Find(targetID);
            if (target?.Type is ActorType.Player or ActorType.Chocobo)
                return target.InstanceID;

            target = _bossmods.WorldState.Actors.Find(Mouseover.Instance?.Object?.ObjectId ?? 0);
            if (target?.Type is ActorType.Player or ActorType.Chocobo)
                return target.InstanceID;

            target = _bossmods.WorldState.Party.WithoutSlot().FirstOrDefault(a => a.InstanceID != Service.ClientState.LocalPlayer?.ObjectId && a.Role == Role.Tank);
            if (target != null)
                return target.InstanceID;

            // can't find good target, deactivate smart-queue entry to prevent silly spam
            Log($"Smart-target failed, removing from queue");
            q.Deactivate();
            return targetID;
        }
    }
}
