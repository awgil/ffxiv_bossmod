namespace BossMod.SCH;

public enum AID : uint
{
    None = 0,
    Sprint = 3,

    // single-target damage GCDs
    Ruin1 = 17869, // L1, 1.5s cast, range 25, single-target 0/0, targets=hostile
    Broil1 = 3584, // L54, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    Broil2 = 7435, // L64, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    Broil3 = 16541, // L72, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    Broil4 = 25865, // L82, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    Bio1 = 17864, // L2, instant, range 25, single-target 0/0, targets=hostile
    Bio2 = 17865, // L26, instant, range 25, single-target 0/0, targets=hostile
    Biolysis = 16540, // L72, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    Ruin2 = 17870, // L38, instant, range 25, single-target 0/0, targets=hostile

    // aoe damage GCDs
    ArtOfWar1 = 16539, // L46, instant, range 0, AOE circle 5/0, targets=self
    ArtOfWar2 = 25866, // L82, instant, range 0, AOE circle 5/0, targets=self, animLock=???

    // single-target heal GCDs
    Physick = 190, // L4, 1.5s cast, range 30, single-target 0/0, targets=self/party/friendly
    Adloquium = 185, // L30, 2.0s cast, range 30, single-target 0/0, targets=self/party/friendly

    // aoe heal GCDs
    Succor = 186, // L35, 2.0s cast, range 0, AOE circle 15/0, targets=self

    // summons
    SummonEos = 17215, // L4, 1.5s cast, range 0, single-target 0/0, targets=self
    SummonSelene = 17216, // L4, 1.5s cast, range 0, single-target 0/0, targets=self

    // damage oGCDs
    EnergyDrain = 167, // L45, instant, 1.0s CD (group 3), range 25, single-target 0/0, targets=hostile

    // heal oGCDs
    WhisperingDawn = 16537, // L20, instant, 60.0s CD (group 13), range 0, single-target 0/0, targets=self
    Lustrate = 189, // L45, instant, 1.0s CD (group 0), range 30, single-target 0/0, targets=self/party/friendly
    SacredSoil = 188, // L50, instant, 30.0s CD (group 5), range 30, Ground circle 10/0, targets=area, animLock=???
    Indomitability = 3583, // L52, instant, 30.0s CD (group 6), range 0, AOE circle 15/0, targets=self, animLock=???
    DeploymentTactics = 3585, // L56, instant, 120.0s CD (group 19), range 30, AOE circle 15/0, targets=self/party, animLock=???
    EmergencyTactics = 3586, // L58, instant, 15.0s CD (group 4), range 0, single-target 0/0, targets=self, animLock=???
    Excogitation = 7434, // L62, instant, 45.0s CD (group 8), range 30, single-target 0/0, targets=self/party, animLock=???
    Aetherpact = 7437, // L70, instant, 3.0s CD (group 2), range 30, single-target 0/0, targets=self/party, animLock=???
    DissolveUnion = 7869, // L70, instant, 1.0s CD (group 1), range 0, single-target 0/0, targets=self, animLock=???
    FeyBlessing = 16543, // L76, instant, 60.0s CD (group 10), range 0, single-target 0/0, targets=self, animLock=???
    Consolation = 16546, // L80, instant, 30.0s CD (group 9) (2 charges), range 0, single-target 0/0, targets=self, animLock=???

    // buff CDs
    LucidDreaming = 7562, // L14, instant, 60.0s CD (group 41), range 0, single-target 0/0, targets=self
    Swiftcast = 7561, // L18, instant, 60.0s CD (group 42), range 0, single-target 0/0, targets=self
    FeyIllumination = 16538, // L40, instant, 120.0s CD (group 21), range 0, single-target 0/0, targets=self
    Surecast = 7559, // L44, instant, 120.0s CD (group 43), range 0, single-target 0/0, targets=self
    Aetherflow = 166, // L45, instant, 60.0s CD (group 12), range 0, single-target 0/0, targets=self
    Dissipation = 3587, // L60, instant, 180.0s CD (group 24), range 0, single-target 0/0, targets=self, animLock=???
    ChainStratagem = 7436, // L66, instant, 120.0s CD (group 20), range 25, single-target 0/0, targets=hostile, animLock=???
    Recitation = 16542, // L74, instant, 90.0s CD (group 14), range 0, single-target 0/0, targets=self, animLock=???
    SummonSeraph = 16545, // L80, instant, 120.0s CD (group 22), range 0, single-target 0/0, targets=self, animLock=???
    Protraction = 25867, // L86, instant, 60.0s CD (group 11), range 30, single-target 0/0, targets=self/party, animLock=???
    Expedient = 25868, // L90, instant, 120.0s CD (group 23), range 0, AOE circle 15/0, targets=self, animLock=???

    // misc
    Resurrection = 173, // L12, 8.0s cast, range 30, single-target 0/0, targets=party/friendly, animLock=???
    Repose = 16560, // L8, 2.5s cast, range 30, single-target 0/0, targets=hostile
    Esuna = 7568, // L10, 1.0s cast, range 30, single-target 0/0, targets=self/party/friendly
    Rescue = 7571, // L48, instant, 120.0s CD (group 45), range 30, single-target 0/0, targets=party, animLock=???

