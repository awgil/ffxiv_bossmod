namespace BossMod;

public static class BossModuleInfo
{
    public enum Expansion
    {
        RealmReborn,
        Heavensward,
        Stormblood,
        Shadowbringers,
        Endwalker,
        Dawntrail,
        Global,

        Count
    }

    public enum Category
    {
        Uncategorized,
        Dungeon,
        Trial,
        Extreme,
        Raid,
        Savage,
        Ultimate,
        Unreal,
        Alliance,
        Chaotic,
        Foray,
        Variant,
        Criterion,
        DeepDungeon,
        FATE,
        Hunt,
        Quest,
        TreasureHunt,
        PVP,
        MaskedCarnivale,
        GoldSaucer,
        Quantum,

        Count
    }

    public enum GroupType
    {
        None,
        CFC, // group id is ContentFinderCondition row
        MaskedCarnivale, // group id is ContentFinderCondition row
        RemovedUnreal, // group id is ContentFinderCondition row
        Quest, // group id is Quest row
        Fate, // group id is Fate row
        Hunt, // group id is HuntRank
        BozjaCE, // group id is ContentFinderCondition row, name id is DynamicEvent row
        BozjaDuel, // group id is ContentFinderCondition row, name id is DynamicEvent row
        EurekaNM, // group id is ContentFinderCondition row, name id is Fate row
        GoldSaucer, // group id is GoldSaucerTextData row
    }

    public enum HuntRank : uint { B, A, S, SS }

    // shorthand expansion names
    public static string ShortName(this Expansion e) => e switch
    {
        Expansion.RealmReborn => "ARR",
        Expansion.Heavensward => "HW",
        Expansion.Stormblood => "SB",
        Expansion.Shadowbringers => "ShB",
        Expansion.Endwalker => "EW",
        Expansion.Dawntrail => "DT",
        _ => e.ToString()
    };
}

// attribute that allows customizing boss module's metadata; it is optional, each field has some defaults that are fine in most cases
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ModuleInfoAttribute() : Attribute
{
    public Type? StatesType { get; set; } // default: ns.xxxStates
    public Type? ConfigType { get; set; } // default: ns.xxxConfig
    public Type? ObjectIDType { get; set; } // default: ns.OID
    public Type? ActionIDType { get; set; } // default: ns.AID
    public Type? StatusIDType { get; set; } // default: ns.SID
    public Type? TetherIDType { get; set; } // default: ns.TetherID
    public Type? IconIDType { get; set; } // default: ns.IconID
    public uint PrimaryActorOID { get; set; } // default: OID.Boss
    public string Contributors { get; set; } = "";
    public BossModuleInfo.Expansion Expansion { get; set; } = BossModuleInfo.Expansion.Count; // default: second namespace level
    public BossModuleInfo.Category Category { get; set; } = BossModuleInfo.Category.Count; // default: third namespace level
    public BossModuleInfo.GroupType GroupType { get; set; } = BossModuleInfo.GroupType.None;
    public uint GroupID { get; set; }
    public uint NameID { get; set; } // usually BNpcName row, unless GroupType uses it differently
    public int SortOrder { get; set; } // default: first number in type name
    public int PlanLevel { get; set; } // if > 0, module supports plans for this level
    public bool Incomplete { get; set; } // user needs to opt-in to load these modules, they probably don't work (but could be useful for prog, e.g. new ultimates)
}
