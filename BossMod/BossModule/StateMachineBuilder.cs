using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    // utility for building state machines for boss modules
    // conventions for id:
    // - high word (mask 0xFFFF0000) is used for high level groups - states sharing high word are grouped together
    // - high byte (mask 0xFF000000) is used for independent subsequences (e.g. forks) - states sharing high byte belong to same subsequence
    // - fourth nibble (mask 0x0000F0000) is used for independent large-scale mechanics that are still parts of the same logical group (typically having single name)
    // - first nibble (mask 0x0000000F) is used for smallest possible states (e.g. cast-start + cast-end)
    // - second and third nibble can be used by modules needing more hierarchy levels
    // this is all done to provide ids that are relatively stable across refactorings (these are used e.g. for cooldown planning)
    public class StateMachineBuilder
    {
        protected BossModule Module;
        private StateMachine.State? _lastState;
        private Dictionary<uint, StateMachine.State> _states;

        public StateMachineBuilder(BossModule module)
        {
            Module = module;
            _states = new();
        }

        // create a simple state without any actions
        public StateMachine.State Simple(uint id, float duration, string name)
        {
            if (_states.ContainsKey(id))
                throw new Exception($"Duplicate state id {id}");

            var state = _states[id] = new() { Name = name, Duration = duration, ID = id };
            if (_lastState != null)
            {
                _lastState.Next = state;
                if ((_lastState.ID & 0xFFFF0000) == (id & 0xFFFF0000))
                    _lastState.EndHint |= StateMachine.StateHint.GroupWithNext;
            }
            else if (Module.InitialState == null)
            {
                Module.InitialState = state;
            }
            else
            {
                throw new Exception($"Failed to link new state {id}");
            }

            _lastState = state;
            return state;
        }

        // create a state triggered by timeout
        public StateMachine.State Timeout(uint id, float duration, string name = "")
        {
            var state = Simple(id, duration, name);
            state.Comment = "Timeout";
            state.Update = timeSinceTransition => timeSinceTransition >= state.Duration ? state.Next : null;
            return state;
        }

        // create a state triggered by custom condition, or if it doesn't happen, by timeout
        public StateMachine.State Condition(uint id, float expected, Func<bool> condition, string name = "", float maxOverdue = 1, float checkDelay = 0)
        {
            var state = Simple(id, expected, name);
            state.Comment = "Generic condition";
            state.Update = timeSinceTransition => timeSinceTransition >= (expected + maxOverdue) || (timeSinceTransition >= checkDelay && condition()) ? state.Next : null;
            return state;
        }

        // create a fork state that checks passed condition; when it returns non-null, next state is one built by corresponding action in dispatch map
        public StateMachine.State ConditionFork<Key>(uint id, float expected, Func<bool> condition, Func<Key> select, Dictionary<Key, Action> dispatch, string name = "")
            where Key : notnull
        {
            Dictionary<Key, StateMachine.State?> stateDispatch = new();

            var state = Simple(id, expected, name);
            state.Comment = $"Fork: [{string.Join(", ", dispatch.Keys)}]";
            state.Update = _ =>
            {
                if (!condition())
                    return null;

                var key = select();
                var fork = stateDispatch.GetValueOrDefault(key);
                if (fork == null)
                    Service.Log($"[StateMachine] Unexpected fork condition result: got {key}");
                return fork;
            };

            var prevInit = Module.InitialState;
            foreach (var (key, action) in dispatch)
            {
                _lastState = Module.InitialState = null;
                action();
                stateDispatch[key] = Module.InitialState;
            }
            Module.InitialState = prevInit;
            _lastState = null;

            state.PotentialSuccessors = stateDispatch.Values.OfType<StateMachine.State>().Distinct().ToArray();
            return state;
        }

        // create a state triggered by component condition (or timeout if it never happens); if component is not present, error is logged and transition is triggered immediately
        public StateMachine.State ComponentCondition<T>(uint id, float expected, Func<T, bool> condition, string name = "", float maxOverdue = 1, float checkDelay = 0) where T : BossModule.Component
        {
            var state = Simple(id, expected, name);
            state.Comment = $"Condition on {typeof(T).Name}";
            state.Update = (timeSinceTransition) =>
            {
                var comp = Module.FindComponent<T>();
                if (comp == null)
                {
                    Service.Log($"[StateMachine] Component {typeof(T)} needed for condition is missing");
                    return state.Next;
                }
                return timeSinceTransition >= (expected + maxOverdue) || (timeSinceTransition >= checkDelay && condition(comp)) ? state.Next : null;
            };
            return state;
        }

        // create a fork state triggered by component condition
        public StateMachine.State ComponentConditionFork<T, Key>(uint id, float expected, Func<T, bool> condition, Func<T, Key> select, Dictionary<Key, Action> dispatch, string name = "")
            where T : BossModule.Component
            where Key : notnull
        {
            Func<bool> cond = () =>
            {
                var comp = Module.FindComponent<T>();
                if (comp == null)
                {
                    Service.Log($"[StateMachine] Component {typeof(T)} needed for condition is missing");
                    return false;
                }
                return condition(comp);
            };
            return ConditionFork(id, expected, cond, () => select(Module.FindComponent<T>()!), dispatch, name);
        }

        // create a state triggered by expected cast start by a primary actor; unexpected casts still trigger a transition, but log error
        public StateMachine.State CastStart<AID>(uint id, AID aid, float delay, string name = "")
            where AID : Enum
        {
            var state = Simple(id, delay, name);
            var expected = ActionID.MakeSpell(aid);
            state.Comment = $"Cast start: {aid}";
            state.Update = _ =>
            {
                var castInfo = Module.PrimaryActor.CastInfo;
                if (castInfo == null)
                    return null;
                if (castInfo.Action != expected)
                    Service.Log($"[StateMachine] Unexpected cast start for actor {Module.PrimaryActor.OID:X}: got {castInfo.Action}, expected {id}");
                return state.Next;
            };
            state.EndHint |= StateMachine.StateHint.BossCastStart;
            return state;
        }

        // create a state triggered by one of a set of expected casts by a primary actor; unexpected casts still trigger a transition, but log error
        public StateMachine.State CastStartMulti<AID>(uint id, IEnumerable<AID> aids, float delay, string name = "")
            where AID : Enum
        {
            var state = Simple(id, delay, name);
            state.Comment = $"Cast start: [{string.Join(", ", aids)}]";
            state.Update = _ =>
            {
                var castInfo = Module.PrimaryActor.CastInfo;
                if (castInfo == null)
                    return null;
                if (!aids.Any(aid => castInfo.IsSpell(aid)))
                    Service.Log($"[StateMachine] Unexpected cast start for actor {Module.PrimaryActor.OID:X}: got {castInfo.Action}");
                return state.Next;
            };
            state.EndHint |= StateMachine.StateHint.BossCastStart;
            return state;
        }

        // create a state triggered by one of a set of expected casts by a primary actor, each of which forking to a separate subsequence
        // values in map are actions building state chains corresponding to each fork
        public StateMachine.State CastStartFork<AID>(uint id, Dictionary<AID, Action> dispatch, float delay, string name = "")
             where AID : Enum
        {
            var state = ConditionFork(id, delay, () => Module.PrimaryActor.CastInfo != null, () => (AID)(object)(Module.PrimaryActor.CastInfo!.IsSpell() ? Module.PrimaryActor.CastInfo.Action.ID : 0), dispatch, name);
            state.EndHint |= StateMachine.StateHint.BossCastStart;
            return state;
        }

        // create a state triggered by cast end by a primary actor
        public StateMachine.State CastEnd(uint id, float castTime, string name = "")
        {
            var state = Simple(id, castTime, name);
            state.Comment = "Cast end";
            state.Update = _ => Module.PrimaryActor.CastInfo == null ? state.Next : null;
            state.EndHint |= StateMachine.StateHint.BossCastEnd;
            return state;
        }

        // create a chain of states: CastStart -> CastEnd; second state uses id+1
        public StateMachine.State Cast<AID>(uint id, AID aid, float delay, float castTime, string name = "")
            where AID : Enum
        {
            CastStart(id, aid, delay, "");
            return CastEnd(id + 1, castTime, name);
        }

        // create a chain of states: CastStartMulti -> CastEnd; second state uses id+1
        public StateMachine.State CastMulti<AID>(uint id, IEnumerable<AID> aids, float delay, float castTime, string name = "")
            where AID : Enum
        {
            CastStartMulti(id, aids, delay, "");
            return CastEnd(id + 1, castTime, name);
        }

        // create a state triggered by a primary actor becoming (un)targetable; automatically sets downtime begin/end flag
        public StateMachine.State Targetable(uint id, bool targetable, float delay, string name = "", float checkDelay = 0)
        {
            var state = Simple(id, delay, name);
            state.Comment = targetable ? "Targetable" : "Untargetable";
            state.Update = timeSinceTransition => timeSinceTransition >= checkDelay && Module.PrimaryActor.IsTargetable == targetable ? state.Next : null;
            state.EndHint |= targetable ? StateMachine.StateHint.DowntimeEnd : StateMachine.StateHint.DowntimeStart;
            return state;
        }
    }
}
