using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

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

            internal PerState(CooldownPlan? plan)
            {
                if (plan != null)
                    foreach (var (k, uses) in plan.PlanAbilities)
                        Abilities[new(k)] = new();
            }
        }

        public CooldownPlan? Plan;
        public PerState Pull;
        public Dictionary<uint, PerState> States = new();

        public CooldownPlanExecution(StateMachine sm, CooldownPlan? plan)
        {
            Plan = plan;
            Pull = new(plan);

            var tree = new StateMachineTree(sm);
            tree.ApplyTimings(plan?.Timings);

            if (tree.Phases.Count > 0)
            {
                ProcessState(tree, tree.Phases[0].StartingNode, Pull, 0, plan);
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
                return s.Downtime.Transition.Value.EstimatedTime - sm!.TimeSinceTransitionClamped;
        }

        public float EstimateTimeToNextPositioning(StateMachine? sm)
        {
            var s = FindStateData(sm?.ActiveState);
            if (s.Positioning.Active)
                return 0;
            else if (s.Positioning.Transition == null)
                return 10000; // no known positionings going forward
            else
                return s.Positioning.Transition.Value.EstimatedTime - sm!.TimeSinceTransitionClamped;
        }

        public void Draw(StateMachine sm)
        {
            var db = Plan != null ? AbilityDefinitions.Classes[Plan.Class] : null;
            var s = FindStateData(sm.ActiveState);
            var t = sm.TimeSinceTransitionClamped;
            foreach (var (action, ability) in s.Abilities)
            {
                float cd = db?.Abilities.GetValueOrDefault(action)?.Cooldown ?? 0;
                int nextWindow = ability.ActivationWindows.FindIndex(w => w.End > t);
                bool windowActive = nextWindow != -1 && t >= ability.ActivationWindows[nextWindow].Start;
                float? nextTransition = windowActive ? ability.ActivationWindows[nextWindow].End : nextWindow != -1 ? ability.ActivationWindows[nextWindow].Start : ability.NextActivation?.EstimatedTime;

                var name = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(action.ID)?.Name.ToString() ?? "(unknown)";
                if (nextTransition == null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0x80808080);
                    ImGui.TextUnformatted(name);
                    ImGui.PopStyleColor();

                }
                else if (windowActive)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ffff);
                    ImGui.TextUnformatted($"{name}: use now! ({nextTransition.Value - t:f1}s left)");
                    ImGui.PopStyleColor();
                }
                else
                {
                    var left = nextTransition.Value - t;
                    ImGui.PushStyleColor(ImGuiCol.Text, left < cd ? 0xffffffff : 0x80808080);
                    ImGui.TextUnformatted($"{name}: in {left:f1}s");
                    ImGui.PopStyleColor();
                }
            }
        }

        private PerState ProcessState(StateMachineTree tree, StateMachineTree.Node curState, PerState prev, float prevDuration, CooldownPlan? plan)
        {
            var s = States[curState.State.ID] = new PerState(Plan);
            s.Downtime.Active = curState.IsDowntime;
            s.Positioning.Active = curState.IsPositioning;

            if (plan != null)
            {
                foreach (var (k, uses) in plan.PlanAbilities)
                {
                    var curAbility = s.Abilities[new(k)];

                    var prevWindows = prev.Abilities[new(k)].ActivationWindows;
                    if (prevWindows.Count > 0)
                    {
                        var last = prevWindows.Last();
                        var overlap = last.End - prevDuration;
                        if (overlap > 0)
                            curAbility.ActivationWindows.Add((0, overlap));
                    }

                    foreach (var use in uses.Where(use => use.StateID == curState.State.ID))
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

            var phaseLeft = tree.Phases[curState.PhaseID].Duration - curState.Time;
            if (phaseLeft > 0 && curState.Successors.Count > 0)
            {
                // transition to next state of the same phase
                foreach (var succ in curState.Successors)
                {
                    ProcessState(tree, succ, s, curState.State.Duration, plan);
                }
            }
            else if (curState.PhaseID + 1 < tree.Phases.Count)
            {
                // transition to next phase
                ProcessState(tree, tree.Phases[curState.PhaseID + 1].StartingNode, s, curState.State.Duration + phaseLeft, plan);
            }
            else
            {
                // transition to enrage
                if (!s.Downtime.Active)
                    s.Downtime.Transition = new() { EstimatedTime = curState.State.Duration + phaseLeft };
            }

            // update transition info from prev to this
            UpdateTransitions(prev, s, curState.State.ID, prevDuration);

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
