using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BossMod
{
    // a set of action-use columns that represent cooldown plan
    // TODO: rework...
    public class CooldownPlannerColumns : Timeline.ColumnGroup
    {
        private CooldownPlan _plan;
        private Action _onModified;
        private StateMachineTree _tree;
        private List<int> _phaseBranches;
        private string _name = "";
        private StateMachineTimings _timings = new();
        private Dictionary<ActionID, ColumnPlannerTrack> _columns = new();

        private float _trackWidth = 80;

        public Class PlanClass => _plan.Class;

        public CooldownPlannerColumns(CooldownPlan plan, Action onModified, Timeline timeline, StateMachineTree tree, List<int> phaseBranches)
            : base(timeline)
        {
            _plan = plan;
            _onModified = onModified;
            _tree = tree;
            _phaseBranches = phaseBranches;
            foreach (var (aid, info) in AbilityDefinitions.Classes[plan.Class].Abilities)
            {
                if (!info.IsPlannable)
                    continue;
                var col = _columns[aid] = Add(new ColumnPlannerTrack(timeline, tree, phaseBranches, Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(aid.ID)?.Name.ToString() ?? "(unknown)"));
                col.Width = _trackWidth;
                col.NotifyModified = onModified;
                col.NewElementEffectLength = info.Definition.EffectDuration;
                col.NewElementCooldownLength = info.Definition.Cooldown;
            }

            ExtractPlanData(plan);
        }

        // TODO: should be removed...
        public ColumnPlannerTrack? TrackForAction(ActionID aid) => _columns.GetValueOrDefault(aid);

        public void DrawCommonControls()
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
            if (ImGui.SliderFloat("###phase-duration", ref selPhase.Duration, 0, selPhase.MaxTime, $"{selPhase.Name}: %.1f"))
            {
                _timings.PhaseDurations[selectedPhase] = selPhase.Duration;
                _tree.ApplyTimings(_timings);
                _onModified();
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

        public void DrawConfig()
        {
            DrawCommonControls();
            // TODO: hide/show for tracks
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
                col.Elements.Clear();
                var list = plan.PlanAbilities.GetValueOrDefault(aid.Raw);
                if (list == null)
                    continue;

                foreach (var e in list)
                {
                    var state = _tree.Nodes.GetValueOrDefault(e.StateID);
                    if (state != null)
                        col.AddElement(state, e.TimeSinceActivation, e.WindowLength);
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
                foreach (var e in col.Elements)
                {
                    list.Add(new(e.Window.AttachNode.State.ID, e.Window.Delay, e.Window.Duration));
                }
            }
            return res;
        }
    }
}
