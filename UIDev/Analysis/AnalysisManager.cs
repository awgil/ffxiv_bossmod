using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIDev
{
    class AnalysisManager : IDisposable
    {
        private List<Replay> _replays = new();
        private Analysis.UnknownActionEffects? _unkEffects;

        public AnalysisManager(string rootPath)
        {
            try
            {
                var di = new DirectoryInfo(rootPath);
                foreach (var fi in di.EnumerateFiles("World_*.log", new EnumerationOptions { RecurseSubdirectories = true }))
                {
                    Service.Log($"Parsing {fi.FullName}...");
                    _replays.Add(ReplayParserLog.Parse(fi.FullName));
                }
            }
            catch (Exception e)
            {
                Service.Log($"Failed to read {rootPath}: {e}");
            }
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            ImGui.Text($"{_replays.Count} logs found");

            if (ImGui.TreeNode("Unknown action effects"))
            {
                if (_unkEffects == null)
                    _unkEffects = new Analysis.UnknownActionEffects(_replays);
                _unkEffects.Draw();
                ImGui.TreePop();
            }
        }
    }
}
