namespace BossMod.Endwalker.Alliance.A13Azeyma
{
    public enum OID : uint
    {
        Boss = 0x38D1,
        AzeymasHeat = 0x38D2, // x6
        WardensFlame = 0x38D3, // x2
        Sunstorm = 0x38D4, // spawn during fight after solar wings cast
        ProdigalSun = 0x38D5, // spawn during fight
        Helper = 0x233C, // x33
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        Teleport = 28834, // Boss->location, no cast
        FanFlames = 28800, // Boss->self, no cast, visual?

        WardensWarmth = 28830, // Boss->self, 5.0s cast, tankbuster visual
        WardensWarmthAOE = 28831, // Helper->players, 5.0s cast, range 6 aoe on 3 tanks

        WardensProminence = 28832, // Boss->self, 6.0s cast, raidwide visual
        WardensProminenceAOE = 28833, // Helper->self, 6.0s cast, raidwide

        SolarWings = 28801, // Boss->self, 4.0s cast
        SolarWingsL = 28802, // Helper->self, 4.0s cast, range 30 135-degree +90 rotated cone
        SolarWingsR = 28803, // Helper->self, 4.0s cast, range 30 135-degree -90 rotated cone
        SolarFlair = 28804, // Sunstorm->self, 1.0s cast, range 15 aoe
        SunShine = 28805, // Boss->self, 3.0s cast, visual (shows AzeymasHeat adds)
        TeleportHeat = 29461, // AzeymasHeat->location, no cast
        HauteAirWings = 28806, // AzeymasHeat->self, 5.0s cast (moves Sunstorm by 18)
        MoveSunstorm = 28807, // Helper->Sunstorm, no cast

        SolarFans = 28813, // Boss->self, 4.0s cast, visual
        SolarFansAOE = 29374, // Helper->location, 4.0s cast, 5 half-width rect between source and target ?
        SolarFansCharge = 28814, // WardensFlame->location, 4.5s cast, 5 half-width rect between source and target ?
        RadiantRhythmFirst = 28815, // Boss->self, 5.0s cast, visual
        RadiantRhythmRest = 28816, // Boss->self, no cast, visual
        RadiantFlight = 28819, // Helper->self, no cast, range 20?-30 donut 90-degree cone, starting from flame and CCW
        TeleportFlame = 28818, // WardensFlame->location, no cast
        RadiantFinish = 28817, // Boss->self, 3.0s cast, visual
        RadiantFlourish = 28820, // WardensFlame->self, 3.0s cast, range 25 aoe
        FinishFans  = 29360, // Boss->self, no cast, visual

        FleetingSpark = 28828, // Boss->self, 5.5s cast, range 60 270-degree cone

        SolarFold = 28808, // Boss->self, 4.0s cast, visual
        SolarFoldAOE = 29166, // Helper->self, 4.0s cast, range 30 half-width 5 cross rects
        FoldingFlareGrowing = 28809, // Helper->self, no cast, growing half-width 2.5 rect
        FoldingFlareFull = 29468, // Helper->self, no cast, range 30 half-width 2.5 rect
        HauteAirFlare = 28810, // AzeymasHeat->self, 12.0s cast
        DancingFlameFirst = 28811, // Helper->self, 0.5s cast, range 30 half-width 15 rect
        DancingFlameRest = 28812, // Helper->self, no cast, range 30 half-width 15 rect

        WildfireWard = 28826, // Boss->self, 5.0s cast, visual
        IlluminatingGlimpse = 28827, // AzeymasHeat->self, 11.0s cast, knockback 15

        NobleDawn = 28821, // Boss->self, 4.0s cast, visual
        SunbeamStart = 28822, // Helper->location, no cast, range 3 visual
        Sunbeam = 28823, // Helper->self, 6.0s cast, range 9 aoe
        SublimeSunset = 28824, // Boss->self, 9.0s cast
        SublimeSunsetAOE = 28825, // ProdigalSun->location, 9.5s cast, range 60 aoe with ? falloff
    };

    public enum SID : uint
    {
        None = 0,
    }

    public enum TetherID : uint
    {
        None = 0,
    }

    public enum IconID : uint
    {
        None = 0,
    }
}
