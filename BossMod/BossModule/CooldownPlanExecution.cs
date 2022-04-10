using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    // per-state list of activation windows based on cooldown plan
    // this solves the problem of e.g. window start being at the end of state and early switch to next
    public class CooldownPlanExecution
    {
        public struct NextEvent
        {
            public uint StateID;
            public float EstimatedTime;
        }

        public struct StateFlag
        {
            public bool Active;
            public NextEvent? Transition;
        }

        public class PerAbility
        {
            public List<(float Start, float End)> ActivationWindows = new();
            public NextEvent? NextActivation = null;
        }

        public class PerState
        {
            public StateFlag Downtime;
            public StateFlag Positioning;
            public Dictionary<ActionID, PerAbility> Abilities = new();
        }

        public CooldownPlan? Plan;
        public PerState Pull = new();
        public Dictionary<uint, PerState> States = new();

        public CooldownPlanExecution(StateMachine.State? initial, CooldownPlan? plan)
        {
            Plan = plan;
            if (initial != null)
            {
                var s = ProcessState(initial, null, null, plan);
                UpdateTransitions(Pull, s, initial.ID, 0);
            }
        }

        public PerState FindStateData(StateMachine.State? s)
        {
            var state = s != null ? States.GetValueOrDefault(s.ID) : null;
            return state ?? Pull;
        }

        public float EstimateTimeToNextDowntime(StateMachine? sm)
        {
            var s = FindStateData(sm?.ActiveState);
            if (s.Downtime.Active)
                return 0;
            else if (s.Downtime.Transition == null)
                return 10000; // this is a fork and we don't know where we'll go - assume there will be no downtime ever...
            else
                return s.Downtime.Transition.Value.EstimatedTime - sm!.TimeSinceTransition;
        }

        public float EstimateTimeToNextPositioning(StateMachine? sm)
        {
            var s = FindStateData(sm?.ActiveState);
            if (s.Positioning.Active)
                return 0;
            else if (s.Positioning.Transition == null)
                return 10000; // no known positionings going forward
            else
                return s.Positioning.Transition.Value.EstimatedTime - sm!.TimeSinceTransition;
        }

        public void Draw(StateMachine? sm)
        {
            var db = Plan != null ? AbilityDefinitions.Classes[Plan.Class] : null;
            var s = FindStateData(sm?.ActiveState);
            var t = sm?.TimeSinceTransition ?? 0;
            foreach (var (action, ability) in s.Abilities)
            {
                float cd = db?.Abilities.GetValueOrDefault(action)?.Cooldown ?? 0;
                int nextWindow = ability.ActivationWindows.FindIndex(w => w.End > t);
                bool windowActive = nextWindow != -1 && t >= ability.ActivationWindows[nextWindow].Start;
                float? nextTransition = windowActive ? ability.ActivationWindows[nextWindow].End : nextWindow != -1 ? ability.ActivationWindows[nextWindow].Start : ability.NextActivation?.EstimatedTime;

                var name = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(action.ID)?.Name.ToString() ?? "(unknown)";
                if (nextTransition == null)
                {
                    ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0x80808080), name);
                }
                else if (windowActive)
                {
                    ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff00ffff), $"{name}: use now! ({nextTransition.Value - t:f1}s left)");
                }
                else
                {
                    var left = nextTransition.Value - t;
                    ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(left < cd ? 0xffffffff : 0x80808080), $"{name}: in {left:f1}s");
                }
            }
        }

        private PerState ProcessState(StateMachine.State curState, PerState? prev, StateMachine.State? prevState, CooldownPlan? plan)
        {
            var s = States[curState.ID] = new PerState();

            if (prev != null)
            {
                var edgeHint = prevState!.EndHint;
                s.Downtime.Active = (prev.Downtime.Active || edgeHint.HasFlag(StateMachine.StateHint.DowntimeStart)) && !edgeHint.HasFlag(StateMachine.StateHint.DowntimeEnd);
                s.Positioning.Active = (prev.Positioning.Active || edgeHint.HasFlag(StateMachine.StateHint.PositioningStart)) && !edgeHint.HasFlag(StateMachine.StateHint.PositioningEnd);
            }

            if (plan != null)
            {
                foreach (var (k, uses) in plan.PlanAbilities)
                {
                    var curAbility = s.Abilities[new(k)] = new();
                    if (prev != null)
                    {
                        var prevWindows = prev.Abilities[new(k)].ActivationWindows;
                        if (prevWindows.Count > 0)
                        {
                            var last = prevWindows.Last();
                            var overlap = last.End - prevState!.Duration;
                            if (overlap > 0)
                                curAbility.ActivationWindows.Add((0, overlap));
                        }
                    }

                    foreach (var use in uses.Where(use => use.StateID == curState.ID))
                    {
                        curAbility.ActivationWindows.Add((use.TimeSinceActivation, use.TimeSinceActivation + use.WindowLength));
                    }

                    // sort and merge activation windows, so that they form disjoint ranges
                    curAbility.ActivationWindows.Sort((l, r) => l.Start.CompareTo(r.Start));
                    for (int i = 1; i < curAbility.ActivationWindows.Count; ++i)
                    {
                        if (curAbility.ActivationWindows[i].Start > curAbility.ActivationWindows[i - 1].End)
                            continue;

                        if (curAbility.ActivationWindows[i].End > curAbility.ActivationWindows[i - 1].End)
                            curAbility.ActivationWindows[i - 1] = (curAbility.ActivationWindows[i - 1].Start, curAbility.ActivationWindows[i].End);

                        curAbility.ActivationWindows.RemoveAt(i--);
                    }
                }
            }

            if (curState.Next != null)
            {
                var next = ProcessState(curState.Next, s, curState, plan);
                UpdateTransitions(s, next, curState.Next.ID, curState.Duration);
            }

            if (curState.PotentialSuccessors != null)
            {
                foreach (var succ in curState.PotentialSuccessors.Where(s => s != curState.Next))
                {
                    ProcessState(succ, s, curState, plan);
                }
            }

            if (curState.Next == null && (curState.PotentialSuccessors?.Length ?? 0) == 0 && !s.Downtime.Active)
            {
                // assume downtime starts after last state (enrage)
                s.Downtime.Transition = new() { EstimatedTime = curState.Duration };
            }

            return s;
        }

        private void UpdateTransitions(PerState s, PerState next, uint nextID, float curDuration)
        {
            UpdateFlagTransition(ref s.Downtime, next.Downtime, nextID, curDuration);
            UpdateFlagTransition(ref s.Positioning, next.Positioning, nextID, curDuration);

            foreach (var (aid, nextAbility) in next.Abilities)
            {
                var curAbilities = s.Abilities.GetOrAdd(aid);
                if (nextAbility.ActivationWindows.Count > 0)
                {
                    curAbilities.NextActivation = new() { StateID = nextID, EstimatedTime = curDuration + nextAbility.ActivationWindows.First().Start };
                }
                else if (nextAbility.NextActivation != null)
                {
                    curAbilities.NextActivation = new() { StateID = nextAbility.NextActivation.Value.StateID, EstimatedTime = curDuration + nextAbility.NextActivation.Value.EstimatedTime };
                }
            }
        }

        private void UpdateFlagTransition(ref StateFlag curFlag, StateFlag nextFlag, uint nextID, float curDuration)
        {
            if (curFlag.Active != nextFlag.Active)
            {
                curFlag.Transition = new() { StateID = nextID, EstimatedTime = curDuration };
            }
            else if (nextFlag.Transition != null)
            {
                curFlag.Transition = new() { StateID = nextFlag.Transition.Value.StateID, EstimatedTime = curDuration + nextFlag.Transition.Value.EstimatedTime };
            }
        }
    }
}
