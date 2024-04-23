namespace BossMod.SMN;

public enum AID : uint
{
    None = 0,
    Sprint = 3,

    // single target GCDs
    Ruin1 = 163, // L1, 1.5s cast, range 25, single-target 0/0, targets=hostile
    Ruin2 = 172, // L30, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    Ruin3 = 3579, // L54, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    Ruin4 = 7426, // L62, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    RubyRuin1 = 25808, // L6, 2.8s cast, range 25, single-target 0/0, targets=hostile
    RubyRuin2 = 25811, // L30, 2.8s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    RubyRuin3 = 25817, // L54, 2.8s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    RubyRite = 25823, // L72, 2.8s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    CrimsonCyclone = 25835, // L86, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    CrimsonStrike = 25885, // L86, instant, range 3, AOE circle 5/0, targets=hostile, animLock=???
    TopazRuin1 = 25809, // L15, instant, range 25, single-target 0/0, targets=hostile
    TopazRuin2 = 25812, // L30, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    TopazRuin3 = 25818, // L54, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    TopazRite = 25824, // L72, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    EmeraldRuin1 = 25810, // L22, instant, range 25, single-target 0/0, targets=hostile
    EmeraldRuin2 = 25813, // L30, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    EmeraldRuin3 = 25819, // L54, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    EmeraldRite = 25825, // L72, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    Slipstream = 25837, // L86, 3.0s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    AstralImpulse = 25820, // L58, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    FountainOfFire = 16514, // L80, instant, range 25, single-target 0/0, targets=hostile, animLock=???

    // aoe GCDs
    Outburst = 16511, // L26, 1.5s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    TriDisaster = 25826, // L74, 1.5s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    RubyOutburst = 25814, // L26, 2.8s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    RubyDisaster = 25827, // L74, 2.8s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    RubyCatastrophe = 25832, // L82, 2.8s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    TopazOutburst = 25815, // L26, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    TopazDisaster = 25828, // L74, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    TopazCatastrophe = 25833, // L82, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    EmeraldOutburst = 25816, // L26, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    EmeraldDisaster = 25829, // L74, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    EmeraldCatastrophe = 25834, // L82, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    AstralFlare = 25821, // L58, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    BrandOfPurgatory = 16515, // L80, instant, range 25, AOE circle 8/0, targets=hostile, animLock=???

    // attunement placeholders
    Gemshine = 25883, // L6, 2.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    PreciousBrilliance = 25884, // L26, 2.5s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    AstralFlow = 25822, // L60, instant, range 0, single-target 0/0, targets=self, animLock=???

    // summons / stances
    SummonCarbuncle = 25798, // L2, 1.5s cast, range 0, single-target 0/0, targets=self
    SummonRuby = 25802, // L6, instant, range 25, single-target 0/0, targets=hostile
    SummonIfrit1 = 25805, // L30, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    SummonIfrit2 = 25838, // L90, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    SummonTopaz = 25803, // L15, instant, range 25, single-target 0/0, targets=hostile
    SummonTitan1 = 25806, // L35, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    SummonTitan2 = 25839, // L90, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    SummonEmerald = 25804, // L22, instant, range 25, single-target 0/0, targets=hostile
    SummonGaruda1 = 25807, // L45, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    SummonGaruda2 = 25840, // L90, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    Aethercharge = 25800, // L6, instant, 60.0s CD (group 6), range 0, single-target 0/0, targets=self
    DreadwyrmTrance = 3581, // L58, instant, 60.0s CD (group 6), range 0, single-target 0/0, targets=self, animLock=???
    SummonBahamut = 7427, // L70, instant, 60.0s CD (group 6), range 25, single-target 0/0, targets=hostile, animLock=???
    SummonPhoenix = 25831, // L80, instant, 60.0s CD (group 6), range 25, single-target 0/0, targets=hostile, animLock=???

    // oGCDs
    EnergyDrain = 16508, // L10, instant, 60.0s CD (group 7), range 25, single-target 0/0, targets=hostile
    EnergySiphon = 16510, // L52, instant, 60.0s CD (group 7), range 25, AOE circle 5/0, targets=hostile, animLock=???
    Fester = 181, // L10, instant, 1.0s CD (group 0), range 25, single-target 0/0, targets=hostile
    Painflare = 3578, // L40, instant, 1.0s CD (group 1), range 25, AOE circle 5/0, targets=hostile, animLock=???
    Deathflare = 3582, // L60, instant, 20.0s CD (group 5), range 25, AOE circle 5/0, targets=hostile, animLock=???
    EnkindleBahamut = 7429, // L70, instant, 20.0s CD (group 3), range 25, single-target 0/0, targets=hostile, animLock=???
    EnkindlePhoenix = 16516, // L80, instant, 20.0s CD (group 3), range 25, single-target 0/0, targets=hostile, animLock=???
    MountainBuster = 25836, // L86, instant, 1.0s CD (group 2), range 25, AOE circle 5/0, targets=hostile, animLock=???

