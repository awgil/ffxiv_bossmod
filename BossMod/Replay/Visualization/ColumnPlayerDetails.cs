using BossMod.Autorotation;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.ReplayVisualization;

// TODO: currently it assumes that there's only one instance that can edit db, it won't refresh if plan is edited and saved in a different instance...
public class ColumnPlayerDetails : Timeline.ColumnGroup
{
    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBraches;
    private readonly Replay _replay;
    private readonly Replay.Encounter _enc;
    private readonly Replay.Participant _player;
    private readonly Class _playerClass;
    private readonly PlanDatabase _planDatabase;
    private readonly ModuleRegistry.Info? _moduleInfo;

    private readonly ColumnPlayerActions _actions;
    private readonly ColumnActorStatuses _statuses;

    private readonly ColumnActorHP _hp;
    private readonly ColumnPlayerGauge? _gauge;
    private readonly ColumnSeparator _resourceSep;

    private int _selectedPlan = -1;
    private CooldownPlannerColumns? _planner;
    private readonly List<Replay.Action> _plannerActions = [];

    public bool PlanModified => _planner?.Modified ?? false;

    public ColumnPlayerDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass, PlanDatabase planDB)
        : base(timeline)
    {
        _tree = tree;
        _phaseBraches = phaseBranches;
        _replay = replay;
        _enc = enc;
        _player = player;
        _playerClass = playerClass;
        _planDatabase = planDB;
        _moduleInfo = ModuleRegistry.FindByOID(enc.OID);

        _actions = Add(new ColumnPlayerActions(timeline, tree, phaseBranches, replay, enc, player, playerClass));
        _actions.Name = player.NameHistory.FirstOrDefault().Value.name;

        _statuses = Add(new ColumnActorStatuses(timeline, tree, phaseBranches, replay, enc, player));

        _hp = Add(new ColumnActorHP(timeline, tree, phaseBranches, replay, enc, player));
        _gauge = ColumnPlayerGauge.Create(timeline, tree, phaseBranches, replay, enc, player, playerClass);
        if (_gauge != null)
            Add(_gauge);
        _resourceSep = Add(new ColumnSeparator(timeline));

        if (_moduleInfo?.PlanLevel > 0)
        {
            var minTime = _enc.Time.Start.AddSeconds(Timeline.MinTime);
            _plannerActions = [.. _replay.Actions.SkipWhile(a => a.Timestamp < minTime).TakeWhile(a => a.Timestamp <= _enc.Time.End).Where(a => a.Source == _player)];
            var plans = _planDatabase.GetPlans(_moduleInfo.ModuleType, _playerClass);
            UpdateSelectedPlan(plans, plans.SelectedIndex);
        }
    }

    public void DrawConfig(UITree tree)
    {
        DrawConfigPlanner(tree);
        foreach (var _1 in tree.Node("Actions"))
            _actions.DrawConfig(tree);
        foreach (var _1 in tree.Node("Statuses"))
            _statuses.DrawConfig(tree);

        foreach (var _1 in tree.Node("Resources"))
        {
            DrawResourceColumnToggle(_hp, "HP");
            if (_gauge != null)
                DrawResourceColumnToggle(_gauge, "Gauge");
        }
    }

    public void SaveChanges()
    {
        if (_moduleInfo != null && _planner != null && _planner.Modified)
        {
            var plans = _planDatabase.GetPlans(_moduleInfo.ModuleType, _playerClass);
            _planDatabase.ModifyPlan(plans.Plans[_selectedPlan], _planner.Plan.MakeClone());
            _planner.Modified = false;
        }
    }

    private void DrawConfigPlanner(UITree tree)
    {
        if (_moduleInfo == null || _moduleInfo.PlanLevel <= 0)
        {
            tree.LeafNode("Planner: not supported for this encounter");
            return;
        }

        foreach (var _1 in tree.Node("Planner"))
        {
            var plans = _planDatabase.GetPlans(_moduleInfo.ModuleType, _playerClass);
            UpdateSelectedPlan(plans, DrawPlanSelector(_moduleInfo.ModuleType, plans, _selectedPlan));
            if (_planner != null)
            {
                ImGui.TextUnformatted($"GUID: {_planner.Plan.Guid}");
                _planner.DrawCommonControls();

                bool haveDifferentPhaseTimes = false;
                for (int i = 0; i < _tree.Phases.Count; ++i)
                {
                    _planner.Modified |= ImGui.SliderFloat($"{_tree.Phases[i].Name}###phase-duration-{i}", ref _planner.Plan.PhaseDurations.Ref(i), 0, _tree.Phases[i].MaxTime, $"%.1f (replay: {_tree.Phases[i].Duration:f1} / {_tree.Phases[i].MaxTime:f1})");
                    haveDifferentPhaseTimes |= _planner.Plan.PhaseDurations[i] != _tree.Phases[i].Duration;
                }

                using (ImRaii.Disabled(!haveDifferentPhaseTimes))
                {
                    if (ImGui.Button("Sync phase durations to replay"))
                    {
                        for (int i = 0; i < _tree.Phases.Count; ++i)
                            _planner.Plan.PhaseDurations[i] = _tree.Phases[i].Duration;
                        _planner.Modified = true;
                    }
                }
            }
        }
    }

    private int DrawPlanSelector(Type moduleType, PlanDatabase.PlanList list, int selection)
    {
        using (ImRaii.Disabled(_planner?.Modified ?? false))
            selection = UIPlanDatabaseEditor.DrawPlanCombo(list, selection, "###planner");

        bool isDefault = selection == list.SelectedIndex;
        ImGui.SameLine();
        if (ImGui.Checkbox("Default", ref isDefault))
        {
            list.SelectedIndex = isDefault ? selection : -1;
            _planDatabase.ModifyManifest(moduleType, _playerClass);
        }
        ImGui.SameLine();
        if (UIMisc.Button("Save", _planner == null || !_planner.Modified, "Current plan was not modified"))
            SaveChanges();
        ImGui.SameLine();
        if (UIMisc.Button("Copy", _planner == null, "No plan selected") && _planner != null && _moduleInfo != null)
        {
            _planner.Plan.Guid = Guid.NewGuid().ToString();
            _planner.Plan.Name += " Copy";
            var plans = _planDatabase.GetPlans(_moduleInfo.ModuleType, _playerClass);
            selection = _selectedPlan = plans.Plans.Count;
            _planDatabase.ModifyPlan(null, _planner.Plan.MakeClone());
            _planner.Modified = false;
        }
        ImGui.SameLine();
        if (UIMisc.Button("Revert", _planner == null || !_planner.Modified, "Current plan was not modified") && _planner != null && _moduleInfo != null)
        {
            var plans = _planDatabase.GetPlans(_moduleInfo.ModuleType, _playerClass);
            _planner.Plan = plans.Plans[_selectedPlan].MakeClone();
            _planner.SyncCreateImport();
            _planner.Modified = false;
        }
        ImGui.SameLine();
        if (UIMisc.Button("New", _planner != null && _planner.Modified, "Current preset is modified, save or discard changes") && _moduleInfo != null)
        {
            var plans = _planDatabase.GetPlans(_moduleInfo.ModuleType, _playerClass);
            var plan = new Plan($"New {plans.Plans.Count + 1}", _moduleInfo.ModuleType) { Guid = Guid.NewGuid().ToString(), Class = _playerClass, Level = _moduleInfo.PlanLevel };
            _planDatabase.ModifyPlan(null, plan);
            selection = plans.Plans.Count - 1;
        }
        ImGui.SameLine();
        if (UIMisc.Button("Delete", 0, (!ImGui.GetIO().KeyShift, "Hold shift to delete"), (_planner == null, "No preset is selected")) && _moduleInfo != null && _selectedPlan >= 0)
        {
            var plans = _planDatabase.GetPlans(_moduleInfo.ModuleType, _playerClass);
            _planDatabase.ModifyPlan(plans.Plans[_selectedPlan], null);
            selection = -1;
        }

        return selection;
    }

    private void UpdateSelectedPlan(PlanDatabase.PlanList list, int newSelection)
    {
        if (_selectedPlan == newSelection)
            return;

        if (_planner != null)
        {
            Columns.Remove(_planner);
            _planner = null;
        }
        _selectedPlan = newSelection;
        if (_selectedPlan >= 0)
        {
            _planner = AddBefore(new CooldownPlannerColumns(list.Plans[newSelection].MakeClone(), Timeline, _tree, _phaseBraches, false, _plannerActions, _enc.Time.Start), _actions);
        }
    }

    private void DrawResourceColumnToggle(IToggleableColumn col, string name)
    {
        bool visible = col.Visible;
        if (ImGui.Checkbox(name, ref visible))
        {
            col.Visible = visible;
            _resourceSep.Width = _hp.Visible || (_gauge?.Visible ?? false) ? 1 : 0;
        }
    }
}
