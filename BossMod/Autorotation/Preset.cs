using Dalamud.Bindings.ImGui;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod.Autorotation;

// preset is a manual override for the rotation state; it describes a set of active modules and selected strategy values for each one
[JsonConverter(typeof(JsonPresetConverter))]
public sealed record class Preset(string Name)
{
    [Flags]
    public enum Modifier
    {
        None = 0,
        Shift = 1,
        Ctrl = 2,
        Alt = 4,
    }

    public record struct ModuleSetting(Modifier Mod, int Track, StrategyValue Value)
    {
        public Modifier Mod = Mod;
        public StrategyValue Value = Value;
    }

    public record class ModuleSettings(Type Type, RotationModuleDefinition Definition, Func<RotationModuleManager, Actor, RotationModule> Builder) : IRotationModuleData
    {
        public readonly List<ModuleSetting> SerializedSettings = [];
        public readonly List<ModuleSetting> TransientSettings = [];
    }

    public string Name = Name;
    public List<ModuleSettings> Modules = [];

    public Preset MakeClone(bool includeTransient)
    {
        var res = new Preset(Name);
        foreach (var m in Modules)
        {
            var ms = new ModuleSettings(m.Type, m.Definition, m.Builder);
            ms.SerializedSettings.AddRange(m.SerializedSettings);
            if (includeTransient)
                ms.TransientSettings.AddRange(m.TransientSettings);
            res.Modules.Add(ms);
        }
        return res;
    }

    public int AddModule(Type t, RotationModuleDefinition def, Func<RotationModuleManager, Actor, RotationModule> builder)
    {
        var insertionIndex = Modules.Count;
        while (insertionIndex > 0 && Modules[insertionIndex - 1].Definition.Order > def.Order)
            --insertionIndex;

        Modules.Insert(insertionIndex, new(t, def, builder));
        return insertionIndex;
    }

    public StrategyValues ActiveStrategyOverrides(int moduleIndex, Modifier mods)
    {
        var m = Modules[moduleIndex];
        var res = new StrategyValues(m.Definition.Configs);
        foreach (ref var s in m.SerializedSettings.AsSpan())
            if ((s.Mod & mods) == s.Mod)
                res.Values[s.Track] = s.Value;
        foreach (ref var s in m.TransientSettings.AsSpan())
            if ((s.Mod & mods) == s.Mod)
                res.Values[s.Track] = s.Value;
        return res;
    }

    public StrategyValues ActiveStrategyOverrides(int moduleIndex) => ActiveStrategyOverrides(moduleIndex, CurrentModifiers());

    public static Modifier CurrentModifiers()
    {
        Modifier mods = Modifier.None;
        if (ImGui.GetIO().KeyShift)
            mods |= Modifier.Shift;
        if (ImGui.GetIO().KeyCtrl)
            mods |= Modifier.Ctrl;
        if (ImGui.GetIO().KeyAlt)
            mods |= Modifier.Alt;
        return mods;
    }
}

// we want to serialize track/option indices as internal names, to simplify making changes
public class JsonPresetConverter : JsonConverter<Preset>
{
    public override Preset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jdoc = JsonDocument.ParseValue(ref reader);
        var res = new Preset(jdoc.RootElement.GetProperty(nameof(Preset.Name)).GetString() ?? "");
        foreach (var jm in jdoc.RootElement.GetProperty(nameof(Preset.Modules)).EnumerateObject())
        {
            var mt = Type.GetType(jm.Name);
            if (mt == null || !RotationModuleRegistry.Modules.TryGetValue(mt, out var md))
            {
                Service.Log($"Error while deserializing preset {res.Name}: failed to find module {jm.Name}");
                continue;
            }

            var mi = res.AddModule(mt, md.Definition, md.Builder);
            var m = res.Modules[mi];
            foreach (var js in jm.Value.EnumerateArray())
            {
                var s = new Preset.ModuleSetting() { Value = new() };

                var trackName = js.GetProperty(nameof(Preset.ModuleSetting.Track)).GetString() ?? "";
                s.Track = md.Definition.Configs.FindIndex(s => s.InternalName == trackName);
                if (s.Track < 0)
                {
                    Service.Log($"Error while deserializing preset {res.Name}: failed to find track {trackName} in module {jm.Name}");
                    continue;
                }

                var optionName = js.GetProperty(nameof(StrategyValue.Option)).GetString() ?? "";
                s.Value.Option = md.Definition.Configs[s.Track].Options.FindIndex(o => o.InternalName == optionName);
                if (s.Value.Option < 0)
                {
                    Service.Log($"Error while deserializing preset {res.Name}: failed to find option {optionName} in track {trackName} in module {jm.Name}");
                    continue;
                }

                if (js.TryGetProperty(nameof(Preset.ModuleSetting.Mod), out var jmod))
                    s.Mod = Enum.Parse<Preset.Modifier>(jmod.GetString() ?? "");
                if (js.TryGetProperty(nameof(StrategyValue.PriorityOverride), out var jprio))
                    s.Value.PriorityOverride = jprio.GetSingle();
                if (js.TryGetProperty(nameof(StrategyValue.Target), out var jtarget))
                    s.Value.Target = Enum.Parse<StrategyTarget>(jtarget.GetString() ?? "");
                if (js.TryGetProperty(nameof(StrategyValue.TargetParam), out var jtp))
                    s.Value.TargetParam = jtp.GetInt32();
                if (js.TryGetProperty(nameof(StrategyValue.Offset1), out var joff1))
                    s.Value.Offset1 = joff1.GetSingle();
                if (js.TryGetProperty(nameof(StrategyValue.Offset2), out var joff2))
                    s.Value.Offset2 = joff2.GetSingle();
                if (js.TryGetProperty(nameof(StrategyValue.Comment), out var jcomment))
                    s.Value.Comment = jcomment.GetString() ?? "";

                m.SerializedSettings.Add(s);
            }
        }
        return res;
    }

    public override void Write(Utf8JsonWriter writer, Preset value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Preset.Name), value.Name);
        writer.WriteStartObject(nameof(Preset.Modules));
        foreach (var m in value.Modules)
        {
            writer.WriteStartArray(m.Type.FullName!);
            foreach (ref var s in m.SerializedSettings.AsSpan())
            {
                writer.WriteStartObject();
                writer.WriteString(nameof(Preset.ModuleSetting.Track), m.Definition.Configs[s.Track].InternalName);
                writer.WriteString(nameof(StrategyValue.Option), m.Definition.Configs[s.Track].Options[s.Value.Option].InternalName);
                if (s.Mod != Preset.Modifier.None)
                    writer.WriteString(nameof(Preset.ModuleSetting.Mod), s.Mod.ToString());
                if (!float.IsNaN(s.Value.PriorityOverride))
                    writer.WriteNumber(nameof(StrategyValue.PriorityOverride), s.Value.PriorityOverride);
                if (s.Value.Target != StrategyTarget.Automatic)
                    writer.WriteString(nameof(StrategyValue.Target), s.Value.Target.ToString());
                if (s.Value.TargetParam != 0)
                    writer.WriteNumber(nameof(StrategyValue.TargetParam), s.Value.TargetParam);
                if (s.Value.Offset1 != 0)
                    writer.WriteNumber(nameof(StrategyValue.Offset1), s.Value.Offset1);
                if (s.Value.Offset2 != 0)
                    writer.WriteNumber(nameof(StrategyValue.Offset2), s.Value.Offset2);
                if (s.Value.Comment.Length > 0)
                    writer.WriteString(nameof(StrategyValue.Comment), s.Value.Comment);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
