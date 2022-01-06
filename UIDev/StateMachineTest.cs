using BossMod;
using ImGuiNET;

namespace UIDev
{
    class StateMachineTest
    {
        class SimpleTransition : StateMachine.ITransition
        {
            public int Dest;
            public double Dur;
            public bool AllowTransition;

            public bool CanActivate()
            {
                return AllowTransition;
            }

            public bool CanTransition(double timeSinceActivation)
            {
                return timeSinceActivation >= Dur;
            }

            public int DestinationState()
            {
                return Dest;
            }

            public double EstimateTimeToTransition(double timeSinceActivation)
            {
                return Dur - timeSinceActivation;
            }
        }

        class ForkState : StateMachine.State
        {
            private StateMachine _sm;

            public ForkState(StateMachine sm, string name)
            {
                Name = name;
                _sm = sm;
            }

            public override void DrawHint(double timeSinceActivation)
            {
                foreach (var t in Transitions)
                {
                    var ct = (SimpleTransition)t;
                    if (ImGui.Button(_sm.States[ct.Dest].Name))
                        ct.AllowTransition = true;
                    ImGui.SameLine();
                }
                ImGui.NewLine();
            }

            public override void Reset()
            {
                foreach (var t in Transitions)
                    ((SimpleTransition)t).AllowTransition = false;
            }
        }

        private StateMachine _sm = new();

        public StateMachineTest()
        {
            _sm.States[0] = new StateMachine.State { Name = "Initial" };
            _sm.States[1] = new StateMachine.State { Name = "Intermediate" };
            _sm.States[2] = new ForkState(_sm, "Fork");
            _sm.States[3] = new StateMachine.State { Name = "Variant A" };
            _sm.States[4] = new StateMachine.State { Name = "Variant B" };
            _sm.States[5] = new StateMachine.State { Name = "Join" };
            _sm.States[6] = new StateMachine.State { Name = "Final" };

            _sm.States[0].Transitions.Add(new SimpleTransition { Dest = 1, Dur = 2, AllowTransition = true });
            _sm.States[1].Transitions.Add(new SimpleTransition { Dest = 2, Dur = 4, AllowTransition = true });
            _sm.States[2].Transitions.Add(new SimpleTransition { Dest = 3, Dur = 2, AllowTransition = false });
            _sm.States[2].Transitions.Add(new SimpleTransition { Dest = 4, Dur = 2, AllowTransition = false });
            _sm.States[3].Transitions.Add(new SimpleTransition { Dest = 5, Dur = 2, AllowTransition = true });
            _sm.States[4].Transitions.Add(new SimpleTransition { Dest = 5, Dur = 2, AllowTransition = true });
            _sm.States[5].Transitions.Add(new SimpleTransition { Dest = 6, Dur = 2, AllowTransition = true });
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
    }
}
