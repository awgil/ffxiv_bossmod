using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using ImGuiNET;
using System;
using System.Text;

namespace BossMod
{
    class WARActions : CommonActions
    {
        public bool Enabled = true;
        public WARRotation.State State { get; private set; }
        public WARRotation.Strategy Strategy;
        public WARRotation.AID NextBestAction { get; private set; } = WARRotation.AID.HeavySwing;

        private BossModuleManager _bossmods;
        private uint _forceMovementFlags = 1; // 0 = force-disable, 3 = force-enable, other = whatever planner says

        public WARActions(BossModuleManager bossmods)
        {
            _bossmods = bossmods;
            State = BuildState(WARRotation.AID.None, 0, 0, 0.1f);
            Strategy = new()
            {
                FirstChargeIn = 0.1f, // by default, always preserve 1 onslaught charge
                SecondChargeIn = 10000, // ... but don't preserve second
                EnableUpheaval = true,
            };
        }

        public void CastSucceeded(ActionID actionID)
        {
            if (actionID.Type != ActionType.Spell)
                return;

            string comment = "";
            switch ((WARRotation.AID)actionID.ID)
            {
                case WARRotation.AID.HeavySwing:
                    if (State.ComboLastMove == WARRotation.AID.HeavySwing || State.ComboLastMove == WARRotation.AID.Maim)
                        comment += $", mistake=wrong-combo({State.ComboLastMove})";
                    if (State.InnerReleaseStacks > 0)
                        comment += $", mistake=wasted-ir-stack";
                    break;
                case WARRotation.AID.Maim:
                    if (State.ComboLastMove != WARRotation.AID.HeavySwing)
                        comment += $", mistake=wrong-combo({State.ComboLastMove})";
                    if (State.InnerReleaseStacks > 0)
                        comment += $", mistake=wasted-ir-stack";
                    if (State.Gauge > 90)
                        comment += $", mistake=overcap-gauge";
                    break;
                case WARRotation.AID.StormPath:
                    if (State.ComboLastMove != WARRotation.AID.Maim)
                        comment += $", mistake=wrong-combo({State.ComboLastMove})";
                    if (State.InnerReleaseStacks > 0)
                        comment += $", mistake=wasted-ir-stack";
                    if (State.Gauge > 80)
                        comment += $", mistake=overcap-gauge";
                    if (State.SurgingTempestLeft <= 0)
                        comment += $", mistake=no-st";
                    break;
                case WARRotation.AID.StormEye:
                    if (State.ComboLastMove != WARRotation.AID.Maim)
                        comment += $", mistake=wrong-combo({State.ComboLastMove})";
                    if (State.InnerReleaseStacks > 0)
                        comment += $", mistake=wasted-ir-stack";
                    if (State.Gauge > 90)
                        comment += $", mistake=overcap-gauge";
                    if (State.SurgingTempestLeft > 30)
                        comment += $", mistake=overcap-st";
                    break;
                case WARRotation.AID.FellCleave:
                    comment += State.InnerReleaseStacks > 0 ? ", spent IR stack" : ", spent gauge";
                    if (State.InfuriateCD < 5)
                        comment += $", mistake=overcap-infuriate";
                    if (State.SurgingTempestLeft <= 0)
                        comment += $", mistake=no-st";
                    break;
                case WARRotation.AID.InnerChaos:
                    if (State.InnerReleaseStacks > 0)
                        comment += $", mistake=wasted-ir-stack";
                    if (State.InfuriateCD < 5)
                        comment += $", mistake=overcap-infuriate";
                    if (State.SurgingTempestLeft <= 0)
                        comment += $", mistake=no-st";
                    break;
                case WARRotation.AID.Overpower:
                    if (State.ComboLastMove == WARRotation.AID.Overpower)
                        comment += $", mistake=wrong-combo({State.ComboLastMove})";
                    if (State.InnerReleaseStacks > 0)
                        comment += $", mistake=wasted-ir-stack";
                    break;
                case WARRotation.AID.MythrilTempest:
                    if (State.ComboLastMove != WARRotation.AID.Overpower)
                        comment += $", mistake=wrong-combo({State.ComboLastMove})";
                    if (State.InnerReleaseStacks > 0)
                        comment += $", mistake=wasted-ir-stack";
                    if (State.Gauge > 80)
                        comment += $", mistake=overcap-gauge";
                    break;
                case WARRotation.AID.Infuriate:
                    if (State.Gauge > 50)
                        comment += $", mistake=overcap-gauge";
                    if (State.NascentChaosLeft > 0)
                        comment += $", mistake=overwrite-nc";
                    if (State.InnerReleaseStacks > 0)
                        comment += $", mistake=infuriate-under-ir";
                    break;
                case WARRotation.AID.Onslaught:
                    // note: onslaught without ST is not really a mistake...
                    break;
                case WARRotation.AID.Upheaval:
                    if (State.SurgingTempestLeft <= 0)
                        comment += $", mistake=no-st";
                    break;
                case WARRotation.AID.InnerRelease:
                    if (State.SurgingTempestLeft <= 0)
                        comment += $", mistake=no-st";
                    if (State.NascentChaosLeft > 0)
                        comment += $", mistake=ir-under-nc";
                    if (State.SurgingTempestLeft > 50)
                        comment += $", mistake=overcap-st";
                    break;
                case WARRotation.AID.Tomahawk:
                    if (State.InnerReleaseStacks > 0)
                        comment += $", mistake=wasted-ir-stack";
                    break;
            }
            Service.Log($"[AR] Cast {actionID}, next-best={NextBestAction}{comment} [{StateString(State)}]");
        }

