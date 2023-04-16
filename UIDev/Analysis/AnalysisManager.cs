using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;

namespace UIDev
{
    class AnalysisManager : IDisposable
    {
        private class Lazy<T>
        {
            private Func<T> _init;
            private T? _impl;

            public Lazy(Func<T> init)
            {
                _init = init;
            }

            public T Get()
            {
                if (_impl == null)
                    _impl = _init();
                return _impl;
            }
        }

        private class Global
        {
            private Lazy<Analysis.UnknownActionEffects> _unkEffects;
            private Lazy<Analysis.AbilityInfo> _abilityInfo;
            private Lazy<Analysis.ClientActions> _clientActions;
            private Lazy<Analysis.EffectResultMispredict> _effectResultMissing;
            private Lazy<Analysis.EffectResultMispredict> _effectResultUnexpected;
            private Lazy<Analysis.EffectResultReorder> _effectResultReorder;

            public Global(List<Replay> replays)
            {
                _unkEffects = new(() => new(replays));
                _abilityInfo = new(() => new(replays));
                _clientActions = new(() => new(replays));
                _effectResultMissing = new(() => new(replays, true));
                _effectResultUnexpected = new(() => new(replays, false));
                _effectResultReorder = new(() => new(replays));
            }

            public void Draw(UITree tree)
            {
                foreach (var n in tree.Node("Unknown action effects"))
                    _unkEffects.Get().Draw(tree);

                foreach (var n in tree.Node("Ability info", false, 0xffffffff, () => _abilityInfo.Get().DrawContextMenu()))
                    _abilityInfo.Get().Draw(tree);

                foreach (var n in tree.Node("Client action weirdness"))
                    _clientActions.Get().Draw(tree);

                foreach (var n in tree.Node("Effect results: missing confirmations"))
                    _effectResultMissing.Get().Draw(tree);

                foreach (var n in tree.Node("Effect results: unexpected confirmations"))
                    _effectResultUnexpected.Get().Draw(tree);

                foreach (var n in tree.Node("Effect results: reorders"))
                    _effectResultReorder.Get().Draw(tree);
            }
        }

        private class PerEncounter
        {
            private Lazy<Analysis.StateTransitionTimings> _transitionTimings;
            private Lazy<Analysis.ParticipantInfo> _participantInfo;
            private Lazy<Analysis.AbilityInfo> _abilityInfo;
            private Lazy<Analysis.StatusInfo> _statusInfo;
            private Lazy<Analysis.IconInfo> _iconInfo;
            private Lazy<Analysis.TetherInfo> _tetherInfo;
            private Lazy<Analysis.ArenaBounds> _arenaBounds;
            private Lazy<Analysis.TEASpecific>? _teaSpecific;

            public PerEncounter(List<Replay> replays, uint oid)
            {
                _transitionTimings = new(() => new(replays, oid));
                _participantInfo = new(() => new(replays, oid));
                _abilityInfo = new(() => new(replays, oid));
                _statusInfo = new(() => new(replays, oid));
                _iconInfo = new(() => new(replays, oid));
                _tetherInfo = new(() => new(replays, oid));
                _arenaBounds = new(() => new(replays, oid));
                if (oid == (uint)BossMod.Shadowbringers.Ultimate.TEA.OID.BossP1)
                    _teaSpecific = new(() => new(replays, oid));
            }

            public void Draw(UITree tree)
            {
                foreach (var n in tree.Node("State transition timings"))
                    _transitionTimings.Get().Draw(tree);

                foreach (var n in tree.Node("Participant info", false, 0xffffffff, () => _participantInfo.Get().DrawContextMenu()))
                    _participantInfo.Get().Draw(tree);

                foreach (var n in tree.Node("Ability info", false, 0xffffffff, () => _abilityInfo.Get().DrawContextMenu()))
                    _abilityInfo.Get().Draw(tree);

                foreach (var n in tree.Node("Status info", false, 0xffffffff, () => _statusInfo.Get().DrawContextMenu()))
                    _statusInfo.Get().Draw(tree);

                foreach (var n in tree.Node("Icon info", false, 0xffffffff, () => _iconInfo.Get().DrawContextMenu()))
                    _iconInfo.Get().Draw(tree);

                foreach (var n in tree.Node("Tether info", false, 0xffffffff, () => _tetherInfo.Get().DrawContextMenu()))
                    _tetherInfo.Get().Draw(tree);

                foreach (var n in tree.Node("Arena bounds"))
                    _arenaBounds.Get().Draw(tree);

                if (_teaSpecific != null)
                    foreach (var n in tree.Node("TEA-specific analysis"))
                        _teaSpecific.Get().Draw(tree);
            }
        }

        private List<Replay> _replays = new();
        private Global _global;
        private Dictionary<uint, PerEncounter> _perEncounter = new(); // key = encounter OID
        private UITree _tree = new();

        public AnalysisManager(string rootPath)
        {
            try
            {
                var di = new DirectoryInfo(rootPath);
                var pattern = "*.log";
                if (!di.Exists && (di.Parent?.Exists ?? false))
                {
                    pattern = di.Name;
                    di = di.Parent;
                }
                foreach (var fi in di.EnumerateFiles(pattern, new EnumerationOptions { RecurseSubdirectories = true }))
                {
                    Service.Log($"Parsing {fi.FullName}...");
                    _replays.Add(ReplayParserLog.Parse(fi.FullName));
                }
            }
            catch (Exception e)
            {
                Service.Log($"Failed to read {rootPath}: {e}");
            }
            _global = new(_replays);
            InitEncounters();
        }

        public AnalysisManager(Replay replay)
        {
            _replays.Add(replay);
            _global = new(_replays);
            InitEncounters();
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
            foreach (var n in _tree.Nodes(_perEncounter, kv => new($"Encounter analysis for {kv.Key:X} ({ModuleRegistry.FindByOID(kv.Key)?.ModuleType.Name})")))
            {
                n.Value.Draw(_tree);
            }
        }

        private void InitEncounters()
        {
            foreach (var replay in _replays)
                foreach (var e in replay.Encounters)
                    if (!_perEncounter.ContainsKey(e.OID))
                        _perEncounter[e.OID] = new(_replays, e.OID);
        }
    }
}
