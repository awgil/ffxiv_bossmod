using BossMod.Autorotation;
using BossMod.ReplayVisualization;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Text.Json;

namespace BossMod;

// a set of use columns that represent a single cooldown plan; note that it works on a private copy of a plan and reports modifications to whoever owns it
public class CooldownPlannerColumns : Timeline.ColumnGroup
{
    public bool Modified;
    public Plan Plan; // note: this is a copy of the plan stored in database
    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBranches;
    private readonly bool _syncTimings;
    private readonly List<Replay.Action> _playerActions;
    private readonly DateTime _encStart;
    private readonly List<List<ColumnPlannerTrackStrategy>> _colsStrategy = [];
    private readonly ColumnPlannerTrackTarget _colTarget;

    private readonly float _trackWidth = 50 * ImGuiHelpers.GlobalScale;

    public Class PlanClass => Plan.Class;

    public CooldownPlannerColumns(Plan plan, Timeline timeline, StateMachineTree tree, List<int> phaseBranches, bool syncTimings, List<Replay.Action> playerActions, DateTime encStart)
        : base(timeline)
    {
        Plan = plan;
        _tree = tree;
        _phaseBranches = phaseBranches;
        _syncTimings = syncTimings;
        _playerActions = playerActions;
        _encStart = encStart;

        _colTarget = Add(new ColumnPlannerTrackTarget(timeline, tree, phaseBranches, BossModuleRegistry.FindByType(plan.Encounter)));
        _colTarget.Width = _trackWidth;
        _colTarget.NotifyModified = OnModifiedTargets;

        SyncCreateImport();
    }

    public void DrawCommonControls()
    {
        if (ImGui.Button("Modules"))
            ImGui.OpenPopup("modules");
        ImGui.SameLine();
        if (ImGui.Button("Column visibility"))
            ImGui.OpenPopup("columns");
        ImGui.SameLine();
        if (ImGui.Button("Export to clipboard"))
            ExportToClipboard();
        ImGui.SameLine();
        if (ImGui.Button("Import from clipboard"))
            ImportFromClipboard();
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150);
        Modified |= ImGui.InputText("Name", ref Plan.Name, 255);

