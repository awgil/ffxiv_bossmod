namespace BossMod;

public static class BossModuleInfo
{
    public enum Maturity
    {
        [PropertyDisplay("Work in progress; might be incomplete or have severe bugs")]
        WIP,

        [PropertyDisplay("Third-party contributed module that wasn't verified by the plugin author; might be working fine, might have any sorts of inconsistencies with other modules - YMMV")]
        Contributed,

        [PropertyDisplay("First-party module created by the plugin author, or a third-party contributed module that was thoroughly verified and effectively taken over by the plugin author")]
        Verified
    }

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
        Foray,
        Criterion,
        DeepDungeon,
        FATE,
        Hunt,
        Quest,
        TreasureHunt,
        PVP,
        MaskedCarnivale,
        GoldSaucer,

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
public sealed class ModuleInfoAttribute(BossModuleInfo.Maturity maturity) : Attribute
{
    public Type? StatesType { get; set; } // default: ns.xxxStates
    public Type? ConfigType { get; set; } // default: ns.xxxConfig
    public Type? ObjectIDType { get; set; } // default: ns.OID
    public Type? ActionIDType { get; set; } // default: ns.AID
    public Type? StatusIDType { get; set; } // default: ns.SID
    public Type? TetherIDType { get; set; } // default: ns.TetherID
    public Type? IconIDType { get; set; } // default: ns.IconID
    public uint PrimaryActorOID { get; set; } // default: OID.Boss
    public BossModuleInfo.Maturity Maturity { get; } = maturity;
    public string Contributors { get; set; } = "";
    public BossModuleInfo.Expansion Expansion { get; set; } = BossModuleInfo.Expansion.Count; // default: second namespace level
    public BossModuleInfo.Category Category { get; set; } = BossModuleInfo.Category.Count; // default: third namespace level
    public BossModuleInfo.GroupType GroupType { get; set; } = BossModuleInfo.GroupType.None;
    public uint GroupID { get; set; }
    public uint NameID { get; set; } // usually BNpcName row, unless GroupType uses it differently
    public int SortOrder { get; set; } // default: first number in type name
    public int PlanLevel { get; set; } // if > 0, module supports plans for this level
}
