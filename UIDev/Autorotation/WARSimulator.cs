using BossMod;
using ImGuiNET;
using System;
using System.Text;

namespace UIDev
{
    class WARSimulator : ITest
    {
        [Flags]
        public enum Mistake
        {
            None = 0,
            GaugeOvercap = 1 << 0,
            SurgingTempestExpire = 1 << 1,
            SurgingTempestOvercap = 1 << 2,
            InnerReleaseDelayed = 1 << 3,
            InnerReleaseExpired = 1 << 4,
            InnerReleaseWasted = 1 << 5, // charge wasted on anything other than FC
            PrimalRendExpired = 1 << 6,
            NascentChaosExpired = 1 << 7,
            NascentChaosOverwrite = 1 << 8,
            InfuriateDelayed = 1 << 9,
            UpheavalDelayed = 1 << 10,
            OnslaughtDelayed = 1 << 11,
            ComboLost = 1 << 12,
            ComboWrongMove = 1 << 13,
            InvalidMove = 1 << 14,
        }

        public WARRotation.State InitialState;
        public WARRotation.Strategy Strategy;
        public int Duration = 250;
        public bool BuffWindowEnable = true;
        public float BuffWindowOffset = 7.5f;
        public float BuffWindowDuration = 20;
        public float BuffWindowFreq = 120;

        public WARSimulator()
        {
            Strategy.EnableUpheaval = true;
        }

