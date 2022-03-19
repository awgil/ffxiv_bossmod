using System;
using System.Collections.Generic;

namespace BossMod
{
    // note: functions that create state chains assign first state to link and return last state
    public static class CommonStates
    {
        // create simple state without any actions; by default, if name is empty, it is marked as substate
        public static StateMachine.State Simple(ref StateMachine.State? link, float duration, string name = "")
        {
            if (link != null)
                Service.Log($"[StateMachine] Overwriting link from {link.Name} to {name}");
            return link = new StateMachine.State
            {
                Name = name,
                Duration = duration,
                EndHint = (name.Length == 0) ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.None,
            };
        }

        // create state triggered by timeout
        public static StateMachine.State Timeout(ref StateMachine.State? link, float duration, string name = "")
        {
            var state = Simple(ref link, duration, name);
            state.Update = (timeSinceTransition) => timeSinceTransition >= state.Duration ? state.Next : null;
            return state;
        }

        // create state triggered by custom condition, or if it doesn't happen, by timeout
        public static StateMachine.State Condition(ref StateMachine.State? link, float expected, Func<bool> condition, string name = "", float maxOverdue = 1, float checkDelay = 0)
        {
            var state = Simple(ref link, expected, name);
            state.Update = (timeSinceTransition) => timeSinceTransition >= (expected + maxOverdue) || (timeSinceTransition >= checkDelay && condition()) ? state.Next : null;
            return state;
        }

        // create state triggered by component condition (or timeout if it never happens); if component is not present, error is logged and transition is triggered immediately
        public static StateMachine.State ComponentCondition<T>(ref StateMachine.State? link, float expected, BossModule module, Func<T, bool> condition, string name = "", float maxOverdue = 1, float checkDelay = 0) where T : BossModule.Component
        {
            var state = Simple(ref link, expected, name);
            state.Update = (timeSinceTransition) =>
            {
                var comp = module.FindComponent<T>();
                if (comp == null)
                {
                    Service.Log($"[StateMachine] Component {typeof(T)} needed for condition is missing");
                    return state.Next;
                }
                return timeSinceTransition >= (expected + maxOverdue) || (timeSinceTransition >= checkDelay && condition(comp)) ? state.Next : null;
            };
            return state;
        }

        // create state triggered by any cast start by a primary actor
        public static StateMachine.State CastStart(ref StateMachine.State? link, BossModule module, float delay, string name = "")
        {
            var state = Simple(ref link, delay, name);
            state.Update = (_) => module.PrimaryActor.CastInfo != null ? state.Next : null;
            state.EndHint |= StateMachine.StateHint.BossCastStart;
            return state;
        }

        // create state triggered by expected cast start by a primary actor; unexpected casts still trigger a transition, but log error
        public static StateMachine.State CastStart<AID>(ref StateMachine.State? link, BossModule module, AID id, float delay, string name = "")
            where AID : Enum
        {
            var state = Simple(ref link, delay, name);
            var expected = ActionID.MakeSpell(id);
            state.Update = (_) =>
            {
                var castInfo = module.PrimaryActor.CastInfo;
                if (castInfo == null)
                    return null;
                if (castInfo.Action != expected)
                    Service.Log($"[StateMachine] Unexpected cast start for actor {module.PrimaryActor.OID:X}: got {castInfo.Action}, expected {id}");
                return state.Next;
            };
            state.EndHint |= StateMachine.StateHint.BossCastStart;
            return state;
        }

        // create state triggered by cast start by a primary actor; map is used to select reaction to each spell
        // is performed either to mapped state (if it is non-null) or, if entry is not found, to default Next state
        public static StateMachine.State CastStart<AID>(ref StateMachine.State? link, BossModule module, Dictionary<AID, (StateMachine.State?, Action)> dispatch, float delay, string name = "")
            where AID : Enum
        {
            var state = Simple(ref link, delay, name);
            state.Update = (_) =>
            {
                var castInfo = module.PrimaryActor.CastInfo;
                if (castInfo == null)
                    return null;
                (StateMachine.State? dest, Action? op) = dispatch.GetValueOrDefault((AID)(object)castInfo.Action.ID);
                if (op != null)
                    op();
                else
                    Service.Log($"[StateMachine] Unexpected cast start for actor {module.PrimaryActor.OID:X}: got {castInfo.Action}");
                return dest ?? state.Next;
            };
            state.EndHint |= StateMachine.StateHint.BossCastStart;

            HashSet<StateMachine.State> successors = new();
            foreach (var v in dispatch)
                if (v.Value.Item1 != null)
                    successors.Add(v.Value.Item1);
            state.PotentialSuccessors = new StateMachine.State[successors.Count];
            successors.CopyTo(state.PotentialSuccessors);

            return state;
        }

        // create state triggered by cast end by a primary actor
        public static StateMachine.State CastEnd(ref StateMachine.State? link, BossModule module, float castTime, string name = "")
        {
            var state = Simple(ref link, castTime, name);
            state.Update = (_) => module.PrimaryActor.CastInfo == null ? state.Next : null;
            state.EndHint |= StateMachine.StateHint.BossCastEnd;
            return state;
        }

        // create a chain of states: CastStart -> CastEnd
        public static StateMachine.State Cast<AID>(ref StateMachine.State? link, BossModule module, AID id, float delay, float castTime, string name = "")
            where AID : Enum
        {
            var s = CastStart(ref link, module, id, delay, "");
            return CastEnd(ref s.Next, module, castTime, name);
        }

        // create a state triggered by a primary actor becoming (un)targetable; automatically sets downtime begin/end flag
        public static StateMachine.State Targetable(ref StateMachine.State? link, BossModule module, bool targetable, float delay, string name = "", float checkDelay = 0)
        {
            var state = Simple(ref link, delay, name);
            state.Update = (timeSinceTransition) => timeSinceTransition >= checkDelay && module.PrimaryActor.IsTargetable == targetable ? state.Next : null;
            state.EndHint |= targetable ? StateMachine.StateHint.DowntimeEnd : StateMachine.StateHint.DowntimeStart;
            return state;
        }
    }
}
