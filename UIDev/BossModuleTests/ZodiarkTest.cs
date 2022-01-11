using BossMod;
using ImGuiNET;

namespace UIDev
{
    class ZodiarkSolverTest : ITest
    {
        private ZodiarkSolver _zodiarkSolver = new();
        private ZodiarkSolver.Control _zodiarkSolverControls = ZodiarkSolver.Control.All;

        public void Dispose()
        {
        }

        public void Draw()
        {
            _zodiarkSolver.Draw(_zodiarkSolverControls);

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
    }

    class ZodiarkStagesTest : ITest
    {
        private ZodiarkStages _zodiarkStages = new();

        public void Dispose()
        {
        }

        public void Draw()
        {
            ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff0000ff), _zodiarkStages.NextEvent > ZodiarkStages.BossEvent.Kokytos ? "some message" : "");
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
    }
}
