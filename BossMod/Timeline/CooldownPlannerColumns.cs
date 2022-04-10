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
        private string _name = "";
        private Dictionary<ActionID, ActionUseColumn> _columns = new();
        private int _selectedBranch = 0;

        private float _trackWidth = 80;

        public CooldownPlannerColumns(CooldownPlan plan, Action onModified, Timeline timeline, StateMachineTree tree, int initialBranch)
        {
            _plan = plan;
            _onModified = onModified;
            _tree = tree;
            _selectedBranch = initialBranch;
            foreach (var (aid, info) in AbilityDefinitions.Classes[plan.Class].Abilities)
            {
                if (!info.IsPlannable)
                    continue;
                var col = _columns[aid] = timeline.AddColumn(new ActionUseColumn(timeline, tree));
                col.Width = _trackWidth;
                col.Name = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(aid.ID)?.Name.ToString() ?? "(unknown)";
                col.SelectedBranch = initialBranch;
                col.Editable = true;
                col.NotifyModified = onModified;
                col.EffectDuration = info.EffectDuration;
                col.Cooldown = info.Cooldown;
            }

            ExtractPlanData(plan);
        }

        public void AddEvent(ActionID aid, ActionUseColumn.Event ev)
        {
            _columns.GetValueOrDefault(aid)?.Events.Add(ev);
        }

        public void SelectBranch(int branch)
        {
            _selectedBranch = branch;
            foreach (var c in _columns.Values)
                c.SelectedBranch = branch;
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
        }

        public void UpdateEditedPlan()
        {
            var plan = BuildPlan();
            _plan.Name = plan.Name;
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
                        col.Entries.Add(new() { AttachNode = state, WindowStart = (state.Predecessor?.Time ?? 0) + e.TimeSinceActivation, WindowLength = e.WindowLength, Duration = col.EffectDuration, Cooldown = col.Cooldown, Name = col.Name });
                }
            }
        }

        private CooldownPlan BuildPlan()
        {
            var res = new CooldownPlan(_plan.Class, _name);
            foreach (var (aid, col) in _columns)
            {
                var list = res.PlanAbilities[aid.Raw];
                foreach (var e in col.Entries.Where(e => e.AttachNode != null))
                {
                    list.Add(new(e.AttachNode!.State.ID, e.WindowStart - (e.AttachNode!.Predecessor?.Time ?? 0), e.WindowLength));
                }
            }
            return res;
        }
    }
}
