using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // base class for encounter configurations that support cooldown planning
    public abstract class CooldownPlanningConfigNode : ConfigNode
    {
        // per-class list of plans
        public class PlanList
        {
            public List<CooldownPlan> Available = new();
            public int SelectedIndex = -1;

            public CooldownPlan? Selected() => SelectedIndex >= 0 && SelectedIndex < Available.Count ? Available[SelectedIndex] : null;
        }

        public int SyncLevel { get; private init; }
        public Dictionary<Class, PlanList> CooldownPlans = new();

        public CooldownPlan? SelectedPlan(Class c) => CooldownPlans.GetValueOrDefault(c)?.Selected();

        public CooldownPlanningConfigNode(int syncLevel)
        {
            SyncLevel = syncLevel;
            foreach (var c in PlanDefinitions.Classes.Keys)
                CooldownPlans[c] = new();
        }

        public void DrawSelectionUI(Class c, StateMachine sm, ModuleRegistry.Info? moduleInfo)
        {
            if (!PlanDefinitions.Classes.ContainsKey(c))
                return; // class is not supported

            var plans = CooldownPlans.GetOrAdd(c);
            var newSelected = DrawPlanCombo(plans, plans.SelectedIndex, "Cooldown plan");
            if (newSelected != plans.SelectedIndex)
            {
                plans.SelectedIndex = newSelected;
                NotifyModified();
            }
            ImGui.SameLine();
            if (ImGui.Button(plans.SelectedIndex >= 0 ? "Edit plan" : "Create new plan"))
            {
                if (plans.SelectedIndex < 0)
                {
                    plans.Available.Add(new(c, SyncLevel, $"New {plans.Available.Count + 1}"));
                    plans.SelectedIndex = plans.Available.Count - 1;
                    NotifyModified();
                }
                StartPlanEditor(plans.Available[plans.SelectedIndex], sm, moduleInfo);
            }
        }

        public static int DrawPlanCombo(PlanList list, int selected, string label)
        {
            ImGui.SetNextItemWidth(100);
            if (ImGui.BeginCombo(label, selected >= 0 && selected < list.Available.Count ? list.Available[selected].Name : "none"))
            {
                if (ImGui.Selectable("none", selected < 0))
                {
                    selected = -1;
                }
                for (int i = 0; i < list.Available.Count; ++i)
                {
                    if (ImGui.Selectable(list.Available[i].Name, selected == i))
                    {
                        selected = i;
                    }
                }
                ImGui.EndCombo();
            }
            return selected;
        }

        public override void DrawCustom(UITree tree, WorldState ws)
        {
            foreach (var _ in tree.Node("Cooldown plans"))
            {
                foreach (var (c, plans) in CooldownPlans)
                {
                    for (int i = 0; i < plans.Available.Count; ++i)
                    {
                        ImGui.PushID($"{c}/{i}");
                        if (ImGui.Button($"Edit"))
                        {
                            StartPlanEditor(plans.Available[i]);
                        }
                        ImGui.SameLine();
                        if (ImGui.Button($"Copy"))
                        {
                            var plan = plans.Available[i].Clone();
                            plan.Name += " Copy";
                            plans.Available.Add(plan);
                            NotifyModified();
                            StartPlanEditor(plan);
                        }
                        ImGui.SameLine();
                        if (ImGui.Button($"Delete"))
                        {
                            if (plans.SelectedIndex == i)
                                plans.SelectedIndex = -1;
                            else if (plans.SelectedIndex > i)
                                plans.SelectedIndex--;
                            plans.Available.RemoveAt(i);
                            --i;
                            NotifyModified();
                        }
                        ImGui.SameLine();
                        bool selected = plans.SelectedIndex == i;
                        if (ImGui.Checkbox($"{c} '{plans.Available[i].Name}'", ref selected))
                        {
                            plans.SelectedIndex = selected ? i : -1;
                            NotifyModified();
                        }
                        ImGui.PopID();
                    }
                }
                ImGui.TextUnformatted("Add new plan:");
                foreach (var (c, plans) in CooldownPlans)
                {
                    ImGui.SameLine();
                    if (ImGui.Button(c.ToString()))
                    {
                        var plan = new CooldownPlan(c, SyncLevel, $"New {plans.Available.Count}");
                        plans.Available.Add(plan);
                        NotifyModified();
                        StartPlanEditor(plan);
                    }
                }
            }
        }

        public override void Deserialize(JObject j, JsonSerializer ser)
        {
            foreach (var p in CooldownPlans.Values)
            {
                p.Available.Clear();
                p.SelectedIndex = -1;
            }

            foreach (var (f, data) in j)
            {
                if (f == "CooldownPlans")
                    DeserializeCooldownPlans(data as JObject, ser);
                else
                    DeserializeField(f, data, ser);
            }
        }

        public override JObject Serialize(JsonSerializer ser)
        {
            var baseType = typeof(CooldownPlanningConfigNode);
            JObject res = new();
            foreach (var f in GetType().GetFields().Where(f => f.DeclaringType != baseType))
            {
                var v = f.GetValue(this);
                if (v != null)
                {
                    res[f.Name] = JToken.FromObject(v, ser);
                }
            }
            res["CooldownPlans"] = SerializeCooldownPlans(ser);
            return res;
        }

        private void StartPlanEditor(CooldownPlan plan, StateMachine? sm, ModuleRegistry.Info? moduleInfo)
        {
            if (sm == null)
                return;
            var editor = new CooldownPlanEditor(plan, sm, moduleInfo, NotifyModified);
            var w = WindowManager.CreateWindow($"Cooldown planner", editor.Draw, () => { }, () => true);
            w.SizeHint = new(600, 600);
            w.MinSize = new(100, 100);
        }

        private void StartPlanEditor(CooldownPlan plan)
        {
            var m = ModuleRegistry.CreateModuleForConfigPlanning(GetType());
            if (m != null)
                StartPlanEditor(plan, m.StateMachine, m.Info);
        }

        private void DeserializeCooldownPlans(JObject? j, JsonSerializer ser)
        {
            if (j == null)
                return;
            foreach (var (c, data) in j)
            {
                Class cls;
                if (!Enum.TryParse(c, out cls))
                    continue; // invalid class
                var plans = CooldownPlans.GetValueOrDefault(cls);
                if (plans == null)
                    continue; // non-plannable class
                var jPlans = data?["Available"] as JArray;
                if (jPlans == null)
                    continue;

                plans.SelectedIndex = data?["SelectedIndex"]?.Value<int>() ?? -1;
                foreach (var jPlan in jPlans)
                {
                    var plan = CooldownPlan.FromJSON(cls, SyncLevel, jPlan as JObject, ser);
                    if (plan != null)
                    {
                        plans.Available.Add(plan);
                    }
                }
            }
        }

        private JObject SerializeCooldownPlans(JsonSerializer ser)
        {
            JObject res = new();
            foreach (var (c, plans) in CooldownPlans)
            {
                if (plans.Available.Count == 0)
                    continue;
                var j = res[c.ToString()] = new JObject();
                j["SelectedIndex"] = plans.SelectedIndex;
                var jPlans = new JArray();
                j["Available"] = jPlans;
                foreach (var plan in plans.Available)
                    jPlans.Add(plan.ToJSON(ser));
            }
            return res;
        }
    }
}
