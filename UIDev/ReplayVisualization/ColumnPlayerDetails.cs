using BossMod;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    public class ColumnPlayerDetails : Timeline.ColumnGroup
    {
        private StateMachineTree _tree;
        private List<int> _phaseBraches;
        private Replay _replay;
        private Replay.Encounter _enc;
        private Replay.Participant _player;
        private Class _playerClass;
        private ModuleRegistry.Info? _moduleInfo;

        private ColumnPlayerActions _actions;
        private ColumnActorStatuses _statuses;

        private ColumnActorHP _hp;
        private ColumnPlayerGauge? _gauge;
        private ColumnSeparator _resourceSep;

        private CooldownPlanningConfigNode? _planConfig;
        private int _selectedPlan = -1;
        private bool _planModified;
        private CooldownPlannerColumns? _planner;

        public bool PlanModified => _planModified;

        public ColumnPlayerDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass)
            : base(timeline)
        {
            _tree = tree;
            _phaseBraches = phaseBranches;
            _replay = replay;
            _enc = enc;
            _player = player;
            _playerClass = playerClass;
            _moduleInfo = ModuleRegistry.FindByOID(enc.OID);

            _actions = Add(new ColumnPlayerActions(timeline, tree, phaseBranches, replay, enc, player, playerClass));
            _actions.Name = player.Name;

            _statuses = Add(new ColumnActorStatuses(timeline, tree, phaseBranches, replay, enc, player));

            _hp = Add(new ColumnActorHP(timeline, tree, phaseBranches, replay, enc, player));
            _gauge = ColumnPlayerGauge.Create(timeline, tree, phaseBranches, replay, enc, player, playerClass);
            if (_gauge != null)
                Add(_gauge);
            _resourceSep = Add(new ColumnSeparator(timeline));

            var info = ModuleRegistry.FindByOID(enc.OID);
            if (info?.CooldownPlanningSupported ?? false)
            {
                _planConfig = Service.Config.Get<CooldownPlanningConfigNode>(info.ConfigType!);
                var plans = _planConfig?.CooldownPlans.GetValueOrDefault(playerClass);
                if (plans != null)
                    UpdateSelectedPlan(plans, plans.SelectedIndex);
            }
        }

        public void DrawConfig(UITree tree)
        {
            DrawConfigPlanner(tree);
            foreach (var n in tree.Node("Actions"))
                _actions.DrawConfig(tree);
            foreach (var n in tree.Node("Statuses"))
                _statuses.DrawConfig(tree);

            foreach (var n in tree.Node("Resources"))
            {
                DrawResourceColumnToggle(_hp, "HP");
                if (_gauge != null)
                    DrawResourceColumnToggle(_gauge, "Gauge");
            }
        }

        public void SaveChanges()
        {
            if (_planner != null && _planModified)
            {
                _planner.UpdateEditedPlan();
                _planConfig?.NotifyModified();
                _planModified = false;
            }
        }

        private void DrawConfigPlanner(UITree tree)
        {
            if (_planConfig == null)
            {
                tree.LeafNode("Planner: not supported for this encounter");
                return;
            }

            var plans = _planConfig.CooldownPlans.GetValueOrDefault(_playerClass);
            if (plans == null)
            {
                tree.LeafNode("Planner: not supported for this class");
                return;
            }

            foreach (var n in tree.Node("Planner"))
            {
                UpdateSelectedPlan(plans, DrawPlanSelector(plans, _selectedPlan));
                _planner?.DrawConfig();
            }
        }

        private int DrawPlanSelector(CooldownPlanningConfigNode.PlanList list, int selection)
        {
            selection = CooldownPlanningConfigNode.DrawPlanCombo(list, selection, "###planner");
            ImGui.SameLine();

            bool isDefault = selection == list.SelectedIndex;
            if (ImGui.Checkbox("Default", ref isDefault))
            {
                list.SelectedIndex = isDefault ? selection : -1;
                _planConfig?.NotifyModified();
            }
            ImGui.SameLine();

            if (ImGui.Button(selection >= 0 ? "Create copy" : "Create new"))
            {
                CooldownPlan plan;
                if (selection >= 0)
                {
                    plan = list.Available[selection].Clone();
                    plan.Name += " Copy";
                }
                else
                {
                    plan = new(_playerClass, _planConfig?.SyncLevel ?? 0, $"New {list.Available.Count}");
                }
                selection = list.Available.Count;
                list.Available.Add(plan);
                _planConfig?.NotifyModified();
            }

            if (_planner != null && _planModified)
            {
                ImGui.SameLine();
                if (ImGui.Button("Save modifications"))
                {
                    SaveChanges();
                }
            }

            return selection;
        }

        private void UpdateSelectedPlan(CooldownPlanningConfigNode.PlanList list, int newSelection)
        {
            if (_selectedPlan == newSelection)
                return;

            if (_planner != null)
            {
                Columns.Remove(_planner);
                _planner = null;
            }
            _selectedPlan = newSelection;
            _planModified = false;
            if (_selectedPlan >= 0)
            {
                _planner = AddBefore(new CooldownPlannerColumns(list.Available[newSelection], () => _planModified = true, Timeline, _tree, _phaseBraches, _moduleInfo, false), _actions);

                // TODO: this should be reworked...
                var minTime = _enc.Time.Start.AddSeconds(Timeline.MinTime);
                foreach (var a in _replay.Actions.SkipWhile(a => a.Timestamp < minTime).TakeWhile(a => a.Timestamp <= _enc.Time.End).Where(a => a.Source == _player))
                {
                    var track = _planner.TrackForAction(a.ID);
                    if (track != null)
                        track.AddHistoryEntryDot(_enc.Time.Start, a.Timestamp, $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget)} #{a.GlobalSequence}", 0xffffffff).AddActionTooltip(a);
                }
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
}
