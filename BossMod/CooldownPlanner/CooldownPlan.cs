using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                var aidStr = j?["ID"]?.Value<string>();
                ActionID actionID;
                object? aid;
                if (Enum.TryParse(aidType, aidStr, out aid) && aid != null)
                    actionID = new(ActionType.Spell, (uint)aid);
                else if (aidStr?.StartsWith("0x") ?? false)
                    actionID = new(uint.Parse(aidStr.Substring(2), NumberStyles.HexNumber));
                else
                    return null;

                var sid = j?["StateID"]?.Value<string>() ?? "";
                if (!actionID || !sid.StartsWith("0x"))
                    return null;
                var usid = uint.Parse(sid.Substring(2), NumberStyles.HexNumber);

                var jTarget = j?["Target"] as JObject;
                var targetType = Type.GetType($"BossMod.PlanTarget.{jTarget?["Type"]?.Value<string>() ?? ""}");
                PlanTarget.ISelector? target = targetType?.IsAssignableTo(typeof(PlanTarget.ISelector)) ?? false ? (PlanTarget.ISelector?)Activator.CreateInstance(targetType) : null;
                if (target != null)
                {
                    foreach (var (f, data) in jTarget!)
                    {
                        if (f != "Type")
                        {
                            var field = targetType?.GetField(f);
                            if (field != null)
                            {
                                var value = data?.ToObject(field.FieldType, ser);
                                if (value != null)
                                {
                                    field.SetValue(target, value);
                                }
                            }
                        }
                    }
                }

                return new ActionUse(actionID, usid, j?["TimeSinceActivation"]?.Value<float>() ?? 0, j?["WindowLength"]?.Value<float>() ?? 0, j?["LowPriority"]?.Value<bool>() ?? false, target ?? new PlanTarget.Self(), j?["Comment"]?.Value<string>() ?? "");
            }

            public JObject ToJSON(Type aidType, JsonSerializer ser)
            {
                var aidStr = ID.Type == ActionType.Spell ? aidType.GetEnumName(ID.ID) : null;
                aidStr ??= $"0x{ID.Raw:X}";

                var target = JObject.FromObject(Target, ser);
                target["Type"] = Target.GetType().Name;

                JObject res = new();
                res["ID"] = aidStr;
                res["StateID"] = $"0x{StateID:X8}";
                res["TimeSinceActivation"] = TimeSinceActivation;
                res["WindowLength"] = WindowLength;
                res["LowPriority"] = LowPriority;
                res["Target"] = target;
                res["Comment"] = Comment;
                return res;
            }
        }

        public Class Class;
        public int Level;
        public string Name;
        public StateMachineTimings Timings = new();
        public List<ActionUse> Actions = new();

        public CooldownPlan(Class @class, int level, string name)
        {
            Class = @class;
            Level = level;
            Name = name;
        }

        public CooldownPlan Clone()
        {
            var res = new CooldownPlan(Class, Level, Name);
            res.Timings = Timings.Clone();
            res.Actions.AddRange(Actions.Select(a => a.Clone()));
            return res;
        }

        public static CooldownPlan? FromJSON(Class @class, int level, JObject? j, JsonSerializer ser)
        {
            var name = j?["Name"]?.Value<string>();
            if (name == null)
                return null;
            var res = new CooldownPlan(@class, level, name);
            res.Timings = j?["Timings"]?.ToObject<StateMachineTimings>(ser) ?? res.Timings;
            var actions = j?["Actions"] as JArray;
            var aidType = PlanDefinitions.Classes[@class].AIDType;
            if (actions != null)
            {
                foreach (var ja in actions)
                {
                    var a = ActionUse.FromJSON(aidType, ja as JObject, ser);
                    if (a != null)
                    {
                        res.Actions.Add(a);
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
            var aidType = PlanDefinitions.Classes[Class].AIDType;
            var actions = new JArray();
            res["Actions"] = actions;
            foreach (var a in Actions)
                actions.Add(a.ToJSON(aidType, ser));
            return res;
        }
    }
}
