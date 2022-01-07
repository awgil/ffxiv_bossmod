using System;

namespace BossMod
{
    // simple state that transitions to next one by timeout
    public class TimeoutState : StateMachine.IState
    {
        private int _dest;
        private string _name;
        private double _timeout;
        private DateTime? _activatedAt;

        private double TimeSinceActivate => _activatedAt != null ? (DateTime.Now - _activatedAt.Value).TotalSeconds : 0;

        public TimeoutState(int nextState, string name, double timeout)
        {
            _dest = nextState;
            _name = name;
            _timeout = timeout;
        }

        public int? Update()
        {
            if (_activatedAt == null)
                _activatedAt = DateTime.Now;

            if (TimeSinceActivate < _timeout)
                return null;

            _activatedAt = null;
            return _dest;
        }

        public string Name()
        {
            return _name;
        }

        public double EstimateTimeToTransition()
        {
            return _activatedAt != null ? Math.Max(0, _timeout - TimeSinceActivate) : _timeout;
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
            _activatedAt = null;
        }
    }
}
