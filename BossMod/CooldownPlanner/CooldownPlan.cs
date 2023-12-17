using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    public class CooldownPlan
    {
        public class ActionUse
        {
            public ActionID ID;
            public uint StateID;
            public float TimeSinceActivation;
            public float WindowLength;
            public bool LowPriority;
            public PlanTarget.ISelector Target;
            public string Comment;
            // TODO: condition, delay-auto

            public ActionUse(ActionID aid, uint stateID, float timeSinceActivation, float windowLength, bool lowPriority, PlanTarget.ISelector target, string comment)
            {
                ID = aid;
                StateID = stateID;
                TimeSinceActivation = timeSinceActivation;
                WindowLength = windowLength;
                LowPriority = lowPriority;
                Target = target;
                Comment = comment;
            }

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

                JObject res = new();
                res["ID"] = ser.SerializeActionID(ID, aidType);
                res["StateID"] = $"0x{StateID:X8}";
                res["TimeSinceActivation"] = TimeSinceActivation;
                res["WindowLength"] = WindowLength;
                res["LowPriority"] = LowPriority;
                res["Target"] = target;
                res["Comment"] = Comment;
                return res;
            }
        }

        public class StrategyOverride
        {
            public uint Value;
            public uint StateID;
            public float TimeSinceActivation;
            public float WindowLength;
            public string Comment;

            public StrategyOverride(uint value, uint stateID, float timeSinceActivation, float windowLength, string comment)
            {
                Value = value;
                StateID = stateID;
                TimeSinceActivation = timeSinceActivation;
                WindowLength = windowLength;
                Comment = comment;
            }

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
                JObject res = new();
                if (valueType != null && Value != 0)
                    res["Value"] = valueType.GetEnumName(Value);
                res["StateID"] = $"0x{StateID:X8}";
                res["TimeSinceActivation"] = TimeSinceActivation;
                res["WindowLength"] = WindowLength;
                res["Comment"] = Comment;
                return res;
            }
        }

        public class TargetOverride
        {
            public uint OID;
            public uint StateID;
            public float TimeSinceActivation;
            public float WindowLength;
            public string Comment;

            public TargetOverride(uint oid, uint stateID, float timeSinceActivation, float windowLength, string comment)
            {
                OID = oid;
                StateID = stateID;
                TimeSinceActivation = timeSinceActivation;
                WindowLength = windowLength;
                Comment = comment;
            }

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
                JObject res = new();
                res["OID"] = $"0x{OID:X}";
                res["StateID"] = $"0x{StateID:X8}";
                res["TimeSinceActivation"] = TimeSinceActivation;
                res["WindowLength"] = WindowLength;
                res["Comment"] = Comment;
                return res;
            }
        }

        public Class Class;
        public int Level;
        public string Name;
        public StateMachineTimings Timings = new();
        public List<ActionUse> Actions = new();
        public List<List<StrategyOverride>> StrategyOverrides = new();
        public List<TargetOverride> TargetOverrides = new();

        public CooldownPlan(Class cls, int level, string name)
        {
            Class = cls;
            Level = level;
            Name = name;

            foreach (var s in PlanDefinitions.Classes[cls].StrategyTracks)
                StrategyOverrides.Add(new());
        }

        public CooldownPlan Clone()
        {
            var res = new CooldownPlan(Class, Level, Name);
            res.Timings = Timings.Clone();
            res.Actions.AddRange(Actions.Select(a => a.Clone()));
            for (int i = 0; i < StrategyOverrides.Count; ++i)
                res.StrategyOverrides[i].AddRange(StrategyOverrides[i].Select(s => s.Clone()));
            res.TargetOverrides.AddRange(TargetOverrides.Select(t => t.Clone()));
            return res;
        }

        public static CooldownPlan? FromJSON(Class cls, int level, JObject? j, JsonSerializer ser)
        {
            var name = j?["Name"]?.Value<string>();
            if (name == null)
                return null;
            var res = new CooldownPlan(cls, level, name);
            res.Timings = j?["Timings"]?.ToObject<StateMachineTimings>(ser) ?? res.Timings;

            var classData = PlanDefinitions.Classes[cls];
            var actions = j?["Actions"] as JArray;
            if (actions != null)
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
                var jstrat = jstrats?[trackData.Name] as JArray;
                if (jstrat == null)
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

            var jtargets = j?["Targets"] as JArray;
            if (jtargets != null)
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
            JObject res = new();
            res["Name"] = Name;
            res["Timings"] = JObject.FromObject(Timings, ser);

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
}
