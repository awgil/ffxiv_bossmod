using System.Collections.Generic;

namespace BossMod.SMN
{
    public enum AID : uint
    {
        None = 0,

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
        Addle = 40, // 90.0 max
        LucidDreaming = 41, // 60.0 max
        Swiftcast = 42, // 60.0 max
        Surecast = 43, // 120.0 max
    }

    public enum MinLevel : int
    {
        SummonCarbuncle = 2, // includes radiant aegis
        Physick = 4,
        SummonRuby = 6, // includes aethercharge, ruby ruin 1 and gemshine
        Addle = 8,
        EnergyDrainFester = 10,
        Sleep = 10,
        Resurrection = 12,
        LucidDreaming = 14,
        SummonTopaz = 15, // unlocked by quest 66639 'Topaz Teachings'; includes topaz ruin 1
        Swiftcast = 18,
        MaimAndMend1 = 20, // passive, damage increase
        SummonEmerald = 22, // includes emerald ruin 1
        Outburst = 26, // includes precious brilliance and elemental outbursts
        Ruin2 = 30, // unlocked by quest 65997 'Sinking Doesmaga'; includes elemental ruins 2
        SummonIfrit1 = 30, // unlocked by quest 66627 'Austerities of Flame'
        SummonTitan1 = 35, // unlocked by quest 66628 'Austerities of Earth'
        MaimAndMend2 = 40, // passive, damage increase
        Painflare = 40, // unlocked by quest 66629 'Shadowing the Summoner'
        Surecast = 44,
        SummonGaruda1 = 45, // unlocked by quest 66631 'Austerities of Wind'
        Enkindle1 = 50, // unlocked by quest 66632 'Primal Burdens'; passive, upgrades summmons
        EnergySiphon = 52, // unlocked by quest 67637 'A Matter of Fact'
        Ruin3 = 54, // unlocked by quest 67638 'A Miner Negotiation'; includes elemental ruins 3
        DreadwyrmTrance = 58, // unlocked by quest 67640 'I Could Have Tranced All Night'; includes astral impulse and astral flare
        Deathflare = 60, // unlocked by quest 67641 'A Flare for the Dramatic'; includes astral flow
        Ruin4 = 62,
        SearingLight = 66,
        SummonBahamut = 70, // unlocked by quest 68165 'An Art for the Living'; includes enkindle bahamut
        ElementalRites = 72,
        TriDisaster = 74, // includes elemental disasters
        SummonPhoenix = 80, // includes rekindle, enkindle phoenix, brand of purgatory, fountain of fire and scarlet flame
        ElementalCatastrophes = 82,
        RuinMastery = 84, // passive, increases potencies
        ElementalMastery = 86, // includes crimson strike/cyclone, mountain buster and slipstream
        EnhancedRadiantAegis = 88, // passive, grants charges to radiant aegis
        Enkindle2 = 90, // passive, unlocks summon ifrit/titan/garuda 2
    }

    public static class Definitions
    {
        public static QuestLockEntry[] QuestsPerLevel = {
            new(15, 66639),
            new(30, 65997),
            new(30, 66627),
            new(35, 66628),
            new(40, 66629),
            new(45, 66631),
            new(50, 66632),
            new(52, 67637),
            new(54, 67638),
            new(58, 67640),
            new(60, 67641),
            new(70, 68165),
        };

