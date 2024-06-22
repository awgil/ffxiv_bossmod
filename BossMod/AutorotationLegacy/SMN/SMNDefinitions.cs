namespace BossMod.SMN;

public enum CDGroup : int
{
    Fester = 0, // 1.0 max
    Painflare = 1, // 1.0 max
    MountainBuster = 2, // 1.0 max
    Enkindle = 3, // 20.0 max, shared by Enkindle Bahamut, Enkindle Phoenix
    Rekindle = 4, // 20.0 max
    Deathflare = 5, // 20.0 max
    Aethercharge = 6, // 60.0 max, shared by Aethercharge, Dreadwyrm Trance, Summon Bahamut, Summon Phoenix
    EnergyDrain = 7, // 60.0 max, shared by Energy Drain, Energy Siphon
    SearingLight = 19, // 120.0 max
    RadiantAegis = 20, // 60.0 max
    Swiftcast = 44, // 60.0 max
    LucidDreaming = 45, // 60.0 max
    Addle = 46, // 90.0 max
    Surecast = 48, // 120.0 max
}

// TODO: regenerate
public enum SID : uint
{
    None = 0,
    Addle = 1203, // applied by Addle to target, -5% phys and -10% magic damage dealt
    LucidDreaming = 1204, // applied by Lucid Dreaming to self, MP restore
    Swiftcast = 167, // applied by Swiftcast to self, next cast is instant
    Sleep = 3, // applied by Sleep to target
}