        using (var popup = ImRaii.Popup("modules"))
        {
            if (popup)
            {
                var disableRemove = !ImGui.GetIO().KeyShift;
                Action? post = null;
                for (int i = 0; i < Plan.Modules.Count; ++i)
                {
                    var m = Plan.Modules[i];
                    if (i != 0 && Plan.Modules[i - 1].Definition.Order != m.Definition.Order)
                        ImGui.Separator();

                    using (var disable = ImRaii.Disabled(i == 0 || Plan.Modules[i - 1].Definition.Order != m.Definition.Order))
                        if (UIMisc.IconButton(Dalamud.Interface.FontAwesomeIcon.ArrowUp, "^", $"###up{i}"))
                            post += SwapModulesAction(i, false);
                    ImGui.SameLine();
                    using (var disable = ImRaii.Disabled(i == Plan.Modules.Count - 1 || Plan.Modules[i + 1].Definition.Order != m.Definition.Order))
                        if (UIMisc.IconButton(Dalamud.Interface.FontAwesomeIcon.ArrowDown, "v", $"###down{i}"))
                            post += SwapModulesAction(i, true);
                    ImGui.SameLine();
                    var added = true;
                    using (var disable = ImRaii.Disabled(disableRemove))
                        if (ImGui.Checkbox(m.Definition.DisplayName, ref added))
                            post += RemoveModuleAction(i);

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        using var tooltip = ImRaii.Tooltip();
                        if (tooltip)
                        {
                            ImGui.TextUnformatted("Hold shift to remove");
                            UIRotationModule.DescribeModule(m.Type, m.Definition);
                        }
                    }
                }
                ImGui.Separator();
                foreach (var (mt, m) in RotationModuleRegistry.Modules.Where(m => (m.Value.Definition.RelatedBossModule == null || m.Value.Definition.RelatedBossModule == Plan.Encounter) && m.Value.Definition.Classes[(int)Plan.Class] && !Plan.Modules.Any(x => x.Type == m.Key)))
                {
                    var added = false;
                    if (ImGui.Checkbox(m.Definition.DisplayName, ref added))
                        post += AddModuleAction(mt, m);

                    if (ImGui.IsItemHovered())
                    {
                        using var tooltip = ImRaii.Tooltip();
                        if (tooltip)
                        {
                            UIRotationModule.DescribeModule(mt, m.Definition);
                        }
                    }
                }
                post?.Invoke();
            }
        }

        using (var popup = ImRaii.Popup("columns"))
        {
            if (popup)
            {
                for (int i = 0; i < Plan.Modules.Count; ++i)
                {
                    if (ImGui.BeginMenu(Plan.Modules[i].Definition.DisplayName))
                    {
                        foreach (var col in _colsStrategy[i])
                        {
                            var visible = col.Width > 0;
                            if (ImGui.Checkbox(col.Name, ref visible))
                                col.Width = visible ? _trackWidth : 0;
                        }
                        ImGui.EndMenu();
                    }
                }
            }
        }
    }

    public int DrawPhaseControls(int selectedPhase)
    {
        if (ImGui.Button("<##Phase") && selectedPhase > 0)
            --selectedPhase;
        ImGui.SameLine();
        ImGui.TextUnformatted($"Current phase: {selectedPhase + 1}/{_tree.Phases.Count}");
        ImGui.SameLine();
        if (ImGui.Button(">##Phase") && selectedPhase < _tree.Phases.Count - 1)
            ++selectedPhase;

        var selPhase = _tree.Phases[selectedPhase];
        ImGui.SameLine();
        if (ImGui.SliderFloat("###phase-duration", ref selPhase.Duration, 0, selPhase.MaxTime, $"{selPhase.Name.Replace("%", "%%", StringComparison.Ordinal)}: %.1f"))
        {
            Plan.PhaseDurations[selectedPhase] = selPhase.Duration;
            if (_syncTimings)
            {
                _tree.ApplyTimings(Plan.PhaseDurations);
                foreach (var cols in _colsStrategy)
                    foreach (var col in cols)
                        col.UpdateAllElements();
                _colTarget.UpdateAllElements();
            }
            Modified = true;
        }

        ImGui.SameLine();
        if (ImGui.Button("<##Branch") && _phaseBranches[selectedPhase] > 0)
            --_phaseBranches[selectedPhase];
        ImGui.SameLine();
        ImGui.TextUnformatted($"Current branch: {_phaseBranches[selectedPhase] + 1}/{selPhase.StartingNode.NumBranches}");
        ImGui.SameLine();
        if (ImGui.Button(">##Branch") && _phaseBranches[selectedPhase] < selPhase.StartingNode.NumBranches - 1)
            ++_phaseBranches[selectedPhase];

        return selectedPhase;
    }

    public void ExportToClipboard() => ImGui.SetClipboardText(JsonSerializer.Serialize(Plan, Serialization.BuildSerializationOptions()));

    public void ImportFromClipboard()
    {
        try
        {
            var plan = JsonSerializer.Deserialize<Plan>(ImGui.GetClipboardText(), Serialization.BuildSerializationOptions())!;
            if (plan.Class != Plan.Class || plan.Encounter != Plan.Encounter)
            {
                Service.Log($"Failed to import: plan belongs to {plan.Class} {plan.Encounter} instead of {Plan.Class} {Plan.Encounter}");
                return;
            }
            plan.Guid = Plan.Guid;

            Modified = true;
            Plan = plan;
            SyncCreateImport();
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to import: {ex}");
        }
    }

    public void SyncCreateImport()
    {
        // add data for missing phases
        for (int i = Plan.PhaseDurations.Count; i < _tree.Phases.Count; ++i)
            Plan.PhaseDurations.Add(_tree.Phases[i].Duration);
        if (_syncTimings)
            _tree.ApplyTimings(Plan.PhaseDurations);

        // remove any existing strategy columns
        foreach (var cols in _colsStrategy)
            foreach (var col in cols)
                Columns.Remove(col);
        _colsStrategy.Clear();

        // add new strategy columns
        for (int i = 0; i < Plan.Modules.Count; ++i)
            AddStrategyColumns(i);

        // clear and readd target overrides
        while (_colTarget.Elements.Count > 0)
            _colTarget.RemoveElement(0);
        foreach (var o in Plan.Targeting)
        {
            var state = _tree.Nodes.GetValueOrDefault(o.StateID);
            if (state != null)
            {
                _colTarget.AddElement(state, o.TimeSinceActivation, o.WindowLength, o.Disabled, (StrategyValueTrack)o.Value);
            }
        }
    }

    private Action AddModuleAction(Type type, RotationModuleRegistry.Entry md) => () =>
    {
        Modified = true;
        var index = Plan.AddModule(type, md.Definition, md.Builder);
        AddStrategyColumns(index);
    };

    private Action RemoveModuleAction(int index) => () =>
    {
        Modified = true;
        Plan.Modules.RemoveAt(index);
        foreach (var col in _colsStrategy[index])
            Columns.Remove(col);
        _colsStrategy.RemoveAt(index);
    };

    private Action SwapModulesAction(int i, bool next) => () =>
    {
        Modified = true;
        var j = next ? i + 1 : i - 1;
        var (colsFirst, colsSecond) = next ? (_colsStrategy[i], _colsStrategy[j]) : (_colsStrategy[j], _colsStrategy[i]); // initially first/second
        (Plan.Modules[i], Plan.Modules[j]) = (Plan.Modules[j], Plan.Modules[i]);
        (_colsStrategy[i], _colsStrategy[j]) = (_colsStrategy[j], _colsStrategy[i]);
        if (_colsStrategy[i].Count > 0 && _colsStrategy[j].Count > 0)
        {
            var iCol = Columns.IndexOf(colsFirst[0]);
            for (int k = 0; k < colsSecond.Count; ++k)
                Columns[iCol++] = colsSecond[k];
            for (int k = 0; k < colsFirst.Count; ++k)
                Columns[iCol++] = colsFirst[k];
        }
    };

    private void AddStrategyColumns(int index)
    {
        var moduleInfo = BossModuleRegistry.FindByType(Plan.Encounter);
        var insertionPoint = FindInsertionPoint(index);
        List<ColumnPlannerTrackStrategy> cols = [];
        _colsStrategy.Insert(index, cols);
        var m = Plan.Modules[index];
        List<int> uiOrder = [.. Enumerable.Range(0, m.Tracks.Count)];
        uiOrder.SortByReverse(i => m.Definition.Configs[i].UIPriority);
        foreach (int i in uiOrder)
        {
            var c1 = m.Definition.Configs[i];
            if (c1 is not StrategyConfigTrack config)
                continue; // TODO draw

            if (config.Options.Count(opt => Plan.Level >= opt.MinLevel && Plan.Level <= opt.MaxLevel) <= 1)
                continue; // don't bother showing tracks that have no customization options

            var col = AddBefore(new ColumnPlannerTrackStrategy(Timeline, _tree, _phaseBranches, config, Plan.Level, moduleInfo, m.Defaults[i]), insertionPoint);
            col.Width = config.UIPriority >= 0 || m.Tracks[i].Count > 0 ? _trackWidth : 0;
            col.NotifyModified = () => OnModifiedStrategy(m, i, col);
            foreach (var entry in m.Tracks[i])
            {
                var state = _tree.Nodes.GetValueOrDefault(entry.StateID);
                if (state != null)
                {
                    col.AddElement(state, entry.TimeSinceActivation, entry.WindowLength, entry.Disabled, entry.Value);
                }
            }
            foreach (var a in _playerActions)
            {
                if (config.AssociatedActions.Contains(a.ID))
                {
                    col.AddHistoryEntryDot(_encStart, a.Timestamp, $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}", 0xffffffff).AddActionTooltip(a);
                }
            }
            cols.Add(col);
        }
    }

    private Timeline.Column FindInsertionPoint(int index)
    {
        for (; index < _colsStrategy.Count; ++index)
            if (_colsStrategy[index].Count > 0)
                return _colsStrategy[index][0];
        return _colTarget;
    }

    private void OnModifiedStrategy(Plan.Module mod, int index, ColumnPlannerTrackStrategy track)
    {
        Modified = true;
        mod.Defaults[index] = track.DefaultOverride;
        var entries = mod.Tracks[index];
        entries.Clear();
        foreach (var e in track.Elements)
        {
            AddEntry(entries, e);
        }
    }

    private void OnModifiedTargets()
    {
        Modified = true;
        Plan.Targeting.Clear();
        foreach (var e in _colTarget.Elements)
        {
            AddEntry(Plan.Targeting, e);
        }
    }

    private void AddEntry(List<Plan.Entry> list, ColumnPlannerTrack.Element elem)
        => list.Add(new(elem.Value) { StateID = elem.Window.AttachNode.State.ID, TimeSinceActivation = elem.Window.Delay, WindowLength = elem.WindowLength, Disabled = elem.Disabled });
}
