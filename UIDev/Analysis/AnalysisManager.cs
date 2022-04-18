using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;

namespace UIDev
{
    class AnalysisManager : IDisposable
    {
        private class Global
        {
            private List<Replay> _replays;
            private Analysis.UnknownActionEffects? _unkEffects;

            public Global(List<Replay> replays)
            {
                _replays = replays;
            }

            public void Draw(Tree tree)
            {
                foreach (var n in tree.Node("Unknown action effects"))
                {
                    if (_unkEffects == null)
                        _unkEffects = new(_replays);
                    _unkEffects.Draw(tree);
                }
            }
        }

        private class PerEncounter
        {
            private List<Replay> _replays;
            private uint _oid;
            private Analysis.StateTransitionTimings? _transitionTimings;
            private Analysis.AbilityInfo? _abilityInfo;
            private Analysis.ArenaBounds? _arenaBounds;

            public PerEncounter(List<Replay> replays, uint oid)
            {
                _replays = replays;
                _oid = oid;
            }

            public void Draw(Tree tree)
            {
                foreach (var n in tree.Node("State transition timings"))
                {
                    if (_transitionTimings == null)
                        _transitionTimings = new(_replays, _oid);
                    _transitionTimings.Draw(tree);
                }

                foreach (var n in tree.Node("Ability info"))
                {
                    if (_abilityInfo == null)
                        _abilityInfo = new(_replays, _oid);
                    _abilityInfo.Draw(tree);
                }

                foreach (var n in tree.Node("Arena bounds"))
                {
                    if (_arenaBounds == null)
                        _arenaBounds = new(_replays, _oid);
                    _arenaBounds.Draw(tree);
                }
            }
        }

        private List<Replay> _replays = new();
        private Global _global;
        private Dictionary<uint, PerEncounter> _perEncounter = new(); // key = encounter OID
        private Tree _tree = new();

        public AnalysisManager(string rootPath)
        {
            _global = new(_replays);
            try
            {
                var di = new DirectoryInfo(rootPath);
                foreach (var fi in di.EnumerateFiles("World_*.log", new EnumerationOptions { RecurseSubdirectories = true }))
                {
                    Service.Log($"Parsing {fi.FullName}...");
                    var replay = ReplayParserLog.Parse(fi.FullName);
                    _replays.Add(replay);
                    foreach (var e in replay.Encounters)
                        if (!_perEncounter.ContainsKey(e.OID))
                            _perEncounter[e.OID] = new(_replays, e.OID);
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
            foreach (var n in _tree.Node("Global analysis"))
            {
                _global.Draw(_tree);
            }
            foreach (var n in _tree.Node("Per-encounter analysis"))
            {
                foreach (var e in _tree.Nodes(_perEncounter, kv => ($"{kv.Key:X} ({ModuleRegistry.TypeForOID(kv.Key)?.Name})", false)))
                {
                    e.Value.Draw(_tree);
                }
            }
        }
    }
}
