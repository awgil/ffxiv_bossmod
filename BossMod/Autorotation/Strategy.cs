using BossMod.Autorotation.xan;
using System.Text.Json;

namespace BossMod.Autorotation;

// target selection strategies; there is an extra int parameter that targets can use for storing more info
public enum StrategyTarget
{
    Automatic, // default 'smart' targeting, for hostile actions usually defaults to current primary target
    Self,
    PartyByAssignment, // parameter is assignment; won't work if assignments aren't set up properly for a party
    PartyWithLowestHP, // parameter is StrategyPartyFiltering, which filters subset of party members
    EnemyWithHighestPriority, // parameter is StrategyEnemySelection, which determines selecton criteria if there are multiple matching enemies
    EnemyByOID, // parameter is oid; not really useful outside planner; selects closest if there are multiple
    PointAbsolute, // absolute x/y coordinates
    PointCenter, // offset from arena center
    PointWaymark, // offset from waymark; parameter is waymark id

    Count
}

[Flags]
public enum StrategyContext
{
    None = 0,
    Preset = 1 << 0,
    Plan = 1 << 1,

    All = Preset | Plan
}

// parameter for party member filtering
[Flags]
public enum StrategyPartyFiltering : int
{
    None = 0,
    IncludeSelf = 1 << 0,
    ExcludeTanks = 1 << 1,
    ExcludeHealers = 1 << 2,
    ExcludeMelee = 1 << 3,
    ExcludeRanged = 1 << 4,
    ExcludeNoPredictedDamage = 1 << 5,
}