    // single-target heal GCDs
    Physick = 16230, // L4, 1.5s cast, range 30, single-target 0/0, targets=self/party/friendly

    // offsensive CDs
    SearingLight = 25801, // L66, instant, 120.0s CD (group 19), range 0, AOE circle 15/0, targets=self, animLock=???
    Swiftcast = 7561, // L18, instant, 60.0s CD (group 42), range 0, single-target 0/0, targets=self
    LucidDreaming = 7562, // L14, instant, 60.0s CD (group 41), range 0, single-target 0/0, targets=self

    // defensive CDs
    RadiantAegis = 25799, // L2, instant, 60.0s CD (group 20), range 0, single-target 0/0, targets=self
    Rekindle = 25830, // L80, instant, 20.0s CD (group 4), range 30, single-target 0/0, targets=self/party, animLock=???
    Addle = 7560, // L8, instant, 90.0s CD (group 40), range 25, single-target 0/0, targets=hostile
    Surecast = 7559, // L44, instant, 120.0s CD (group 43), range 0, single-target 0/0, targets=self, animLock=???

    // misc
    Resurrection = 173, // L12, 8.0s cast, range 30, single-target 0/0, targets=party/friendly, animLock=???
    Sleep = 25880, // L10, 2.5s cast, range 30, AOE circle 5/0, targets=hostile

    // pet abilities
    PetRadiantAegis = 25841, // L2, instant, 0.0s CD (group -1), range 30, single-target 0/0, targets=party, animLock=???
    PetWyrmwave = 7428, // L70, instant, 0.0s CD (group -1), range 50, single-target 0/0, targets=hostile, animLock=???
    PetAkhMorn = 7449, // L70, instant, 0.0s CD (group -1), range 50, AOE circle 5/0, targets=hostile, animLock=???
    PetEverlastingFlight = 16517, // L80, instant, 0.0s CD (group -1), range 0, AOE circle 15/0, targets=self, animLock=???
    PetRevelation = 16518, // L80, instant, 0.0s CD (group -1), range 50, AOE circle 5/0, targets=hostile, animLock=???
    PetGlitteringRuby = 25843, // L6, instant, 0.0s CD (group -1), range 3, single-target 0/0, targets=hostile, animLock=???
    PetGlitteringTopaz = 25844, // L15, instant, 0.0s CD (group -1), range 3, single-target 0/0, targets=hostile, animLock=???
    PetGlitteringEmerald = 25845, // L22, instant, 0.0s CD (group -1), range 25, single-target 0/0, targets=hostile, animLock=???
    PetBurningStrike = 25846, // L30, instant, 0.0s CD (group -1), range 3, single-target 0/0, targets=hostile, animLock=???
    PetRockBuster = 25847, // L35, instant, 0.0s CD (group -1), range 3, single-target 0/0, targets=hostile, animLock=???
    PetAerialSlash = 25848, // L45, instant, 0.0s CD (group -1), range 25, AOE circle 5/0, targets=hostile, animLock=???
    PetInferno1 = 25849, // L50, instant, 0.0s CD (group -1), range 5, AOE cone 5/0, targets=hostile, animLock=???
    PetEarthenFury1 = 25850, // L50, instant, 0.0s CD (group -1), range 0, AOE circle 5/0, targets=self, animLock=???
    PetAerialBlast1 = 25851, // L50, instant, 0.0s CD (group -1), range 25, AOE circle 5/0, targets=hostile, animLock=???
    PetScarletFlame = 16519, // L80, instant, 0.0s CD (group -1), range 50, single-target 0/0, targets=hostile, animLock=???
    PetInferno2 = 25852, // L90, instant, 0.0s CD (group -1), range 35, AOE circle 5/0, targets=hostile, animLock=???
    PetEarthenFury2 = 25853, // L90, instant, 0.0s CD (group -1), range 35, AOE circle 5/0, targets=hostile, animLock=???
    PetAerialBlast2 = 25854, // L90, instant, 0.0s CD (group -1), range 35, AOE circle 5/0, targets=hostile, animLock=???
}

