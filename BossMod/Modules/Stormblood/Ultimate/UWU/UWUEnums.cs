namespace BossMod.Stormblood.Ultimate.UWU
{
    public enum OID : uint
    {
        Garuda = 0x2212, // R1.700, x1
        GarudaSister = 0x2213, // R1.360, x2 (chirada & suparna)
        RazorPlume = 0x2214, // R0.500, spawn during fight if awakened earlier than expected
        SpinyPlume = 0x2216, // R0.500, spawn during fight
        SatinPlume = 0x2215, // R0.500, spawn during fight
        SpinyShield = 0x1E8F68, // R0.500, EventObj type, spawn during fight

        Ifrit = 0x221A, // R5.000, x4
        InfernalNail = 0x221B, // R1.000-2.000, spawn during fight

        Titan = 0x2217, // R4.550, x1
        BombBoulder = 0x2218, // R1.300, spawn during fight
        GraniteGaol = 0x2219, // R1.800, spawn during fight

        Lahabrea = 0x221C, // R0.500, x1
        TheUltimaWeapon = 0x221E, // R6.000, x1
        Helper = 0x233C, // R0.500, x14

        //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x8, EventObj type
        //_Gen_Actor1ea988 = 0x1EA988, // R0.500, EventObj type, spawn during fight
        BeyondLimits = 0x1EA989, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttackGaruda = 870, // Garuda->player, no cast, single-target
        Slipstream = 11091, // Garuda->self, 2.5s cast, range 10+R 90-degree cone aoe
        Downburst = 11088, // Garuda->self, no cast, range 10+R 90-degree cone cleave
        AerialBlast = 11093, // Garuda->self, 3.0s cast, raidwide
        MistralShriek = 11092, // Garuda->self, 3.0s cast, raidwide
        FeatherRain = 11085, // Helper->location, 1.0s cast, range 3 circle puddle
        MistralSongBoss = 11074, // Garuda->self, no cast, ???
        MistralSongAdds = 11083, // Suparna->self, no cast, ???
        GreatWhirlwind = 11073, // Helper->location, 3.0s cast, range 8 circle puddle (on mistral song interceptor)
        Friction = 11080, // Garuda->players, 2.0s cast, range 5 circle
        SuperCyclone1 = 11079, // Helper->location, no cast, raidwide on thermal low 1 cleanse
        SuperCyclone2 = 11189, // Helper->location, no cast, raidwide on thermal low 2 cleanse
        EyeOfTheStorm = 11090, // Helper->self, 3.0s cast, range 12-25 donut
        WickedWheel = 11086, // Garuda->self, 3.0s cast, range 7+R circle aoe
        WickedTornado = 11087, // Helper->self, no cast, range ?-20 donut
        Mesohigh = 11081, // Suparna->player, no cast, range 3 circle
        Featherlance = 11075, // RazorPlume->self, no cast, range 8 circle aoe on death
        ThermalTumult = 11076, // SatinPlume->self, no cast, range 20 circle sleep if not killed in ~20s
        Cyclone = 11077, // SpinyPlume->player, no cast, single-target, mini-tankbuster with thermal low stack
        Gigastorm = 11078, // SpinyPlume->self, 3.0s cast, range 6+R circle aoe on death

        AutoAttackIfrit = 11089, // Ifrit->player, no cast, single-target
        //??? = 461, // Ifrit->self, no cast, single-target, visual ???
        CrimsonCyclone = 11103, // Ifrit->self, 3.0s cast, range 44+R width 18 rect aoe
        CrimsonCycloneCross = 11104, // Helper->self, no cast, range 44+R width 10 rect aoe
        RadiantPlume = 11105, // Helper->location, 4.0s cast, range 8 circle aoe
        Hellfire = 11102, // Ifrit->self, 3.0s cast, raidwide
        VulcanBurst = 11095, // Ifrit->self, no cast, range 16+R circle aoe with knockback 15
        Incinerate = 11094, // Ifrit->self, no cast, range 10+R 120-degree cone 3-hit tankbuster
        InfernalFetters = 11289, // Helper->player, no cast, single-target, tether between two players
        InfernoHowl = 11099, // Ifrit->player, 2.0s cast, single-target, apply searing wind
        SearingWind = 11100, // Helper->location, no cast, range 14 circle around player with searing wind debuff, not including player himself
        Eruption = 11097, // Ifrit->self, 2.5s cast, single-target, visual
        EruptionAOE = 11098, // Helper->location, 3.0s cast, range 8 circle puddle
        InfernalSurge = 11096, // InfernalNail->self, no cast, raidwide on nail death
        FlamingCrush = 11101, // Ifrit->player, no cast, range 4 circle stack

        AutoAttackTitan = 872, // Titan->player, no cast, single-target
        Geocrush1 = 11517, // Titan->self, 3.0s cast, raidwide with 18 falloff
        EarthenFury = 11152, // Titan->self, 3.0s cast, raidwide
        RockBuster = 11106, // Titan->self, no cast, range 6+R ?-degree cone cleave
        MountainBuster = 11107, // Titan->self, no cast, range 11+R ?-degree cone cleave
        WeightOfTheLand = 11108, // Titan->self, 2.5s cast, single-target, visual
        WeightOfTheLandAOE = 11109, // Helper->location, 3.0s cast, range 6 circle puddle
        Geocrush2 = 11110, // Titan->self, 3.0s cast, raidwide with ? falloff
        //??? = 11112, // Titan->self, no cast, single-target, visual
        //??? = 11153, // Helper->self, no cast, range 40 circle
        Upheaval = 11111, // Titan->self, 4.0s cast, raidwide with knockback 24
        Bury = 11113, // BombBoulder->self, no cast, range 3+R circle aoe on spawn
        Burst = 11114, // BombBoulder->self, 3.5s cast, range 5+R circle aoe
        RockThrowBoss = 11115, // Titan->player, no cast, single-target, gaol target selection
        RockThrowHelper = 11116, // Helper->player, no cast, single-target, gaol target selection
        GraniteImpact = 11117, // GraniteGaol->self, 18.0s cast, wipe if gaols not destroyed in time
        Freefire = 11118, // GraniteGaol->self, no cast, range 6 circle aoe if gaol is destroyed by burst/freefire
        LandslideBoss = 11119, // Titan->self, 2.2s cast, range 40+R width 6 rect aoe
        LandslideHelper = 11120, // Helper->self, 2.2s cast, range 40+R width 6 rect aoe
        Tumult = 11288, // Titan->self, no cast, multi-hit raidwide
    };

    public enum SID : uint
    {
        InfernalFetters = 377, // none->player, extra=1-7
        SearingWind = 1578, // Ifrit->player, extra=0x0
        //ThermalLow = 1525, // SpinyPlume/Garuda->player, extra=0x1/0x2
        //ThermalHigh = 1526, // GarudaSister->player, extra=0x0
        //ThermalHigh = 380, // none->player, extra=0x0
        //Sleep = 1510, // SatinPlume->player, extra=0x0

        //AetheriallyCharged = 1528, // none->Garuda/Ifrit, extra=0x1/0x2/0x3
        Woken = 1529, // none->Garuda/Ifrit, extra=0x0
        //BeyondLimits = 1530, // none->player, extra=0x0
        //Woken = 1591, // none->Lahabrea, extra=0x0
    };

    public enum IconID : uint
    {
        MistralSong = 16, // player
        FlamingCrush = 117, // player
    };
}
