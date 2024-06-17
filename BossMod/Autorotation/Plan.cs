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

    public string Guid = "";
    public string Name = Name;
    public Type Encounter = Encounter;
    public Class Class;
    public int Level;
    public List<float> PhaseDurations = [];
    public Dictionary<Type, List<List<Entry>>> Modules = []; // [RM][track] = entries
    public List<Entry> Targeting = []; // note that Value.Option & Value.Priority are (currently?) ignored

    public Plan MakeClone()
    {
        var res = this with { PhaseDurations = [.. PhaseDurations], Modules = [], Targeting = [.. Targeting] };
        foreach (var kv in Modules)
            res.Modules[kv.Key] = [.. kv.Value.Select(t => new List<Entry>([.. t]))];
        return res;
    }

    // this maintains the invariant that each module has entry list per track
    public List<List<Entry>> AddModule(Type t)
    {
        var m = Modules[t] = [];
        foreach (var _ in RotationModuleRegistry.Modules[t].Definition.Configs)
            m.Add([]);
        return m;
    }
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
        foreach (var jd in jdoc.RootElement.GetProperty(nameof(Plan.PhaseDurations)).EnumerateArray())
        {
            res.PhaseDurations.Add(jd.GetSingle());
        }
        foreach (var jm in jdoc.RootElement.GetProperty(nameof(Plan.Modules)).EnumerateObject())
        {
            var mt = Type.GetType(jm.Name);
            if (mt == null || !RotationModuleRegistry.Modules.TryGetValue(mt, out var md))
            {
                Service.Log($"Error while deserializing plan {name} for L{res.Level} {res.Class} encounter {encName}: failed to find module {jm.Name}");
                continue;
            }

            var m = res.AddModule(mt);
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
                    ReadEntryFields(ref s, in js);
                    track.Add(s);
                }
            }
        }
        foreach (var jt in jdoc.RootElement.GetProperty(nameof(Plan.Targeting)).EnumerateArray())
        {
            var s = new Plan.Entry(new());
            ReadEntryFields(ref s, in jt);
            res.Targeting.Add(s);
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
        writer.WriteStartArray(nameof(Plan.PhaseDurations));
        foreach (var d in value.PhaseDurations)
            writer.WriteNumberValue(d);
        writer.WriteEndArray();
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
                    writer.WriteString(nameof(StrategyValue.Option), cfg.Options[s.Value.Option].InternalName);
                    WriteEntryFields(writer, in s);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
        writer.WriteStartArray(nameof(Plan.Targeting));
        foreach (ref var t in value.Targeting.AsSpan())
        {
            writer.WriteStartObject();
            WriteEntryFields(writer, in t);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private void ReadEntryFields(ref Plan.Entry entry, in JsonElement jelem)
    {
        entry.StateID = uint.Parse((jelem.GetProperty(nameof(Plan.Entry.StateID)).GetString() ?? "")[2..], System.Globalization.NumberStyles.HexNumber);
        entry.TimeSinceActivation = jelem.GetProperty(nameof(Plan.Entry.TimeSinceActivation)).GetSingle();
        entry.WindowLength = jelem.GetProperty(nameof(Plan.Entry.WindowLength)).GetSingle();
        if (jelem.TryGetProperty(nameof(StrategyValue.PriorityOverride), out var jprio))
            entry.Value.PriorityOverride = jprio.GetSingle();
        if (jelem.TryGetProperty(nameof(StrategyValue.Target), out var jtarget))
            entry.Value.Target = Enum.Parse<StrategyTarget>(jtarget.GetString() ?? "");
        if (jelem.TryGetProperty(nameof(StrategyValue.TargetParam), out var jtp))
            entry.Value.TargetParam = jtp.GetInt32();
        if (jelem.TryGetProperty(nameof(StrategyValue.Comment), out var jcomment))
            entry.Value.Comment = jcomment.GetString() ?? "";
    }

    private void WriteEntryFields(Utf8JsonWriter writer, in Plan.Entry entry)
    {
        writer.WriteString(nameof(Plan.Entry.StateID), $"0x{entry.StateID:X8}");
        writer.WriteNumber(nameof(Plan.Entry.TimeSinceActivation), entry.TimeSinceActivation);
        writer.WriteNumber(nameof(Plan.Entry.WindowLength), entry.WindowLength);
        if (!float.IsNaN(entry.Value.PriorityOverride))
            writer.WriteNumber(nameof(StrategyValue.PriorityOverride), entry.Value.PriorityOverride);
        if (entry.Value.Target != StrategyTarget.Automatic)
            writer.WriteString(nameof(StrategyValue.Target), entry.Value.Target.ToString());
        if (entry.Value.TargetParam != 0)
            writer.WriteNumber(nameof(StrategyValue.TargetParam), entry.Value.TargetParam);
        if (entry.Value.Comment.Length > 0)
            writer.WriteString(nameof(StrategyValue.Comment), entry.Value.Comment);
    }
}
