using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod.Autorotation;

// preset is a list of rotation states tied to encounter timeline
[JsonConverter(typeof(JsonPlanConverter))]
public sealed record class Plan(string Name, Type Encounter)
{
    public record struct Entry(StrategyValue Value)
    {
        public uint StateID;
        public float TimeSinceActivation;
        public float WindowLength;
        public StrategyValue Value = Value;
    }

    public string Name = Name;
    public Type Encounter = Encounter;
    public Class Class;
    public int Level;
    public readonly Dictionary<Type, List<List<Entry>>> Modules = []; // [RM][track] = entries
}

// we want to serialize track/option indices as internal names, to simplify making changes
public class JsonPlanConverter : JsonConverter<Plan>
{
    public override Plan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jdoc = JsonDocument.ParseValue(ref reader);
        var name = jdoc.RootElement.GetProperty(nameof(Plan.Name)).GetString() ?? "";
        var encName = jdoc.RootElement.GetProperty(nameof(Plan.Encounter)).GetString() ?? "";
        var encType = Type.GetType(encName);
        var encInfo = encType != null ? ModuleRegistry.FindByType(encType) : null;
        if (encInfo == null)
        {
            Service.Log($"Error while deserializing plan {name}: failed to find encounter {encName}");
            return null;
        }

        var res = new Plan(name, encType!)
        {
            Class = Enum.Parse<Class>(jdoc.RootElement.GetProperty(nameof(Plan.Class)).GetString() ?? ""),
            Level = jdoc.RootElement.GetProperty(nameof(Plan.Level)).GetInt32()
        };
        foreach (var jm in jdoc.RootElement.GetProperty(nameof(Plan.Modules)).EnumerateObject())
        {
            var mt = Type.GetType(jm.Name);
            if (mt == null || !RotationModuleRegistry.Modules.TryGetValue(mt, out var md))
            {
                Service.Log($"Error while deserializing plan {name} for L{res.Level} {res.Class} encounter {encName}: failed to find module {jm.Name}");
                continue;
            }

            var m = res.Modules[mt] = [];
            foreach (var _ in md.Definition.Configs)
                m.Add([]);
            foreach (var jt in jm.Value.EnumerateObject())
            {
                var iTrack = md.Definition.Configs.FindIndex(s => s.InternalName == jt.Name);
                if (iTrack < 0)
                {
                    Service.Log($"Error while deserializing plan {name} for L{res.Level} {res.Class} encounter {encName}: failed to find track {jt.Name} in module {jm.Name}");
                    continue;
                }
                var track = m[iTrack];
                var cfg = md.Definition.Configs[iTrack];
                foreach (var js in jt.Value.EnumerateArray())
                {
                    var s = new Plan.Entry(new());

                    var optionName = js.GetProperty(nameof(StrategyValue.Option)).GetString() ?? "";
                    s.Value.Option = cfg.Options.FindIndex(o => o.InternalName == optionName);
                    if (s.Value.Option < 0)
                    {
                        Service.Log($"Error while deserializing plan {name} for L{res.Level} {res.Class} encounter {encName}: failed to find option {optionName} in track {jt.Name} in module {jm.Name}");
                        continue;
                    }
                    s.StateID = uint.Parse((js.GetProperty(nameof(Plan.Entry.StateID)).GetString() ?? "")[2..], System.Globalization.NumberStyles.HexNumber);
                    s.TimeSinceActivation = js.GetProperty(nameof(Plan.Entry.TimeSinceActivation)).GetSingle();
                    s.WindowLength = js.GetProperty(nameof(Plan.Entry.WindowLength)).GetSingle();
                    if (js.TryGetProperty(nameof(StrategyValue.PriorityOverride), out var jprio))
                        s.Value.PriorityOverride = jprio.GetSingle();
                    if (js.TryGetProperty(nameof(StrategyValue.Target), out var jtarget))
                        s.Value.Target = Enum.Parse<StrategyTarget>(jtarget.GetString() ?? "");
                    if (js.TryGetProperty(nameof(StrategyValue.TargetParam), out var jtp))
                        s.Value.TargetParam = jtp.GetInt32();
                    if (js.TryGetProperty(nameof(StrategyValue.Comment), out var jcomment))
                        s.Value.Comment = jcomment.GetString() ?? "";

                    track.Add(s);
                }
            }
        }
        return res;
    }

    public override void Write(Utf8JsonWriter writer, Plan value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Plan.Name), value.Name);
        writer.WriteString(nameof(Plan.Encounter), value.Encounter.FullName);
        writer.WriteString(nameof(Plan.Class), value.Class.ToString());
        writer.WriteNumber(nameof(Plan.Level), value.Level);
        writer.WriteStartObject(nameof(Plan.Modules));
        foreach (var m in value.Modules)
        {
            var md = RotationModuleRegistry.Modules[m.Key].Definition;
            writer.WriteStartObject(m.Key.FullName!);
            for (int iTrack = 0; iTrack < m.Value.Count; ++iTrack)
            {
                var track = m.Value[iTrack];
                if (track.Count == 0)
                    continue;

                var cfg = md.Configs[iTrack];
                writer.WriteStartArray(cfg.InternalName);
                foreach (ref var s in track.AsSpan())
                {
                    writer.WriteStartObject();
                    writer.WriteString(nameof(Plan.Entry.StateID), $"0x{s.StateID:X8}");
                    writer.WriteNumber(nameof(Plan.Entry.TimeSinceActivation), s.TimeSinceActivation);
                    writer.WriteNumber(nameof(Plan.Entry.WindowLength), s.WindowLength);
                    writer.WriteString(nameof(StrategyValue.Option), cfg.Options[s.Value.Option].InternalName);
                    if (!float.IsNaN(s.Value.PriorityOverride))
                        writer.WriteNumber(nameof(StrategyValue.PriorityOverride), s.Value.PriorityOverride);
                    if (s.Value.Target != StrategyTarget.Automatic)
                        writer.WriteString(nameof(StrategyValue.Target), s.Value.Target.ToString());
                    if (s.Value.TargetParam != 0)
                        writer.WriteNumber(nameof(StrategyValue.TargetParam), s.Value.TargetParam);
                    if (s.Value.Comment.Length > 0)
                        writer.WriteString(nameof(StrategyValue.Comment), s.Value.Comment);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