        public static Dictionary<ActionID, ActionDefinition> SupportedActions;
        static Definitions()
        {
            SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionStr);
            SupportedActions.GCDCast(AID.Ruin1, 25, 1.5f);
            SupportedActions.GCDCast(AID.Ruin2, 25, 1.5f);
            SupportedActions.GCDCast(AID.Ruin3, 25, 1.5f);
            SupportedActions.GCD(AID.Ruin4, 25);
            SupportedActions.GCDCast(AID.RubyRuin1, 25, 2.8f);
            SupportedActions.GCDCast(AID.RubyRuin2, 25, 2.8f);
            SupportedActions.GCDCast(AID.RubyRuin3, 25, 2.8f);
            SupportedActions.GCDCast(AID.RubyRite, 25, 2.8f);
            SupportedActions.GCD(AID.CrimsonCyclone, 25);
            SupportedActions.GCD(AID.CrimsonStrike, 3);
            SupportedActions.GCD(AID.TopazRuin1, 25);
            SupportedActions.GCD(AID.TopazRuin2, 25);
            SupportedActions.GCD(AID.TopazRuin3, 25);
            SupportedActions.GCD(AID.TopazRite, 25);
            SupportedActions.GCD(AID.EmeraldRuin1, 25);
            SupportedActions.GCD(AID.EmeraldRuin2, 25);
            SupportedActions.GCD(AID.EmeraldRuin3, 25);
            SupportedActions.GCD(AID.EmeraldRite, 25);
            SupportedActions.GCDCast(AID.Slipstream, 25, 3.0f);
            SupportedActions.GCD(AID.AstralImpulse, 25);
            SupportedActions.GCD(AID.FountainOfFire, 25);
            SupportedActions.GCDCast(AID.Outburst, 25, 1.5f);
            SupportedActions.GCDCast(AID.TriDisaster, 25, 1.5f);
            SupportedActions.GCDCast(AID.RubyOutburst, 25, 2.8f);
            SupportedActions.GCDCast(AID.RubyDisaster, 25, 2.8f);
            SupportedActions.GCDCast(AID.RubyCatastrophe, 25, 2.8f);
            SupportedActions.GCD(AID.TopazOutburst, 25);
            SupportedActions.GCD(AID.TopazDisaster, 25);
            SupportedActions.GCD(AID.TopazCatastrophe, 25);
            SupportedActions.GCD(AID.EmeraldOutburst, 25);
            SupportedActions.GCD(AID.EmeraldDisaster, 25);
            SupportedActions.GCD(AID.EmeraldCatastrophe, 25);
            SupportedActions.GCD(AID.AstralFlare, 25);
            SupportedActions.GCD(AID.BrandOfPurgatory, 25);
            SupportedActions.GCDCast(AID.Gemshine, 25, 2.5f);
            SupportedActions.GCDCast(AID.PreciousBrilliance, 25, 2.5f);
            SupportedActions.GCD(AID.AstralFlow, 0);
            SupportedActions.GCDCast(AID.SummonCarbuncle, 0, 1.5f);
            SupportedActions.GCD(AID.SummonRuby, 25);
            SupportedActions.GCD(AID.SummonIfrit1, 25);
            SupportedActions.GCD(AID.SummonIfrit2, 25);
            SupportedActions.GCD(AID.SummonTopaz, 25);
            SupportedActions.GCD(AID.SummonTitan1, 25);
            SupportedActions.GCD(AID.SummonTitan2, 25);
            SupportedActions.GCD(AID.SummonEmerald, 25);
            SupportedActions.GCD(AID.SummonGaruda1, 25);
            SupportedActions.GCD(AID.SummonGaruda2, 25);
            SupportedActions.OGCD(AID.Aethercharge, 0, CDGroup.Aethercharge, 60.0f);
            SupportedActions.OGCD(AID.DreadwyrmTrance, 0, CDGroup.Aethercharge, 60.0f);
            SupportedActions.OGCD(AID.SummonBahamut, 25, CDGroup.Aethercharge, 60.0f);
            SupportedActions.OGCD(AID.SummonPhoenix, 25, CDGroup.Aethercharge, 60.0f);
            SupportedActions.OGCD(AID.EnergyDrain, 25, CDGroup.EnergyDrain, 60.0f);
            SupportedActions.OGCD(AID.EnergySiphon, 25, CDGroup.EnergyDrain, 60.0f);
            SupportedActions.OGCD(AID.Fester, 25, CDGroup.Fester, 1.0f);
            SupportedActions.OGCD(AID.Painflare, 25, CDGroup.Painflare, 1.0f);
            SupportedActions.OGCD(AID.Deathflare, 25, CDGroup.Deathflare, 20.0f);
            SupportedActions.OGCD(AID.EnkindleBahamut, 25, CDGroup.Enkindle, 20.0f);
            SupportedActions.OGCD(AID.EnkindlePhoenix, 25, CDGroup.Enkindle, 20.0f);
            SupportedActions.OGCD(AID.MountainBuster, 25, CDGroup.MountainBuster, 1.0f);
            SupportedActions.GCDCast(AID.Physick, 30, 1.5f);
            SupportedActions.OGCD(AID.SearingLight, 0, CDGroup.SearingLight, 120.0f);
            SupportedActions.OGCD(AID.Swiftcast, 0, CDGroup.Swiftcast, 60.0f);
            SupportedActions.OGCD(AID.LucidDreaming, 0, CDGroup.LucidDreaming, 60.0f);
            SupportedActions.OGCD(AID.RadiantAegis, 0, CDGroup.RadiantAegis, 60.0f);
            SupportedActions.OGCD(AID.Rekindle, 30, CDGroup.Rekindle, 20.0f);
            SupportedActions.OGCD(AID.Addle, 25, CDGroup.Addle, 90.0f);
            SupportedActions.OGCD(AID.Surecast, 0, CDGroup.Surecast, 120.0f);
            SupportedActions.GCDCast(AID.Resurrection, 30, 8.0f);
            SupportedActions.GCDCast(AID.Sleep, 30, 2.5f);
        }
    }

    public enum SID : uint
    {
        None = 0,
        Addle = 1203, // applied by Addle to target, -5% phys and -10% magic damage dealt
        LucidDreaming = 1204, // applied by Lucid Dreaming to self, MP restore
        Swiftcast = 167, // applied by Swiftcast to self, next cast is instant
        Sleep = 3, // applied by Sleep to target
    }
}