    // pet abilities
    PetEmbrace = 802, // L1, instant, 3.0s CD (group 76), range 30, single-target 0/0, targets=self/party/friendly, animLock=???
    PetSeraphicVeil = 16548, // L80, instant, 3.0s CD (group 76), range 30, single-target 0/0, targets=self/party, animLock=???
    PetWhisperingDawn = 803, // L20, instant, 0.0s CD (group -1), range 0, AOE circle 15/0, targets=self, animLock=???
    PetAngelsWhisper = 16550, // L80, instant, 0.0s CD (group -1), range 0, AOE circle 15/0, targets=self, animLock=???
    PetFeyIllumination = 805, // L40, instant, 0.0s CD (group -1), range 0, AOE circle 15/0, targets=self, animLock=???
    PetSeraphicIllumination = 16551, // L80, instant, 0.0s CD (group -1), range 0, AOE circle 15/0, targets=self, animLock=???
    PetFeyUnion = 7438, // L70, instant, 0.0s CD (group -1), range 30, single-target 0/0, targets=party, animLock=???
    PetFeyBlessing = 16544, // L76, instant, 0.0s CD (group -1), range 0, AOE circle 20/0, targets=self, animLock=???
    PetConsolation = 16547, // L80, instant, 0.0s CD (group -1), range 0, AOE circle 20/0, targets=self, animLock=???
}

public enum TraitID : uint
{
    None = 0,
    MaimAndMend1 = 66, // L20, damage & healing increase
    CorruptionMastery1 = 324, // L26, bio1 -> bio2 upgrade
    MaimAndMend2 = 69, // L40, damage & healing increase
    BroilMastery1 = 214, // L54, ruin1 -> broil1 upgrade, potency increase
    BroilMastery2 = 184, // L64, broil1 -> broil2 upgrade, potency increase
    CorruptionMastery2 = 311, // L72, bio2 -> biolysis upgrade
    BroilMastery3 = 312, // L72, broil2 -> broil3 upgrade, potency increase
    EnhancedSacredSoil = 313, // L78, adds hot
    BroilMastery4 = 491, // L82, broil3 -> broil4 upgrade, potency increase
    ArtOfWarMastery = 492, // L82, art of war 1 -> art of war 2 upgrade
    EnhancedHealingMagic = 493, // L85, potency increase
    EnhancedDeploymentTactics = 494, // L88, reduce cd
}

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

public static class Definitions
{
    public static readonly uint[] UnlockQuests = [66633, 66634, 66637, 66638, 67208, 67209, 67210, 67211, 67212, 68463];

    public static bool Unlocked(AID aid, int level, int questProgress)
    {
        return aid switch
        {
            AID.Bio1 => level >= 2,
            AID.SummonEos => level >= 4,
            AID.SummonSelene => level >= 4,
            AID.Physick => level >= 4,
            AID.Repose => level >= 8,
            AID.Esuna => level >= 10,
            AID.Resurrection => level >= 12,
            AID.LucidDreaming => level >= 14,
            AID.Swiftcast => level >= 18,
            AID.WhisperingDawn => level >= 20,
            AID.PetWhisperingDawn => level >= 20,
            AID.Bio2 => level >= 26,
            AID.Adloquium => level >= 30 && questProgress > 0,
            AID.Succor => level >= 35 && questProgress > 1,
            AID.Ruin2 => level >= 38,
            AID.PetFeyIllumination => level >= 40,
            AID.FeyIllumination => level >= 40,
            AID.Surecast => level >= 44,
            AID.Lustrate => level >= 45 && questProgress > 2,
            AID.EnergyDrain => level >= 45,
            AID.Aetherflow => level >= 45,
            AID.ArtOfWar1 => level >= 46,
            AID.Rescue => level >= 48,
            AID.SacredSoil => level >= 50 && questProgress > 3,
            AID.Indomitability => level >= 52 && questProgress > 4,
            AID.Broil1 => level >= 54 && questProgress > 5,
            AID.DeploymentTactics => level >= 56 && questProgress > 6,
            AID.EmergencyTactics => level >= 58 && questProgress > 7,
            AID.Dissipation => level >= 60 && questProgress > 8,
            AID.Excogitation => level >= 62,
            AID.Broil2 => level >= 64,
            AID.ChainStratagem => level >= 66,
            AID.DissolveUnion => level >= 70 && questProgress > 9,
            AID.Aetherpact => level >= 70 && questProgress > 9,
            AID.PetFeyUnion => level >= 70 && questProgress > 9,
            AID.Biolysis => level >= 72,
            AID.Broil3 => level >= 72,
            AID.Recitation => level >= 74,
            AID.FeyBlessing => level >= 76,
            AID.PetFeyBlessing => level >= 76,
            AID.PetSeraphicVeil => level >= 80,
            AID.PetConsolation => level >= 80,
            AID.PetAngelsWhisper => level >= 80,
            AID.Consolation => level >= 80,
            AID.SummonSeraph => level >= 80,
            AID.PetSeraphicIllumination => level >= 80,
            AID.Broil4 => level >= 82,
            AID.ArtOfWar2 => level >= 82,
            AID.Protraction => level >= 86,
            AID.Expedient => level >= 90,
            _ => true,
        };
    }

