using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // a set of action-use columns that represent cooldown plan
    public class CooldownPlannerColumns
    {
        private CooldownPlan _plan;
        private Action _onModified;
        private StateMachineTree _tree;
        private List<int> _phaseBranches;
        private string _name = "";
        private StateMachineTimings _timings = new();
        private Dictionary<ActionID, ActionUseColumn> _columns = new();
        private int _selectedPhase = 0;

        private float _trackWidth = 80;

        public Class PlanClass => _plan.Class;

        public CooldownPlannerColumns(CooldownPlan plan, Action onModified, Timeline timeline, StateMachineTree tree, List<int> phaseBranches)
        {
            _plan = plan;
            _onModified = onModified;
            _tree = tree;
            _phaseBranches = phaseBranches;
            foreach (var (aid, info) in AbilityDefinitions.Classes[plan.Class].Abilities)
            {
                if (!info.IsPlannable)
                    continue;
                var col = _columns[aid] = timeline.AddColumn(new ActionUseColumn(timeline, tree, phaseBranches));
                col.Width = _trackWidth;
                col.Name = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(aid.ID)?.Name.ToString() ?? "(unknown)";
                col.Editable = true;
                col.NotifyModified = onModified;
                col.EffectDuration = info.Definition.EffectDuration;
                col.Cooldown = info.Definition.Cooldown;
            }

            ExtractPlanData(plan);
        }

        public void AddEvent(ActionID aid, ActionUseColumn.Event ev)
        {
            _columns.GetValueOrDefault(aid)?.Events.Add(ev);
        }

        public void ClearEvents()
        {
            foreach (var c in _columns.Values)
                c.Events.Clear();
        }

        public void DrawControls()
        {
            if (ImGui.Button("Export to clipboard"))
                ExportToClipboard();
            ImGui.SameLine();
            if (ImGui.Button("Import from clipboard"))
                ImportFromClipboard();
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputText("Name", ref _name, 255))
                _onModified();

            if (ImGui.Button("<##Phase") && _selectedPhase > 0)
                --_selectedPhase;
            ImGui.SameLine();
            ImGui.TextUnformatted($"Current phase: {_selectedPhase + 1}/{_tree.Phases.Count}");
            ImGui.SameLine();
            if (ImGui.Button(">##Phase") && _selectedPhase < _tree.Phases.Count - 1)
                ++_selectedPhase;

            var selPhase = _tree.Phases[_selectedPhase];
            ImGui.SameLine();
            if (ImGui.SliderFloat("###phase-duration", ref selPhase.Duration, 0, selPhase.MaxTime, $"{selPhase.Name}: %.1f"))
            {
                _timings.PhaseDurations[_selectedPhase] = selPhase.Duration;
                _tree.ApplyTimings(_timings);
                _onModified();
            }

            ImGui.SameLine();
            if (ImGui.Button("<##Branch") && _phaseBranches[_selectedPhase] > 0)
                --_phaseBranches[_selectedPhase];
            ImGui.SameLine();
            ImGui.TextUnformatted($"Current branch: {_phaseBranches[_selectedPhase] + 1}/{selPhase.StartingNode.NumBranches}");
            ImGui.SameLine();
            if (ImGui.Button(">##Branch") && _phaseBranches[_selectedPhase] < selPhase.StartingNode.NumBranches - 1)
                ++_phaseBranches[_selectedPhase];
        }

        public void UpdateEditedPlan()
        {
            var plan = BuildPlan();
            _plan.Name = plan.Name;
            _plan.Timings = plan.Timings;
            _plan.PlanAbilities = plan.PlanAbilities;
        }

        public void ExportToClipboard()
        {
            ImGui.SetClipboardText(JObject.FromObject(BuildPlan()).ToString());
        }

        public void ImportFromClipboard()
        {
            try
            {
                var plan = JObject.Parse(ImGui.GetClipboardText()).ToObject<CooldownPlan>();
                if (plan != null && plan.Class == _plan.Class)
                {
                    ExtractPlanData(plan);
                    _onModified();
                }
                else
                {
                    Service.Log($"Failed to import: plan belong to {plan?.Class} instead of {_plan.Class}");
                }
            }
            catch (Exception ex)
            {
                Service.Log($"Failed to import: {ex}");
            }
        }

        private void ExtractPlanData(CooldownPlan plan)
        {
            _name = plan.Name;
            _tree.ApplyTimings(plan.Timings);

            _timings = new();
            foreach (var p in _tree.Phases)
                _timings.PhaseDurations.Add(p.Duration);

            foreach (var (aid, col) in _columns)
            {
                col.Entries.Clear();
                var list = plan.PlanAbilities.GetValueOrDefault(aid.Raw);
                if (list == null)
                    continue;

                foreach (var e in list)
                {
                    var state = _tree.Nodes.GetValueOrDefault(e.StateID);
                    if (state != null)
                        col.Entries.Add(new(state, e.TimeSinceActivation, e.WindowLength, col.EffectDuration, col.Cooldown, col.Name));
                }
            }
        }

        private CooldownPlan BuildPlan()
        {
            var res = new CooldownPlan(_plan.Class, _name);
            res.Timings = _timings.Clone();
            foreach (var (aid, col) in _columns)
            {
                var list = res.PlanAbilities[aid.Raw];
                foreach (var e in col.Entries.Where(e => e.AttachNode != null))
                {
                    list.Add(new(e.AttachNode!.State.ID, e.WindowStartDelay, e.WindowLength));
                }
            }
            return res;
        }
    }
}
