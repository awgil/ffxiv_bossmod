using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayAnalysis;

sealed class AnalysisManager
{
    public sealed class Global(
        Lazy<UnknownActionEffects> unkEffects,
        Lazy<ParticipantInfo> participantInfo,
        Lazy<AbilityInfo> abilityInfo,
        Lazy<ClassDefinitions> classDefinitions,
        Lazy<ClientActions> clientActions,
        EffectResultMispredict.Factory effFactory,
        Lazy<EffectResultReorder> effectResultReorder
    )
    {
        private readonly Lazy<UnknownActionEffects> _unkEffects = unkEffects;
        private readonly Lazy<ParticipantInfo> _participantInfo = participantInfo;
        private readonly Lazy<AbilityInfo> _abilityInfo = abilityInfo;
        private readonly Lazy<ClassDefinitions> _classDefinitions = classDefinitions;
        private readonly Lazy<ClientActions> _clientActions = clientActions;
        private readonly Lazy<EffectResultMispredict> _effectResultMissing = new(() => effFactory(true));
        private readonly Lazy<EffectResultMispredict> _effectResultUnexpected = new(() => effFactory(false));
        private readonly Lazy<EffectResultReorder> _effectResultReorder = effectResultReorder;

        public void Draw(UITree tree)
        {
            foreach (var n in tree.Node("Unknown action effects"))
                _unkEffects.Value.Draw(tree);

            foreach (var n in tree.Node("Participant info", false, 0xffffffff, () => _participantInfo.Value.DrawContextMenu()))
                _participantInfo.Value.Draw(tree);

            foreach (var n in tree.Node("Ability info", false, 0xffffffff, () => _abilityInfo.Value.DrawContextMenu()))
                _abilityInfo.Value.Draw(tree);

            foreach (var n in tree.Node("Player class definitions"))
                _classDefinitions.Value.Draw(tree);

            foreach (var n in tree.Node("Client action weirdness"))
                _clientActions.Value.Draw(tree);

            foreach (var n in tree.Node("Effect results: missing confirmations"))
                _effectResultMissing.Value.Draw(tree);

            foreach (var n in tree.Node("Effect results: unexpected confirmations"))
                _effectResultUnexpected.Value.Draw(tree);

            foreach (var n in tree.Node("Effect results: reorders"))
                _effectResultReorder.Value.Draw(tree);
        }
    }

    public class PerEncounter
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
        private readonly Lazy<TEASpecific>? _teaSpecific;
        private readonly Lazy<TOPSpecific>? _topSpecific;

        public delegate PerEncounter Factory(uint oid);

        public PerEncounter(IEnumerable<Replay> rrs, uint oid, BossModuleRegistry bmr, ActionEffectParser aep)
        {
            var replays = rrs.ToList();
            _transitionTimings = new(() => new(replays, oid));
            _participantInfo = new(() => new(replays, oid, bmr));
            _abilityInfo = new(() => new(replays, oid, bmr, aep));
            _statusInfo = new(() => new(replays, oid, bmr));
            _iconInfo = new(() => new(replays, oid, bmr));
            _tetherInfo = new(() => new(replays, oid, bmr));
            _mapEffectInfo = new(() => new(replays, oid, bmr));
            _directorInfo = new(() => new(replays, oid, bmr));
            _arenaBounds = new(() => new(replays, oid));
            if (oid == (uint)Shadowbringers.Ultimate.TEA.OID.BossP1)
                _teaSpecific = new(() => new(replays, oid));
            if (oid == (uint)Endwalker.Ultimate.TOP.OID.Boss)
                _topSpecific = new(() => new(replays, oid));
        }

        public void Draw(UITree tree)
        {
            foreach (var n in tree.Node("State transition timings"))
                _transitionTimings.Value.Draw(tree);

            foreach (var n in tree.Node("Participant info", false, 0xffffffff, () => _participantInfo.Value.DrawContextMenu()))
                _participantInfo.Value.Draw(tree);

            foreach (var n in tree.Node("Ability info", false, 0xffffffff, () => _abilityInfo.Value.DrawContextMenu()))
                _abilityInfo.Value.Draw(tree);

            foreach (var n in tree.Node("Status info", false, 0xffffffff, () => _statusInfo.Value.DrawContextMenu()))
                _statusInfo.Value.Draw(tree);

            foreach (var n in tree.Node("Icon info", false, 0xffffffff, () => _iconInfo.Value.DrawContextMenu()))
                _iconInfo.Value.Draw(tree);

            foreach (var n in tree.Node("Tether info", false, 0xffffffff, () => _tetherInfo.Value.DrawContextMenu()))
                _tetherInfo.Value.Draw(tree);

            foreach (var n in tree.Node("Map effect info", false, 0xffffffff))
                _mapEffectInfo.Value.Draw(tree);

            foreach (var n in tree.Node("Director update info", false, 0xffffffff))
                _directorInfo.Value.Draw(tree);

            foreach (var n in tree.Node("Arena bounds"))
                _arenaBounds.Value.Draw(tree);

            if (_teaSpecific != null)
                foreach (var n in tree.Node("TEA-specific analysis"))
                    _teaSpecific.Value.Draw(tree);

            if (_topSpecific != null)
                foreach (var n in tree.Node("TOP-specific analysis"))
                    _topSpecific.Value.Draw(tree);
        }
    }

    private readonly List<Replay> _replays;
    private readonly BossModuleRegistry _bmr;
    private readonly Global _global;
    private readonly PerEncounter.Factory pfac;
    private readonly Dictionary<uint, PerEncounter> _perEncounter = []; // key = encounter OID
    private readonly UITree _tree = new();

    public AnalysisManager(IEnumerable<Replay> replays, BossModuleRegistry bmr, Global global, PerEncounter.Factory pfac)
    {
        _replays = [.. replays];
        _bmr = bmr;
        _global = global;
        this.pfac = pfac;
        InitEncounters();
    }

    public void Draw()
    {
        ImGui.TextUnformatted($"{_replays.Count} logs found");
        foreach (var n in _tree.Node("Global analysis"))
        {
            _global.Draw(_tree);
        }
        foreach (var n in _tree.Nodes(_perEncounter, kv => new($"Encounter analysis for {kv.Key:X} ({_bmr.FindByOID(kv.Key)?.ModuleType.Name})")))
        {
            n.Value.Draw(_tree);
        }
    }

    private void InitEncounters()
    {
        foreach (var replay in _replays)
            foreach (var e in replay.Encounters)
                if (!_perEncounter.ContainsKey(e.OID))
                    _perEncounter[e.OID] = pfac.Invoke(e.OID);
    }
}
