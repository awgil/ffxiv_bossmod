using System;
using System.Text;

namespace BossMod
{
    // this state is used if there is a fork in state machine, and taken branch is determined when actor starts casting one of the spells
    public class VariantSpellCastState : StateMachine.IState
    {
        public struct Variant
        {
            public int NextState;
            public string Name;
            public uint ActionID;
            public double CastDuration;
            public double ResolveTime;

            public Variant(int nextState, string name, uint actionID, double castDuration, double resolveTime = 0)
            {
                Name = name;
                ActionID = actionID;
                NextState = nextState;
                CastDuration = castDuration;
                ResolveTime = resolveTime;
            }
        }

        private WorldState _ws;
        private uint _watchedOID;
        private double _timeBeforeCast; // from state activation to cast start
        private Variant[] _variants;
        private bool _activated = false;
        private int _startedCastIndex = -1;
        private bool _castFinished = false;
        private DateTime _lastEvent = DateTime.Now;

        private double TimeSinceLastEvent => (DateTime.Now - _lastEvent).TotalSeconds;

        public VariantSpellCastState(WorldState ws, uint oid, double timeBeforeCast, Variant[] variants)
        {
            _ws = ws;
            _watchedOID = oid;
            _timeBeforeCast = timeBeforeCast;
            _variants = variants;
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

            if (!_castFinished || TimeSinceLastEvent < _variants[_startedCastIndex].ResolveTime)
                return null;

            var dest = _variants[_startedCastIndex].NextState;
            DoClear();
            return dest;
        }

        public string Name()
        {
            if (_startedCastIndex >= 0)
                return _variants[_startedCastIndex].Name;

            var res = new StringBuilder();
            foreach (var v in _variants)
            {
                if (res.Length > 0)
                    res.Append(" -or- ");
                res.Append(v.Name);
            }
            return res.ToString();
        }

        public double EstimateTimeToTransition()
        {
            if (_castFinished)
                return Math.Max(0, _variants[_startedCastIndex].ResolveTime - TimeSinceLastEvent);
            else if (_startedCastIndex >= 0)
                return _variants[_startedCastIndex].ResolveTime + Math.Max(0, _variants[_startedCastIndex].CastDuration - TimeSinceLastEvent);
            else if (_activated)
                return Math.Max(0, _timeBeforeCast - TimeSinceLastEvent);
            else
                return _timeBeforeCast;
        }

        public int? PredictedNextState()
        {
            return _startedCastIndex < 0 ? null : _variants[_startedCastIndex].NextState;
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
            _activated = _castFinished = false;
            _startedCastIndex = -1;
            _ws.ActorCastStarted -= HandleCastStarted;
            _ws.ActorCastFinished -= HandleCastFinished;
            Clear();
        }

        private void HandleCastStarted(object? sender, WorldState.Actor actor)
        {
            if (actor.OID == _watchedOID && _startedCastIndex < 0)
            {
                for (int i = 0; i < _variants.Length; ++i)
                {
                    if (_variants[i].ActionID == actor.CastInfo!.ActionID)
                    {
                        _startedCastIndex = i;
                        _lastEvent = DateTime.Now;
                        CastStarted(actor);
                        return;
                    }
                }

                Service.Log($"Unexpected spell cast start: {actor.CastInfo!.ActionID}");
            }
        }

        private void HandleCastFinished(object? sender, WorldState.Actor actor)
        {
            if (actor.OID == _watchedOID && !_castFinished && _startedCastIndex >= 0)
            {
                _castFinished = true;
                _lastEvent = DateTime.Now;
                CastFinished(actor);
            }
        }
    }
}
