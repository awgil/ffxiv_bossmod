using BossMod;
using BossMod.WAR;
using ImGuiNET;
using System;

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

        public Rotation.State InitialState = new(new float[80]) { Level = 90, AnimationLockDelay = 0.1f };
        public int Duration = 260;
        public bool AOERotation = false;
        public bool KeepOnslaughtCharge = false;
        public bool StartWithTomahawk = true;
        public float BuffWindowOffset = 7.8f;
        public float BuffWindowDuration = 20;
        public float BuffWindowFreq = 120;

        public void Draw()
        {
            ImGui.InputInt("Sim duration (GCDs)", ref Duration);
            ImGui.Checkbox("Keep onslaught charge", ref KeepOnslaughtCharge);
            ImGui.Checkbox("AOE rotation", ref AOERotation);
            ImGui.Checkbox("Start with tomahawk", ref StartWithTomahawk);
            ImGui.SliderFloat("Buff window offset", ref BuffWindowOffset, 0, 60);
            ImGui.SliderFloat("Buff window duration", ref BuffWindowDuration, 1, 30);
            ImGui.SliderFloat("Buff window frequency", ref BuffWindowFreq, 30, 120);

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
                    ImGui.SliderFloat("Inner Release cd", ref InitialState.Cooldowns[(int)(InitialState.Unlocked(AID.InnerRelease) ? CDGroup.InnerRelease : CDGroup.Berserk)], 0, 60);
                }
                ImGui.SliderFloat("Infuriate cd", ref InitialState.Cooldowns[(int)CDGroup.Infuriate], 0, 120);
                ImGui.SliderFloat("Upheaval cd", ref InitialState.Cooldowns[(int)CDGroup.Upheaval], 0, 30);
                ImGui.SliderFloat("Onslaught cd", ref InitialState.Cooldowns[(int)CDGroup.Onslaught], 0, 90);
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

            var strategy = new Rotation.Strategy();
            strategy.PositionLockIn = 10000;
            strategy.OnslaughtStrategy = KeepOnslaughtCharge ? Rotation.Strategy.OnslaughtUse.Automatic : Rotation.Strategy.OnslaughtUse.NoReserve;
            //strategy.Potion = PotionUse;

            var state = new Rotation.State(new float[0]);
            foreach (var f in state.GetType().GetFields())
                f.SetValue(state, f.GetValue(InitialState));
            state.Cooldowns = new float[80];
            float t = 0;
            var fightLength = Duration * 2.5f;
            while (t < fightLength)
            {
                strategy.FightEndIn = fightLength - t;
                var nextBuffCycleIndex = MathF.Ceiling((t - BuffWindowOffset) / BuffWindowFreq);
                strategy.RaidBuffsIn = t < BuffWindowOffset ? 0 : (BuffWindowOffset + nextBuffCycleIndex * BuffWindowFreq - t);

                var action = t == 0 && state.GCD == 0 && StartWithTomahawk && state.Unlocked(AID.Tomahawk) ? ActionID.MakeSpell(AID.Tomahawk) : GetNextBestAction(state, strategy, AOERotation);
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

        public ActionID GetNextBestAction(Rotation.State state, Rotation.Strategy strategy, bool aoe)
        {
            ActionID res = new();
            if (state.AnimationLock + 2 * state.OGCDSlotLength <= state.GCD) // first ogcd slot
                res = Rotation.GetNextBestOGCD(state, strategy, state.GCD - state.OGCDSlotLength, aoe);
            if (!res && state.AnimationLock + state.OGCDSlotLength <= state.GCD) // second/only ogcd slot
                res = Rotation.GetNextBestOGCD(state, strategy, state.GCD, aoe);
            if (!res) // gcd
                res = ActionID.MakeSpell(Rotation.GetNextBestGCD(state, strategy, aoe));
            return res;
        }

        public Mistake AdvanceTime(Rotation.State state, ref float t, float newAnimLock, float cooldown)
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

            for (int i = 0; i < 80; ++i)
                state.Cooldowns[i] -= dt;

            if (state.GCD < 0)
            {
                res |= Mistake.GCDExpired;
                state.Cooldowns[CommonDefinitions.GCDGroup] = 0;
            }

            if (state.ComboTimeLeft > 0 && (state.ComboTimeLeft -= dt) <= 0)
            {
                res |= Mistake.ComboLost;
                state.ComboTimeLeft = 0;
                state.ComboLastAction = 0;
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

            if (state.Unlocked(AID.Berserk) && state.CD(CDGroup.InnerRelease) < -2.5f)
                res |= Mistake.InnerReleaseDelayed;
            if (state.Unlocked(AID.Upheaval) && state.CD(CDGroup.Upheaval) < -2.5f)
                res |= Mistake.UpheavalDelayed;
            if (state.Unlocked(AID.Onslaught) && state.CD(CDGroup.Onslaught) < -2.5f)
                res |= Mistake.OnslaughtDelayed;
            if (state.Unlocked(AID.Infuriate) && state.CD(CDGroup.Infuriate) < -2.5f)
                res |= Mistake.InfuriateDelayed;

            return res;
        }

        public (Mistake, bool) Cast(Rotation.State state, ActionID action, ref float t)
        {
            var res = Mistake.None;
            bool isOGCD = false;
            if (action == CommonDefinitions.IDSprint)
            {
                isOGCD = true;
                res |= AdvanceTime(state, ref t, 0.6f, state.SprintCD);
                res |= AdjustCD(state, CommonDefinitions.SprintCDGroup, 60, 60);
            }
            else if (action == CommonDefinitions.IDPotionStr)
            {
                isOGCD = true;
                res |= AdvanceTime(state, ref t, 1.1f, state.PotionCD);
                res |= AdjustCD(state, CommonDefinitions.PotionCDGroup, 270, 270);
            }
            else
            {
                var aid = (AID)action.ID;
                switch (aid)
                {
                    case AID.HeavySwing:
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (state.Unlocked(AID.Maim))
                            ComboAdvance(state, ref res, AID.None, AID.HeavySwing);
                        break;
                    case AID.Maim:
                        res |= CheckValid(state.Unlocked(AID.Maim));
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (ComboAdvance(state, ref res, AID.HeavySwing, state.Unlocked(AID.StormPath) ? AID.Maim : AID.None))
                        {
                            res |= GainGauge(state, 10);
                        }
                        break;
                    case AID.StormPath:
                        res |= CheckValid(state.Unlocked(AID.StormPath));
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (ComboAdvance(state, ref res, AID.Maim, AID.None))
                        {
                            res |= GainGauge(state, 20);
                        }
                        break;
                    case AID.StormEye:
                        res |= CheckValid(state.Unlocked(AID.StormEye));
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (ComboAdvance(state, ref res, AID.Maim, AID.None))
                        {
                            res |= GainGauge(state, 10);
                            res |= GainSurgingTempest(state, 30);
                        }
                        break;
                    case AID.InnerBeast:
                    case AID.FellCleave:
                        res |= CheckValid(aid == AID.FellCleave ? state.Unlocked(AID.FellCleave) : state.Unlocked(AID.InnerBeast));
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= CheckValid(state.NascentChaosLeft <= 0);
                        res |= CheckValid(state.InnerReleaseStacks > 0 || state.Gauge >= 50);
                        res |= UseIRCharge(state, true, 50);
                        if (state.Unlocked(TraitID.EnhancedInfuriate) && (state.Cooldowns[(int)CDGroup.Infuriate] -= 5) <= 0)
                        {
                            res |= Mistake.InfuriateDelayed;
                        }
                        break;
                    case AID.InnerChaos:
                    case AID.ChaoticCyclone:
                        res |= CheckValid(aid == AID.InnerChaos ? state.Unlocked(AID.InnerChaos) : state.Unlocked(AID.ChaoticCyclone));
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= CheckValid(state.NascentChaosLeft > 0);
                        res |= CheckValid(state.InnerReleaseStacks > 0 || state.Gauge >= 50);
                        res |= UseIRCharge(state, false, 50);
                        state.NascentChaosLeft = 0;
                        // TODO: verify this...
                        if (state.Unlocked(TraitID.EnhancedInfuriate) && (state.Cooldowns[(int)CDGroup.Infuriate] -= 5) <= 0)
                        {
                            res |= Mistake.InfuriateDelayed;
                        }
                        break;
                    case AID.PrimalRend:
                        res |= CheckValid(state.Unlocked(AID.PrimalRend));
                        res |= AdvanceTime(state, ref t, 1.15f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= CheckValid(state.PrimalRendLeft > 0);
                        state.PrimalRendLeft = 0;
                        break;
                    case AID.Overpower:
                        res |= CheckValid(state.Unlocked(AID.Overpower));
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (state.Unlocked(AID.MythrilTempest))
                            ComboAdvance(state, ref res, AID.None, AID.Overpower);
                        break;
                    case AID.MythrilTempest:
                        res |= CheckValid(state.Unlocked(AID.MythrilTempest));
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
                        if (ComboAdvance(state, ref res, AID.Overpower, AID.None))
                        {
                            if (state.Unlocked(TraitID.MasteringTheBeast))
                                res |= GainGauge(state, 20);
                            res |= GainSurgingTempest(state, 30);
                        }
                        break;
                    case AID.SteelCyclone:
                    case AID.Decimate:
                        res |= CheckValid(aid == AID.Decimate ? state.Unlocked(AID.Decimate) : state.Unlocked(AID.SteelCyclone));
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= CheckValid(state.NascentChaosLeft <= 0);
                        res |= CheckValid(state.InnerReleaseStacks > 0 || state.Gauge >= 50);
                        res |= UseIRCharge(state, true, 50);
                        if (state.Unlocked(TraitID.EnhancedInfuriate) && (state.Cooldowns[(int)CDGroup.Infuriate] -= 5) <= 0)
                        {
                            res |= Mistake.InfuriateDelayed;
                        }
                        break;
                    case AID.Infuriate:
                        isOGCD = true;
                        res |= CheckValid(state.Unlocked(AID.Infuriate));
                        res |= AdvanceTime(state, ref t, 0.6f, state.CD(CDGroup.Infuriate) - 60);
                        res |= AdjustCD(state, (int)CDGroup.Infuriate, 60, 120);
                        res |= GainGauge(state, 50);
                        if (state.NascentChaosLeft > 0)
                        {
                            res |= Mistake.NascentChaosOverwrite;
                        }
                        if (state.Unlocked(AID.ChaoticCyclone))
                        {
                            state.NascentChaosLeft = 30;
                        }
                        break;
                    case AID.Onslaught:
                        isOGCD = true;
                        res |= CheckValid(state.Unlocked(AID.Onslaught));
                        res |= AdvanceTime(state, ref t, 0.6f, state.CD(CDGroup.Onslaught) - 60);
                        res |= AdjustCD(state, (int)CDGroup.Onslaught, 30, 90, state.Unlocked(TraitID.EnhancedOnslaught) ? 0 : 30);
                        break;
                    case AID.Upheaval:
                    case AID.Orogeny:
                        isOGCD = true;
                        res |= CheckValid(aid == AID.Orogeny ? state.Unlocked(AID.Orogeny) : state.Unlocked(AID.Upheaval));
                        res |= AdvanceTime(state, ref t, 0.6f, state.CD(CDGroup.Upheaval));
                        res |= AdjustCD(state, (int)CDGroup.Upheaval, 30, 30);
                        break;
                    case AID.Berserk:
                    case AID.InnerRelease:
                        isOGCD = true;
                        res |= CheckValid(aid == AID.InnerRelease ? state.Unlocked(AID.InnerRelease) : state.Unlocked(AID.Berserk));
                        res |= AdvanceTime(state, ref t, 0.6f, state.CD(aid == AID.InnerRelease ? CDGroup.InnerRelease : CDGroup.Berserk));
                        res |= AdjustCD(state, (int)CDGroup.InnerRelease, 60, 60);
                        state.InnerReleaseLeft = 15;
                        state.InnerReleaseStacks = 3;
                        if (state.Unlocked(AID.PrimalRend))
                            state.PrimalRendLeft = 30;
                        if (state.SurgingTempestLeft > 0)
                            res |= GainSurgingTempest(state, 10);
                        break;
                    case AID.Tomahawk:
                        res |= CheckValid(state.Unlocked(AID.Tomahawk));
                        res |= AdvanceTime(state, ref t, 0.6f, state.GCD);
                        res |= AdjustCD(state, CommonDefinitions.GCDGroup, 2.5f, 2.5f);
                        res |= UseIRCharge(state, false, 0);
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

        public string ActionString(ActionID action)
        {
            return action == CommonDefinitions.IDSprint ? "Sprint" : action.Type == ActionType.Item ? "StatPotion" : ((AID)action.ID).ToString();
        }

        private void DrawActionRow(ActionID action, bool isGCD, Mistake mistake, float t, Rotation.State state, Rotation.Strategy strategy)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{t:f1}");

            ImGui.TableNextColumn();
            ImGui.PushStyleColor(ImGuiCol.Text, state.RaidBuffsLeft > 0 ? 0xff00ff00 : 0xffffffff);
            ImGui.TextUnformatted($"{(isGCD ? "" : "** ")}{ActionString(action)}");
            ImGui.PopStyleColor();

            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{MistakeString(mistake)}");

            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{state.Gauge:f0}");
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{state.SurgingTempestLeft:f1}");

            ImGui.TableNextColumn();
            if (state.InnerReleaseStacks > 0)
                ImGui.TextUnformatted($"{state.InnerReleaseStacks} stacks, {state.InnerReleaseLeft:f1} left");
            else if (state.CD(state.Unlocked(AID.InnerRelease) ? CDGroup.InnerRelease : CDGroup.Berserk) > 0)
                ImGui.TextUnformatted($"{state.CD(state.Unlocked(AID.InnerRelease) ? CDGroup.InnerRelease : CDGroup.Berserk):f1} cd");
            else
                ImGui.TextUnformatted($"{state.CD(state.Unlocked(AID.InnerRelease) ? CDGroup.InnerRelease : CDGroup.Berserk):f1} delay");

            ImGui.TableNextColumn();
            if (state.CD(CDGroup.Infuriate) > 60)
                ImGui.TextUnformatted($"0, {(state.CD(CDGroup.Infuriate) - 60):f1} cd");
            else if (state.CD(CDGroup.Infuriate) > 0)
                ImGui.TextUnformatted($"1, {state.CD(CDGroup.Infuriate):f1} cd");
            else
                ImGui.TextUnformatted($"2, {state.CD(CDGroup.Infuriate):f1} delay");

            ImGui.TableNextColumn();
            if (state.NascentChaosLeft > 0)
                ImGui.TextUnformatted($"{state.NascentChaosLeft:f1}");

            ImGui.TableNextColumn();
            if (state.PrimalRendLeft > 0)
                ImGui.TextUnformatted($"{state.PrimalRendLeft:f1}");

            ImGui.TableNextColumn();
            if (state.CD(CDGroup.Upheaval) > 0)
                ImGui.TextUnformatted($"{state.CD(CDGroup.Upheaval):f1} cd");
            else
                ImGui.TextUnformatted($"{state.CD(CDGroup.Upheaval):f1} delay");

            ImGui.TableNextColumn();
            if (state.CD(CDGroup.Onslaught) > 60)
                ImGui.TextUnformatted($"0, {(state.CD(CDGroup.Onslaught) - 60):f1} cd");
            else if (state.CD(CDGroup.Onslaught) > 30)
                ImGui.TextUnformatted($"1, {(state.CD(CDGroup.Onslaught) - 30):f1} cd");
            else if (state.CD(CDGroup.Onslaught) > 0)
                ImGui.TextUnformatted($"2, {state.CD(CDGroup.Onslaught):f1} cd");
            else
                ImGui.TextUnformatted($"3, {state.CD(CDGroup.Onslaught):f1} delay");

            ImGui.TableNextColumn();
            if (state.ComboTimeLeft > 0)
                ImGui.TextUnformatted($"{state.ComboLastMove} ({state.ComboTimeLeft:f1} left)");
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

        private Mistake AdjustCD(Rotation.State state, int cooldownGroup, float useCD, float maxCD, float minCD = 0)
        {
            return IncrementOvercap(ref state.Cooldowns[cooldownGroup], useCD, minCD, maxCD) ? Mistake.InvalidMove : Mistake.None;
        }

        private Mistake GainGauge(Rotation.State state, int value)
        {
            if (!state.Unlocked(AID.InnerBeast))
                return Mistake.None;

            if ((state.Gauge += value) > 100)
            {
                state.Gauge = 100;
                return Mistake.GaugeOvercap;
            }
            return Mistake.None;
        }

        private Mistake GainSurgingTempest(Rotation.State state, float value)
        {
            return IncrementOvercap(ref state.SurgingTempestLeft, value, 0, 60) ? Mistake.SurgingTempestOvercap : Mistake.None;
        }

        private Mistake UseIRCharge(Rotation.State state, bool isFC, int gaugeCost)
        {
            if (!isFC || !state.Unlocked(AID.InnerRelease) || state.InnerReleaseStacks <= 0)
                state.Gauge -= gaugeCost;

            if (state.InnerReleaseStacks > 0 && (isFC || !state.Unlocked(AID.InnerRelease)))
            {
                if (--state.InnerReleaseStacks == 0)
                    state.InnerReleaseLeft = 0;
            }
            return Mistake.None;
        }

        private bool ComboAdvance(Rotation.State state, ref Mistake mistake, AID prev, AID next)
        {
            if (state.ComboLastMove != prev)
            {
                mistake |= Mistake.ComboWrongMove;
                if (prev == AID.None)
                {
                    // restart combo chain anyway
                    state.ComboLastAction = (uint)next;
                    state.ComboTimeLeft = 30;
                }
                else
                {
                    state.ComboLastAction = 0;
                    state.ComboTimeLeft = 0;
                }
                return false;
            }
            else
            {
                state.ComboLastAction = (uint)next;
                state.ComboTimeLeft = next != AID.None ? 30 : 0;
                return true;
            }
        }
    }
}
