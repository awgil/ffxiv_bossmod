using BossMod.Autorotation;
using BossMod.ReplayVisualization;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
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
    private readonly Dictionary<Type, List<ColumnPlannerTrackStrategy>> _colsStrategy = [];
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

        _colTarget = Add(new ColumnPlannerTrackTarget(timeline, tree, phaseBranches, ModuleRegistry.FindByType(plan.Encounter)));
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
                foreach (var (mt, m) in RotationModuleRegistry.Modules.Where(m => (m.Value.Definition.RelatedBossModule == null || m.Value.Definition.RelatedBossModule == Plan.Encounter) && m.Value.Definition.Classes[(int)Plan.Class]))
                {
                    var added = Plan.Modules.ContainsKey(mt);
                    var disable = added && !ImGui.GetIO().KeyShift;
                    using (ImRaii.Disabled(disable))
                    {
                        if (ImGui.Checkbox(m.Definition.DisplayName, ref added))
                        {
                            if (added)
                            {
                                Plan.AddModule(mt);
                                AddStrategyColumns(mt);
                            }
                            else
                            {
                                Plan.Modules.Remove(mt);
                                if (_colsStrategy.Remove(mt, out var cols))
                                    foreach (var col in cols)
                                        Columns.Remove(col);
                            }
                            Modified = true;
                        }
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        using var tooltip = ImRaii.Tooltip();
                        if (tooltip)
                        {
                            if (disable)
                                ImGui.TextUnformatted("Hold shift to remove");
                            UIRotationModule.DescribeModule(mt, m.Definition);
                        }
                    }
                }
            }
        }

        using (var popup = ImRaii.Popup("columns"))
        {
            if (popup)
            {
                foreach (var (mt, cols) in _colsStrategy)
                {
                    if (ImGui.BeginMenu(RotationModuleRegistry.Modules[mt].Definition.DisplayName))
                    {
                        foreach (var col in cols)
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
                    foreach (var col in cols.Value)
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
            foreach (var col in cols.Value)
                Columns.Remove(col);
        _colsStrategy.Clear();

        // add new strategy columns
        foreach (var mt in Plan.Modules.Keys)
            AddStrategyColumns(mt);

        // clear and readd target overrides
        while (_colTarget.Elements.Count > 0)
            _colTarget.RemoveElement(0);
        foreach (var o in Plan.Targeting)
        {
            var state = _tree.Nodes.GetValueOrDefault(o.StateID);
            if (state != null)
            {
                _colTarget.AddElement(state, o.TimeSinceActivation, o.WindowLength, o.Disabled, o.Value);
            }
        }
    }

    private void AddStrategyColumns(Type t)
    {
        var moduleInfo = ModuleRegistry.FindByType(Plan.Encounter);
        var cols = _colsStrategy[t] = [];
        var md = RotationModuleRegistry.Modules[t].Definition;
        var tracks = Plan.Modules[t];
        List<int> uiOrder = [.. Enumerable.Range(0, tracks.Count)];
        uiOrder.SortByReverse(i => md.Configs[i].UIPriority);
        foreach (int i in uiOrder)
        {
            var col = AddBefore(new ColumnPlannerTrackStrategy(Timeline, _tree, _phaseBranches, md.Configs[i], Plan.Level, moduleInfo), _colTarget);
            col.Width = md.Configs[i].UIPriority >= 0 ? _trackWidth : 0;
            col.NotifyModified = () => OnModifiedStrategy(tracks[i], col);
            foreach (var entry in tracks[i])
            {
                var state = _tree.Nodes.GetValueOrDefault(entry.StateID);
                if (state != null)
                {
                    col.AddElement(state, entry.TimeSinceActivation, entry.WindowLength, entry.Disabled, entry.Value);
                }
            }
            foreach (var a in _playerActions)
            {
                if (md.Configs[i].AssociatedActions.Contains(a.ID))
                {
                    col.AddHistoryEntryDot(_encStart, a.Timestamp, $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}", 0xffffffff).AddActionTooltip(a);
                }
            }
            cols.Add(col);
        }
    }

    private void OnModifiedStrategy(List<Plan.Entry> entries, ColumnPlannerTrackStrategy track)
    {
        Modified = true;
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
