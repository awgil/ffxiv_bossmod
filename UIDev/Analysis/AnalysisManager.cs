using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;

namespace UIDev
{
    class AnalysisManager : IDisposable
    {
        private List<Replay> _replays = new();
        private Analysis.UnknownActionEffects? _unkEffects;
        private Analysis.StateTransitionTimings? _transitionTimings;
        private Analysis.AbilityInfo? _abilityInfo;
        private Analysis.ArenaBounds? _arenaBounds;
        private Tree _tree = new();

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
            ImGui.TextUnformatted($"{_replays.Count} logs found");

            foreach (var n in _tree.Node("Unknown action effects"))
            {
                if (_unkEffects == null)
                    _unkEffects = new(_replays, _tree);
                _unkEffects.Draw();
            }

            foreach (var n in _tree.Node("State transition timings"))
            {
                if (_transitionTimings == null)
                    _transitionTimings = new(_replays, _tree);
                _transitionTimings.Draw();
            }

            foreach (var n in _tree.Node("Ability info"))
            {
                if (_abilityInfo == null)
                    _abilityInfo = new(_replays, _tree);
                _abilityInfo.Draw();
            }

            foreach (var n in _tree.Node("Arena bounds"))
            {
                if (_arenaBounds == null)
                    _arenaBounds = new(_replays, _tree);
                _arenaBounds.Draw();
            }
        }
    }
}
