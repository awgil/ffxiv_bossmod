namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN5TrinityAvowed;

public enum OID : uint
{
    Boss = 0x30E6, // R3.400, x?
    Helper = 0x233C, // R0.500, x?, mixed types
    SparkArrow = 0x30E7, // R2.000, x?
    FlameArrow = 0x30E8, // R2.000, spawn during fight (gives +2 temperature)
    FrostArrow = 0x30E9, // R2.000, x?
    GlacierArrow = 0x30EA, // R2.000, spawn during fight (gives -2 temperature)
    SwirlingOrb = 0x30EB, // R1.000, x?
    TempestuousOrb = 0x30EC, // R1.000, x?
    BlazingOrb = 0x30ED, // R2.250, x?
    RoaringOrb = 0x30EE, // R1.500, x?
    AvowedAvatar = 0x30EF, // R3.400, x?
}

public enum AID : uint
{
    AutoAttackSword = 22904, // Boss->player, no cast, single-target
    AutoAttackBow = 22905, // Boss->player, no cast, single-target
    AutoAttackStaff = 22906, // Boss->player, no cast, single-target

    WrathOfBozja = 22901, // Boss->self/player, 5.0s cast, range 60 ?-degree cone
    GloryOfBozja = 22902, // Boss->self, 5.0s cast, range 85 circle

    RemoveSword = 22914, // Boss->self, no cast, single-target, visual (change model to default)
    RemoveBow = 22915, // Boss->self, no cast, single-target, visual (change model to default)
    RemoveStaff = 22916, // Boss->self, no cast, single-target, visual (change model to default) /
    AllegiantArsenalSword = 22917, // Boss->self, 3.0s cast, single-target, visual (change model)
    AllegiantArsenalBow = 22918, // Boss->self, 3.0s cast, single-target, visual (change model)
    AllegiantArsenalStaff = 22919, // Boss->self, 3.0s cast, single-target, visual (change model)
    InfernalSlash = 22897, // Boss->self, no cast, range 70 270-degree cone aoe (when changing to sword)
    Flashvane = 22898, // Boss->self, no cast, range 70 270-degree cone aoe (when changing to bow)
    FuryOfBozja = 22899, // Boss->self, no cast, range 10 circle aoe (when changing to staff)

    FlamesOfBozja = 22910, // Boss->self, 3.0s cast, single-target, visual (prepare for big flame)
    FlamesOfBozjaExtra = 23353, // Helper->self, 7.0s cast, range 45 width 50 rect, visual?
    FlamesOfBozjaAOE = 22888, // Helper->self, 7.0s cast, range 45 width 50 rect aoe (oneshot)
    HotAndColdBow = 23472, // Boss->self, 3.0s cast, single-target, visual (applies temperature debuffs)
    ShimmeringShot = 22911, // Boss->self, 3.0s cast, single-target, visual (creates arrows)
    ChillArrow = 22889, // SparkArrow->self, no cast, range 10 width 10 rect, gives +1 temperature (despite name)
    FreezingArrow = 22890, // FlameArrow->self, no cast, range 10 width 10 rect, gives +2 temperature (despite name)
    HeatedArrow = 22891, // FrostArrow->self, no cast, range 10 width 10 rect, gives -1 temperature (despite name)
    SearingArrow = 22892, // GlacierArrow->self, no cast, range 10 width 10 rect, gives -2 temperature (despite name)
    ElementalBrandBow = 23474, // Boss->self, 3.0s cast, single-target, visual (applies brand debuffs)

