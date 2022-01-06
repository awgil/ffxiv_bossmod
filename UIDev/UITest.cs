using ImGuiNET;
using ImGuiScene;
using System.Numerics;
using BossMod;
using System;

namespace UIDev
{
    class UITest : IPluginUIMock
    {
        public static void Main(string[] args)
        {
            UIBootstrap.Inititalize(new UITest());
        }

        private SimpleImGuiScene? scene;
        private StateMachineTest? _smTest;
        private ZodiarkSolver? _zodiarkSolver;
        private ZodiarkSolver.Control _zodiarkSolverControls = ZodiarkSolver.Control.All;
        private ZodiarkStages? _zodiarkStages;
        private Aspho1SStages? _aspho1SStages;
        private DateTime _startTime = DateTime.Now;

        public void Initialize(SimpleImGuiScene scene)
        {
            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup here

            scene.OnBuildUI += Draw;

            // saving this only so we can kill the test application by closing the window
            // (instead of just by hitting escape)
            this.scene = scene;
        }

        public void Dispose()
        {
        }

        // You COULD go all out here and make your UI generic and work on interfaces etc, and then
        // mock dependencies and conceivably use exactly the same class in this testbed and the actual plugin
        // That is, however, a bit excessive in general - it could easily be done for this sample, but I
        // don't want to imply that is easy or the best way to go usually, so it's not done here either
        private void Draw()
        {
            if (!DrawWindow("Zodiark overlap demo", new Vector2(375, 330), DrawMainWindow))
                scene!.ShouldQuit = true;
            if (_smTest != null && !DrawWindow("State machine test", new Vector2(300, 300), DrawSMTest))
                _smTest = null;
            if (_zodiarkSolver != null && !DrawWindow("Zodiark solver", new Vector2(375, 330), DrawZodiarkSolver))
                _zodiarkSolver = null;
            if (_zodiarkStages != null && !DrawWindow("Zodiark stages", new Vector2(375, 330), DrawZodiarkStages))
                _zodiarkStages = null;
            if (_aspho1SStages != null && !DrawWindow("Aspho1S stages", new Vector2(375, 330), DrawAspho1SStages))
                _aspho1SStages = null;
        }

        public void DrawMainWindow()
        {
            ImGui.Text($"Running time: {DateTime.Now - _startTime}");

            bool smTestVisible = _smTest != null;
            if (ImGui.Checkbox("Show state machine test", ref smTestVisible))
                _smTest = smTestVisible ? new StateMachineTest() : null;

            bool zodiarkSolverVisible = _zodiarkSolver != null;
            if (ImGui.Checkbox("Show zodiark solver", ref zodiarkSolverVisible))
                _zodiarkSolver = zodiarkSolverVisible ? new ZodiarkSolver() : null;

            bool zodiarkStagesVisible = _zodiarkStages != null;
            if (ImGui.Checkbox("Show zodiark stages", ref zodiarkStagesVisible))
                _zodiarkStages = zodiarkStagesVisible ? new ZodiarkStages() : null;

            bool aspho1sStagesVisible = _aspho1SStages != null;
            if (ImGui.Checkbox("Show aspho1s stages", ref aspho1sStagesVisible))
                _aspho1SStages = aspho1sStagesVisible ? new Aspho1SStages() : null;
        }

        private void DrawSMTest()
        {
            _smTest!.Draw();
        }

        private void DrawZodiarkSolver()
        {
            _zodiarkSolver!.Draw(_zodiarkSolverControls);

            if (ImGui.Button("Clear"))
            {
                _zodiarkSolver.Clear();
            }

            float scale = _zodiarkSolver.Scale;
            if (ImGui.SliderFloat("Scale", ref scale, 0.25F, 3F))
            {
                _zodiarkSolver.Scale = scale;
            }

            for (int i = 1; i < (int)ZodiarkSolver.Control.All; i <<= 1)
            {
                var flag = (ZodiarkSolver.Control)i;
                bool controlActive = _zodiarkSolverControls.HasFlag(flag);
                if (ImGui.Checkbox($"Control: {flag.ToString()}", ref controlActive))
                {
                    if (controlActive)
                        _zodiarkSolverControls |= flag;
                    else
                        _zodiarkSolverControls &= ~flag;
                }
            }
        }

        private void DrawZodiarkStages()
        {
            ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff0000ff), _zodiarkStages!.NextEvent > ZodiarkStages.BossEvent.Kokytos ? "some message" : "");
            _zodiarkStages.Draw();
            _zodiarkStages.DrawDebugButtons();

            if (ImGui.Button("Cast: diag TL"))
                _zodiarkStages.Solver.ActiveLine = ZodiarkSolver.LinePos.TL;
            ImGui.SameLine();
            if (ImGui.Button("Cast: diag TR"))
                _zodiarkStages.Solver.ActiveLine = ZodiarkSolver.LinePos.TR;
            ImGui.SameLine();
            if (ImGui.Button("Cast: rot CW"))
                _zodiarkStages.Solver.ActiveRot = ZodiarkSolver.RotDir.CW;
            ImGui.SameLine();
            if (ImGui.Button("Cast: rot CCW"))
                _zodiarkStages.Solver.ActiveRot = ZodiarkSolver.RotDir.CCW;
        }

        private void DrawAspho1SStages()
        {
            _aspho1SStages!.Draw();
            _aspho1SStages!.DrawDebugButtons();
        }

        private bool DrawWindow(string name, Vector2 sizeHint, Action drawFn)
        {
            bool visible = true;
            ImGui.SetNextWindowSize(sizeHint, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(sizeHint, new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin(name, ref visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                drawFn();
            }
            ImGui.End();
            return visible;
        }

        //public void DrawSettingsWindow()
        //{
        //    if (!SettingsVisible)
        //    {
        //        return;
        //    }

        //    ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
        //    if (ImGui.Begin("A Wonderful Configuration Window", ref this.settingsVisible,
        //        ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        //    {
        //        if (ImGui.Checkbox("Random Config Bool", ref this.fakeConfigBool))
        //        {
        //            // nothing to do in a fake ui!
        //        }
        //    }
        //    ImGui.End();
        //}
    }
}
