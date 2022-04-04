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
            GCDExpired = 1 << 15,
        }

        public WARRotation.State InitialState = new() { Level = 90, AnimationLockDelay = 0.1f };
        public int Duration = 260;
        public bool KeepOnslaughtCharge = false;
        public float BuffWindowOffset = 7.5f;
        public float BuffWindowDuration = 20;
        public float BuffWindowFreq = 120;
        public WARRotation.Strategy.PotionUse PotionUse = WARRotation.Strategy.PotionUse.DelayUntilBuffs;

        public void Draw()
        {
            ImGui.InputInt("Sim duration (GCDs)", ref Duration);
            ImGui.Checkbox("Keep onslaught charge", ref KeepOnslaughtCharge);
            ImGui.SliderFloat("Buff window offset", ref BuffWindowOffset, 0, 60);
            ImGui.SliderFloat("Buff window duration", ref BuffWindowDuration, 1, 30);
            ImGui.SliderFloat("Buff window frequency", ref BuffWindowFreq, 30, 120);
            if (ImGui.BeginCombo("Potion use strategy", PotionUse.ToString()))
            {
                foreach (var opt in Enum.GetValues<WARRotation.Strategy.PotionUse>())
                    if (ImGui.Selectable(opt.ToString(), opt.Equals(PotionUse)))
                        PotionUse = opt;
                ImGui.EndCombo();
            }

            if (ImGui.CollapsingHeader("Initial state setup"))
            {
                ImGui.SliderInt("Level", ref InitialState.Level, 1, 90);
                ImGui.SliderFloat("Animation lock delay", ref InitialState.AnimationLockDelay, 0, 0.5f);
                ImGui.SliderInt("Gauge", ref InitialState.Gauge, 0, 100);
                ImGui.SliderFloat("Surging Tempest time", ref InitialState.SurgingTempestLeft, 0, 60);
                ImGui.SliderFloat("Nascent Chaos time", ref InitialState.NascentChaosLeft, 0, 30);
                ImGui.SliderFloat("Primal Rend time", ref InitialState.PrimalRendLeft, 0, 60);
                ImGui.SliderFloat("Inner Release time", ref InitialState.InnerReleaseLeft, 0, 15);
                if (InitialState.InnerReleaseLeft > 0)
                {
                    ImGui.SliderInt("Inner Release stacks", ref InitialState.InnerReleaseStacks, 0, 3);
                }
                else
                {
                    ImGui.SliderFloat("Inner Release cd", ref InitialState.InnerReleaseCD, 0, 60);
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

            var strategy = new WARRotation.Strategy();
            strategy.PositionLockIn = 10000;
            strategy.FirstChargeIn = KeepOnslaughtCharge ? 0.1f : 10000;
            strategy.SecondChargeIn = 10000;
            strategy.Potion = PotionUse;

            var state = new WARRotation.State();
            foreach (var f in state.GetType().GetFields())
                f.SetValue(state, f.GetValue(InitialState));
            float t = 0;
            var fightLength = Duration * 2.5f;
            while (t < fightLength)
            {
                strategy.FightEndIn = fightLength - t;
                var nextBuffCycleIndex = MathF.Ceiling((t - BuffWindowOffset) / BuffWindowFreq);
                strategy.RaidBuffsIn = t < BuffWindowOffset ? 0 : (BuffWindowOffset + nextBuffCycleIndex * BuffWindowFreq - t);

                var action = WARRotation.GetNextBestAction(state, strategy);
                (var mistake, var ogcd) = Cast(state, action, ref t);
                DrawActionRow(action, !ogcd, mistake, t, state, strategy);
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

        public Mistake AdvanceTime(WARRotation.State state, ref float t, float newAnimLock, float cooldown)
        {
            var dt = MathF.Max(state.AnimationLock, cooldown);
            t += dt;
            var curBuffCycleIndex = MathF.Floor((t - BuffWindowOffset) / BuffWindowFreq);
            var timeInCycle = t - (BuffWindowOffset + curBuffCycleIndex * BuffWindowFreq);
            state.RaidBuffsLeft = MathF.Max(0, BuffWindowDuration - timeInCycle);

            var potionLeft = MathF.Max(state.PotionCD - 240, 0);
            state.RaidBuffsLeft = MathF.Max(state.RaidBuffsLeft, potionLeft);

            var res = Mistake.None;
            state.AnimationLock = state.AnimationLockDelay + newAnimLock;
            if (state.GCD >= 0 && (state.GCD -= dt) < 0)
            {
                res |= Mistake.GCDExpired;
                state.GCD = 0;
            }
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
            if (state.UnlockedBerserk && state.InnerReleaseCD < -2.5f)
                res |= Mistake.InnerReleaseDelayed;
            state.UpheavalCD -= dt;
            if (state.UnlockedUpheaval && state.UpheavalCD < -2.5f)
                res |= Mistake.UpheavalDelayed;
            state.OnslaughtCD -= dt;
            if (state.UnlockedOnslaught && state.OnslaughtCD < -2.5f)
                res |= Mistake.OnslaughtDelayed;
            state.InfuriateCD -= dt;
            if (state.UnlockedInfuriate && state.InfuriateCD < -2.5f)
                res |= Mistake.InfuriateDelayed;

            state.RampartCD -= dt;
            state.VengeanceCD -= dt;
            state.ThrillOfBattleCD -= dt;
            state.HolmgangCD -= dt;
            state.EquilibriumCD -= dt;
            state.ReprisalCD -= dt;
            state.ShakeItOffCD -= dt;
            state.BloodwhettingCD -= dt;
            state.ArmsLengthCD -= dt;
            state.ProvokeCD -= dt;
            state.ShirkCD -= dt;
            state.SprintCD -= dt;
            state.PotionCD -= dt;

            return res;
        }

        public (Mistake, bool) Cast(WARRotation.State state, ActionID action, ref float t)
        {
            var res = Mistake.None;
            bool isOGCD = false;
            if (action == WARRotation.IDSprint)
            {
                isOGCD = true;
                res |= AdvanceTime(state, ref t, 0.6f, state.SprintCD);
                res |= AdjustCD(ref state.SprintCD, 60, 60);
            }
            else if (action == WARRotation.IDStatPotion)
            {
                isOGCD = true;
                res |= AdvanceTime(state, ref t, 1.1f, state.PotionCD);
                res |= AdjustCD(ref state.PotionCD, 270, 270);
            }
            else
            {
                var aid = (WARRotation.AID)action.ID;
                switch (aid)
                {
                    case WARRotation.AID.HeavySwing:
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (state.UnlockedMaim)
                            ComboAdvance(state, ref res, WARRotation.AID.None, WARRotation.AID.HeavySwing);
                        break;
                    case WARRotation.AID.Maim:
                        res |= CheckValid(state.UnlockedMaim);
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (ComboAdvance(state, ref res, WARRotation.AID.HeavySwing, state.UnlockedStormPath ? WARRotation.AID.Maim : WARRotation.AID.None))
                        {
                            res |= GainGauge(state, 10);
                        }
                        break;
                    case WARRotation.AID.StormPath:
                        res |= CheckValid(state.UnlockedStormPath);
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (ComboAdvance(state, ref res, WARRotation.AID.Maim, WARRotation.AID.None))
                        {
                            res |= GainGauge(state, 20);
                        }
                        break;
                    case WARRotation.AID.StormEye:
                        res |= CheckValid(state.UnlockedStormEye);
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (ComboAdvance(state, ref res, WARRotation.AID.Maim, WARRotation.AID.None))
                        {
                            res |= GainGauge(state, 10);
                            res |= GainSurgingTempest(state, 30);
                        }
                        break;
                    case WARRotation.AID.InnerBeast:
                    case WARRotation.AID.FellCleave:
                        res |= CheckValid(aid == WARRotation.AID.FellCleave ? state.UnlockedFellCleave : state.UnlockedInnerBeast);
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                        res |= CheckValid(state.NascentChaosLeft <= 0);
                        res |= CheckValid(state.InnerReleaseStacks > 0 || state.Gauge >= 50);
                        res |= UseIRCharge(state, true, 50);
                        if (state.UnlockedEnhancedInfuriate && (state.InfuriateCD -= 5) <= 0)
                        {
                            res |= Mistake.InfuriateDelayed;
                        }
                        break;
                    case WARRotation.AID.InnerChaos:
                    case WARRotation.AID.ChaoticCyclone:
                        res |= CheckValid(aid == WARRotation.AID.InnerChaos ? state.UnlockedInnerChaos : state.UnlockedChaoticCyclone);
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                        res |= CheckValid(state.NascentChaosLeft > 0);
                        res |= CheckValid(state.InnerReleaseStacks > 0 || state.Gauge >= 50);
                        res |= UseIRCharge(state, false, 50);
                        state.NascentChaosLeft = 0;
                        // TODO: verify this...
                        if (state.UnlockedEnhancedInfuriate && (state.InfuriateCD -= 5) <= 0)
                        {
                            res |= Mistake.InfuriateDelayed;
                        }
                        break;
                    case WARRotation.AID.PrimalRend:
                        res |= CheckValid(state.UnlockedPrimalRend);
                        res |= AdvanceTime(state, ref t, 1.15f, state.GCD);
                        res |= AdjustCD(ref state.GCD, 2.5f, 2.5f);
                        res |= CheckValid(state.PrimalRendLeft > 0);
                        state.PrimalRendLeft = 0;
                        break;
                    case WARRotation.AID.Infuriate:
                        isOGCD = true;
                        res |= CheckValid(state.UnlockedInfuriate);
                        res |= AdvanceTime(state, ref t, 0.6f, state.InfuriateCD - 60);
                        res |= AdjustCD(ref state.InfuriateCD, 60, 120);
                        res |= GainGauge(state, 50);
                        if (state.NascentChaosLeft > 0)
                        {
                            res |= Mistake.NascentChaosOverwrite;
                        }
                        if (state.UnlockedChaoticCyclone)
                        {
                            state.NascentChaosLeft = 30;
                        }
                        break;
                    case WARRotation.AID.Onslaught:
                        isOGCD = true;
                        res |= CheckValid(state.UnlockedOnslaught);
                        res |= AdvanceTime(state, ref t, 0.6f, state.OnslaughtCD - (state.UnlockedEnhancedOnslaught ? 60 : 30));
                        res |= AdjustCD(ref state.OnslaughtCD, 30, state.UnlockedEnhancedOnslaught ? 90 : 60);
                        break;
                    case WARRotation.AID.Upheaval:
                        isOGCD = true;
                        res |= CheckValid(state.UnlockedUpheaval);
                        res |= AdvanceTime(state, ref t, 0.6f, state.UpheavalCD);
                        res |= AdjustCD(ref state.UpheavalCD, 30, 30);
                        break;
                    case WARRotation.AID.Berserk:
                    case WARRotation.AID.InnerRelease:
                        isOGCD = true;
                        res |= CheckValid(aid == WARRotation.AID.InnerRelease ? state.UnlockedInnerRelease : state.UnlockedBerserk);
                        res |= AdvanceTime(state, ref t, 0.6f, state.InnerReleaseCD);
                        res |= AdjustCD(ref state.InnerReleaseCD, 60, 60);
                        state.InnerReleaseLeft = 15;
                        state.InnerReleaseStacks = 3;
                        if (state.UnlockedPrimalRend)
                            state.PrimalRendLeft = 30;
                        if (state.SurgingTempestLeft > 0)
                            res |= GainSurgingTempest(state, 10);
                        break;
                    default:
                        throw new Exception($"Unexpected action {aid}");
                }
            }
            return (res, isOGCD);
        }

        public string MistakeString(Mistake mistake)
        {
            return mistake == Mistake.None ? "" : mistake.ToString();
        }

        private void DrawActionRow(ActionID action, bool isGCD, Mistake mistake, float t, WARRotation.State state, WARRotation.Strategy strategy)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.Text($"{t:f1}");

            var name = action == WARRotation.IDSprint ? "Sprint" : action == WARRotation.IDStatPotion ? "StatPotion" : ((WARRotation.AID)action.ID).ToString();
            ImGui.TableNextColumn(); ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(state.RaidBuffsLeft > 0 ? 0xff00ff00 : 0xffffffff), isGCD ? $"{name}" : $"** {name}");
            ImGui.TableNextColumn(); ImGui.Text($"{MistakeString(mistake)}");

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

        private Mistake CheckValid(bool valid)
        {
            return valid ? Mistake.None : Mistake.InvalidMove;
        }

        private Mistake AdjustCD(ref float tracker, float useCD, float maxCD)
        {
            return IncrementOvercap(ref tracker, useCD, 0, maxCD) ? Mistake.InvalidMove : Mistake.None;
        }

        private Mistake GainGauge(WARRotation.State state, int value)
        {
            if (!state.UnlockedInnerBeast)
                return Mistake.None;

            if ((state.Gauge += value) > 100)
            {
                state.Gauge = 100;
                return Mistake.GaugeOvercap;
            }
            return Mistake.None;
        }

        private Mistake GainSurgingTempest(WARRotation.State state, float value)
        {
            return IncrementOvercap(ref state.SurgingTempestLeft, value, 0, 60) ? Mistake.SurgingTempestOvercap : Mistake.None;
        }

        private Mistake UseIRCharge(WARRotation.State state, bool isFC, int gaugeCost)
        {
            if (!state.UnlockedInnerRelease || state.InnerReleaseStacks <= 0)
                state.Gauge -= gaugeCost;

            Mistake mistake = Mistake.None;
            if (state.InnerReleaseStacks > 0)
            {
                if (!isFC)
                    mistake |= Mistake.InnerReleaseWasted;
                if (--state.InnerReleaseStacks == 0)
                    state.InnerReleaseLeft = 0;
            }
            return mistake;
        }

        private bool ComboAdvance(WARRotation.State state, ref Mistake mistake, WARRotation.AID prev, WARRotation.AID next)
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
