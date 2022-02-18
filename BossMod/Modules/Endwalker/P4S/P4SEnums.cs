namespace BossMod.P4S
{
    public enum OID : uint
    {
        Boss1 = 0x35FD, // first phase boss
        Pinax = 0x35FE, // '', 4x exist at start at [90/110, 90/110]
        Orb = 0x35FF, // orbs spawned by Belone Bursts
        Boss2 = 0x3600, // second phase boss (exists from start, but recreated on checkpoint)
        Akantha = 0x3601, // ?? 'akantha', 12 exist at start
        Helper = 0x233C, // 38 exist at start
    };

    public enum AID : uint
    {
        ElegantEviscerationSecond = 26649, // Boss1->target, no cast, second hit
        SettingTheScene = 27083, // Boss1->Boss1
        ShiftStart = 27086, // Boss1->Boss1, no cast, tp to center, sword glowing (?)
        Pinax = 27087, // Boss1->Boss1
        MekhaneAcid = 27088, // Helper->target, no cast
        MekhaneLava = 27089, // Helper->target, no cast
        MekhaneWell = 27090, // Helper->Helper, no cast, affects arena
        MekhaneLevinstrike = 27091, // Helper->Helper, no cast, affects arena
        PinaxAcid = 27092, // Helper->Helper, affects corner
        PinaxLava = 27093, // Helper->Helper, affects corner
        PinaxWell = 27094, // Helper->Helper, affects corner
        PinaxLevinstrike = 27095, // Helper->Helper, affects corner
        Bloodrake = 27096, // Boss1->Boss1
        BeloneBursts = 27097, // Boss1->Boss1
        BeloneBurstsAOETank = 27098, // Orb->target, no cast
        BeloneBurstsAOEHealer = 27099, // Orb->target, no cast
        BeloneBurstsAOEDPS = 27100, // Orb->target, no cast
        BeloneCoils = 27101, // Boss1->Boss1
        BeloneCoilsDPS = 27102, // Helper->Helper, role towers ('no heals/tanks' variant)
        BeloneCoilsTH = 27103, // Helper->Helper, role towers ('no dps' variant)
        DirectorsBelone = 27110, // Boss1->Boss1
        DirectorsBeloneDebuffs = 27111, // Helper->target, no cast, just applies Role Call debuffs
        CursedCasting2 = 27112, // Helper->target, no cast, during second director's belone, does something bad if no role call?..
        CursedCasting1 = 27113, // Helper->target, no cast, during first director's belone, does something bad if no role call?..
        AethericChlamys = 27116, // Boss1->Boss1
        InversiveChlamys = 27117, // Boss1->Boss1
        InversiveChlamysAOE = 27119, // Helper->target, no cast, damage to tethered targets
        ElementalBelone = 27122, // Boss1->Boss1
        Periaktoi = 27124, // Boss1->Boss1
        PeriaktoiSafeAcid = 27125, // Helper->Helper (unconfirmed)
        PeriaktoiSafeLava = 27126, // Helper->Helper
        PeriaktoiSafeWell = 27127, // Helper->Helper
        PeriaktoiSafeLevinstrike = 27128, // Helper->Helper
        PeriaktoiDangerAcid = 27129, // Helper->Helper
        PeriaktoiDangerLava = 27130, // Helper->Helper
        PeriaktoiDangerWell = 27131, // Helper->Helper
        PeriaktoiDangerLevinstrike = 27132, // Helper->Helper
        NortherlyShiftCloak = 27133, // Boss1->Boss1
        SoutherlyShiftCloak = 27134, // Boss1->Boss1 (unconfirmed)
        EasterlyShiftCloak = 27135, // Boss1->Boss1 (unconfirmed)
        WesterlyShiftCloak = 27136, // Boss1->Boss1 (unconfirmed)
        ShiftingStrikeCloak = 27137, // Helper->Helper
        NortherlyShiftSword = 27138, // Boss1->Boss1
        SoutherlyShiftSword = 27139, // Boss1->Boss1
        EasterlyShiftSword = 27140, // Boss1->Boss1
        WesterlyShiftSword = 27141, // Boss1->Boss1 (unconfirmed)
        ShiftingStrikeSword = 27142, // Helper->Helper, sword attack
        ElegantEvisceration = 27144, // Boss1->target
        Decollation = 27145, // Boss1->Boss1
        AkanthaiAct1 = 27148, // Boss2->Boss2
        AkanthaiExplodeAOE = 27149, // Helper->Helper
        AkanthaiExplodeTower = 27150, // Helper->Helper
        AkanthaiExplodeKnockback = 27152, // Helper->Helper
        AkanthaiVisualTower = 27153, // Akantha->Akantha
        AkanthaiVisualAOE = 27154, // Akantha->Akantha
        AkanthaiVisualKnockback = 27155, // Akantha->Akantha
        AkanthaiWaterBreakAOE = 27156, // Helper->targets, no cast
        AkanthaiDarkBreakAOE = 27158, // Helper->targets, no cast
        AkanthaiFireBreakAOE = 27160, // Helper->targets, no cast
        AkanthaiWindBreakAOE = 27162, // Helper->targets, no cast
        FleetingImpulseAOE = 27164, // Helper->target, no cast
        HellsSting = 27166, // Boss2->Boss2
        HellsStingSecond = 27167, // Boss2->Boss2, no cast
        HellsStingAOE1 = 27168, // Helper->Helper
        HellsStingAOE2 = 27169, // Helper->Helper
        KothornosKock = 27170, // Boss2->Boss2
        KothornosKickJump = 27171, // Boss2->target, no cast
        KothornosQuake2 = 27172, // Boss2->Boss2, no cast
        KothornosQuakeAOE = 27173, // Helper->target, no cast
        Nearsight = 27174, // Boss2->Boss2
        Farsight = 27175, // Boss2->Boss2
        NearsightAOE = 27176, // Helper->target, no cast
        DarkDesign = 27177, // Boss2->Boss2
        DarkDesignAOE = 27178, // Helper->location
        HeartStake = 27179, // Boss2->target
        UltimateImpulse = 27180, // Boss2->Boss2
        SearingStream = 27181, // Boss2->Boss2
        WreathOfThorns1 = 27183, // Boss2->Boss2
        WreathOfThorns2 = 27184, // Boss2->Boss2
        WreathOfThorns3 = 27185, // Boss2->Boss2
        WreathOfThorns4 = 27186, // Boss2->Boss2
        WreathOfThorns5 = 27188, // Boss2->Boss2
        WreathOfThorns6 = 27189, // Boss2->Boss2
        AkanthaiCurtainCall = 27190, // Boss2->Boss2
        Enrage = 27191, // Boss2->Boss2
        FarsightAOE = 28123, // Helper->target, no cast
        VengefulBelone = 28194, // Boss1->Boss1
        KothornosQuake1 = 28276, // Boss2->Boss2, no cast
        HeartStakeSecond = 28279, // Boss2->target, no cast
        DemigodDouble = 28280, // Boss2->target
        AkanthaiAct2 = 28340, // Boss2->Boss2
        AkanthaiAct3 = 28341, // Boss2->Boss2
        AkanthaiAct4 = 28342, // Boss2->Boss2
        AkanthaiFinale = 28343, // Boss2->Boss2
        FleetingImpulse = 28344, // Boss2->Boss2
        InversiveChlamysAOE2 = 28437, // Helper->target, no cast, damage to tethered targets (during belone coils)
    };

    public enum SID : uint
    {
        OrbRole = 2056,
        ThriceComeRuin = 2530,
        RoleCall = 2802,
        Miscast = 2803,
        Thornpricked = 2804,
        ActingDPS = 2925,
        ActingHealer = 2926,
        ActingTank = 2927,
    }

    public enum TetherID : uint
    {
        ExplosiveAether = 17,
        Chlamys = 89,
        Bloodrake = 165,
        WreathOfThornsPairsClose = 172,
        WreathOfThorns = 173, // also used when pairs are about to break
    }

    public enum IconID : uint
    {
        None = 0,
        AkanthaiWater = 300, // act 4
        AkanthaiDark = 301, // acts 2 & 4 & 6
        AkanthaiWind = 302, // acts 2 & 5
        AkanthaiFire = 303, // act 2
    }
}
