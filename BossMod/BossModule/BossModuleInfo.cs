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
        Event,

        Count
    }
}

// attribute that allows customizing boss module's metadata; it is optional, each field has some defaults that are fine in most cases
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ModuleInfoAttribute : Attribute
{
    public Type? StatesType; // default: ns.xxxStates
    public Type? ConfigType; // default: ns.xxxConfig
    public Type? ObjectIDType; // default: ns.OID
    public Type? ActionIDType; // default: ns.AID
    public Type? StatusIDType; // default: ns.SID
    public Type? TetherIDType; // default: ns.TetherID
    public Type? IconIDType; // default: ns.IconID
    public uint PrimaryActorOID; // default: OID.Boss
    public string? DisplayName;
    public BossModuleInfo.Expansion Expansion = BossModuleInfo.Expansion.Count; // default: second namespace level
    public BossModuleInfo.Category Category = BossModuleInfo.Category.Count; // default: third namespace level
    public uint QuestID; // default: 0
    public uint DynamicEventID; // default: 0
    public uint FateID; // default: 0
    public uint NotoriousMonsterID; // default: 0
    public uint NameID; // default: 0
    public uint CFCID; // default: 0
}
