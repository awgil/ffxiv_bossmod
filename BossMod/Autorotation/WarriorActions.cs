using Dalamud.Game.ClientState.JobGauge.Types;
using ImGuiNET;
using System;

namespace BossMod
{
    class WarriorActions
    {
        private unsafe float* _comboTimeLeft = null;
        private unsafe WARRotation.AID* _comboLastMove = null;
        private unsafe FFXIVClientStructs.FFXIV.Client.Game.ActionManager* _actionManager = null;

        public unsafe float ComboTimeLeft => *_comboTimeLeft;
        public unsafe WARRotation.AID ComboLastMove => *_comboLastMove;
        public WARRotation.State State { get; private set; }
        public WARRotation.Strategy Strategy = new();

        public unsafe WarriorActions()
        {
            IntPtr comboPtr = Service.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 80 7E 21 00", 0x178);
            _comboTimeLeft = (float*)comboPtr;
            _comboLastMove = (WARRotation.AID*)(comboPtr + 0x4);

            _actionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
            State = BuildState();
        }

        public void Update()
        {
            var currState = BuildState();

            // check expired buffs
            bool inCombat = Service.ClientState.LocalPlayer?.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) ?? false;
            if (inCombat)
            {
                if (currState.InnerReleaseLeft == 0 && State.InnerReleaseLeft != 0 && State.InnerReleaseLeft < 1)
                    Service.Log($"[AR] Mistake: Inner Release expired");
                if (currState.NascentChaosLeft == 0 && State.NascentChaosLeft != 0 && State.NascentChaosLeft < 1)
                    Service.Log($"[AR] Mistake: Nascent Chaos expired");
                if (currState.PrimalRendLeft == 0 && State.PrimalRendLeft != 0 && State.PrimalRendLeft < 1)
                    Service.Log($"[AR] Mistake: Primal Rend expired");
                if (currState.SurgingTempestLeft == 0 && State.SurgingTempestLeft != 0 && State.SurgingTempestLeft < 1)
                    Service.Log($"[AR] Mistake: Surging Tempest expired");
            }

            if (currState.ComboLastMove != State.ComboLastMove)
            {
                Service.Log($"[AR] Cast combo move: {currState.ComboLastMove}");
                if (currState.ComboLastMove == WARRotation.AID.Maim && State.Gauge > 90)
                    Service.Log($"[AR] Mistake: Maim overcapped gauge");
                if (currState.ComboLastMove == WARRotation.AID.StormEye && State.Gauge > 90)
                    Service.Log($"[AR] Mistake: Storm Eye overcapped gauge");
                if (currState.ComboLastMove == WARRotation.AID.StormPath && State.Gauge > 80)
                    Service.Log($"[AR] Mistake: Storm Path overcapped gauge");
            }

            if (currState.SurgingTempestLeft > 58 && State.SurgingTempestLeft < 58)
                Service.Log($"[AR] Mistake: overcapped Surging Tempest");

            if (currState.NascentChaosLeft == 0 && State.NascentChaosLeft > 1)
            {
                Service.Log($"[AR] Cast Inner Chaos");
                if (currState.InnerReleaseStacks < State.InnerReleaseStacks)
                    Service.Log($"[AR] Mistake: Inner Chaos under IR");
            }
            else if (currState.InnerReleaseStacks < State.InnerReleaseStacks && State.InnerReleaseLeft > 1)
            {
                // spent IR stack
                Service.Log($"[AR] Cast Fell Cleave (spent IR stack)");
            }
            else if (currState.Gauge < State.Gauge)
            {
                Service.Log($"[AR] Cast Fell Cleave (spent gauge)");
            }

            if (currState.PrimalRendLeft == 0 && State.PrimalRendLeft > 1)
                Service.Log($"[AR] Cast Primal Rend");
            if (currState.InnerReleaseCD > State.InnerReleaseCD)
                Service.Log($"[AR] Cast IR");
            if (currState.UpheavalCD > State.UpheavalCD)
                Service.Log($"[AR] Cast Upheaval");
            if (currState.InfuriateCD > State.InfuriateCD)
                Service.Log($"[AR] Cast Infuriate");
            if (currState.OnslaughtCD > State.OnslaughtCD)
                Service.Log($"[AR] Cast Onslaught");

            State = currState;
        }

        public void DrawActionHint()
        {
            ImGui.Text($"Gauge: {State.Gauge}");
            ImGui.Text($"Surging tempest left: {State.SurgingTempestLeft:f2}");
            ImGui.Text($"Nascent chaos left: {State.NascentChaosLeft:f2}");
            ImGui.Text($"Primal rend left: {State.PrimalRendLeft:f2}");
            ImGui.Text($"Inner release left: {State.InnerReleaseLeft:f2}");
            ImGui.Text($"Combo State: last={State.ComboLastMove}, time left={State.ComboTimeLeft:f2}");
            ImGui.Text($"GCD: {State.GCD:f2}");
            ImGui.Text($"Infuriate: {State.InfuriateCD:f2}");
            ImGui.Text($"Upheaval: {State.UpheavalCD:f2}");
            ImGui.Text($"Inner Release: {State.InnerReleaseCD:f2}");
            ImGui.Text($"Onslaught: {State.OnslaughtCD:f2}");
            ImGui.Text($"Next GCD: {WARRotation.GetNextBestGCD(State, Strategy)}");
            ImGui.Text($"Next oGCD: {WARRotation.GetNextBestOGCD(State, Strategy)}");
            ImGui.Text($"Next action: {WARRotation.GetNextBestAction(State, Strategy)}");
        }

        public unsafe float ActionCooldown(WARRotation.AID action)
        {
            return _actionManager->GetRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, (uint)action) - _actionManager->GetRecastTimeElapsed(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, (uint)action);
        }

        public WARRotation.State BuildState()
        {
            WARRotation.State s = new();
            if (Service.ClientState.LocalPlayer != null)
            {
                s.ComboTimeLeft = ComboTimeLeft;
                s.ComboLastMove = ComboLastMove;
                s.Gauge = Service.JobGauges.Get<WARGauge>().BeastGauge;

                foreach (var status in Service.ClientState.LocalPlayer.StatusList)
                {
                    switch ((WARRotation.StatusID)status.StatusId)
                    {
                        case WARRotation.StatusID.SurgingTempest:
                            s.SurgingTempestLeft = status.RemainingTime;
                            break;
                        case WARRotation.StatusID.NascentChaos:
                            s.NascentChaosLeft = status.RemainingTime;
                            break;
                        case WARRotation.StatusID.InnerRelease:
                            s.InnerReleaseLeft = status.RemainingTime;
                            s.InnerReleaseStacks = status.StackCount;
                            break;
                        case WARRotation.StatusID.PrimalRend:
                            s.PrimalRendLeft = status.RemainingTime;
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
    }
}
