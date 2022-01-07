using BossMod;
using ImGuiNET;
using System.Numerics;
using System.Text;

namespace UIDev
{
    class StateMachineTest : ITest
    {
        class ForkState : StateMachine.IState
        {
            private int[] _destinations;
            private int? _selectedTarget;
            private bool _active;
            private StateMachine? _sm;

            public ForkState(int[] destinations)
            {
                _destinations = destinations;
            }

            public int? Update()
            {
                _active = true;

                if (_selectedTarget == null)
                    return null;

                int dest = _selectedTarget.Value;
                _selectedTarget = null;
                _active = false;
                return dest;
            }

            public string Name()
            {
                StringBuilder res = new();
                foreach (var t in _destinations)
                {
                    if (res.Length > 0)
                        res.Append(" -or- ");
                    res.Append(_sm!.States[t].Name());
                }
                return res.ToString();
            }

            public double EstimateTimeToTransition()
            {
                return -1;
            }

            public int? PredictedNextState()
            {
                return null;
            }

            public bool DrawHint()
            {
                if (!_active)
                    return false; // not yet activated...

                foreach (var t in _destinations)
                {
                    if (ImGui.Button(_sm!.States[t].Name()))
                        _selectedTarget = t;
                    ImGui.SameLine();
                }
                ImGui.NewLine();
                return true;
            }

            public void Reset(StateMachine sm)
            {
                _sm = sm;
                _selectedTarget = null;
                _active = false;
            }
        }

        private WorldState _ws;
        private StateMachine _sm;

        public StateMachineTest()
        {
            _ws = new();
            _ws.AddActor(1, 1, WorldState.ActorType.Enemy, new Vector3(), 0, 1);

            StateMachine.Desc desc = new();
            desc.States[0] = new TimeoutState(1, "Initial", 2);
            desc.States[1] = new TimeoutState(2, "Intermediate", 2);
            //desc.States[1].Transitions.Add(new SpellCastTransition(_ws, 2, 1, 1, 3, 3, 3));
            desc.States[2] = new ForkState(new int[]{ 3, 4 });
            desc.States[3] = new TimeoutState(5, "Variant A", 2);
            desc.States[4] = new TimeoutState(5, "Variant B", 2);
            desc.States[5] = new TimeoutState(6, "Join", 2);
            desc.States[6] = new TimeoutState(-1, "Final", 1000);

            _sm = new(desc);
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            _sm.Update();
            _sm.Draw();

            if (ImGui.Button("Start"))
                _sm.Start();
            ImGui.SameLine();
            if (ImGui.Button("Reset"))
                _sm.Reset();
        }

        //private bool DrawHintIntermediate(double? timeActive)
        //{
        //    if (timeActive != null)
        //    {
        //        ImGui.Text($"Active for: {timeActive:f1}");

        //        var act = _ws.FindActor(1)!;
        //        if (ImGui.Button(act.CastInfo == null ? "Start cast" : "End cast"))
        //            _ws.UpdateCastInfo(act, act.CastInfo == null ? new WorldState.CastInfo { ActionID = 1 } : null);
        //    }
        //    return true;
        //}
    }
}