        public void Update(uint comboLastAction, float comboTimeLeft, float animLock, float animLockDelay)
        {
            var currState = BuildState((WARRotation.AID)comboLastAction, comboTimeLeft, animLock, animLockDelay);
            LogStateChange(State, currState);
            State = currState;

            Strategy.RaidBuffsIn = _bossmods.RaidCooldowns.NextDamageBuffIn(_bossmods.WorldState.CurrentTime);
            if (_forceMovementFlags == 0)
                Strategy.PositionLockIn = 0;
            else if (_forceMovementFlags == 3 || _bossmods.ActiveModule == null)
                Strategy.PositionLockIn = 10000;
            else
                Strategy.PositionLockIn = _bossmods.ActiveModule.StateMachine.EstimateTimeToNextPositioning();
            Strategy.FightEndIn = _bossmods.ActiveModule != null ? _bossmods.ActiveModule.StateMachine.EstimateTimeToNextDowntime() : 0;

            var nextBest = Enabled ? WARRotation.GetNextBestAction(State, Strategy) : WARRotation.AID.HeavySwing;
            if (nextBest != NextBestAction)
                Service.Log($"[AR] Next-best changed from {NextBestAction} to {nextBest} [{StateString(State)}]");
            NextBestAction = nextBest;
        }

        public void DrawOverlay()
        {
            var switchToDefault = _forceMovementFlags == 0;
            if (ImGui.CheckboxFlags("Enable movement", ref _forceMovementFlags, 3) && switchToDefault)
                _forceMovementFlags = 1;
            ImGui.Text($"Next: {NextBestAction}");
            ImGui.Text($"GCD={State.GCD:f3}, Lock={State.AnimationLock:f3}, RBLeft={State.RaidBuffsLeft:f2}");
            ImGui.Text($"FightEnd={Strategy.FightEndIn:f3}, PosLock={Strategy.PositionLockIn:f3}, RBIn={Strategy.RaidBuffsIn:f2}");
        }

        public void DrawDebug()
        {
            ImGui.Checkbox("Enable", ref Enabled);
        }

        public WARRotation.State BuildState(WARRotation.AID comboLastAction, float comboTimeLeft, float animLock, float animLockDelay)
        {
            WARRotation.State s = new();
            if (Service.ClientState.LocalPlayer != null)
            {
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

                s.InfuriateCD = SpellCooldown(WARRotation.AID.Infuriate);
                s.UpheavalCD = SpellCooldown(WARRotation.AID.Upheaval);
                s.InnerReleaseCD = SpellCooldown(WARRotation.AID.InnerRelease);
                s.OnslaughtCD = SpellCooldown(WARRotation.AID.Onslaught);
                s.GCD = SpellCooldown(WARRotation.AID.HeavySwing);
            }
            return s;
        }

        private static void LogStateChange(WARRotation.State prev, WARRotation.State curr)
        {
            // do nothing if not in combat
            if (Service.ClientState.LocalPlayer == null || !Service.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat))
                return;

            // detect expired buffs
            if (curr.InnerReleaseLeft == 0 && prev.InnerReleaseLeft != 0 && prev.InnerReleaseLeft < 1)
                Service.Log($"[AR] Expired IR [{StateString(curr)}]");
            if (curr.NascentChaosLeft == 0 && prev.NascentChaosLeft != 0 && prev.NascentChaosLeft < 1)
                Service.Log($"[AR] Expired NC [{StateString(curr)}]");
            if (curr.PrimalRendLeft == 0 && prev.PrimalRendLeft != 0 && prev.PrimalRendLeft < 1)
                Service.Log($"[AR] Expired PR [{StateString(curr)}]");
            if (curr.SurgingTempestLeft == 0 && prev.SurgingTempestLeft != 0 && prev.SurgingTempestLeft < 1)
                Service.Log($"[AR] Expired ST [{StateString(curr)}]");
            if (curr.ComboTimeLeft == 0 && prev.ComboTimeLeft != 0 && prev.ComboTimeLeft < 1)
                Service.Log($"[AR] Expired combo [{StateString(curr)}]");
        }

        private static string StateString(WARRotation.State s)
        {
            return $"g={s.Gauge}, RB={s.RaidBuffsLeft:f1}, ST={s.SurgingTempestLeft:f1}, NC={s.NascentChaosLeft:f1}, PR={s.PrimalRendLeft:f1}, IR={s.InnerReleaseStacks}/{s.InnerReleaseLeft:f1}, IRCD={s.InnerReleaseCD:f1}, InfCD={s.InfuriateCD:f1}, UphCD={s.UpheavalCD:f1}, OnsCD={s.OnslaughtCD:f1}, GCD={s.GCD:f3}, ALock={s.AnimationLock:f3}, ALockDelay={s.AnimationLockDelay:f3}";
        }
    }
}
