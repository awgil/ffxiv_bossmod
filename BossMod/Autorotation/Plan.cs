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
        public bool Disabled;
        public StrategyValue Value = Value;
    }

    public readonly record struct Module(Type Type, RotationModuleDefinition Definition, Func<RotationModuleManager, Actor, RotationModule> Builder, List<List<Entry>> Tracks, List<StrategyValue> Defaults) : IRotationModuleData
    {
        public readonly Module MakeClone() => this with { Tracks = [.. Tracks.Select(t => new List<Entry>([.. t]))], Defaults = [.. Defaults] };
    }

    public string Guid = "";
    public string Name = Name;
    public Type Encounter = Encounter;
    public Class Class;
    public int Level;
    public List<float> PhaseDurations = [];
    public List<Module> Modules = [];
    public List<Entry> Targeting = []; // note that Value.Option & Value.Priority are (currently?) ignored

    public Plan MakeClone() => this with { PhaseDurations = [.. PhaseDurations], Modules = [.. Modules.Select(m => m.MakeClone())], Targeting = [.. Targeting] };

    // this maintains the invariant that each module has entry list per track
    public int AddModule(Type t, RotationModuleDefinition def, Func<RotationModuleManager, Actor, RotationModule> builder)
    {
        List<List<Entry>> tracks = [];
        List<StrategyValue> defaults = [];
        foreach (var cfg in def.Configs)
        {
            tracks.Add([]);
            defaults.Add(cfg.CreateEmpty());
        }

        var insertionIndex = Modules.Count;
        while (insertionIndex > 0 && Modules[insertionIndex - 1].Definition.Order > def.Order)
            --insertionIndex;

        Modules.Insert(insertionIndex, new(t, def, builder, tracks, defaults));
        return insertionIndex;
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
        var encInfo = encType != null ? BossModuleRegistry.FindByType(encType) : null;
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

            var mi = res.AddModule(mt, md.Definition, md.Builder);
            var m = res.Modules[mi].Tracks;
            foreach (var jt in jm.Value.EnumerateObject())
            {
                if (jt.Name == "_defaults")
                {
                    foreach (var jd in jt.Value.EnumerateObject())
                    {
                        var dTrack = md.Definition.Configs.FindIndex(s => s.InternalName == jd.Name);
                        if (dTrack < 0)
                        {
                            Service.Log($"Error while deserializing plan {name} for L{res.Level} {res.Class} encounter {encName}: failed to find track {jd.Name} in module {jm.Name}");
                            continue;
                        }

                        switch (md.Definition.Configs[dTrack])
                        {
                            case StrategyConfigTrack sct:
                                var optionName = jd.Value.GetString() ?? "";
                                var s = new StrategyValueTrack()
                                {
                                    Option = sct.Options.FindIndex(o => o.InternalName == optionName)
                                };
                                if (s.Option < 0)
                                {
                                    Service.Log($"Error while deserializing plan {name} for L{res.Level} {res.Class} encounter {encName}: failed to find option {optionName} in track {jd.Name} in module {jm.Name}");
                                    continue;
                                }
                                res.Modules[mi].Defaults[dTrack] = s;
                                break;
                            case StrategyConfigInt sci:
                                res.Modules[mi].Defaults[dTrack] = new StrategyValueInt() { Value = jd.Value.GetInt64() };
                                break;
                            case StrategyConfigFloat scf:
                                res.Modules[mi].Defaults[dTrack] = new StrategyValueFloat() { Value = (float)jd.Value.GetDouble() };
                                break;
                        }
                    }
                    continue;
                }

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
                    Plan.Entry s;
                    switch (md.Definition.Configs[iTrack])
                    {
                        case StrategyConfigTrack cfgTrack:
                            var optionName = js.GetProperty(nameof(StrategyValueTrack.Option)).GetString() ?? "";
                            var t = new StrategyValueTrack
                            {
                                Option = cfgTrack.Options.FindIndex(o => o.InternalName == optionName)
                            };
                            if (t.Option < 0)
                            {
                                Service.Log($"Error while deserializing plan {name} for L{res.Level} {res.Class} encounter {encName}: failed to find option {optionName} in track {jt.Name} in module {jm.Name}");
                                continue;
                            }
                            s = new Plan.Entry(t);
                            break;
                        case StrategyConfigFloat cfgScalar:
                            var f = new StrategyValueFloat
                            {
                                Value = js.GetProperty(nameof(StrategyValueFloat.Value)).GetSingle()
                            };
                            s = new Plan.Entry(f);
                            break;
                        case StrategyConfigInt cfgInt:
                            var ci = new StrategyValueInt
                            {
                                Value = js.GetProperty(nameof(StrategyValueInt.Value)).GetInt64()
                            };
                            s = new Plan.Entry(ci);
                            break;
                        default:
                            Service.Log($"Error while deserializing: unrecognized config type {cfg.GetType()}");
                            continue;
                    }

                    ReadEntryFields(ref s, in js);
                    track.Add(s);
                }
            }
        }
        foreach (var jt in jdoc.RootElement.GetProperty(nameof(Plan.Targeting)).EnumerateArray())
        {
            var s = new Plan.Entry(new StrategyValueTrack());
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
            writer.WriteStartObject(m.Type.FullName!);
            for (int iTrack = 0; iTrack < m.Tracks.Count; ++iTrack)
            {
                var track = m.Tracks[iTrack];
                if (track.Count == 0)
                    continue;

                var cfg = m.Definition.Configs[iTrack];
                writer.WriteStartArray(cfg.InternalName);
                foreach (ref var s in track.AsSpan())
                {
                    writer.WriteStartObject();
                    cfg.SerializeValue(writer, s.Value);
                    WriteEntryFields(writer, in s);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
            writer.WriteStartObject("_defaults");
            for (int iDef = 0; iDef < m.Defaults.Count; ++iDef)
            {
                var def = m.Defaults[iDef];
                if (def == default)
                    continue;

                switch (m.Definition.Configs[iDef])
                {
                    case StrategyConfigTrack cfg:
                        writer.WriteString(cfg.InternalName, cfg.Options[((StrategyValueTrack)def).Option].InternalName);
                        break;
                    case StrategyConfigInt cfgi:
                        writer.WriteNumber(cfgi.InternalName, ((StrategyValueInt)def).Value);
                        break;
                    case StrategyConfigFloat cfgf:
                        writer.WriteNumber(cfgf.InternalName, ((StrategyValueFloat)def).Value);
                        break;
                }
            }
            writer.WriteEndObject();
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
        if (jelem.TryGetProperty(nameof(Plan.Entry.Disabled), out var jdisabled))
            entry.Disabled = jdisabled.GetBoolean();

        entry.Value.DeserializeFields(jelem);
    }

    private void WriteEntryFields(Utf8JsonWriter writer, in Plan.Entry entry)
    {
        writer.WriteString(nameof(Plan.Entry.StateID), $"0x{entry.StateID:X8}");
        writer.WriteNumber(nameof(Plan.Entry.TimeSinceActivation), entry.TimeSinceActivation);
        writer.WriteNumber(nameof(Plan.Entry.WindowLength), entry.WindowLength);
        if (entry.Disabled)
            writer.WriteBoolean(nameof(Plan.Entry.Disabled), entry.Disabled);

        entry.Value.SerializeFields(writer);
    }
}
