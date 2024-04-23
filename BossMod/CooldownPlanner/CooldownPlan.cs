using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BossMod;

public class CooldownPlan(Class cls, int level, string name)
{
    public class ActionUse(ActionID aid, uint stateID, float timeSinceActivation, float windowLength, bool lowPriority, PlanTarget.ISelector target, string comment)
    {
        public ActionID ID = aid;
        public uint StateID = stateID;
        public float TimeSinceActivation = timeSinceActivation;
        public float WindowLength = windowLength;
        public bool LowPriority = lowPriority;
        public PlanTarget.ISelector Target = target;
        public string Comment = comment;
        // TODO: condition, delay-auto

        public ActionUse Clone() => new(ID, StateID, TimeSinceActivation, WindowLength, LowPriority, Target.Clone(), Comment);

        public static ActionUse? FromJSON(Type aidType, JObject? j, JsonSerializer ser)
        {
            var actionID = ser.DeserializeActionID(j?["ID"], aidType);
            var stateID = ser.DeserializeHex(j?["StateID"]);
            if (actionID == null || stateID == null)
                return null;

            var jTarget = j?["Target"] as JObject;
            var targetType = Type.GetType($"BossMod.PlanTarget.{jTarget?["Type"]?.Value<string>() ?? ""}");
            PlanTarget.ISelector? target = targetType?.IsAssignableTo(typeof(PlanTarget.ISelector)) ?? false ? (PlanTarget.ISelector?)Activator.CreateInstance(targetType) : null;
            if (target != null)
            {
                foreach (var (f, data) in jTarget!)
                {
                    if (f != "Type")
                    {
                        ser.DeserializeField(f, data, target);
                    }
                }
            }

            return new ActionUse(actionID.Value, stateID.Value, j?["TimeSinceActivation"]?.Value<float>() ?? 0, j?["WindowLength"]?.Value<float>() ?? 0, j?["LowPriority"]?.Value<bool>() ?? false, target ?? new PlanTarget.Self(), j?["Comment"]?.Value<string>() ?? "");
        }

        public JObject ToJSON(Type aidType, JsonSerializer ser)
        {
            var target = JObject.FromObject(Target, ser);
            target["Type"] = Target.GetType().Name;

            JObject res = new()
            {
                ["ID"] = ser.SerializeActionID(ID, aidType),
                ["StateID"] = $"0x{StateID:X8}",
                ["TimeSinceActivation"] = TimeSinceActivation,
                ["WindowLength"] = WindowLength,
                ["LowPriority"] = LowPriority,
                ["Target"] = target,
                ["Comment"] = Comment
            };
            return res;
        }
    }

    public class StrategyOverride(uint value, uint stateID, float timeSinceActivation, float windowLength, string comment)
    {
        public uint Value = value;
        public uint StateID = stateID;
        public float TimeSinceActivation = timeSinceActivation;
        public float WindowLength = windowLength;
        public string Comment = comment;

        public StrategyOverride Clone() => new(Value, StateID, TimeSinceActivation, WindowLength, Comment);

        public static StrategyOverride? FromJSON(Type? valueType, JObject? j, JsonSerializer ser)
        {
            var value = valueType != null ? (ser.DeserializeEnum(j?["Value"], valueType) ?? 0) : 1;
            var stateID = ser.DeserializeHex(j?["StateID"]);
            if (stateID == null)
                return null;
            return new StrategyOverride(value, stateID.Value, j?["TimeSinceActivation"]?.Value<float>() ?? 0, j?["WindowLength"]?.Value<float>() ?? 0, j?["Comment"]?.Value<string>() ?? "");
        }

        public JObject ToJSON(Type? valueType, JsonSerializer ser)
        {
            JObject res = [];
            if (valueType != null && Value != 0)
                res["Value"] = valueType.GetEnumName(Value);
            res["StateID"] = $"0x{StateID:X8}";
            res["TimeSinceActivation"] = TimeSinceActivation;
            res["WindowLength"] = WindowLength;
            res["Comment"] = Comment;
            return res;
        }
    }

    public class TargetOverride(uint oid, uint stateID, float timeSinceActivation, float windowLength, string comment)
    {
        public uint OID = oid;
        public uint StateID = stateID;
        public float TimeSinceActivation = timeSinceActivation;
        public float WindowLength = windowLength;
        public string Comment = comment;

