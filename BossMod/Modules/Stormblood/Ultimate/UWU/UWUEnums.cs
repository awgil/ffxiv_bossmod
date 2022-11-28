namespace BossMod.Stormblood.Ultimate.UWU
{
    public enum OID : uint
    {
        Garuda = 0x2212, // R1.700, x1
        GarudaSister = 0x2213, // R1.360, x2 (chirada & suparna)
        SpinyPlume = 0x2216, // R0.500, spawn during fight
        SatinPlume = 0x2215, // R0.500, spawn during fight
        SpinyShield = 0x1E8F68, // R0.500, EventObj type, spawn during fight

        Ifrit = 0x221A, // R5.000, x4
        InfernalNail = 0x221B, // R1.000-2.000, spawn during fight

        Titan = 0x2217, // R4.550, x1
        Lahabrea = 0x221C, // R0.500, x1
        TheUltimaWeapon = 0x221E, // R6.000, x1
        Helper = 0x233C, // R0.500, x14

        //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x8, EventObj type
        //_Gen_Actor1ea989 = 0x1EA989, // R0.500, EventObj type, spawn during fight
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
        Mesohigh = 11081, // Suparna->player, no cast, range 3 circle
        ThermalTumult = 11076, // SatinPlume->self, no cast, range 20 circle sleep if not killed in ~20s
        Cyclone = 11077, // SpinyPlume->player, no cast, single-target, mini-tankbuster with thermal low stack
        Gigastorm = 11078, // SpinyPlume->self, 3.0s cast, range 6+R circle aoe on death

        //_Weaponskill_CrimsonCyclone = 11103, // Ifrit->self, 3.0s cast, range 44+R width 18 rect
        //_Weaponskill_RadiantPlume = 11105, // Helper->location, 4.0s cast, range 8 circle
        //_Weaponskill_Hellfire = 11102, // Ifrit->self, 3.0s cast, range 30+R circle
        //_Weaponskill_VulcanBurst = 11095, // Ifrit->self, no cast, range 16+R circle
        //_Weaponskill_Incinerate = 11094, // Ifrit->self, no cast, range 10+R ?-degree cone
        //_AutoAttack_ = 11089, // Ifrit->player, no cast, single-target
        //__г‚¤гѓ•гѓЄгѓјгѓ€пјљз‚ЋзЌ„гЃ®жҐ”еЏ¬е–љ = 461, // Ifrit->self, no cast, single-target
        //_Ability_InfernalFetters = 11289, // Helper->player, no cast, single-target
        //_Ability_InfernoHowl = 11099, // Ifrit->player, 2.0s cast, single-target
        //_Weaponskill_Eruption = 11097, // Ifrit->self, 2.5s cast, single-target
        //_Ability_SearingWind = 11100, // Helper->location, no cast, ???
        //_Weaponskill_Eruption = 11098, // Helper->location, 3.0s cast, range 8 circle
        //_Weaponskill_InfernalSurge = 11096, // InfernalNail->self, no cast, range 50 circle
        //_Weaponskill_FlamingCrush = 11101, // Ifrit->player, no cast, range 4 circle
    };

    public enum IconID : uint
    {
        MistralSong = 16, // player
        //_Gen_Icon_117 = 117, // player
    };
}
