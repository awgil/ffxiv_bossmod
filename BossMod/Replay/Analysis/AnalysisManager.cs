using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayAnalysis;

public sealed class AnalysisManager : IDisposable
{
    public class Lazy<T>(Func<T> init)
    {
        private readonly Func<T> _init = init;
        private T? _impl;

        public T Get() => _impl ??= _init();
    }

    private sealed class Global(List<Replay> replays)
    {
        private readonly Lazy<UnknownActionEffects> _unkEffects = new(() => new(replays));
        private readonly Lazy<ParticipantInfo> _participantInfo = new(() => new(replays));
        private readonly Lazy<AbilityInfo> _abilityInfo = new(() => new(replays));
        private readonly Lazy<ClassDefinitions> _classDefinitions = new(() => new(replays));
        private readonly Lazy<ClientActions> _clientActions = new(() => new(replays));
        private readonly Lazy<EffectResultMispredict> _effectResultMissing = new(() => new(replays, true));
        private readonly Lazy<EffectResultMispredict> _effectResultUnexpected = new(() => new(replays, false));
        private readonly Lazy<EffectResultReorder> _effectResultReorder = new(() => new(replays));

        public void Draw(UITree tree)
        {
            foreach (var n in tree.Node("Unknown action effects"))
                _unkEffects.Get().Draw(tree);

            foreach (var n in tree.Node("Participant info", false, 0xffffffff, () => _participantInfo.Get().DrawContextMenu()))
                _participantInfo.Get().Draw(tree);

            foreach (var n in tree.Node("Ability info", false, 0xffffffff, () => _abilityInfo.Get().DrawContextMenu()))
                _abilityInfo.Get().Draw(tree);

            foreach (var n in tree.Node("Player class definitions"))
                _classDefinitions.Get().Draw(tree);

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
        private readonly Lazy<StateTransitionTimings> _transitionTimings;
        private readonly Lazy<ParticipantInfo> _participantInfo;
        private readonly Lazy<AbilityInfo> _abilityInfo;
        private readonly Lazy<StatusInfo> _statusInfo;
        private readonly Lazy<IconInfo> _iconInfo;
        private readonly Lazy<TetherInfo> _tetherInfo;
        private readonly Lazy<MapEffectInfo> _mapEffectInfo;
        private readonly Lazy<DirectorInfo> _directorInfo;
        private readonly Lazy<ArenaBounds> _arenaBounds;
        private readonly List<(string, Lazy<CustomAnalyzer>)> _customAnalyzers = [];

        public PerEncounter(List<Replay> replays, uint oid)
        {
            _transitionTimings = new(() => new(replays, oid));
            _participantInfo = new(() => new(replays, oid));
            _abilityInfo = new(() => new(replays, oid));
            _statusInfo = new(() => new(replays, oid));
            _iconInfo = new(() => new(replays, oid));
            _tetherInfo = new(() => new(replays, oid));
            _mapEffectInfo = new(() => new(replays, oid));
            _directorInfo = new(() => new(replays, oid));
            _arenaBounds = new(() => new(replays, oid));
            if (AnalyzerRegistry.ByID(oid) is { } info)
                _customAnalyzers.Add((info.Label, new(() => info.Factory(replays, oid))));
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

            foreach (var n in tree.Node("Map effect info", false, 0xffffffff))
                _mapEffectInfo.Get().Draw(tree);

            foreach (var n in tree.Node("Director update info", false, 0xffffffff))
                _directorInfo.Get().Draw(tree);

            foreach (var n in tree.Node("Arena bounds"))
                _arenaBounds.Get().Draw(tree);

            foreach (var (label, analyzer) in _customAnalyzers)
                foreach (var n in tree.Node(label))
                    analyzer.Get().Draw(tree);
        }
    }

    private readonly List<Replay> _replays;
    private readonly Global _global;
    private readonly Dictionary<uint, PerEncounter> _perEncounter = []; // key = encounter OID
    private readonly UITree _tree = new();

    public AnalysisManager(List<Replay> replays)
    {
        _replays = replays;
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
        foreach (var n in _tree.Nodes(_perEncounter, kv => new($"Encounter analysis for {kv.Key:X} ({BossModuleRegistry.FindByOID(kv.Key)?.ModuleType.Name})")))
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
