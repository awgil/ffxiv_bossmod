using BossMod.Autorotation;
using System.Text.Json.Serialization;

namespace BossMod.ActionTweaks.CustomCombos;

// note: the numbers are serialized, so don't change them
public enum CustomComboConditionType
{
    // action
    ActionUsable = 0, // unlocked, ready and game reports it as usable
    ActionReady = 1, // action is off cooldown (or will be within Value seconds), gcd is ignored
    ActionCharges = 2, // action has at least Count charges
    UsedRecently = 3, // action ID was used within the last Value seconds

    // combo & timing
    LastComboAction = 4, // game combo state says the last combo action was ID
    ComboTimeRemaining = 5, // game combo timer compared to Value seconds
    CanWeave = 6, // an oGCD taking Value seconds fits before the next GCD
    CanLateWeave = 7, // we're in the second half of the GCD window, but an oGCD still fits
    CombatTime = 8, // time since combat start compared to Value seconds

    // self
    StatusOnSelf = 9, // status ID with more than Value seconds left and at least Count stacks
    PlayerHP = 10, // player HP percent compared to Value
    PlayerMP = 11, // player MP compared to Value
    PlayerLevel = 12, // player level is at least Value
    InCombat = 13,
    Moving = 14,
    PetPresent = 15,

    // gauge
    Gauge = 16, // job gauge Field compared to Value

    // target
    HasTarget = 17,
    StatusOnTarget = 18, // same as StatusOnSelf, but on current target
    TargetHP = 19, // target HP percent compared to Value
    TargetInActionRange = 20, // current target is in range of action ID
    TargetDistance = 21, // distance to target hitbox compared to Value
    Positional = 22, // player is at target's Positional (rear/flank/front)

    // enemies & encounter
    EnemiesNearPlayer = 23, // at least Count enemies within Value yalms of the player
    EnemiesNearTarget = 24, // at least Count enemies within Value yalms of the current target
    InBossFight = 25, // module is active
    RaidwideIncoming = 26, // predicted raidwide damage on player within Value seconds
    TankbusterIncoming = 27, // predicted tankbuster on player within Value seconds

    // party
    PartyMembersBelowHP = 28, // at least Count party members (including player) are below Value percent HP
    PartyAverageHP = 29, // average HP percent of party compared to Value
}

public enum CustomComboCompare { Less, LessEqual, Equal, GreaterEqual, Greater }

public enum CustomComboGroupMode
{
    None, // not a group (leaf)
    All,
    Any,
}

// leaf condition (Group == None) or group of child conditions
public sealed class CustomComboCondition
{
    public CustomComboGroupMode Group;
    public List<CustomComboCondition> Children = [];

    [JsonConverter(typeof(JsonNumberEnumConverter<CustomComboConditionType>))] // overrides the global string enum converter
    public CustomComboConditionType Type;
    public bool Negate;
    public uint ID; // action or status id, depending on type
    public float Value; // seconds, percent, level, radius or gauge value, depending on type
    public int Count; // charges, stacks or count, depending on type
    public bool OnlySelf = true; // for status conditions: only consider statuses applied by the player
    public CustomComboCompare Compare = CustomComboCompare.GreaterEqual;
    public string Field = ""; // for gauge conditions: gauge field key
    public Positional Positional = Positional.Rear;
}

public sealed class CustomComboStep
{
    public bool Enabled = true;
    public uint Action;
    public CustomComboGroupMode Match = CustomComboGroupMode.All; // how top-level conditions are combined
    public List<CustomComboCondition> Conditions = [];
}

// trigger action is replaced by the first enabled step whose conditions pass, otherwise by the next action of the sequence, otherwise left as is
public sealed class CustomCombo
{
    public string Name = "";
    public bool Enabled = true;
    public Class Job = Class.None; // None = any class/job
    public uint Trigger;
    public List<CustomComboStep> Steps = []; // checked top to bottom before the sequence
    public List<uint> Sequence = []; // combo chain in execution order, e.g. mch's split shot -> slug shot -> clean shot
}

[ConfigDisplay(Name = "Custom combos", Parent = typeof(ActionTweaksConfig), Order = -5)]
public sealed class CustomComboConfig : ConfigNode
{
    [PropertyDisplay("Enable custom combos")]
    public bool Enabled = false;

    public string AutorotationPreset = ""; // for autorotation suggestions; empty = first preset for the class

    public List<CustomCombo> Combos = [];
    public HashSet<string> Presets = []; // keys of enabled presets
    public ComboHotbarsConfig Hotbars = new();

    [JsonIgnore]
    private readonly ComboHotbarsEditor _hotbarsEditor = new();

    public override void DrawCustom(string? page, UITree tree, WorldState ws, RotationDatabase? db)
    {
        _hotbarsEditor.Draw(this);
    }
}
