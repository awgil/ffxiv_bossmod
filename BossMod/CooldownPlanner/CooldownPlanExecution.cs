using ImGuiNET;
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
            public ActionID ID;
            public ActionDefinition Definition;
            public List<(float Start, float End)> ActivationWindows = new();
            public NextEvent? NextActivation = null;

            public PerAbility(ActionID id, ActionDefinition definition)
            {
                ID = id;
                Definition = definition;
            }
        }

        public class PerState
        {
            public StateFlag Downtime;
            public StateFlag Positioning;
            public StateFlag Vulnerable;
            public List<PerAbility> Abilities = new();

            internal PerState(CooldownPlan? plan)
            {
                if (plan == null)
                    return;

                var classData = AbilityDefinitions.Classes[plan.Class];
                foreach (var (k, uses) in plan.PlanAbilities)
                {
                    var action = new ActionID(k);
                    Abilities.Add(new(action, classData.Abilities[action].Definition));
                }
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
                ProcessState(tree, tree.Phases[0].StartingNode, Pull, 0);
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

        public float EstimateTimeToNextVulnerable(StateMachine? sm)
        {
            var s = FindStateData(sm?.ActiveState);
            if (s.Vulnerable.Active)
                return 0;
            else if (s.Vulnerable.Transition == null)
                return 10000; // no known vulnerabilities going forward
            else
                return s.Vulnerable.Transition.Value.EstimatedTime - sm!.TimeSinceTransitionClamped;
        }

        public IEnumerable<(ActionID Action, ActionDefinition Definition, float TimeLeft)> ActiveActions(StateMachine sm)
        {
            var progress = sm.TimeSinceTransitionClamped;
            var stateData = FindStateData(sm.ActiveState);
            foreach (var plan in stateData.Abilities)
            {
                var activeWindow = plan.ActivationWindows.FindIndex(w => w.Start <= progress && w.End > progress);
                if (activeWindow != -1)
                {
                    yield return (plan.ID, plan.Definition, plan.ActivationWindows[activeWindow].End - progress);
                }
            }
        }

        public void Draw(StateMachine sm)
        {
            var db = Plan != null ? AbilityDefinitions.Classes[Plan.Class] : null;
            var s = FindStateData(sm.ActiveState);
            var t = sm.TimeSinceTransitionClamped;
            foreach (var ability in s.Abilities)
            {
                var cd = ability.Definition.Cooldown;
                int nextWindow = ability.ActivationWindows.FindIndex(w => w.End > t);
                bool windowActive = nextWindow != -1 && t >= ability.ActivationWindows[nextWindow].Start;
                float? nextTransition = windowActive ? ability.ActivationWindows[nextWindow].End : nextWindow != -1 ? ability.ActivationWindows[nextWindow].Start : ability.NextActivation?.EstimatedTime;

                var name = ability.ID.Name();
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

        private PerState ProcessState(StateMachineTree tree, StateMachineTree.Node curState, PerState prev, float prevDuration)
        {
            var s = States[curState.State.ID] = new PerState(Plan);
            s.Downtime.Active = curState.IsDowntime;
            s.Positioning.Active = curState.IsPositioning;
            s.Vulnerable.Active = curState.IsVulnerable;

            if (Plan != null)
            {
                foreach (var (curAbility, prevAbility) in s.Abilities.Zip(prev.Abilities))
                {
                    var uses = Plan.PlanAbilities[curAbility.ID.Raw];
                    if (prevAbility.ActivationWindows.Count > 0)
                    {
                        var last = prevAbility.ActivationWindows.Last();
                        var overlap = last.End - prevDuration;
                        if (overlap > 0)
                            curAbility.ActivationWindows.Add((0, overlap));
                    }

                    foreach (var use in uses.Where(use => use.StateID == curState.State.ID))
                    {
                        curAbility.ActivationWindows.Add((use.TimeSinceActivation, use.TimeSinceActivation + use.WindowLength));
                    }

                    // sort and merge activation windows, so that they form disjoint ranges
                    curAbility.ActivationWindows.SortBy(e => e.Start);
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
                    ProcessState(tree, succ, s, curState.State.Duration);
                }
            }
            else if (curState.PhaseID + 1 < tree.Phases.Count)
            {
                // transition to next phase
                ProcessState(tree, tree.Phases[curState.PhaseID + 1].StartingNode, s, curState.State.Duration + phaseLeft);
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
            UpdateFlagTransition(ref s.Vulnerable, next.Vulnerable, nextID, curDuration);

            foreach (var (nextAbility, curAbility) in next.Abilities.Zip(s.Abilities))
            {
                if (nextAbility.ActivationWindows.Count > 0)
                {
                    curAbility.NextActivation = new() { StateID = nextID, EstimatedTime = curDuration + nextAbility.ActivationWindows.First().Start };
                }
                else if (nextAbility.NextActivation != null)
                {
                    curAbility.NextActivation = new() { StateID = nextAbility.NextActivation.Value.StateID, EstimatedTime = curDuration + nextAbility.NextActivation.Value.EstimatedTime };
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
