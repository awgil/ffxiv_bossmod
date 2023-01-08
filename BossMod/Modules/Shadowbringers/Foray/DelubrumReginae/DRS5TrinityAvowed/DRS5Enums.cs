namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5TrinityAvowed
{
    public enum OID : uint
    {
        Boss = 0x30DC, // R3.400, x1
        Helper = 0x233C, // R0.500, x19, and more spawn during fight
        SparkArrow = 0x30DD, // R2.000, spawn during fight (gives +1 temperature)
        FlameArrow = 0x30DE, // R2.000, spawn during fight (gives +2 temperature)
        FrostArrow = 0x30DF, // R2.000, spawn during fight (gives -1 temperature)
        GlacierArrow = 0x30E0, // R2.000, spawn during fight (gives -2 temperature)
        SwirlingOrb = 0x30E1, // R1.000, spawn during fight (gives -1 temperature)
        TempestuousOrb = 0x30E2, // R1.000, spawn during fight (gives -2 temperature)
        BlazingOrb = 0x30E3, // R1.500, spawn during fight (gives +1 temperature)
        RoaringOrb = 0x30E4, // R1.500, spawn during fight (gives +2 temperature)
        AvowedAvatar = 0x30E5, // R3.400, x11
    };

    public enum AID : uint
    {
        AutoAttackSword = 22864, // Boss->player, no cast, single-target
        AutoAttackBow = 22865, // Boss->player, no cast, single-target
        AutoAttackStaff = 22866, // Boss->player, no cast, single-target
        AutoAttackDefault = 22867, // Boss->player, no cast, single-target

        WrathOfBozja = 22862, // Boss->self, 5.0s cast, range 60 ?-degree cone shared (3-target) tankbuster
        WrathOfBozjaBow = 23477, // Boss->self, 5.0s cast, range 60 ?-degree cone shared (3-target) tankbuster (when holding bow)
        GloryOfBozja = 22863, // Boss->self, 5.0s cast, range 85 circle raidwide
        GloryOfBozjaAOE = 23346, // Helper->self, 5.6s cast, range 85 circle raidwide (hits second half of raid)

        RemoveSword = 22914, // Boss->self, no cast, single-target, visual (change model to default)
        RemoveBow = 22915, // Boss->self, no cast, single-target, visual (change model to default)
        RemoveStaff = 22916, // Boss->self, no cast, single-target, visual (change model to default)
        AllegiantArsenalSword = 22917, // Boss->self, 3.0s cast, single-target, visual (change model)
        AllegiantArsenalBow = 22918, // Boss->self, 3.0s cast, single-target, visual (change model)
        AllegiantArsenalStaff = 22919, // Boss->self, 3.0s cast, single-target, visual (change model)
        InfernalSlash = 22858, // Boss->self, no cast, range 70 270-degree cone aoe (when changing to sword)
        Flashvane = 22859, // Boss->self, no cast, range 70 270-degree cone aoe (when changing to bow)
        FuryOfBozja = 22860, // Boss->self, no cast, range 10 circle aoe (when changing to staff)

        QuickMarchBow = 23475, // Boss->self, 3.0s cast, single-target, visual (applies march debuffs)
        FlamesOfBozja = 22910, // Boss->self, 3.0s cast, single-target, visual (prepare for big flame)
        FlamesOfBozjaExtra = 23357, // Helper->self, 9.0s cast, range 45 width 50 rect, visual?
        FlamesOfBozjaAOE = 22841, // Helper->self, 9.0s cast, range 45 width 50 rect aoe (oneshot)
        HotAndColdBow = 23472, // Boss->self, 3.0s cast, single-target, visual (applies temperature debuffs)
        ShimmeringShot = 22911, // Boss->self, 3.0s cast, single-target, visual (creates arrows)
        ChillArrow1 = 22842, // SparkArrow->self, no cast, range 10 width 10 rect, gives +1 temperature (despite name)
        FreezingArrow1 = 22843, // FlameArrow->self, no cast, range 10 width 10 rect, gives +2 temperature (despite name)
        HeatedArrow1 = 22844, // FrostArrow->self, no cast, range 10 width 10 rect, gives -1 temperature (despite name)
        SearingArrow1 = 22845, // GlacierArrow->self, no cast, range 10 width 10 rect, gives -2 temperature (despite name)
        ChillArrow2 = 22846, // Helper->self, no cast, range 10 width 10 rect, gives +1 temperature (despite name)
        FreezingArrow2 = 22847, // Helper->self, no cast, range 10 width 10 rect, gives +2 temperature (despite name)
        HeatedArrow2 = 22848, // Helper->self, no cast, range 10 width 10 rect, gives -1 temperature (despite name)
        SearingArrow2 = 22849, // Helper->self, no cast, range 10 width 10 rect, gives -2 temperature (despite name)
        ElementalBrandBow = 23474, // Boss->self, 3.0s cast, single-target, visual (applies brand debuffs)

        HotAndColdStaff = 22907, // Boss->self, 3.0s cast, single-target, visual (applies temperature debuffs)
        QuickMarchStaff = 22913, // Boss->self, 3.0s cast, single-target, visual (applies march debuffs)
        FreedomOfBozja = 22908, // Boss->self, 3.0s cast, single-target, visual (spawn orbs)
        ElementalImpact1 = 22825, // TempestuousOrb/SwirlingOrb->self, 5.0s cast, range 60 circle, visual (proximity)
        ElementalImpact2 = 22827, // BlazingOrb/RoaringOrb->self, 5.0s cast, range 60 circle, visual (proximity)
        ElementalImpactAOE1 = 20308, // Helper->self, no cast, range 60 circle with 20 falloff
        ElementalImpactAOE2 = 20060, // Helper->self, no cast, range 60 circle with 20 falloff
        ElementalImpactAOE3 = 20307, // Helper->self, no cast, range 60 circle with 20 falloff
        ElementalImpactAOE4 = 20306, // Helper->self, no cast, range 60 circle with 20 falloff
        ChillBlast1 = 22829, // SwirlingOrb->self, 8.0s cast, range 22 circle, gives -1 temperature
        FreezingBlast1 = 22830, // TempestuousOrb->self, 8.0s cast, range 22 circle, gives -2 temperature
        HeatedBlast1 = 22831, // BlazingOrb->self, 8.0s cast, range 22 circle, gives +1 temperature
        SearingBlast1 = 22832, // RoaringOrb->self, 8.0s cast, range 22 circle, gives +2 temperature
        ChillBlast2 = 22833, // Helper->self, 8.0s cast, range 22 circle, gives -1 temperature
        FreezingBlast2 = 22834, // Helper->self, 8.0s cast, range 22 circle, gives -2 temperature
        HeatedBlast2 = 22835, // Helper->self, 8.0s cast, range 22 circle, gives +1 temperature
        SearingBlast2 = 22836, // Helper->self, 8.0s cast, range 22 circle, gives +2 temperature
        ElementalBrandStaff = 22909, // Boss->self, 3.0s cast, single-target, visual (applies brand debuffs)

        // TODO: i'm not sure what is the difference between A/B variants, it seems each can be cast either by boss or by avatar...
        HotAndColdSword = 23471, // Boss->self, 3.0s cast, single-target, visual (applies temperature debuffs)
        UnwaveringApparition = 23354, // Boss->self, 3.0s cast, single-target, visual (show clones and disappear)
        BladeOfEntropyBH11 = 22850, // Boss->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyAC11 = 22851, // AvowedAvatar->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyBH12 = 22852, // Helper->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyAC12 = 22853, // Helper->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyAH11 = 22854, // AvowedAvatar->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyBC11 = 22855, // Boss->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyAH12 = 22856, // Helper->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyBC12 = 22857, // Helper->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyBH21 = 22870, // Boss->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyBC21 = 22871, // Boss->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyBH22 = 22872, // Helper->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyBC22 = 22873, // Helper->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyAH21 = 22874, // AvowedAvatar->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyAC21 = 22875, // AvowedAvatar->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyAH22 = 22876, // Helper->self, 10.0s cast, range 40 180-degree cone
        BladeOfEntropyAC22 = 22877, // Helper->self, 10.0s cast, range 40 180-degree cone
        ElementalBrandSword = 23473, // Boss->self, 3.0s cast, single-target, visual (applies brand debuffs)

        UnseenEye = 23476, // Boss->self, 3.0s cast, single-target, visual (show clones for crisscross aoe)
        GleamingArrow = 22861, // AvowedAvatar->self, 6.0s cast, range 60 width 10 rect aoe
        ApplyHotBrand1 = 22837, // Helper->self, no cast, range 60 circle, converts brand to temperature
        ApplyHotBrand2 = 22838, // Helper->self, no cast, range 60 circle, converts brand to temperature
        ApplyColdBrand1 = 22839, // Helper->self, no cast, range 60 circle, converts brand to temperature
        ApplyColdBrand2 = 22840, // Helper->self, no cast, range 60 circle, converts brand to temperature
        ClearTemperatures = 23332, // Helper->self, no cast, range 60 circle, visual (clear temperatures?)
    };

    public enum SID : uint
    {
        ForwardMarch = 1293, // none->player, extra=0x0
        AboutFace = 1294, // none->player, extra=0x0
        LeftFace = 1295, // none->player, extra=0x0
        RightFace = 1296, // none->player, extra=0x0
        ForcedMarch = 1257, // none->player, extra=0x4/0x1/0x2/0x8

        Intemperate = 2275, // Boss->player, extra=0x0
        Normal = 2204, // Helper/GlacierArrow/FlameArrow/FrostArrow/SparkArrow/TempestuousOrb/RoaringOrb/BlazingOrb/SwirlingOrb/AvowedAvatar/Boss->player, extra=0x0
        RunningHot1 = 2205, // Boss/Helper/FrostArrow/FlameArrow->player, extra=0x0
        RunningHot2 = 2212, // Boss/Helper->player, extra=0x0
        RunningCold1 = 2268, // Boss/Helper/BlazingOrb/SwirlingOrb/AvowedAvatar/GlacierArrow/SparkArrow->player, extra=0x0
        RunningCold2 = 2274, // Boss/Helper/FrostArrow->player, extra=0x0
        HotBrand1 = 2277, // none->player, extra=0x0
        HotBrand2 = 2291, // none->player, extra=0x0
        ColdBrand1 = 2292, // none->player, extra=0x0
        ColdBrand2 = 2296, // none->player, extra=0x0
        //_Gen_ = 2056, // none->FrostArrow/FlameArrow/SparkArrow/GlacierArrow/BlazingOrb/TempestuousOrb/SwirlingOrb/RoaringOrb, extra=0xF3/0xF2/0xF1/0xF4

        HotBlade1 = 2297, // none->AvowedAvatar/Boss, extra=0x0
        HotBlade2 = 2298, // none->Boss/AvowedAvatar, extra=0x0
        ColdBlade1 = 2299, // none->AvowedAvatar, extra=0x0
        ColdBlade2 = 2300, // none->AvowedAvatar, extra=0x0
    };
}
