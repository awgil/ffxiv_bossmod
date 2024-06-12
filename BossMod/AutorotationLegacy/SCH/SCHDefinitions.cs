namespace BossMod.SCH;

public enum CDGroup : int
{
    Lustrate = 0, // 1.0 max
    DissolveUnion = 1, // 1.0 max
    Aetherpact = 2, // 3.0 max
    EnergyDrain = 3, // 1.0 max
    EmergencyTactics = 4, // 15.0 max
    SacredSoil = 5, // 30.0 max
    Indomitability = 6, // 30.0 max
    Excogitation = 8, // 45.0 max
    Consolation = 9, // 2*30.0 max
    FeyBlessing = 10, // 60.0 max
    Protraction = 11, // 60.0 max
    Aetherflow = 12, // 60.0 max
    WhisperingDawn = 13, // 60.0 max
    Recitation = 14, // 90.0 max
    DeploymentTactics = 19, // 120.0 max
    ChainStratagem = 20, // 120.0 max
    FeyIllumination = 21, // 120.0 max
    SummonSeraph = 22, // 120.0 max
    Expedient = 23, // 120.0 max
    Dissipation = 24, // 180.0 max
    Swiftcast = 44, // 60.0 max
    LucidDreaming = 45, // 60.0 max
    Surecast = 48, // 120.0 max
    Rescue = 49, // 120.0 max
    PetEmbrace = 76, // 3.0 max, shared by Embrace, Seraphic Veil
}

public enum SID : uint
{
    None = 0,
    Bio1 = 179, // applied by Bio1 to target, dot
    Bio2 = 189, // applied by Bio2 to target, dot
    Biolysis = 0xFFFFFF, // TODO!
    Galvanize = 297, // applied by Adloquium to target, shield
    LucidDreaming = 1204, // applied by Lucid Dreaming to self
    Swiftcast = 167, // applied by Swiftcast to self
    Sleep = 3, // applied by Repose to target
}