        public TargetOverride Clone() => new(OID, StateID, TimeSinceActivation, WindowLength, Comment);

        public static TargetOverride? FromJSON(JObject? j, JsonSerializer ser)
        {
            var oid = ser.DeserializeHex(j?["OID"]) ?? 0;
            var stateID = ser.DeserializeHex(j?["StateID"]);
            if (stateID == null)
                return null;
            return new TargetOverride(oid, stateID.Value, j?["TimeSinceActivation"]?.Value<float>() ?? 0, j?["WindowLength"]?.Value<float>() ?? 0, j?["Comment"]?.Value<string>() ?? "");
        }

        public JObject ToJSON(JsonSerializer ser)
        {
            JObject res = new()
            {
                ["OID"] = $"0x{OID:X}",
                ["StateID"] = $"0x{StateID:X8}",
                ["TimeSinceActivation"] = TimeSinceActivation,
                ["WindowLength"] = WindowLength,
                ["Comment"] = Comment
            };
            return res;
        }
    }

    public Class Class = cls;
    public int Level = level;
    public string Name = name;
    public StateMachineTimings Timings = new();
    public List<ActionUse> Actions = [];
    public List<List<StrategyOverride>> StrategyOverrides = [.. PlanDefinitions.Classes[cls].StrategyTracks.Select(_ => new List<StrategyOverride>())];
    public List<TargetOverride> TargetOverrides = [];

    public CooldownPlan Clone() => new(Class, Level, Name)
    {
        Timings = Timings.Clone(),
        Actions = [.. Actions.Select(a => a.Clone())],
        StrategyOverrides = [.. StrategyOverrides.Select(l => l.Select(s => s.Clone()).ToList())],
        TargetOverrides = [.. TargetOverrides.Select(t => t.Clone())]
    };

    public static CooldownPlan? FromJSON(Class cls, int level, JObject? j, JsonSerializer ser)
    {
        var name = j?["Name"]?.Value<string>();
        if (name == null)
            return null;
        var res = new CooldownPlan(cls, level, name);
        res.Timings = j?["Timings"]?.ToObject<StateMachineTimings>(ser) ?? res.Timings;

        var classData = PlanDefinitions.Classes[cls];
        if (j?["Actions"] is JArray actions)
        {
            foreach (var ja in actions)
            {
                var a = ActionUse.FromJSON(classData.AIDType, ja as JObject, ser);
                if (a != null)
                {
                    res.Actions.Add(a);
                }
            }
        }

        var jstrats = j?["Strategies"] as JObject;
        foreach (var (trackData, resStrats) in classData.StrategyTracks.Zip(res.StrategyOverrides))
        {
            if (jstrats?[trackData.Name] is not JArray jstrat)
                continue;

            foreach (var js in jstrat)
            {
                var s = StrategyOverride.FromJSON(trackData.Values, js as JObject, ser);
                if (s != null)
                {
                    resStrats.Add(s);
                }
            }
        }

        if (j?["Targets"] is JArray jtargets)
        {
            foreach (var jt in jtargets)
            {
                var t = TargetOverride.FromJSON(jt as JObject, ser);
                if (t != null)
                {
                    res.TargetOverrides.Add(t);
                }
            }
        }

        return res;
    }

    public JObject ToJSON(JsonSerializer ser)
    {
        JObject res = new()
        {
            ["Name"] = Name,
            ["Timings"] = JObject.FromObject(Timings, ser)
        };

        var classData = PlanDefinitions.Classes[Class];
        var actions = new JArray();
        res["Actions"] = actions;
        foreach (var a in Actions)
            actions.Add(a.ToJSON(classData.AIDType, ser));

        var strats = new JObject();
        res["Strategies"] = strats;
        foreach (var (trackData, trackOverrides) in classData.StrategyTracks.Zip(StrategyOverrides))
        {
            var overrides = new JArray();
            strats[trackData.Name] = overrides;
            foreach (var o in trackOverrides)
            {
                overrides.Add(o.ToJSON(trackData.Values, ser));
            }
        }

        var targets = new JArray();
        res["Targets"] = targets;
        foreach (var t in TargetOverrides)
            targets.Add(t.ToJSON(ser));

        return res;
    }
}
