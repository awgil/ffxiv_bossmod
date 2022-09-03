using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // utility for building state machines for boss modules
    // conventions for id:
    // - high word (mask 0xFFFF0000) is used for high level groups - states sharing high word are grouped together
    // - high byte (mask 0xFF000000) is used for independent subsequences (e.g. forks and phases) - states sharing high byte belong to same subsequence
    // - fourth nibble (mask 0x0000F0000) is used for independent large-scale mechanics that are still parts of the same logical group (typically having single name)
    // - first nibble (mask 0x0000000F) is used for smallest possible states (e.g. cast-start + cast-end)
    // - second and third nibble can be used by modules needing more hierarchy levels
    // this is all done to provide ids that are relatively stable across refactorings (these are used e.g. for cooldown planning)
    public class StateMachineBuilder
    {
        // wrapper that simplifies building phases
        public class Phase
        {
            public StateMachine.Phase Raw { get; private init; }
            private BossModule _module;

            public Phase(StateMachine.Phase raw, BossModule module)
            {
                Raw = raw;
                _module = module;
            }

            // note that there is no "deactivate on exit", since phase change clears all components automatically
            public Phase ActivateOnEnter<C>(bool condition = true) where C : BossComponent, new()
            {
                if (condition)
                    Raw.Enter.Add(_module.ActivateComponent<C>);
                return this;
            }
        }

        // wrapper that simplifies building states
        public class State
        {
            public StateMachine.State Raw { get; private init; }
            private BossModule _module;

            public State(StateMachine.State raw, BossModule module)
            {
                Raw = raw;
                _module = module;
            }

            public State ActivateOnEnter<C>(bool condition = true) where C : BossComponent, new()
            {
                if (condition)
                    Raw.Enter.Add(_module.ActivateComponent<C>);
                return this;
            }

            public State DeactivateOnExit<C>(bool condition = true) where C : BossComponent
            {
                if (condition)
                    Raw.Exit.Add(_module.DeactivateComponent<C>);
                return this;
            }

            public State SetHint(StateMachine.StateHint h, bool condition = true)
            {
                if (condition)
                    Raw.EndHint |= h;
                return this;
            }

            public State ClearHint(StateMachine.StateHint h, bool condition = true)
            {
                if (condition)
                    Raw.EndHint &= ~h;
                return this;
            }
        }

        protected BossModule Module;
        private List<StateMachine.Phase> _phases = new();
        private StateMachine.State? _curInitial;
        private StateMachine.State? _lastState;
        private Dictionary<uint, StateMachine.State> _states = new();

        public StateMachineBuilder(BossModule module)
        {
            Module = module;
        }

        public StateMachine Build()
        {
            return new(_phases);
        }

        // create a simple phase; buildState is called to fill out phase states, argument is seqID << 24
        // note that on exit, all components are removed, since generally phase transition can happen at any time
        public Phase SimplePhase(uint seqID, Action<uint> buildState, string name, float dur = -1)
        {
            if (_curInitial != null)
                throw new Exception($"Trying to create phase '{name}' while inside another phase");
            buildState(seqID << 24);
            if (_curInitial == null)
                throw new Exception($"Phase '{name}' has no states");
            var phase = new StateMachine.Phase(_curInitial, name, dur);
            phase.Exit.Add(Module.ClearComponents);
            _phases.Add(phase);
            _curInitial = _lastState = null;
            return new(phase, Module);
        }

        // create a phase triggered by primary actor's hp reaching specific threshold
        public Phase HPPercentPhase(uint seqID, Action<uint> buildState, string name, float hpThreshold, float dur)
        {
            var phase = SimplePhase(seqID, buildState, name, dur);
            phase.Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsDead || Module.PrimaryActor.HP.Cur < Module.PrimaryActor.HP.Max * hpThreshold;
            return phase;
        }

        // create a phase for typical single-phase fight (triggered by primary actor dying)
        public Phase DeathPhase(uint seqID, Action<uint> buildState)
        {
            return HPPercentPhase(seqID, buildState, "Boss death", 0, -1);
        }

        // create a single-state phase; useful for modules with trivial state machines
        public Phase TrivialPhase(float enrage = 10000)
        {
            return DeathPhase(0, id => { SimpleState(id, enrage, "Enrage"); });
        }

        // create a simple state without any actions
        public State SimpleState(uint id, float duration, string name)
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
            else if (_curInitial == null)
            {
                _curInitial = state;
            }
            else
            {
                throw new Exception($"Failed to link new state {id}");
            }

            _lastState = state;
            return new(state, Module);
        }

        // create a state triggered by timeout
        public State Timeout(uint id, float duration, string name = "")
        {
            var state = SimpleState(id, duration, name);
            state.Raw.Comment = "Timeout";
            state.Raw.Update = timeSinceTransition => timeSinceTransition >= state.Raw.Duration ? state.Raw.Next : null;
            return state;
        }

        // create a state triggered by custom condition, or if it doesn't happen, by timeout
        public State Condition(uint id, float expected, Func<bool> condition, string name = "", float maxOverdue = 1, float checkDelay = 0)
        {
            var state = SimpleState(id, expected, name);
            state.Raw.Comment = "Generic condition";
            state.Raw.Update = timeSinceTransition =>
            {
                if (timeSinceTransition < checkDelay)
                    return null; // too early to check for condition

                if (condition())
                    return state.Raw.Next;

                if (timeSinceTransition < expected + maxOverdue)
                    return null;

                Module.ReportError(null, $"State {id:X}: transition triggered because of overdue");
                return state.Raw.Next;
            };
            return state;
        }

        // create a fork state that checks passed condition; when it returns non-null, next state is one built by corresponding action in dispatch map
        public State ConditionFork<Key>(uint id, float expected, Func<bool> condition, Func<Key> select, Dictionary<Key, (uint seqID, Action<uint> buildState)> dispatch, string name = "")
            where Key : notnull
        {
            Dictionary<Key, StateMachine.State?> stateDispatch = new();

            var state = SimpleState(id, expected, name);
            state.Raw.Comment = $"Fork: [{string.Join(", ", dispatch.Keys)}]";
            state.Raw.Update = _ =>
            {
                if (!condition())
                    return null;

                var key = select();
                var fork = stateDispatch.GetValueOrDefault(key);
                if (fork == null)
                    Module.ReportError(null, $"State {id:X}: unexpected fork condition result: got {key}");
                return fork;
            };

            var prevInit = _curInitial;
            foreach (var (key, action) in dispatch)
            {
                _lastState = _curInitial = null;
                action.buildState(action.seqID << 24);
                stateDispatch[key] = _curInitial;
            }
            _curInitial = prevInit;
            _lastState = null;

            state.Raw.PotentialSuccessors = stateDispatch.Values.OfType<StateMachine.State>().Distinct().ToArray();
            return state;
        }

        // create a state triggered by component condition (or timeout if it never happens); if component is not present, error is logged and transition is triggered immediately
        public State ComponentCondition<T>(uint id, float expected, Func<T, bool> condition, string name = "", float maxOverdue = 1, float checkDelay = 0) where T : BossComponent
        {
            var state = SimpleState(id, expected, name);
            state.Raw.Comment = $"Condition on {typeof(T).Name}";
            state.Raw.Update = (timeSinceTransition) =>
            {
                if (timeSinceTransition < checkDelay)
                    return null; // too early to check for condition

                var comp = Module.FindComponent<T>();
                if (comp == null)
                {
                    Module.ReportError(null, $"State {id:X}: component {typeof(T)} needed for condition is missing");
                    return state.Raw.Next;
                }

                if (condition(comp))
                    return state.Raw.Next;

                if (timeSinceTransition < expected + maxOverdue)
                    return null;

                Module.ReportError(null, $"State {id:X}: transition triggered because of overdue");
                return state.Raw.Next;
            };
            return state;
        }

        // create a fork state triggered by component condition
        public State ComponentConditionFork<T, Key>(uint id, float expected, Func<T, bool> condition, Func<T, Key> select, Dictionary<Key, (uint seqID, Action<uint> buildState)> dispatch, string name = "")
            where T : BossComponent
            where Key : notnull
        {
            Func<bool> cond = () =>
            {
                var comp = Module.FindComponent<T>();
                if (comp == null)
                {
                    Module.ReportError(null, $"State {id:X}: component {typeof(T)} needed for condition is missing");
                    return false;
                }
                return condition(comp);
            };
            return ConditionFork(id, expected, cond, () => select(Module.FindComponent<T>()!), dispatch, name);
        }

        // create a state triggered by expected cast start by arbitrary actor; unexpected casts still trigger a transition, but log error
        public State ActorCastStart<AID>(uint id, Func<Actor?> actorAcc, AID aid, float delay, bool isBoss = false, string name = "")
            where AID : Enum
        {
            var state = SimpleState(id, delay, name).SetHint(StateMachine.StateHint.BossCastStart, isBoss);
            var expected = ActionID.MakeSpell(aid);
            state.Raw.Comment = $"Cast start: {aid}";
            state.Raw.Update = _ =>
            {
                var castInfo = actorAcc()?.CastInfo;
                if (castInfo == null)
                    return null;
                if (castInfo.Action != expected)
                    Module.ReportError(null, $"State {id:X}: unexpected cast start: got {castInfo.Action}, expected {expected}");
                return state.Raw.Next;
            };
            return state;
        }

        // create a state triggered by expected cast start by a primary actor; unexpected casts still trigger a transition, but log error
        public State CastStart<AID>(uint id, AID aid, float delay, string name = "")
            where AID : Enum
        {
            return ActorCastStart(id, () => Module.PrimaryActor, aid, delay, true, name);
        }

        // create a state triggered by one of a set of expected casts by arbitrary actor; unexpected casts still trigger a transition, but log error
        public State ActorCastStartMulti<AID>(uint id, Func<Actor?> actorAcc, IEnumerable<AID> aids, float delay, bool isBoss = false, string name = "")
            where AID : Enum
        {
            var state = SimpleState(id, delay, name).SetHint(StateMachine.StateHint.BossCastStart, isBoss);
            state.Raw.Comment = $"Cast start: [{string.Join(", ", aids)}]";
            state.Raw.Update = _ =>
            {
                var castInfo = actorAcc()?.CastInfo;
                if (castInfo == null)
                    return null;
                if (!aids.Any(aid => castInfo.IsSpell(aid)))
                    Module.ReportError(null, $"State {id:X}: unexpected cast start: got {castInfo.Action}");
                return state.Raw.Next;
            };
            return state;
        }

        // create a state triggered by one of a set of expected casts by a primary actor; unexpected casts still trigger a transition, but log error
        public State CastStartMulti<AID>(uint id, IEnumerable<AID> aids, float delay, string name = "")
            where AID : Enum
        {
            return ActorCastStartMulti(id, () => Module.PrimaryActor, aids, delay, true, name);
        }

        // create a state triggered by one of a set of expected casts by a primary actor, each of which forking to a separate subsequence
        // values in map are actions building state chains corresponding to each fork
        public State CastStartFork<AID>(uint id, Dictionary<AID, (uint seqID, Action<uint> buildState)> dispatch, float delay, string name = "")
             where AID : Enum
        {
            return ConditionFork(id, delay, () => Module.PrimaryActor.CastInfo != null, () => (AID)(object)(Module.PrimaryActor.CastInfo!.IsSpell() ? Module.PrimaryActor.CastInfo.Action.ID : 0), dispatch, name)
                .SetHint(StateMachine.StateHint.BossCastStart);
        }

        // create a state triggered by cast end by arbitrary actor
        public State ActorCastEnd(uint id, Func<Actor?> actorAcc, float castTime, bool isBoss = false, string name = "")
        {
            var state = SimpleState(id, castTime, name).SetHint(StateMachine.StateHint.BossCastEnd, isBoss);
            state.Raw.Comment = "Cast end";
            state.Raw.Update = _ => actorAcc()?.CastInfo == null ? state.Raw.Next : null;
            return state;
        }

        // create a state triggered by cast end by a primary actor
        public State CastEnd(uint id, float castTime, string name = "")
        {
            return ActorCastEnd(id, () => Module.PrimaryActor, castTime, true, name);
        }

        // create a chain of states: ActorCastStart -> ActorCastEnd; second state uses id+1
        public State ActorCast<AID>(uint id, Func<Actor?> actorAcc, AID aid, float delay, float castTime, bool isBoss = false, string name = "")
            where AID : Enum
        {
            ActorCastStart(id, actorAcc, aid, delay, isBoss, "");
            return ActorCastEnd(id + 1, actorAcc, castTime, isBoss, name);
        }

        // create a chain of states: CastStart -> CastEnd; second state uses id+1
        public State Cast<AID>(uint id, AID aid, float delay, float castTime, string name = "")
            where AID : Enum
        {
            CastStart(id, aid, delay, "");
            return CastEnd(id + 1, castTime, name);
        }

        // create a chain of states: ActorCastStartMulti -> ActorCastEnd; second state uses id+1
        public State ActorCastMulti<AID>(uint id, Func<Actor?> actorAcc, IEnumerable<AID> aids, float delay, float castTime, bool isBoss = false, string name = "")
            where AID : Enum
        {
            ActorCastStartMulti(id, actorAcc, aids, delay, isBoss, "");
            return ActorCastEnd(id + 1, actorAcc, castTime, isBoss, name);
        }

        // create a chain of states: CastStartMulti -> CastEnd; second state uses id+1
        public State CastMulti<AID>(uint id, IEnumerable<AID> aids, float delay, float castTime, string name = "")
            where AID : Enum
        {
            CastStartMulti(id, aids, delay, "");
            return CastEnd(id + 1, castTime, name);
        }

        // create a state triggered by arbitrary actor becoming (un)targetable
        public State ActorTargetable(uint id, Func<Actor?> actorAcc, bool targetable, float delay, string name = "", float checkDelay = 0)
        {
            var state = SimpleState(id, delay, name);
            state.Raw.Comment = targetable ? "Targetable" : "Untargetable";
            state.Raw.Update = timeSinceTransition => timeSinceTransition >= checkDelay && actorAcc()?.IsTargetable == targetable ? state.Raw.Next : null;
            return state;
        }

        // create a state triggered by a primary actor becoming (un)targetable; automatically sets downtime begin/end flag
        public State Targetable(uint id, bool targetable, float delay, string name = "", float checkDelay = 0)
        {
            return ActorTargetable(id, () => Module.PrimaryActor, targetable, delay, name, checkDelay)
                .SetHint(targetable ? StateMachine.StateHint.DowntimeEnd : StateMachine.StateHint.DowntimeStart);
        }
    }
}