// parameter for prioritizing enemies
public enum StrategyEnemySelection : int
{
    Closest = 0,
    LowestCurHP = 1,
    HighestCurHP = 2,
    LowestMaxHP = 3,
    HighestMaxHP = 4,
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class TrackAttribute() : Attribute
{
    public TrackAttribute(string displayName) : this()
    {
        DisplayName = displayName;
    }

    public string? DisplayName { get; }
    public string? InternalName;
    public float UiPriority;
    public Type? Renderer;
    public ActionID[] ActionIDs = [];

    public object Action
    {
        set => Actions = [value];
        get => Actions[0];
    }
    public object[] Actions
    {
        set => ActionIDs = [.. value.Select(v => v switch {
            BozjaHolsterID id => BozjaActionID.GetNormal(id),
            var x => ActionID.MakeSpell((Enum)x)
        })];
        get => [.. ActionIDs];
    }
    public object Item
    {
        set => ActionIDs = [new(ActionType.Item, (uint)(int)value)];
        get => ActionIDs[0];
    }

    // fallback values for all options in track
    public float Cooldown;
    public float Effect;
    public ActionTargets Targets;
    public int MinLevel;
    public int MaxLevel;
    public float DefaultPriority;
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class NumberAttribute() : Attribute
{
    public float UiPriority;
    public string DisplayName = "";

    public Type? Renderer;

    public float MinValue;
    public float MaxValue = float.MaxValue;
    public float Speed = 1;
    public bool Slider = true;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
public sealed class OptionAttribute() : Attribute
{
    public OptionAttribute(string displayName) : this()
    {
        DisplayName = displayName;
    }

    public string? DisplayName { get; }
    public float Cooldown;
    public float Effect;
    public ActionTargets Targets;
    public int MinLevel;
    public int MaxLevel;
    public float DefaultPriority;
    public StrategyContext Context;
    public uint Color;
}

public abstract record class StrategyConfig(
    string InternalName, // unique name of the config; it is used for serialization, so it can't really be changed without losing user data (or writing config converter)
    string DisplayName, // if non-empty, this name is used for all UI instead of internal name
    float UIPriority, // tracks are sorted by UI priority for display; negative are hidden by default
    Type Renderer // custom drawing for regular config, plan UI still uses old editor
)
{
    public abstract StrategyValue CreateEmpty();
    public abstract StrategyValue CreateForEditor();

    public abstract bool IsDefault(StrategyValue val);
    public abstract string ToDisplayString(StrategyValue val);
    public abstract void SerializeValue(Utf8JsonWriter writer, StrategyValue val);

    public string UIName => DisplayName.Length > 0 ? DisplayName : InternalName;
}

// the tuning knobs of the rotation module are represented by strategy config rather than usual global config classes, since we they need to be changed dynamically by planner or manual input
public record class StrategyConfigTrack(
    Type OptionEnum, // type of the enum used for options
    string InternalName,
    string DisplayName,
    float UIPriority,
    Type Renderer
) : StrategyConfig(InternalName, DisplayName, UIPriority, Renderer)
{
    public readonly List<StrategyOption> Options = [];
    public readonly List<ActionID> AssociatedActions = []; // these actions will be shown on the track in the planner ui

    public override StrategyValueTrack CreateEmpty() => new();
    public override StrategyValueTrack CreateForEditor() => new() { Option = Options.Count > 1 ? 1 : 0 };

    public override bool IsDefault(StrategyValue val) => ((StrategyValueTrack)val).Option == 0;
    public override string ToDisplayString(StrategyValue val) => Options[((StrategyValueTrack)val).Option].UIName;
    public override void SerializeValue(Utf8JsonWriter writer, StrategyValue val)
    {
        writer.WriteString(nameof(StrategyValueTrack.Option), Options[((StrategyValueTrack)val).Option].InternalName);
    }
}

public record class StrategyConfigFloat(
    string InternalName,
    string DisplayName,
    float MinValue,
    float MaxValue,
    float UIPriority,
    Type Renderer,
    bool Drag = true,
    float Speed = 1
) : StrategyConfig(InternalName, DisplayName, UIPriority, Renderer)
{
    public override StrategyValueFloat CreateEmpty() => new() { Value = MinValue };
    public override StrategyValueFloat CreateForEditor() => new() { Value = MinValue };

    public override bool IsDefault(StrategyValue val) => (((StrategyValueFloat)val).Value - MinValue) < 1e-8;
    public override string ToDisplayString(StrategyValue val) => ((StrategyValueFloat)val).Value.ToString("f1");
    public override void SerializeValue(Utf8JsonWriter writer, StrategyValue val)
    {
        writer.WriteNumber(nameof(StrategyValueFloat.Value), ((StrategyValueFloat)val).Value);
    }
}

public record class StrategyConfigInt(
    string InternalName,
    string DisplayName,
    long MinValue,
    long MaxValue,
    float UIPriority,
    Type Renderer,
    bool Drag = true,
    float Speed = 0.25f
) : StrategyConfig(InternalName, DisplayName, UIPriority, Renderer)
{
    public override StrategyValueInt CreateEmpty() => new() { Value = MinValue };
    public override StrategyValueInt CreateForEditor() => throw new NotImplementedException();

    public override bool IsDefault(StrategyValue val) => ((StrategyValueInt)val).Value == MinValue;
    public override string ToDisplayString(StrategyValue val) => ((StrategyValueInt)val).Value.ToString();
    public override void SerializeValue(Utf8JsonWriter writer, StrategyValue val)
    {
        writer.WriteNumber(nameof(StrategyValueInt.Value), ((StrategyValueInt)val).Value);
    }
}

// each strategy config has a unique set of allowed options; each option has a set of properties describing how it is rendered in planner and what further configuration parameters it supports
// note: first option (with index 0) should correspond to the default automatic behaviour; second option (with index 1) should correspond to most often used override (it's selected by default when adding override)
public record class StrategyOption(string InternalName, string DisplayName)
{
    public string InternalName = InternalName; // unique name of the option; it is used for serialization, so it can't really be changed without losing user data (or writing config converter)
    public string DisplayName = DisplayName; // if non-empty, this name is used for all UI instead of internal name
    public float Cooldown; // if > 0, this time after window end is shaded to notify user about associated action cooldown
    public float Effect; // if > 0, this time after window start is shaded to notify user about associated effect duration
    public ActionTargets SupportedTargets; // valid targets for relevant action; used to filter target options for values
    public int MinLevel = 1; // min character level for this option to be available
    public int MaxLevel = int.MaxValue; // max character level for this option to be available
    public float DefaultPriority = ActionQueue.Priority.Medium; // default priority that is used if no override is defined
    public StrategyContext Context = StrategyContext.All;
    public uint Color; // used in CD planner timeline instead of default planner colors, if set

    public string UIName => DisplayName.Length > 0 ? DisplayName : InternalName;
}

public abstract record class StrategyValue
{
    public string Comment = "";
    public float ExpireIn = float.MaxValue;

    public virtual bool IsDefault() => false;

    public abstract void DeserializeFields(JsonElement js);
    public abstract void SerializeFields(Utf8JsonWriter writer);
}

// value represents the concrete option of a config that is selected at a given time; it can be either put on the planner timeline, or configured as part of manual overrides
public record class StrategyValueTrack : StrategyValue
{
    public int Option; // index of the selected option among the Options list of the corresponding config
    public float PriorityOverride = float.NaN; // priority override for the action controlled by the config; not all configs support it, if not set the default priority is used
    public StrategyTarget Target; // target selection strategy
    public int TargetParam; // strategy-specific parameter
    public float Offset1; // x or r coordinate
    public float Offset2; // y or phi coordinate

    public override bool IsDefault() => Option == 0;

    public override void DeserializeFields(JsonElement js)
    {
        if (js.TryGetProperty(nameof(PriorityOverride), out var jprio))
            PriorityOverride = jprio.GetSingle();
        if (js.TryGetProperty(nameof(Target), out var jtarget))
            Target = Enum.Parse<StrategyTarget>(jtarget.GetString() ?? "");
        if (js.TryGetProperty(nameof(TargetParam), out var jtp))
            TargetParam = jtp.GetInt32();
        if (js.TryGetProperty(nameof(Offset1), out var joff1))
            Offset1 = joff1.GetSingle();
        if (js.TryGetProperty(nameof(Offset2), out var joff2))
            Offset2 = joff2.GetSingle();
        if (js.TryGetProperty(nameof(Comment), out var jcomment))
            Comment = jcomment.GetString() ?? "";
    }

    public override void SerializeFields(Utf8JsonWriter writer)
    {
        if (!float.IsNaN(PriorityOverride))
            writer.WriteNumber(nameof(PriorityOverride), PriorityOverride);
        if (Target != StrategyTarget.Automatic)
            writer.WriteString(nameof(Target), Target.ToString());
        if (TargetParam != 0)
            writer.WriteNumber(nameof(TargetParam), TargetParam);
        if (Offset1 != 0)
            writer.WriteNumber(nameof(Offset1), Offset1);
        if (Offset2 != 0)
            writer.WriteNumber(nameof(Offset2), Offset2);
        if (Comment.Length > 0)
            writer.WriteString(nameof(Comment), Comment);
    }
}

public record class StrategyValueFloat : StrategyValue
{
    public float Value;

    public override void DeserializeFields(JsonElement js)
    {
        if (js.TryGetProperty(nameof(Value), out var v))
            Value = v.GetSingle();
    }

    public override void SerializeFields(Utf8JsonWriter writer)
    {
        writer.WriteNumber(nameof(Value), Value);
    }
}

public record class StrategyValueInt : StrategyValue
{
    public long Value;

    public override void DeserializeFields(JsonElement js)
    {
        if (js.TryGetProperty(nameof(Value), out var v))
            Value = v.GetInt64();
    }

    public override void SerializeFields(Utf8JsonWriter writer)
    {
        writer.WriteNumber(nameof(Value), Value);
    }
}

public readonly record struct StrategyValues(List<StrategyConfig> Configs) : IStrategyCommon
{
    public readonly StrategyValue[] Values = [.. Configs.Select(c => c.CreateEmpty())];

    // unfortunately, c# doesn't support partial type inference, and forcing user to spell out track enum twice is obnoxious, so here's the hopefully cheap solution
    public readonly struct OptionRef(StrategyConfigTrack config, StrategyValueTrack value)
    {
        public readonly StrategyConfigTrack Config = config;
        public readonly StrategyValueTrack Value = value;

        public OptionType As<OptionType>() where OptionType : Enum
        {
            if (Config.OptionEnum != typeof(OptionType))
                throw new ArgumentException($"Unexpected option type for {Config.InternalName}: expected {Config.OptionEnum.FullName}, got {typeof(OptionType).FullName}");
            return (OptionType)(object)Value.Option;
        }

        public float Priority(float defaultPrio) => float.IsNaN(Value.PriorityOverride) ? defaultPrio : Value.PriorityOverride;
        public float Priority() => Priority(Config.Options[Value.Option].DefaultPriority);
    }

    public readonly OptionRef Option<TrackIndex>(TrackIndex index) where TrackIndex : Enum
    {
        var idx = (int)(object)index;
        if (Configs[idx] is StrategyConfigTrack c)
            return new(c, (StrategyValueTrack)Values[idx]);
        throw new ArgumentException($"wrong type for strategy option: got {Configs[idx].GetType()}/{Values[idx].GetType()}, expected Track type");
    }

    public float GetFloat<TrackIndex>(TrackIndex index) where TrackIndex : Enum
    {
        var idx = (int)(object)index;
        if (Configs[idx] is StrategyConfigFloat)
            return ((StrategyValueFloat)Values[idx]).Value;
        throw new ArgumentException($"wrong type for strategy option: got {Configs[idx].GetType()}/{Values[idx].GetType()}, expected Float type");
    }

    public long GetInt<TrackIndex>(TrackIndex index) where TrackIndex : Enum
    {
        var idx = (int)(object)index;
        if (Configs[idx] is StrategyConfigInt)
            return ((StrategyValueInt)Values[idx]).Value;
        throw new ArgumentException($"wrong type for strategy option: got {Configs[idx].GetType()}/{Values[idx].GetType()}, expected Int type");
    }

    public Targeting Targeting => Option(SharedTrack.Targeting).As<Targeting>();
    public AOEStrategy AOE => Option(SharedTrack.AOE).As<AOEStrategy>();
}

public record struct Track<T>(T Value, StrategyValue Raw, float DefaultPriority) where T : struct
{
    public readonly float ExpireIn => Raw.ExpireIn;
    public readonly StrategyValueTrack TrackRaw => (StrategyValueTrack)Raw;

    public readonly float Priority(float defaultPrio) => float.IsNaN(TrackRaw.PriorityOverride) ? defaultPrio : TrackRaw.PriorityOverride;
    public readonly float Priority() => Priority(DefaultPriority);

    public static implicit operator T(Track<T> self) => self.Value;

    public readonly Track<U> Cast<U>() where U : struct => new((U)(object)Value, Raw, DefaultPriority);

    public override readonly string ToString() => $"Track({Value}, Raw={Raw})";
}

static class ValueConverter
{
    public static T FromValues<T>(StrategyValues values) where T : struct
    {
        object val = default(T);

        var i = 0;
        foreach (var field in typeof(T).GetFields())
        {
            switch (values.Values[i])
            {
                case StrategyValueTrack t:
                    field.SetValue(val, Activator.CreateInstance(field.FieldType, [Enum.ToObject(field.FieldType.GenericTypeArguments[0], t.Option), t, ((StrategyConfigTrack)values.Configs[i]).Options[t.Option].DefaultPriority]));
                    break;
                case StrategyValueFloat f:
                    field.SetValue(val, new Track<float>(f.Value, f, float.NaN));
                    break;
                case StrategyValueInt i2:
                    field.SetValue(val, new Track<long>(i2.Value, i2, float.NaN));
                    break;
            }
            i++;
        }

        return (T)val;
    }
}