    public static bool Unlocked(TraitID tid, int level, int questProgress)
    {
        return tid switch
        {
            TraitID.MaimAndMend1 => level >= 20,
            TraitID.CorruptionMastery1 => level >= 26,
            TraitID.MaimAndMend2 => level >= 40,
            TraitID.BroilMastery1 => level >= 54 && questProgress > 5,
            TraitID.BroilMastery2 => level >= 64,
            TraitID.CorruptionMastery2 => level >= 72,
            TraitID.BroilMastery3 => level >= 72,
            TraitID.EnhancedSacredSoil => level >= 78,
            TraitID.BroilMastery4 => level >= 82,
            TraitID.ArtOfWarMastery => level >= 82,
            TraitID.EnhancedHealingMagic => level >= 85,
            TraitID.EnhancedDeploymentTactics => level >= 88,
            _ => true,
        };
    }

    public static readonly Dictionary<ActionID, ActionDefinition> SupportedActions = BuildSupportedActions();
    private static Dictionary<ActionID, ActionDefinition> BuildSupportedActions()
    {
        var res = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionMnd);
        res.GCDCast(AID.Ruin1, 25, 1.5f);
        res.GCDCast(AID.Broil1, 25, 1.5f);
        res.GCDCast(AID.Broil2, 25, 1.5f);
        res.GCDCast(AID.Broil3, 25, 1.5f);
        res.GCDCast(AID.Broil4, 25, 1.5f);
        res.GCD(AID.Bio1, 25);
        res.GCD(AID.Bio2, 25);
        res.GCD(AID.Biolysis, 25);
        res.GCD(AID.Ruin2, 25);
        res.GCD(AID.ArtOfWar1, 0);
        res.GCD(AID.ArtOfWar2, 0);
        res.GCDCast(AID.Physick, 30, 1.5f);
        res.GCDCast(AID.Adloquium, 30, 2.0f);
        res.GCDCast(AID.Succor, 0, 2.0f);
        res.GCDCast(AID.SummonEos, 0, 1.5f);
        res.GCDCast(AID.SummonSelene, 0, 1.5f);
        res.OGCD(AID.EnergyDrain, 25, CDGroup.EnergyDrain, 1.0f);
        res.OGCD(AID.WhisperingDawn, 0, CDGroup.WhisperingDawn, 60.0f);
        res.OGCD(AID.Lustrate, 30, CDGroup.Lustrate, 1.0f);
        res.OGCD(AID.SacredSoil, 30, CDGroup.SacredSoil, 30.0f);
        res.OGCD(AID.Indomitability, 0, CDGroup.Indomitability, 30.0f);
        res.OGCD(AID.DeploymentTactics, 30, CDGroup.DeploymentTactics, 120.0f);
        res.OGCD(AID.EmergencyTactics, 0, CDGroup.EmergencyTactics, 15.0f);
        res.OGCD(AID.Excogitation, 30, CDGroup.Excogitation, 45.0f);
        res.OGCD(AID.Aetherpact, 30, CDGroup.Aetherpact, 3.0f);
        res.OGCD(AID.DissolveUnion, 0, CDGroup.DissolveUnion, 1.0f);
        res.OGCD(AID.FeyBlessing, 0, CDGroup.FeyBlessing, 60.0f);
        res.OGCDWithCharges(AID.Consolation, 0, CDGroup.Consolation, 30.0f, 2);
        res.OGCD(AID.LucidDreaming, 0, CDGroup.LucidDreaming, 60.0f);
        res.OGCD(AID.Swiftcast, 0, CDGroup.Swiftcast, 60.0f);
        res.OGCD(AID.FeyIllumination, 0, CDGroup.FeyIllumination, 120.0f);
        res.OGCD(AID.Surecast, 0, CDGroup.Surecast, 120.0f);
        res.OGCD(AID.Aetherflow, 0, CDGroup.Aetherflow, 60.0f);
        res.OGCD(AID.Dissipation, 0, CDGroup.Dissipation, 180.0f);
        res.OGCD(AID.ChainStratagem, 25, CDGroup.ChainStratagem, 120.0f);
        res.OGCD(AID.Recitation, 0, CDGroup.Recitation, 90.0f);
        res.OGCD(AID.SummonSeraph, 0, CDGroup.SummonSeraph, 120.0f);
        res.OGCD(AID.Protraction, 30, CDGroup.Protraction, 60.0f);
        res.OGCD(AID.Expedient, 0, CDGroup.Expedient, 120.0f);
        res.GCDCast(AID.Resurrection, 30, 8.0f);
        res.GCDCast(AID.Repose, 30, 2.5f);
        res.GCDCast(AID.Esuna, 30, 1.0f);
        res.OGCD(AID.Rescue, 30, CDGroup.Rescue, 120.0f);
        return res;
    }
}
