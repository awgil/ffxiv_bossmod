//using ImGuiNET;
//using System.Reflection;

//namespace BossMod;

//// base class for encounter configurations that support cooldown planning
//public abstract class CooldownPlanningConfigNode : ConfigNode
//{
//    // per-class list of plans
//    public class PlanList
//    {
//        public List<CooldownPlan> Available = [];
//        public int SelectedIndex = -1;

//        public CooldownPlan? Selected() => SelectedIndex >= 0 && SelectedIndex < Available.Count ? Available[SelectedIndex] : null;
//    }

//    public int SyncLevel { get; private init; }
//    public Dictionary<Class, PlanList> CooldownPlans = [];

//    public CooldownPlan? SelectedPlan(Class c) => CooldownPlans.GetValueOrDefault(c)?.Selected();

//    protected CooldownPlanningConfigNode(int syncLevel)
//    {
//        SyncLevel = syncLevel;
//        foreach (var c in PlanDefinitions.Classes.Keys)
//            CooldownPlans[c] = new();
//    }

//    public void DrawSelectionUI(Class c, StateMachine sm, ModuleRegistry.Info? moduleInfo)
//    {
//        if (!PlanDefinitions.Classes.ContainsKey(c))
//            return; // class is not supported

//        var plans = CooldownPlans.GetOrAdd(c);
//        var newSelected = DrawPlanCombo(plans, plans.SelectedIndex, "Cooldown plan");
//        if (newSelected != plans.SelectedIndex)
//        {
//            plans.SelectedIndex = newSelected;
//            Modified.Fire();
//        }
//        ImGui.SameLine();
//        if (ImGui.Button(plans.SelectedIndex >= 0 ? "Edit plan" : "Create new plan"))
//        {
//            if (plans.SelectedIndex < 0)
//            {
//                plans.Available.Add(new(c, SyncLevel, $"New {plans.Available.Count + 1}"));
//                plans.SelectedIndex = plans.Available.Count - 1;
//                Modified.Fire();
//            }
//            StartPlanEditor(plans.Available[plans.SelectedIndex], sm, moduleInfo);
//        }
//    }

//    public override void DrawCustom(UITree tree, WorldState ws)
//    {
//        foreach (var _ in tree.Node("Cooldown plans"))
//        {
//            foreach (var (c, plans) in CooldownPlans)
//            {
//                for (int i = 0; i < plans.Available.Count; ++i)
//                {
//                    ImGui.PushID($"{c}/{i}");
//                    if (ImGui.Button($"Edit"))
//                    {
//                        StartPlanEditor(plans.Available[i]);
//                    }
//                    ImGui.SameLine();
//                    if (ImGui.Button($"Copy"))
//                    {
//                        var plan = plans.Available[i].Clone();
//                        plan.Name += " Copy";
//                        plans.Available.Add(plan);
//                        Modified.Fire();
//                        StartPlanEditor(plan);
//                    }
//                    ImGui.SameLine();
//                    if (UIMisc.DangerousButton($"Delete"))
//                    {
//                        if (plans.SelectedIndex == i)
//                            plans.SelectedIndex = -1;
//                        else if (plans.SelectedIndex > i)
//                            plans.SelectedIndex--;
//                        plans.Available.RemoveAt(i);
//                        --i;
//                        Modified.Fire();
//                    }
//                    ImGui.SameLine();
//                    bool selected = plans.SelectedIndex == i;
//                    if (ImGui.Checkbox($"{c} '{plans.Available[i].Name}'", ref selected))
//                    {
//                        plans.SelectedIndex = selected ? i : -1;
//                        Modified.Fire();
//                    }
//                    ImGui.PopID();
//                }
//            }
//            ImGui.TextUnformatted("Add new plan:");
//            foreach (var (c, plans) in CooldownPlans)
//            {
//                ImGui.SameLine();
//                if (ImGui.Button(c.ToString()))
//                {
//                    var plan = new CooldownPlan(c, SyncLevel, $"New {plans.Available.Count}");
//                    plans.Available.Add(plan);
//                    Modified.Fire();
//                    StartPlanEditor(plan);
//                }
//            }
//        }
//    }

//    public override void Deserialize(JObject j, JsonSerializer ser)
//    {
//        foreach (var p in CooldownPlans.Values)
//        {
//            p.Available.Clear();
//            p.SelectedIndex = -1;
//        }

//        foreach (var (f, data) in j)
//        {
//            if (f == "CooldownPlans")
//                DeserializeCooldownPlans(data as JObject, ser);
//            else
//                ser.DeserializeField(f, data, this);
//        }
//    }

//    public override JObject Serialize(JsonSerializer ser)
//    {
//        var baseType = typeof(CooldownPlanningConfigNode);
//        JObject res = [];
//        foreach (var f in GetType().GetFields().Where(f => f.DeclaringType != baseType && f.GetCustomAttribute<JsonIgnoreAttribute>() == null))
//        {
//            var v = f.GetValue(this);
//            if (v != null)
//            {
//                res[f.Name] = JToken.FromObject(v, ser);
//            }
//        }
//        res["CooldownPlans"] = SerializeCooldownPlans(ser);
//        return res;
//    }

//    private void StartPlanEditor(CooldownPlan plan, StateMachine? sm, ModuleRegistry.Info? moduleInfo)
//    {
//        if (sm == null)
//            return;
//        _ = new CooldownPlanEditorWindow(plan, sm, moduleInfo, Modified.Fire);
//    }

//    private void StartPlanEditor(CooldownPlan plan)
//    {
//        var m = ModuleRegistry.CreateModuleForConfigPlanning(GetType());
//        if (m != null)
//            StartPlanEditor(plan, m.StateMachine, m.Info);
//    }

//    private void DeserializeCooldownPlans(JObject? j, JsonSerializer ser)
//    {
//        if (j == null)
//            return;
//        foreach (var (c, data) in j)
//        {
//            if (!Enum.TryParse(c, out Class cls))
//                continue; // invalid class
//            var plans = CooldownPlans.GetValueOrDefault(cls);
//            if (plans == null)
//                continue; // non-plannable class
//            if (data?["Available"] is not JArray jPlans)
//                continue;

//            plans.SelectedIndex = data?["SelectedIndex"]?.Value<int>() ?? -1;
//            foreach (var jPlan in jPlans)
//            {
//                var plan = CooldownPlan.FromJSON(cls, SyncLevel, jPlan as JObject, ser);
//                if (plan != null)
//                {
//                    plans.Available.Add(plan);
//                }
//            }
//        }
//    }

//    private JObject SerializeCooldownPlans(JsonSerializer ser)
//    {
//        JObject res = [];
//        foreach (var (c, plans) in CooldownPlans)
//        {
//            if (plans.Available.Count == 0)
//                continue;
//            var j = res[c.ToString()] = new JObject();
//            j["SelectedIndex"] = plans.SelectedIndex;
//            var jPlans = new JArray();
//            j["Available"] = jPlans;
//            foreach (var plan in plans.Available)
//                jPlans.Add(plan.ToJSON(ser));
//        }
//        return res;
//    }
//}
