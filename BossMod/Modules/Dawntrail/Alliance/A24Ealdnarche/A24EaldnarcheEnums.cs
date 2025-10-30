namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

public enum OID : uint
{
    Boss = 0x460F, // R7.000, x1
    Helper = 0x233C, // R0.500, x36, Helper type
    OrbitalWind = 0x493B, // R1.000, x0 (spawn during fight)
    OrbitalLevin = 0x493A, // R1.500, x0 (spawn during fight)
    OrbitalFlame = 0x4939, // R1.300, x0 (spawn during fight)
    Unk1 = 0x4824, // R2.000, x2
    Unk2 = 0x49E2, // R5.000, x1
}

public enum AID : uint
{
    AutoAttack = 44359, // Boss->player, no cast, single-target
    UranosCascadeCast = 44369, // Boss->self, 4.0+1.0s cast, single-target
    UranosCascade = 44370, // Helper->player, 5.0s cast, range 6 circle
    CronosSlingCast1 = 44361, // Boss->self, 7.0s cast, single-target
    CronosSlingCast2 = 44362, // Boss->self, 7.0s cast, single-target
    CronosSlingCast3 = 44363, // Boss->self, 7.0s cast, single-target
    CronosSlingCast4 = 44364, // Boss->self, 7.0s cast, single-target
    CronosSlingOut = 44365, // Helper->self, 7.5s cast, range 9 circle
    CronosSlingIn = 44366, // Helper->self, 7.5s cast, range 6-70 donut
    CronosSlingLeft = 44367, // Helper->self, 13.3s cast, range 70 width 136 rect
    CronosSlingRight = 44368, // Helper->self, 13.3s cast, range 70 width 136 rect
    EmpyrealVortexCast = 44397, // Boss->self, 4.0+1.0s cast, single-target
    EmpyrealVortexVisual = 44398, // Helper->self, 4.3s cast, single-target
    EmpyrealVortexRaidwide = 44399, // Helper->self, no cast, range 75 circle
    EmpyrealVortexSpread = 44400, // Helper->players, 5.0s cast, range 5 circle
    EmpyrealVortexPuddle = 44401, // Helper->location, 5.0s cast, range 6 circle
    WarpCast = 44374, // Boss->self, 4.0s cast, single-target
    WarpTarget = 44375, // Helper->self, 4.0s cast, range 6 circle
    BossTeleport = 44360, // Boss->location, no cast, single-target
    Sleepga = 44376, // Boss->self, 3.0s cast, range 70 width 70 rect
    GaeaStreamCast = 44371, // Boss->self, 3.0+1.0s cast, single-target
    GaeaStreamFirst = 44372, // Helper->self, 4.0s cast, range 4 width 24 rect
    GaeaStreamRest = 44373, // Helper->self, 2.0s cast, range 4 width 24 rect
    OmegaJavelinCast = 44380, // Boss->self, 4.0+1.0s cast, single-target
    OmegaJavelinSpread = 44381, // Helper->location, no cast, range 6 circle
    OmegaJavelinRepeat = 44382, // Helper->location, 4.5s cast, range 6 circle
    DuplicateCast = 44407, // Boss->self, 2.0+1.0s cast, single-target
    DuplicateFast = 44408, // Helper->self, 0.7s cast, range 16 width 16 rect
    Excelsior = 45120, // Boss->self, 7.0s cast, range 35 circle
    StunVacuum = 44823, // Helper->self, 7.5s cast, range 100 circle
    PhaseShift1 = 44377, // Boss->self, 3.0s cast, single-target
    PhaseShift2 = 44334, // Boss->self, no cast, single-target
    PhaseShift3 = 44378, // Helper->self, 22.0s cast, range 100 circle
    TileSwap = 44379, // Helper->self, 13.0s cast, range 16 width 16 rect
    VisionsOfParadise = 44405, // Boss->self, 7.0+1.0s cast, single-target
    DuplicateSlow = 44406, // Helper->self, 1.0s cast, range 16 width 16 rect
    StellarBurstIndicator = 44413, // Helper->player, 4.0s cast, single-target
    StellarBurstCast = 44402, // Boss->self, 4.0+1.0s cast, single-target
    StellarBurst1 = 44403, // Helper->player, 5.0s cast, single-target
    StellarBurst2 = 44404, // Helper->location, 5.0s cast, range 24 circle
    Quake = 44388, // Helper->self, 6.0s cast, range 16 width 48 rect
    TornadoAttract = 44395, // Helper->self, 6.0s cast, range 35 circle
    AncientTriad = 44383, // Boss->self, 6.0s cast, single-target
    TornadoPuddle = 44341, // Helper->self, 7.0s cast, range 5 circle
    Burst = 44386, // Helper->location, 7.0s cast, range 5 circle
    TornadoHit = 44396, // Helper->self, no cast, range 3 circle
    Shock = 44387, // OrbitalLevin->player, no cast, single-target
    Freeze = 44389, // Helper->self, 6.0s cast, range 16 width 48 rect
    FlarePuddle = 44384, // Helper->location, 7.0s cast, range 5 circle
    FlareRect = 44385, // OrbitalFlame->location, 4.0s cast, range 70 width 6 rect
    FloodProximity = 44390, // Helper->self, 6.0s cast, range 35 circle
    Flood1 = 44391, // Helper->self, 10.0s cast, range 8 circle
    Flood2 = 44392, // Helper->self, 12.0s cast, range 8-16 donut
    Flood3 = 44393, // Helper->self, 14.0s cast, range 16-24 donut
    Flood4 = 44394, // Helper->self, 16.0s cast, range 24-36 donut
}

public enum IconID : uint
{
    TankbusterSpread = 344, // player->self
    Spread = 376, // player->self
    OmegaJavelin = 466, // player->self
    Target = 23, // player->self
    StellarBurst = 608, // Unk1->self
    LockOn = 210, // player->self
}

public enum TetherID : uint
{
    OrbitalLevin = 6, // OrbitalLevin->player
    TileSwap = 352, // Unk->Unk
}
