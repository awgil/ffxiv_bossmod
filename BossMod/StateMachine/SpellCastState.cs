using System;

namespace BossMod
{
    // state that transitions to next one when actor with specific OID casts a spell
    // default implementation accepts any spell, but this can be subclassed to customize
    // checks any actor that matches OID, so it's best used when it is unique
    public class SpellCastState : StateMachine.IState
    {
        private int _dest;
        private string _name;
        private WorldState _ws;
        private uint _watchedOID;
        private double _timeBeforeCast; // from state activation to cast start
        private double _castDuration; // from cast start to cast end
        private double _resolveTime; // from cast end to transition
        private bool _activated = false;
        private bool _castStarted = false;
        private bool _castFinished = false;
        private DateTime _lastEvent = DateTime.Now;

        private double TimeSinceLastEvent => (DateTime.Now - _lastEvent).TotalSeconds;

        public SpellCastState(int nextState, string name, WorldState ws, uint oid, double timeBeforeCast, double castDuration, double resolveTime = 0)
        {
            _dest = nextState;
            _name = name;
            _ws = ws;
            _watchedOID = oid;
            _timeBeforeCast = timeBeforeCast;
            _castDuration = castDuration;
            _resolveTime = resolveTime;
        }

        public int? Update()
        {
            if (!_activated)
            {
                _ws.ActorCastStarted += HandleCastStarted;
                _ws.ActorCastFinished += HandleCastFinished;
                _activated = true;
                _lastEvent = DateTime.Now;
            }

            if (!_castFinished || TimeSinceLastEvent < _resolveTime)
                return null;

            DoClear();
            return _dest;
        }

        public string Name()
        {
            return _name;
        }

        public double EstimateTimeToTransition()
        {
            if (_castFinished)
                return Math.Max(0, _resolveTime - TimeSinceLastEvent);
            else if (_castStarted)
                return _resolveTime + Math.Max(0, _castDuration - TimeSinceLastEvent);
            else if (_activated)
                return _resolveTime + _castDuration + Math.Max(0, _timeBeforeCast - TimeSinceLastEvent);
            else
                return _resolveTime + _castDuration + _timeBeforeCast;
        }

        public int? PredictedNextState()
        {
            return _dest;
        }

        public virtual bool DrawHint()
        {
            return false;
        }

        public void Reset(StateMachine sm)
        {
            DoClear();
        }

        protected virtual void CastStarted(WorldState.Actor actor) { }
        protected virtual void CastFinished(WorldState.Actor actor) { }
        protected virtual void Clear() { }

        private void DoClear()
        {
            _activated = _castStarted = _castFinished = false;
            _ws.ActorCastStarted -= HandleCastStarted;
            _ws.ActorCastFinished -= HandleCastFinished;
            Clear();
        }

        private void HandleCastStarted(object? sender, WorldState.Actor actor)
        {
            if (actor.OID == _watchedOID && !_castStarted)
            {
                _castStarted = true;
                _lastEvent = DateTime.Now;
                CastStarted(actor);
            }
        }

        private void HandleCastFinished(object? sender, WorldState.Actor actor)
        {
            if (actor.OID == _watchedOID && !_castFinished)
            {
                _castFinished = true;
                _lastEvent = DateTime.Now;
                CastFinished(actor);
            }
        }
    }

    // SpellCastState that expects concrete spell; if unexpected spell is cast, it accepts it, but logs error
    public class ExpectedSpellCastState : SpellCastState
    {
        private uint _expectedAID;

        public ExpectedSpellCastState(int nextState, string name, WorldState ws, uint oid, uint aid, double timeBeforeCast, double castDuration, double resolveTime = 0)
            : base(nextState, name, ws, oid, timeBeforeCast, castDuration, resolveTime)
        {
            _expectedAID = aid;
        }

        protected override void CastStarted(WorldState.Actor actor)
        {
            if (actor.CastInfo!.ActionID != _expectedAID)
                Service.Log($"Unexpected cast start for actor {actor.OID:X}: got {actor.CastInfo!.ActionID}, expected {_expectedAID}");
        }
    }
}