        public void Draw()
        {
            ImGui.InputInt("Sim duration (GCDs)", ref Duration);

            ImGui.Checkbox("Use buff window", ref BuffWindowEnable);
            if (BuffWindowEnable)
            {
                ImGui.SliderFloat("Buff window offset", ref BuffWindowOffset, 0, 60);
                ImGui.SliderFloat("Buff window duration", ref BuffWindowOffset, 1, 30);
                ImGui.SliderFloat("Buff window frequency", ref BuffWindowOffset, 30, 120);
            }
            else
            {
                ImGui.Checkbox("Spend gauge", ref Strategy.SpendGauge);
            }

            ImGui.Checkbox("Allow movement", ref Strategy.EnableMovement);
            ImGui.Checkbox("Aggressive", ref Strategy.Aggressive);
            ImGui.SliderFloat("Need charge in", ref Strategy.NeedChargeIn, 0, 30);
            if (ImGui.CollapsingHeader("Initial state setup"))
            {
                ImGui.SliderInt("Gauge", ref InitialState.Gauge, 0, 100);
                ImGui.SliderFloat("Surging Tempest time", ref InitialState.SurgingTempestLeft, 0, 60);
                ImGui.SliderFloat("Nascent Chaos time", ref InitialState.NascentChaosLeft, 0, 30);
                ImGui.SliderFloat("Primal Rend time", ref InitialState.PrimalRendLeft, 0, 60);
                ImGui.SliderFloat("Inner Release time", ref InitialState.InnerReleaseLeft, 0, 15);
                if (InitialState.InnerReleaseLeft > 0)
                {
                    ImGui.SliderInt("stacks", ref InitialState.InnerReleaseStacks, 0, 3);
                }
                else
                {
                    ImGui.SliderFloat("cd", ref InitialState.InnerReleaseCD, 0, 60);
                }
                ImGui.SliderFloat("Infuriate cd", ref InitialState.InfuriateCD, 0, 120);
                ImGui.SliderFloat("Upheaval cd", ref InitialState.UpheavalCD, 0, 30);
                ImGui.SliderFloat("Onslaught cd", ref InitialState.OnslaughtCD, 0, 90);
            }

            ImGui.BeginTable("sim", 12);
            ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, 150);
            ImGui.TableSetupColumn("Mistake");
            ImGui.TableSetupColumn("Gauge", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("ST", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("IR", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Infuriate", ImGuiTableColumnFlags.WidthFixed, 80);
            ImGui.TableSetupColumn("NC", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("PR", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Upheaval", ImGuiTableColumnFlags.WidthFixed, 70);
            ImGui.TableSetupColumn("Onslaught", ImGuiTableColumnFlags.WidthFixed, 80);
            ImGui.TableSetupColumn("Combo", ImGuiTableColumnFlags.WidthFixed, 150);
            ImGui.TableHeadersRow();

            var strategy = Strategy;
            if (BuffWindowEnable)
            {
                strategy.SpendGauge = false;
                strategy.EnableMovement = false;
                strategy.EnableUpheaval = false;
            }
            var state = InitialState;
            var mistake = Mistake.None;
            for (int i = 0; i < Duration; ++i)
            {
                float t = 2.5f * i;
                bool opener = t < 5 + (BuffWindowEnable ? BuffWindowOffset : 7.5);
                if (BuffWindowEnable)
                {
                    strategy.SpendGauge = ((t + BuffWindowFreq - BuffWindowOffset) % BuffWindowFreq) < BuffWindowDuration;
                    strategy.EnableUpheaval = t >= BuffWindowOffset;
                    strategy.EnableMovement = t >= BuffWindowOffset && Strategy.EnableMovement;
                }

                var gcd = WARRotation.GetNextBestGCD(state, strategy);
                mistake |= Cast(ref state, gcd); // include mistake from last advance in prev iteration
                DrawActionRow(gcd, true, ref mistake, t, state, strategy);

                mistake |= AdvanceTime(ref state, 0.8f, opener, i == 0, opener || !strategy.EnableUpheaval, opener || !strategy.EnableMovement);
                var ogcd1 = WARRotation.GetNextBestOGCD(state, strategy);
                if (ogcd1 != WARRotation.AID.None)
                {
                    mistake |= Cast(ref state, ogcd1);
                    DrawActionRow(ogcd1, false, ref mistake, t + 0.8f, state, strategy);
                }

                mistake |= AdvanceTime(ref state, 0.8f, opener, i == 0, opener || !strategy.EnableUpheaval, opener || !strategy.EnableMovement);
                var ogcd2 = WARRotation.GetNextBestOGCD(state, strategy);
                if (ogcd2 != WARRotation.AID.None)
                {
                    mistake |= Cast(ref state, ogcd2);
                    DrawActionRow(ogcd2, false, ref mistake, t + 1.6f, state, strategy);
                }

                mistake |= AdvanceTime(ref state, 0.9f, opener, i == 0, opener || !strategy.EnableUpheaval, opener || !strategy.EnableMovement);
            }

            ImGui.EndTable();
        }

        public ImGuiWindowFlags WindowFlags()
        {
            return ImGuiWindowFlags.None;
        }

        public void Dispose()
        {
        }

        public Mistake AdvanceTime(ref WARRotation.State state, float dt, bool delayIR, bool delayInfuriate, bool delayUpheaval, bool delayOnslaught)
        {
            var res = Mistake.None;
            state.GCD = MathF.Max(0, state.GCD - dt);
            if (state.ComboTimeLeft > 0 && (state.ComboTimeLeft -= dt) <= 0)
            {
                res |= Mistake.ComboLost;
                state.ComboTimeLeft = 0;
                state.ComboLastMove = WARRotation.AID.None;
            }
            if (state.SurgingTempestLeft > 0 && (state.SurgingTempestLeft -= dt) <= 0)
            {
                res |= Mistake.SurgingTempestExpire;
                state.SurgingTempestLeft = 0;
            }
            if (state.NascentChaosLeft > 0 && (state.NascentChaosLeft -= dt) <= 0)
            {
                res |= Mistake.NascentChaosExpired;
                state.NascentChaosLeft = 0;
            }
            if (state.PrimalRendLeft > 0 && (state.PrimalRendLeft -= dt) <= 0)
            {
                res |= Mistake.PrimalRendExpired;
                state.PrimalRendLeft = 0;
            }
            if (state.InnerReleaseLeft > 0 && (state.InnerReleaseLeft -= dt) <= 0)
            {
                res |= Mistake.InnerReleaseExpired;
                state.InnerReleaseLeft = 0;
                state.InnerReleaseStacks = 0;
            }

            state.InnerReleaseCD -= dt;
            if (!delayIR && state.InnerReleaseCD <= -2.5)
                res |= Mistake.InnerReleaseDelayed;
            state.UpheavalCD -= dt;
            if (!delayUpheaval && state.UpheavalCD <= -2.5)
                res |= Mistake.UpheavalDelayed;
            state.OnslaughtCD -= dt;
            if (!delayOnslaught && state.OnslaughtCD < 0)
                res |= Mistake.OnslaughtDelayed;
            state.InfuriateCD -= dt;
            if (!delayInfuriate && state.InfuriateCD < 0)
                res |= Mistake.InfuriateDelayed;

            return res;
        }

        public Mistake Cast(ref WARRotation.State state, WARRotation.AID action)
        {
            var res = Mistake.None;
            switch (action)
            {
                case WARRotation.AID.HeavySwing:
                    res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                    UseIRCharge(ref state, ref res, false);
                    ComboAdvance(ref state, ref res, WARRotation.AID.None, WARRotation.AID.HeavySwing);
                    break;
                case WARRotation.AID.Maim:
                    res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                    UseIRCharge(ref state, ref res, false);
                    if (ComboAdvance(ref state, ref res, WARRotation.AID.HeavySwing, WARRotation.AID.Maim))
                    {
                        res |= GainGauge(ref state, 10);
                    }
                    break;
                case WARRotation.AID.StormPath:
                    res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                    UseIRCharge(ref state, ref res, false);
                    if (ComboAdvance(ref state, ref res, WARRotation.AID.Maim, WARRotation.AID.None))
                    {
                        res |= GainGauge(ref state, 20);
                    }
                    break;
                case WARRotation.AID.StormEye:
                    res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                    UseIRCharge(ref state, ref res, false);
                    if (ComboAdvance(ref state, ref res, WARRotation.AID.Maim, WARRotation.AID.None))
                    {
                        res |= GainGauge(ref state, 10);
                        res |= GainSurgingTempest(ref state, 30);
                    }
                    break;
                case WARRotation.AID.FellCleave:
                    res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                    if (state.NascentChaosLeft > 0 || (state.InnerReleaseStacks <= 0 && state.Gauge < 50))
                    {
                        res |= Mistake.InvalidMove;
                        break;
                    }
                    if (!UseIRCharge(ref state, ref res, true))
                    {
                        state.Gauge -= 50;
                    }
                    if ((state.InfuriateCD -= 5) <= 0)
                    {
                        res |= Mistake.InfuriateDelayed;
                    }
                    break;
                case WARRotation.AID.InnerChaos:
                    res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                    if (state.NascentChaosLeft <= 0 || (state.InnerReleaseStacks <= 0 && state.Gauge < 50))
                    {
                        res |= Mistake.InvalidMove;
                        break;
                    }
                    if (!UseIRCharge(ref state, ref res, false))
                    {
                        state.Gauge -= 50;
                    }
                    state.NascentChaosLeft = 0;
                    // TODO: verify this...
                    if ((state.InfuriateCD -= 5) <= 0)
                    {
                        res |= Mistake.InfuriateDelayed;
                    }
                    break;
                case WARRotation.AID.PrimalRend:
                    res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                    if (state.PrimalRendLeft <= 0)
                    {
                        res |= Mistake.InvalidMove;
                    }
                    state.PrimalRendLeft = 0;
                    break;
                case WARRotation.AID.Infuriate:
                    res |= AdjustCD(ref state.InfuriateCD, 60, 120);
                    res |= GainGauge(ref state, 50);
                    if (state.NascentChaosLeft > 0)
                    {
                        res |= Mistake.NascentChaosOverwrite;
                    }
                    state.NascentChaosLeft = 30;
                    break;
                case WARRotation.AID.Onslaught:
                    res |= AdjustCD(ref state.OnslaughtCD, 30, 90);
                    break;
                case WARRotation.AID.Upheaval:
                    res |= AdjustCD(ref state.UpheavalCD, 30, 30);
                    break;
                case WARRotation.AID.InnerRelease:
                    res |= AdjustCD(ref state.InnerReleaseCD, 60, 60);
                    state.InnerReleaseLeft = 15;
                    state.InnerReleaseStacks = 3;
                    state.PrimalRendLeft = 30;
                    if (state.SurgingTempestLeft > 0)
                        res |= GainSurgingTempest(ref state, 10);
                    break;
            }
            return res;
        }

        public string MistakeString(Mistake mistake)
        {
            return mistake == Mistake.None ? "" : mistake.ToString();
        }

        private void DrawActionRow(WARRotation.AID action, bool isGCD, ref Mistake mistake, float t, WARRotation.State state, WARRotation.Strategy strategy)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.Text($"{t:f1}");
            ImGui.TableNextColumn(); ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(strategy.SpendGauge ? 0xff00ff00 : 0xffffffff), isGCD ? $"{action}" : $"** {action}");

            ImGui.TableNextColumn(); ImGui.Text($"{MistakeString(mistake)}");
            mistake = Mistake.None; // reset mistake, so that next row doesn't include leftovers from previous

            ImGui.TableNextColumn(); ImGui.Text($"{state.Gauge:f0}");
            ImGui.TableNextColumn(); ImGui.Text($"{state.SurgingTempestLeft:f1}");

            ImGui.TableNextColumn();
            if (state.InnerReleaseStacks > 0)
                ImGui.Text($"{state.InnerReleaseStacks} stacks, {state.InnerReleaseLeft:f1} left");
            else if (state.InnerReleaseCD > 0)
                ImGui.Text($"{state.InnerReleaseCD:f1} cd");
            else
                ImGui.Text($"{state.InnerReleaseCD:f1} delay");

            ImGui.TableNextColumn();
            if (state.InfuriateCD > 60)
                ImGui.Text($"0, {(state.InfuriateCD - 60):f1} cd");
            else if (state.InfuriateCD > 0)
                ImGui.Text($"1, {state.InfuriateCD:f1} cd");
            else
                ImGui.Text($"2, {state.InfuriateCD:f1} delay");

            ImGui.TableNextColumn();
            if (state.NascentChaosLeft > 0)
                ImGui.Text($"{state.NascentChaosLeft:f1}");

            ImGui.TableNextColumn();
            if (state.PrimalRendLeft > 0)
                ImGui.Text($"{state.PrimalRendLeft:f1}");

            ImGui.TableNextColumn();
            if (state.UpheavalCD > 0)
                ImGui.Text($"{state.UpheavalCD:f1} cd");
            else
                ImGui.Text($"{state.UpheavalCD:f1} delay");

            ImGui.TableNextColumn();
            if (state.OnslaughtCD > 60)
                ImGui.Text($"0, {(state.OnslaughtCD - 60):f1} cd");
            else if (state.OnslaughtCD > 30)
                ImGui.Text($"1, {(state.OnslaughtCD - 30):f1} cd");
            else if (state.OnslaughtCD > 0)
                ImGui.Text($"2, {state.OnslaughtCD:f1} cd");
            else
                ImGui.Text($"3, {state.OnslaughtCD:f1} delay");

            ImGui.TableNextColumn();
            if (state.ComboTimeLeft > 0)
                ImGui.Text($"{state.ComboLastMove} ({state.ComboTimeLeft:f1} left)");
        }

        private bool IncrementOvercap(ref float tracker, float value, float min, float max)
        {
            tracker = MathF.Max(min, tracker);
            if ((tracker += value) > max)
            {
                tracker = max;
                return true;
            }
            return false;
        }

        private Mistake AdjustCD(ref float tracker, float useCD, float maxCD)
        {
            return IncrementOvercap(ref tracker, useCD, 0, maxCD) ? Mistake.InvalidMove : Mistake.None;
        }

        private Mistake GainGauge(ref WARRotation.State state, int value)
        {
            if ((state.Gauge += value) > 100)
            {
                state.Gauge = 100;
                return Mistake.GaugeOvercap;
            }
            return Mistake.None;
        }

        private Mistake GainSurgingTempest(ref WARRotation.State state, float value)
        {
            return IncrementOvercap(ref state.SurgingTempestLeft, value, 0, 60) ? Mistake.SurgingTempestOvercap : Mistake.None;
        }

        private bool UseIRCharge(ref WARRotation.State state, ref Mistake mistake, bool isFC)
        {
            if (state.InnerReleaseStacks <= 0)
                return false;
            if (!isFC)
                mistake |= Mistake.InnerReleaseWasted;
            if (--state.InnerReleaseStacks == 0)
                state.InnerReleaseLeft = 0;
            return true;
        }

        private bool ComboAdvance(ref WARRotation.State state, ref Mistake mistake, WARRotation.AID prev, WARRotation.AID next)
        {
            if (state.ComboLastMove != prev)
            {
                mistake |= Mistake.ComboWrongMove;
                if (prev == WARRotation.AID.None)
                {
                    // restart combo chain anyway
                    state.ComboLastMove = next;
                    state.ComboTimeLeft = 30;
                }
                else
                {
                    state.ComboLastMove = WARRotation.AID.None;
                    state.ComboTimeLeft = 0;
                }
                return false;
            }
            else
            {
                state.ComboLastMove = next;
                state.ComboTimeLeft = next != WARRotation.AID.None ? 30 : 0;
                return true;
            }
        }
    }
}