    HotAndColdStaff = 22907, // Boss->self, 3.0s cast, single-target, visual (applies temperature debuffs)
    QuickMarchStaff = 22913, // Boss->self, 3.0s cast, single-target, visual (applies march debuffs)
    FreedomOfBozja = 22908, // Boss->self, 3.0s cast, single-target, visual (spawn orbs)
    ElementalImpact1 = 22880, // TempestuousOrb/SwirlingOrb->self, 5.0s cast, range 60 circle, visual (proximity)
    ElementalImpact2 = 22882, // BlazingOrb/RoaringOrb->self, 5.0s cast, range 60 circle, visual (proximity)
    ElementalImpactAOE1 = 20377, // Helper->self, no cast, range 60 circle with 20 falloff
    ElementalImpactAOE2 = 20378, // Helper->self, no cast, range 60 circle with 20 falloff
    ElementalImpactAOE3 = 20309, // Helper->self, no cast, range 60 circle with 20 falloff
    ElementalImpactAOE4 = 20310, // Helper->self, no cast, range 60 circle with 20 falloff
    ChillBlast = 22884, // SwirlingOrb->self, 8.0s cast, range 22 circle, gives -1 temperature
    FreezingBlast = 22885, // TempestuousOrb->self, 8.0s cast, range 22 circle, gives -2 temperature
    HeatedBlast = 22886, // BlazingOrb->self, 8.0s cast, range 22 circle, gives +1 temperature
    SearingBlast = 22887, // RoaringOrb->self, 8.0s cast, range 22 circle, gives +2 temperature

    HotAndColdSword = 23471, // Boss->self, 3.0s cast, single-target, visual (applies temperature debuffs)
    BladeOfEntropyBH11 = 22893, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyAC11 = 22894, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyBH12 = 22895, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyAC12 = 22896, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyAH11 = 23397, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyBC11 = 23398, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyAH12 = 23399, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyBC12 = 23400, // Boss->self, 5.0s cast, range 40 180-degree cone

    UnseenEyeBow = 23476, // Boss->self, 3.0s cast, single-target, visual (show clones for crisscross aoe)
    UnseenEyeStaff = 22912, // Boss->self, 3.0s cast, single-target, visual (show clones for crisscross aoe)
    GleamingArrow = 22900, // AvowedAvatar->self, 6.0s cast, range 60 width 10 rect aoe
    ClearTemperatures = 23332, // Helper->self, no cast, range 60 circle, visual (clear temperatures?)
}

public enum SID : uint
{
    UnknownStatus = 2056, // none->BlazingOrb/SwirlingOrb/SparkArrow/FrostArrow/TempestuousOrb/RoaringOrb/30EA/30E8, extra=0xF1/0xF3/0xF4/0xF2
    BrinkOfDeath = 44, // none->player, extra=0x0
    ColdBlade1 = 2299, // none->Boss, extra=0x0
    ColdBlade2 = 2300, // none->Boss, extra=0x0
    Doom = 2519, // AvowedAvatar->player, extra=0x0
    HotBlade1 = 2297, // none->Boss, extra=0x0
    HotBlade2 = 2298, // none->Boss, extra=0x0
    Intemperate = 2275, // Boss->player, extra=0x0
    MagicVulnerabilityUp = 1138, // SwirlingOrb/BlazingOrb/SparkArrow/FrostArrow/TempestuousOrb/RoaringOrb/30E8/30EA->player, extra=0x0
    Normal = 2204, // SwirlingOrb/BlazingOrb/SparkArrow/FrostArrow/Boss/TempestuousOrb/RoaringOrb/30EA/30E8->player, extra=0x0
    RunningCold1 = 2268, // Boss->player, extra=0x0
    RunningCold2 = 2274, // Boss->player, extra=0x0
    RunningHot1 = 2205, // Boss->player, extra=0x0
    RunningHot2 = 2212, // BlazingOrb/SparkArrow/Boss->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    TwiceComeRuin = 2485, // Boss/AvowedAvatar->player, extra=0x1
    Weakness = 43, // none->player, extra=0x0

    HotBrand1 = 2277, // none->player, extra=0x0
    HotBrand2 = 2291, // none->player, extra=0x0
    ColdBrand1 = 2292, // none->player, extra=0x0
    ColdBrand2 = 2296, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player
}