public enum TraitID : uint
{
    None = 0,
    EnhancedAethercharge1 = 466, // L15, ruby/topaz arcanum
    MaimAndMend1 = 66, // L20, damage increase
    EnhancedAethercharge2 = 467, // L22, emerald arcanum
    RuinMastery1 = 217, // L30, ruin1 -> ruin2 upgrade
    RubySummoningMastery = 468, // L30, summon ruby -> ifrit upgrade
    TopazSummoningMastery = 469, // L35, summon topaz -> titan upgrade
    MaimAndMend2 = 69, // L40, damage increase
    EmeraldSummoningMastery = 470, // L45, summon emerald -> garuda upgrade
    Enkindle1 = 471, // L50, summon attacks upgrade
    RuinMastery2 = 473, // L54, ruin2 -> ruin3 upgrade
    AetherchargeMastery = 474, // L58, aethercharge -> dreadwyrm trance upgrade
    EnhancedEnergySiphon = 475, // L62, further ruin status
    EnhancedDreadwyrmTrance = 178, // L70, dreadwyrm trance -> summon bahamut upgrade
    RuinMastery3 = 476, // L72, gemshine upgrade
    OutburstMastery1 = 477, // L74, outburst -> tri-disaster upgrade
    EnhancedSummonBahamut = 502, // L80, summon bahamut -> phoenix upgrade
    OutburstMastery2 = 478, // L82, precious brilliance upgrade
    RuinMastery4 = 479, // L84, potency increase
    ElementalMastery = 503, // L86
    EnhancedRadiantAegis = 480, // L88, second charge
    Enkindle2 = 481, // L90, summon upgrade
}

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

public enum SID : uint
{
    None = 0,
    Addle = 1203, // applied by Addle to target, -5% phys and -10% magic damage dealt
    LucidDreaming = 1204, // applied by Lucid Dreaming to self, MP restore
    Swiftcast = 167, // applied by Swiftcast to self, next cast is instant
    Sleep = 3, // applied by Sleep to target
}

public static class Definitions
{
    public static readonly uint[] UnlockQuests = [66639, 65997, 66627, 66628, 66629, 66631, 66632, 67637, 67638, 67640, 67641, 68165];

    public static bool Unlocked(AID aid, int level, int questProgress)
    {
        return aid switch
        {
            AID.RadiantAegis => level >= 2,
            AID.PetRadiantAegis => level >= 2,
            AID.SummonCarbuncle => level >= 2,
            AID.Physick => level >= 4,
            AID.Aethercharge => level >= 6,
            AID.SummonRuby => level >= 6,
            AID.PetGlitteringRuby => level >= 6,
            AID.RubyRuin1 => level >= 6,
            AID.Gemshine => level >= 6,
            AID.Addle => level >= 8,
            AID.EnergyDrain => level >= 10,
            AID.Fester => level >= 10,
            AID.Sleep => level >= 10,
            AID.Resurrection => level >= 12,
            AID.LucidDreaming => level >= 14,
            AID.SummonTopaz => level >= 15 && questProgress > 0,
            AID.PetGlitteringTopaz => level >= 15,
            AID.TopazRuin1 => level >= 15 && questProgress > 0,
            AID.Swiftcast => level >= 18,
            AID.SummonEmerald => level >= 22,
            AID.PetGlitteringEmerald => level >= 22,
            AID.EmeraldRuin1 => level >= 22,
            AID.PreciousBrilliance => level >= 26,
            AID.TopazOutburst => level >= 26,
            AID.EmeraldOutburst => level >= 26,
            AID.RubyOutburst => level >= 26,
            AID.Outburst => level >= 26,
            AID.Ruin2 => level >= 30 && questProgress > 1,
            AID.PetBurningStrike => level >= 30,
            AID.EmeraldRuin2 => level >= 30 && questProgress > 1,
            AID.TopazRuin2 => level >= 30 && questProgress > 1,
            AID.SummonIfrit1 => level >= 30 && questProgress > 2,
            AID.RubyRuin2 => level >= 30 && questProgress > 1,
            AID.SummonTitan1 => level >= 35 && questProgress > 3,
            AID.PetRockBuster => level >= 35,
            AID.Painflare => level >= 40 && questProgress > 4,
            AID.Surecast => level >= 44,
            AID.SummonGaruda1 => level >= 45 && questProgress > 5,
            AID.PetAerialSlash => level >= 45,
            AID.PetEarthenFury1 => level >= 50,
            AID.PetAerialBlast1 => level >= 50,
            AID.PetInferno1 => level >= 50,
            AID.EnergySiphon => level >= 52 && questProgress > 7,
            AID.EmeraldRuin3 => level >= 54 && questProgress > 8,
            AID.TopazRuin3 => level >= 54 && questProgress > 8,
            AID.Ruin3 => level >= 54 && questProgress > 8,
            AID.RubyRuin3 => level >= 54 && questProgress > 8,
            AID.DreadwyrmTrance => level >= 58 && questProgress > 9,
            AID.AstralImpulse => level >= 58 && questProgress > 9,
            AID.AstralFlare => level >= 58 && questProgress > 9,
            AID.AstralFlow => level >= 60 && questProgress > 10,
            AID.Deathflare => level >= 60 && questProgress > 10,
            AID.Ruin4 => level >= 62,
            AID.SearingLight => level >= 66,
            AID.SummonBahamut => level >= 70 && questProgress > 11,
            AID.PetWyrmwave => level >= 70 && questProgress > 11,
            AID.EnkindleBahamut => level >= 70 && questProgress > 11,
            AID.PetAkhMorn => level >= 70 && questProgress > 11,
            AID.EmeraldRite => level >= 72,
            AID.RubyRite => level >= 72,
            AID.TopazRite => level >= 72,
            AID.TriDisaster => level >= 74,
            AID.TopazDisaster => level >= 74,
            AID.EmeraldDisaster => level >= 74,
            AID.RubyDisaster => level >= 74,
            AID.PetRevelation => level >= 80,
            AID.Rekindle => level >= 80,
            AID.PetEverlastingFlight => level >= 80,
            AID.EnkindlePhoenix => level >= 80,
            AID.BrandOfPurgatory => level >= 80,
            AID.FountainOfFire => level >= 80,
            AID.SummonPhoenix => level >= 80,
            AID.PetScarletFlame => level >= 80,
            AID.EmeraldCatastrophe => level >= 82,
            AID.TopazCatastrophe => level >= 82,
            AID.RubyCatastrophe => level >= 82,
            AID.CrimsonStrike => level >= 86,
            AID.CrimsonCyclone => level >= 86,
            AID.MountainBuster => level >= 86,
            AID.Slipstream => level >= 86,
            AID.PetInferno2 => level >= 90,
            AID.PetEarthenFury2 => level >= 90,
            AID.PetAerialBlast2 => level >= 90,
            AID.SummonGaruda2 => level >= 90,
            AID.SummonTitan2 => level >= 90,
            AID.SummonIfrit2 => level >= 90,
            _ => true,
        };
    }

