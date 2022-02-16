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

        // create state triggered by any cast start by a particular actor
        public static StateMachine.State CastStart(ref StateMachine.State? link, Func<WorldState.Actor?> actorAcc, float delay, string name = "", bool actorIsBoss = true)
        {
            var state = Simple(ref link, delay, name);
            state.Update = (_) => actorAcc()?.CastInfo != null ? state.Next : null;
            if (actorIsBoss)
                state.EndHint |= StateMachine.StateHint.BossCastStart;
            return state;
        }

        // create state triggered by expected cast start by a particular actor; unexpected casts still trigger a transition, but log error
        public static StateMachine.State CastStart<AID>(ref StateMachine.State? link, Func<WorldState.Actor?> actorAcc, AID id, float delay, string name = "", bool actorIsBoss = true)
            where AID : Enum
        {
            var state = Simple(ref link, delay, name);
            var expected = ActionID.MakeSpell(id);
            state.Update = (_) =>
            {
                var castInfo = actorAcc()?.CastInfo;
                if (castInfo == null)
                    return null;
                if (castInfo.Action != expected)
                    Service.Log($"[StateMachine] Unexpected cast start for actor {actorAcc()?.OID:X}: got {castInfo.Action}, expected {id}");
                return state.Next;
            };
            if (actorIsBoss)
                state.EndHint |= StateMachine.StateHint.BossCastStart;
            return state;
        }

        // create state triggered by cast start by a particular actor; map is used to select reaction to each spell
        // is performed either to mapped state (if it is non-null) or, if entry is not found, to default Next state
        public static StateMachine.State CastStart<AID>(ref StateMachine.State? link, Func<WorldState.Actor?> actorAcc, Dictionary<AID, (StateMachine.State?, Action)> dispatch, float delay, string name = "", bool actorIsBoss = true)
            where AID : Enum
        {
            var state = Simple(ref link, delay, name);
            state.Update = (_) =>
            {
                var castInfo = actorAcc()?.CastInfo;
                if (castInfo == null)
                    return null;
                (StateMachine.State? dest, Action? op) = dispatch.GetValueOrDefault((AID)(object)castInfo.Action.ID);
                if (op != null)
                    op();
                else
                    Service.Log($"[StateMachine] Unexpected cast start for actor {actorAcc()?.OID:X}: got {castInfo.Action}");
                return dest ?? state.Next;
            };
            if (actorIsBoss)
                state.EndHint |= StateMachine.StateHint.BossCastStart;

            HashSet<StateMachine.State> successors = new();
            foreach (var v in dispatch)
                if (v.Value.Item1 != null)
                    successors.Add(v.Value.Item1);
            state.PotentialSuccessors = new StateMachine.State[successors.Count];
            successors.CopyTo(state.PotentialSuccessors);

            return state;
        }

        // create state triggered by cast end by a particular actor
        public static StateMachine.State CastEnd(ref StateMachine.State? link, Func<WorldState.Actor?> actorAcc, float castTime, string name = "", bool actorIsBoss = true)
        {
            var state = Simple(ref link, castTime, name);
            state.Update = (_) => actorAcc()?.CastInfo == null ? state.Next : null;
            if (actorIsBoss)
                state.EndHint |= StateMachine.StateHint.BossCastEnd;
            return state;
        }

        // create a chain of states: CastStart -> CastEnd (and optionally -> Timeout)
        public static StateMachine.State Cast<AID>(ref StateMachine.State? link, Func<WorldState.Actor?> actorAcc, AID id, float delay, float castTime, string name = "", bool actorIsBoss = true)
            where AID : Enum
        {
            var s = CastStart(ref link, actorAcc, id, delay, "", actorIsBoss);
            return CastEnd(ref s.Next, actorAcc, castTime, name, actorIsBoss);
        }

        public static StateMachine.State Cast<AID>(ref StateMachine.State? link, Func<WorldState.Actor?> actorAcc, AID id, float delay, float castTime, float resolve, string name = "", bool actorIsBoss = true)
            where AID : Enum
        {
            var s = CastStart(ref link, actorAcc, id, delay, "", actorIsBoss);
            s = CastEnd(ref s.Next, actorAcc, castTime, "", actorIsBoss);
            return Timeout(ref s.Next, resolve, name);
        }

        // create a state triggered by a particular actor becoming (un)targetable; automatically sets downtime begin/end flag
        public static StateMachine.State Targetable(ref StateMachine.State? link, Func<WorldState.Actor?> actorAcc, bool targetable, float delay, string name = "")
        {
            var state = Simple(ref link, delay, name);
            state.Update = (_) => actorAcc()?.IsTargetable == targetable ? state.Next : null;
            state.EndHint |= targetable ? StateMachine.StateHint.DowntimeEnd : StateMachine.StateHint.DowntimeStart;
            return state;
        }
    }
}
