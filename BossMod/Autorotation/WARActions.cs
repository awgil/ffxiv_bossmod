using Dalamud.Game.ClientState.JobGauge.Types;
using ImGuiNET;
using System;
using System.Text;

namespace BossMod
{
    class WARActions
    {
        private unsafe FFXIVClientStructs.FFXIV.Client.Game.ActionManager* _actionManager = null;

        public bool Enabled = true;
        public WARRotation.State State { get; private set; }
        public WARRotation.Strategy Strategy;
        public WARRotation.AID NextBestAction { get; private set; } = WARRotation.AID.HeavySwing;

        public unsafe WARActions()
        {
            _actionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
            State = BuildState(WARRotation.AID.None, 0);

            Strategy = new()
            {
                SpendGauge = true,
                EnableUpheaval = true,
                EnableMovement = false,
                NeedChargeIn = 0,
                Aggressive = false,
            };
        }

        public void CastSucceeded(WorldState.ActionType actionType, uint actionID)
        {
            if (actionType != WorldState.ActionType.Spell)
                return;

            string comment = "";
            switch ((WARRotation.AID)actionID)
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
            Service.Log($"[AR] Cast {Utils.ActionString(actionID)}, next-best={NextBestAction}{comment} [{StateString(State)}]");
        }

        public void Update(uint comboLastAction, float comboTimeLeft)
        {
            var currState = BuildState((WARRotation.AID)comboLastAction, comboTimeLeft);
            LogStateChange(State, currState);
            State = currState;
            var nextBest = Enabled ? WARRotation.GetNextBestAction(State, Strategy) : WARRotation.AID.HeavySwing;
            if (nextBest != NextBestAction)
                Service.Log($"[AR] Next-best changed from {NextBestAction} to {nextBest} [{StateString(State)}]");
            NextBestAction = nextBest;
        }

        public void DrawActionHint(bool extended)
        {
            ImGui.Checkbox("Spend mode", ref Strategy.SpendGauge);
            ImGui.Checkbox("Enable movement", ref Strategy.EnableMovement);
            ImGui.Text($"Next: {NextBestAction}, GCD={State.GCD:f1}");
            if (extended)
            {
                ImGui.Checkbox("Enable", ref Enabled);
                ImGui.SliderFloat("Need charge in", ref Strategy.NeedChargeIn, 0, 30);
                ImGui.Text($"Gauge: {State.Gauge}");
                ImGui.Text($"Surging tempest left: {State.SurgingTempestLeft:f1}");
                ImGui.Text($"Nascent chaos left: {State.NascentChaosLeft:f1}");
                ImGui.Text($"Primal rend left: {State.PrimalRendLeft:f1}");
                ImGui.Text($"Inner release left: {State.InnerReleaseLeft:f1} ({State.InnerReleaseStacks} stacks)");
                ImGui.Text($"Combo State: last={State.ComboLastMove}, time left={State.ComboTimeLeft:f1}");
                ImGui.Text($"Infuriate: {State.InfuriateCD:f1}");
                ImGui.Text($"Upheaval: {State.UpheavalCD:f1}");
                ImGui.Text($"Inner Release: {State.InnerReleaseCD:f1}");
                ImGui.Text($"Onslaught: {State.OnslaughtCD:f1}");
                ImGui.Text($"Next GCD: {WARRotation.GetNextBestGCD(State, Strategy)}");
                ImGui.Text($"Next oGCD: {WARRotation.GetNextBestOGCD(State, Strategy)}");
            }
        }

        public unsafe float ActionCooldown(WARRotation.AID action)
        {
            return _actionManager->GetRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, (uint)action) - _actionManager->GetRecastTimeElapsed(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, (uint)action);
        }

        public WARRotation.State BuildState(WARRotation.AID comboLastAction, float comboTimeLeft)
        {
            WARRotation.State s = new();
            if (Service.ClientState.LocalPlayer != null)
            {
                s.ComboTimeLeft = comboTimeLeft;
                s.ComboLastMove = comboLastAction;
                s.Gauge = Service.JobGauges.Get<WARGauge>().BeastGauge;

                foreach (var status in Service.ClientState.LocalPlayer.StatusList)
                {
                    // note: when buff is applied, it has large negative duration, and it is updated to correct one ~0.6sec later
                    switch ((WARRotation.StatusID)status.StatusId)
                    {
                        case WARRotation.StatusID.SurgingTempest:
                            s.SurgingTempestLeft = status.RemainingTime < -25 ? 30 : status.RemainingTime;
                            break;
                        case WARRotation.StatusID.NascentChaos:
                            s.NascentChaosLeft = status.RemainingTime < -25 ? 30 : status.RemainingTime;
                            break;
                        case WARRotation.StatusID.InnerRelease:
                            s.InnerReleaseLeft = status.RemainingTime < -10 ? 15 : status.RemainingTime;
                            s.InnerReleaseStacks = status.StackCount;
                            break;
                        case WARRotation.StatusID.PrimalRend:
                            s.PrimalRendLeft = status.RemainingTime < -25 ? 30 : status.RemainingTime;
                            break;
                    }
                }

                s.InfuriateCD = ActionCooldown(WARRotation.AID.Infuriate);
                s.UpheavalCD = ActionCooldown(WARRotation.AID.Upheaval);
                s.InnerReleaseCD = ActionCooldown(WARRotation.AID.InnerRelease);
                s.OnslaughtCD = ActionCooldown(WARRotation.AID.Onslaught);
                s.GCD = ActionCooldown(WARRotation.AID.HeavySwing);
            }
            return s;
        }

        private static void LogStateChange(WARRotation.State prev, WARRotation.State curr)
        {
            // do nothing if not in combat
            if (Service.ClientState.LocalPlayer == null || !Service.ClientState.LocalPlayer.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat))
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
        }

        private static string StateString(WARRotation.State s)
        {
            return $"g={s.Gauge}, ST={s.SurgingTempestLeft:f1}, NC={s.NascentChaosLeft:f1}, PR={s.PrimalRendLeft:f1}, IR={s.InnerReleaseStacks}/{s.InnerReleaseLeft:f1}, IRCD={s.InnerReleaseCD:f1}, InfCD={s.InfuriateCD:f1}, UphCD={s.UpheavalCD:f1}, OnsCD={s.OnslaughtCD:f1}, GCD={s.GCD:f1}";
        }
    }
}
