namespace BossMod.Autorotation;

// target selection strategies; there is an extra int parameter that targets can use for storing more info
public enum StrategyTarget
{
    Automatic, // default 'smart' targeting, for hostile actions usually defaults to current primary target
    Self,
    PartyByAssignment, // parameter is assignment; won't work if assignments aren't set up properly for a party
    PartyWithLowestHP, // parameter is whether self is allowed (1) or not (0)
    EnemyWithHighestPriority, // selects closest if there are multiple
    EnemyByOID, // parameter is oid; not really useful outside planner; selects closest if there are multiple

    Count
}

// the tuning knobs of the rotation module are represented by strategy config rather than usual global config classes, since we they need to be changed dynamically by planner or manual input
public record class StrategyConfig(
    Type OptionEnum, // type of the enum used for options
    string InternalName, // unique name of the config; it is used for serialization, so it can't really be changed without losing user data (or writing config converter)
    string DisplayName, // if non-empty, this name is used for all UI instead of internal name
    float UIPriority) // tracks are sorted by UI priority for display; negative are hidden by default
{
    public readonly List<StrategyOption> Options = [];
    public readonly List<ActionID> AssociatedActions = []; // these actions will be shown on the track in the planner ui

    public string UIName => DisplayName.Length > 0 ? DisplayName : InternalName;

    public StrategyOption AddOption<Index>(Index expectedIndex, StrategyOption option) where Index : Enum
    {
        if (typeof(Index) != OptionEnum)
            throw new ArgumentException($"Unexpected index type for {option.InternalName}: expected {OptionEnum.FullName}, got {typeof(Index).FullName}");
        if (Options.Count != (int)(object)expectedIndex)
            throw new ArgumentException($"Unexpected index value for {option.InternalName}: expected {expectedIndex} ({(int)(object)expectedIndex}), got {Options.Count}");
        Options.Add(option);
        return option;
    }

    public void AddAssociatedAction<AID>(AID aid) where AID : Enum => AssociatedActions.Add(ActionID.MakeSpell(aid));
    public void AddAssociatedActions<AID>(params AID[] aids) where AID : Enum
    {
        foreach (var aid in aids)
            AddAssociatedAction(aid);
    }
}

// each strategy config has a unique set of allowed options; each option has a set of properties describing how it is rendered in planner and what further configuration parameters it supports
// note: first option (with index 0) should correspond to the default automatic behaviour; second option (with index 1) should correspond to most often used override (it's selected by default when adding override)
public record class StrategyOption(
    uint Color, // color used in the UI to present this option; doesn't have to be unique, but it helps with ux...
    ActionTargets SupportedTargets, // valid targets for relevant action; used to filter target options for values
    string InternalName, // unique name of the option; it is used for serialization, so it can't really be changed without losing user data (or writing config converter)
    string DisplayName = "", // if non-empty, this name is used for all UI instead of internal name
    float Cooldown = 0, // if > 0, this time after window end is shaded to notify user about associated action cooldown
    float Effect = 0, // if > 0, this time after window start is shaded to notify user about associated effect duration
    int MinLevel = 1, // min character level for this option to be available
    int MaxLevel = int.MaxValue, // max character level for this option to be available
    float DefaultPriority = ActionQueue.Priority.Low) // default priority that is used if no override is defined
{
    public string UIName => DisplayName.Length > 0 ? DisplayName : InternalName;
}

// value represents the concrete option of a config that is selected at a given time; it can be either put on the planner timeline, or configured as part of manual overrides
public record struct StrategyValue()
{
    public int Option; // index of the selected option among the Options list of the corresponding config
    public float PriorityOverride = float.NaN; // priority override for the action controlled by the config; not all configs support it, if not set the default priority is used
    public StrategyTarget Target; // target selection strategy
    public int TargetParam; // strategy-specific parameter
    public string Comment = ""; // user-editable comment string
}

public record struct StrategyValues(List<StrategyConfig> Configs)
{
    public StrategyValue[] Values = Utils.MakeArray(Configs.Count, new StrategyValue());

    // unfortunately, c# doesn't support partial type inference, and forcing user to spell out track enum twice is obnoxious, so here's the hopefully cheap solution
    public readonly ref struct OptionRef(ref StrategyConfig config, ref StrategyValue value)
    {
        public readonly ref readonly StrategyConfig Config = ref config;
        public readonly ref readonly StrategyValue Value = ref value;

        public OptionType As<OptionType>() where OptionType : Enum
        {
            if (Config.OptionEnum != typeof(OptionType))
                throw new ArgumentException($"Unexpected option type for {Config.InternalName}: expected {Config.OptionEnum.FullName}, got {typeof(OptionType).FullName}");
            return (OptionType)(object)Value.Option;
        }

        public float Priority() => float.IsNaN(Value.PriorityOverride) ? Config.Options[Value.Option].DefaultPriority : Value.PriorityOverride;
    }

    public readonly OptionRef Option<TrackIndex>(TrackIndex index) where TrackIndex : Enum
    {
        var idx = (int)(object)index;
        return new(ref Configs.Ref(idx), ref Values[idx]);
    }
}
