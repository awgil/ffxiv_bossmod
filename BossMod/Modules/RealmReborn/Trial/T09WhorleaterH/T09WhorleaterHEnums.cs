namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    public enum OID : uint
    {
        Boss = 0xA67, // R4.500, x1
        Tail = 0xA86, // R4.500, spawn during fight
        Spume = 0xA85, // R1.000, spawn during fight
        Sahagin = 0xA84, // R1.500, spawn during fight
        DangerousSahagins = 0xA83, // R1.500, spawn during fight
        Converter = 0x1E922A, // R2.000, x1, EventObj type
        HydroshotZone = 0x1E9230, // R0.500, EventObj type, spawn during fight
        DreadstormZone = 0x1E9231, // R0.500, EventObj type, spawn during fight
        Actor1e922f = 0x1E922F, // R2.000, x1, EventObj type
        AnotherSpinningDiveHelper = 0xA88, // R0.500, x14, 523 type
        Actor1e9229 = 0x1E9229, // R2.000, x1, EventObj type
        Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
        Leviathan_Unk2 = 0xACD, // R0.500, x4
        Actor1e922b = 0x1E922B, // R2.000, x1, EventObj type
        ElementalConverter = 0xB84, // R0.500, x1
        Exit = 0x1E850B, // R0.500, x1, EventObj type
        Actor1e9228 = 0x1E9228, // R2.000, x1, EventObj type
        SpinningDiveHelper = 0xA87, // R4.500, 523 type, spawn during fight
    };

    public enum AID : uint
    {
        BodySlamRectAOE = 1860, // Boss->self, no cast, range 30+R width 10 rect
        BodySlamNorth = 1938, // ACD->self, no cast, ???
        VeilOfTheWhorl = 2165, // Boss->self, no cast, single-target
        AutoAttack = 1853, // Boss->player, no cast, single-target
        MantleOfTheWhorl = 2164, // Tail->self, no cast, single-target
        ScaleDarts = 1857, // Tail->player, no cast, single-target
        AutoAttack2 = 870, // Sahagin->player, no cast, single-target
        AetherDraw = 1870, // Spume->B84, no cast, single-target
        StunShot = 1862, // Sahagin->player, no cast, single-target
        DreadTide = 1877, // A88->location, no cast, range 2 circle
        DreadTide_Boss = 1876, // Boss->self, no cast, single-target
        AquaBreath = 1855, // Boss->self, no cast, range 10+R circle
        TailWhip = 1856, // Tail->self, no cast, range 10+R circle
        Waterspout = 1859, // A88->location, no cast, range 4 circle
        Waterspout_Boss = 1858, // Boss->self, no cast, single-target
        Hydroshot = 1864, // Sahagin->location, 2.5s cast, range 5 circle
        TidalRoar = 1868, // A88->self, no cast, range 60+R circle
        TidalRoar_Boss = 1867, // Boss->self, no cast, single-target
        SpinningDiveSnapshot = 1869, // A87->self, no cast, range 46+R width 16 rect
        SpinningDiveEffect = 1861, // A88->self, no cast, range 46+R width 16 rect
        Splash = 1871, // Spume->self, no cast, range 50+R circle
        GrandFall = 1873, // A88->location, 3.0s cast, range 8 circle
        TidalWave = 1872, // Boss->self, no cast, range 60+R circle
        BodySlamSouth = 1937, // ACD->self, no cast, ???
        Dreadstorm = 1865, // Wavetooth Sahagin --> location, 3.0s cast, range 50, 6 circle
        Ruin = 2214, // Sahagin-->player --> 1.0s cast time
    };

    public enum SID : uint
    {
        VeilOfTheWhorl = 478, // Boss->A88/Boss/A87, extra=0x64
        MantleOfTheWhorl = 477, // Tail->Tail, extra=0x64
        Paralysis = 17, // Sahagin->player, extra=0x0
        Invincibility = 775, // none->A88/Boss/Tail/A87, extra=0x0
        Dropsy = 272, // none->player, extra=0x0
        Heavy = 14, // A88->player, extra=0x19
        Hysteria = 296 // from dreadstorm AOE
    };
}
