using System;

namespace BossMod
{
    // note: functions that create state chains assign first state to link and return last state
    public class CommonStates
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
            state.Update = (float timeSinceTransition) => state.Done = timeSinceTransition >= state.Duration;
            return state;
        }

        // create state triggered by any cast start by a particular actor
        public static StateMachine.State CastStart(ref StateMachine.State? link, WorldState.Actor actor, float delay, string name = "", bool actorIsBoss = true)
        {
            var state = Simple(ref link, delay, name);
            state.Update = (float timeSinceTransition) => state.Done = actor.CastInfo != null;
            if (actorIsBoss)
                state.EndHint |= StateMachine.StateHint.BossCastStart;
            return state;
        }

        // create state triggered by expected cast start by a particular actor; unexpected casts still trigger a transition, but log error
        public static StateMachine.State CastStart<AID>(ref StateMachine.State? link, WorldState.Actor actor, AID id, float delay, string name = "", bool actorIsBoss = true)
            where AID : Enum
        {
            var state = CastStart(ref link, actor, delay, name, actorIsBoss);
            state.Exit = () =>
            {
                if (actor.CastInfo != null && actor.CastInfo.ActionID != Convert.ToUInt32(id))
                {
                    Service.Log($"Unexpected cast start for actor {actor.OID:X}: got {actor.CastInfo!.ActionID}, expected {id}");
                }
            };
            return state;
        }

        // create state triggered by cast end by a particular actor
        public static StateMachine.State CastEnd(ref StateMachine.State? link, WorldState.Actor actor, float castTime, string name = "", bool actorIsBoss = true)
        {
            var state = Simple(ref link, castTime, name);
            state.Update = (float timeSinceTransition) => state.Done = actor.CastInfo == null;
            if (actorIsBoss)
                state.EndHint |= StateMachine.StateHint.BossCastEnd;
            return state;
        }

        // create a chain of states: CastStart -> CastEnd (and optionally -> Timeout)
        public static StateMachine.State Cast<AID>(ref StateMachine.State? link, WorldState.Actor actor, AID id, float delay, float castTime, string name = "", bool actorIsBoss = true)
            where AID : Enum
        {
            var s = CastStart(ref link, actor, id, delay, "", actorIsBoss);
            return CastEnd(ref s.Next, actor, castTime, name, actorIsBoss);
        }

        public static StateMachine.State Cast<AID>(ref StateMachine.State? link, WorldState.Actor actor, AID id, float delay, float castTime, float resolve, string name = "", bool actorIsBoss = true)
            where AID : Enum
        {
            var s = CastStart(ref link, actor, id, delay, "", actorIsBoss);
            s = CastEnd(ref s.Next, actor, castTime, "", actorIsBoss);
            return Timeout(ref s.Next, resolve, name);
        }
    }
}
