using BossMod;
using ImGuiNET;
using System.Numerics;
using System.Text;

namespace UIDev
{
    class StateMachineTest : ITest
    {
        private WorldState _ws = new();
        private StateMachine _sm = new();
        private StateMachine.State? _initial;
        private StateMachine.State? _fork;
        private StateMachine.State? _varA;
        private StateMachine.State? _varB;

        public StateMachineTest()
        {
            _ws.AddActor(1, 1, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new Vector3(), 0, 1, true);

            var s = CommonStates.Timeout(ref _initial, 2, "Initial");
            s = CommonStates.Timeout(ref s.Next, 2);

            _fork = s.Next = new StateMachine.State { Name = "Fork A -or- B", Duration = 3 };
            _fork.Update = (float timeSinceTransition) => { _fork.Done = _fork.Next != null; };
            _fork.Exit = () => { _fork.Next = null; };

            CommonStates.Timeout(ref _varA, 3, "Variant A");
            CommonStates.Timeout(ref _varB, 2, "Variant B");
            _varA!.Next = _varB!.Next = CommonStates.Timeout(ref s, 2, "Join");
            CommonStates.Timeout(ref s!.Next, 5, "Final");
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            _sm.Update();
            _sm.Draw();

            if (ImGui.Button("Start"))
                _sm.ActiveState = _initial;
            ImGui.SameLine();
            if (ImGui.Button("Reset"))
                _sm.ActiveState = null;

            if (_fork!.Active)
            {
                if (ImGui.Button("Choose A"))
                    _fork.Next = _varA;
                ImGui.SameLine();
                if (ImGui.Button("Choose B"))
                    _fork.Next = _varB;
            }
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
