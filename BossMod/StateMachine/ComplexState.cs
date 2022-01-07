using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    // complex state contains nested state machine and a single automatic transition that activates when nested state machine reaches null state
    public class ComplexState : StateMachine.IState
    {
        private int _dest;
        private StateMachine _nested;

        public ComplexState(int nextState, StateMachine.Desc desc)
        {
            _nested = new(desc);
            _dest = nextState;
        }

        public int? Update()
        {
            if (_nested.ActiveState == null)
                _nested.Start();

            if (_nested.ActiveState != null)
                _nested.Update();

            return _nested.ActiveState == null ? _dest : null;
        }

        public string Name()
        {
            if (_nested.ActiveState == null)
                return _nested.BuildStateChain(_nested.FindState(0), " + ", 20);
            else if (_nested.NextState == null)
                return _nested.ActiveState.Name();
            else
                return $"({_nested.ActiveString()} + {_nested.BuildStateChain(_nested.NextState, " + ", 20)})";
        }

        public double EstimateTimeToTransition()
        {
            double res = _nested.ActiveState?.EstimateTimeToTransition() ?? 0;
            var next = _nested.ActiveState != null ? _nested.NextState : _nested.FindState(0);
            while (next != null)
            {
                var dest = next.PredictedNextState();
                if (dest == null)
                    return -1;

                res += next.EstimateTimeToTransition();
                next = _nested.FindState(dest);
            }
            return res;
        }

        public int? PredictedNextState()
        {
            return _dest;
        }

        public virtual bool DrawHint()
        {
            var hintState = _nested.ActiveState ?? _nested.FindState(0);
            while (hintState != null && !hintState.DrawHint())
            {
                hintState = _nested.FindState(hintState.PredictedNextState());
            }
            return hintState != null;
        }

        public void Reset(StateMachine sm)
        {
            _nested.Reset();
        }
    }
}