    public static bool Unlocked(TraitID tid, int level, int questProgress)
    {
        return tid switch
        {
            TraitID.EnhancedAethercharge1 => level >= 15 && questProgress > 0,
            TraitID.MaimAndMend1 => level >= 20,
            TraitID.EnhancedAethercharge2 => level >= 22,
            TraitID.RuinMastery1 => level >= 30 && questProgress > 1,
            TraitID.RubySummoningMastery => level >= 30 && questProgress > 2,
            TraitID.TopazSummoningMastery => level >= 35 && questProgress > 3,
            TraitID.MaimAndMend2 => level >= 40,
            TraitID.EmeraldSummoningMastery => level >= 45 && questProgress > 5,
            TraitID.Enkindle1 => level >= 50 && questProgress > 6,
            TraitID.RuinMastery2 => level >= 54 && questProgress > 8,
            TraitID.AetherchargeMastery => level >= 58 && questProgress > 9,
            TraitID.EnhancedEnergySiphon => level >= 62,
            TraitID.EnhancedDreadwyrmTrance => level >= 70 && questProgress > 11,
            TraitID.RuinMastery3 => level >= 72,
            TraitID.OutburstMastery1 => level >= 74,
            TraitID.EnhancedSummonBahamut => level >= 80,
            TraitID.OutburstMastery2 => level >= 82,
            TraitID.RuinMastery4 => level >= 84,
            TraitID.ElementalMastery => level >= 86,
            TraitID.EnhancedRadiantAegis => level >= 88,
            TraitID.Enkindle2 => level >= 90,
            _ => true,
        };
    }

    public static readonly Dictionary<ActionID, ActionDefinition> SupportedActions = BuildSupportedActions();
    private static Dictionary<ActionID, ActionDefinition> BuildSupportedActions()
    {
        var res = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionInt);
        res.GCDCast(AID.Ruin1, 25, 1.5f);
        res.GCDCast(AID.Ruin2, 25, 1.5f);
        res.GCDCast(AID.Ruin3, 25, 1.5f);
        res.GCD(AID.Ruin4, 25);
        res.GCDCast(AID.RubyRuin1, 25, 2.8f);
        res.GCDCast(AID.RubyRuin2, 25, 2.8f);
        res.GCDCast(AID.RubyRuin3, 25, 2.8f);
        res.GCDCast(AID.RubyRite, 25, 2.8f);
        res.GCD(AID.CrimsonCyclone, 25);
        res.GCD(AID.CrimsonStrike, 3);
        res.GCD(AID.TopazRuin1, 25);
        res.GCD(AID.TopazRuin2, 25);
        res.GCD(AID.TopazRuin3, 25);
        res.GCD(AID.TopazRite, 25);
        res.GCD(AID.EmeraldRuin1, 25);
        res.GCD(AID.EmeraldRuin2, 25);
        res.GCD(AID.EmeraldRuin3, 25);
        res.GCD(AID.EmeraldRite, 25);
        res.GCDCast(AID.Slipstream, 25, 3.0f);
        res.GCD(AID.AstralImpulse, 25);
        res.GCD(AID.FountainOfFire, 25);
        res.GCDCast(AID.Outburst, 25, 1.5f);
        res.GCDCast(AID.TriDisaster, 25, 1.5f);
        res.GCDCast(AID.RubyOutburst, 25, 2.8f);
        res.GCDCast(AID.RubyDisaster, 25, 2.8f);
        res.GCDCast(AID.RubyCatastrophe, 25, 2.8f);
        res.GCD(AID.TopazOutburst, 25);
        res.GCD(AID.TopazDisaster, 25);
        res.GCD(AID.TopazCatastrophe, 25);
        res.GCD(AID.EmeraldOutburst, 25);
        res.GCD(AID.EmeraldDisaster, 25);
        res.GCD(AID.EmeraldCatastrophe, 25);
        res.GCD(AID.AstralFlare, 25);
        res.GCD(AID.BrandOfPurgatory, 25);
        res.GCDCast(AID.Gemshine, 25, 2.5f);
        res.GCDCast(AID.PreciousBrilliance, 25, 2.5f);
        res.GCD(AID.AstralFlow, 0);
        res.GCDCast(AID.SummonCarbuncle, 0, 1.5f);
        res.GCD(AID.SummonRuby, 25);
        res.GCD(AID.SummonIfrit1, 25);
        res.GCD(AID.SummonIfrit2, 25);
        res.GCD(AID.SummonTopaz, 25);
        res.GCD(AID.SummonTitan1, 25);
        res.GCD(AID.SummonTitan2, 25);
        res.GCD(AID.SummonEmerald, 25);
        res.GCD(AID.SummonGaruda1, 25);
        res.GCD(AID.SummonGaruda2, 25);
        res.OGCD(AID.Aethercharge, 0, CDGroup.Aethercharge, 60.0f);
        res.OGCD(AID.DreadwyrmTrance, 0, CDGroup.Aethercharge, 60.0f);
        res.OGCD(AID.SummonBahamut, 25, CDGroup.Aethercharge, 60.0f);
        res.OGCD(AID.SummonPhoenix, 25, CDGroup.Aethercharge, 60.0f);
        res.OGCD(AID.EnergyDrain, 25, CDGroup.EnergyDrain, 60.0f);
        res.OGCD(AID.EnergySiphon, 25, CDGroup.EnergyDrain, 60.0f);
        res.OGCD(AID.Fester, 25, CDGroup.Fester, 1.0f);
        res.OGCD(AID.Painflare, 25, CDGroup.Painflare, 1.0f);
        res.OGCD(AID.Deathflare, 25, CDGroup.Deathflare, 20.0f);
        res.OGCD(AID.EnkindleBahamut, 25, CDGroup.Enkindle, 20.0f);
        res.OGCD(AID.EnkindlePhoenix, 25, CDGroup.Enkindle, 20.0f);
        res.OGCD(AID.MountainBuster, 25, CDGroup.MountainBuster, 1.0f);
        res.GCDCast(AID.Physick, 30, 1.5f);
        res.OGCD(AID.SearingLight, 0, CDGroup.SearingLight, 120.0f);
        res.OGCD(AID.Swiftcast, 0, CDGroup.Swiftcast, 60.0f);
        res.OGCD(AID.LucidDreaming, 0, CDGroup.LucidDreaming, 60.0f);
        res.OGCD(AID.RadiantAegis, 0, CDGroup.RadiantAegis, 60.0f);
        res.OGCD(AID.Rekindle, 30, CDGroup.Rekindle, 20.0f);
        res.OGCD(AID.Addle, 25, CDGroup.Addle, 90.0f);
        res.OGCD(AID.Surecast, 0, CDGroup.Surecast, 120.0f);
        res.GCDCast(AID.Resurrection, 30, 8.0f);
        res.GCDCast(AID.Sleep, 30, 2.5f);
        return res;
    }
}
