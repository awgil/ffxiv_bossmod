#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

public enum OID : uint
{
    Boss = 0x4DC1, // R6.000, x1
    Helper = 0x233C, // R0.500, x28, Helper type
    YawningVoid1 = 0x4DC2, // R1.000, x2
    YawningVoid2 = 0x4DC3, // R1.000, x2
    BossVoidTether = 0x4DB8, // R5.000, x2
    VoidWildChargeBig = 0x4BFA, // R2.000, x0 (spawn during fight), Helper type
    VoidWildChargeSmall = 0x4BF7, // R1.000, x0 (spawn during fight), Helper type
    VoidGazeSmall = 0x4DC5, // R0.850, x0 (spawn during fight)
    VoidGazeBig = 0x4DC6, // R1.750, x0 (spawn during fight)
    VoidVacuum = 0x4DC4, // R0.850, x0 (spawn during fight), Helper type
    VoidChase = 0x4EB5, // R0.850, x0 (spawn during fight), Helper type
    LoomingShadow = 0x4DC7, // R12.500, x0 (spawn during fight)
    ProtectiveShadow = 0x4DC8, // R5.000, x0 (spawn during fight)
    AggressiveShadow = 0x4DC9, // R5.000, x0 (spawn during fight)
    SoothingShadow = 0x4DCA, // R5.000, x0 (spawn during fight)
    BeaconInTheDark = 0x4DCB, // R5.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 49937, // Boss->player, no cast, single-target
    Meteorain = 50049, // Boss->self, 5.0s cast, range 40 circle
    BossJump = 49927, // Boss->location, no cast, single-target

    NaughtGrowsCast1 = 49975, // Boss->self, 7.0+1.0s cast, single-target
    NaughtGrowsCast2 = 49976, // Boss->self, 7.0+1.0s cast, single-target
    NaughtGrowsCircleBig = 49977, // YawningVoid1->self, 8.0s cast, range 40 circle
    NaughtGrowsDonutBig = 49978, // YawningVoid1->self, 8.0s cast, range 40-60 donut
    NaughtGrowsCircleSmall = 49979, // Helper->self, 8.0s cast, range 12 circle
    NaughtGrowsDonutSmall = 49980, // Helper->self, 8.0s cast, range 6-40 donut
    ReturnToNothing = 49983, // VoidWildChargeSmall->location, no cast, width 6 rect charge
    GreatReturnToNothing = 49984, // VoidWildChargeBig->location, no cast, width 6 rect charge

    NaughtWakes = 49973, // Boss->self, 2.0+1.0s cast, single-target
    YawningVoidTeleport = 49974, // YawningVoid1->location, no cast, single-target
    MeltdownCast = 50040, // Boss->self, 4.0+1.0s cast, range 40 circle
    MeltdownPuddle = 50041, // Helper->location, 4.5s cast, range 5 circle
    MeltdownSpread = 50042, // Helper->players, 5.5s cast, range 5 circle

    AiryEmptinessCast = 50032, // Boss->self, 4.0+1.0s cast, single-target
    DenseEmptiness = 50033, // Boss->self, 4.0+1.0s cast, single-target
    AiryEmptinessProtean = 50034, // Helper->self, no cast, range 60 60-degree cone
    DenseEmptinessProtean = 50035, // Helper->self, no cast, range 60 100-degree cone

    GazeOfTheVoidCast = 50002, // Boss->self, 6.0+1.0s cast, single-target
    GazeOfTheVoidSpawn1 = 50003, // Helper->self, 7.0s cast, single-target
    GazeOfTheVoidSpawn2 = 50004, // Helper->self, 7.0s cast, single-target
    GazeOfTheVoid = 50005, // Helper->self, 7.0s cast, range 40 45-degree cone
    Burst = 50006, // VoidGazeSmall->self, no cast, range 5 circle
    ViolentBurst = 50007, // VoidGazeBig->self, no cast, range 6 circle

    VacuumCast = 49994, // Boss->self, 2.0+1.0s cast, single-target
    SilentTorrentAppear1 = 49995, // VoidVacuum->location, 3.5s cast, single-target
    SilentTorrentAppear2 = 49996, // VoidVacuum->location, 3.5s cast, single-target
    SilentTorrentAppear3 = 49997, // VoidVacuum->location, 3.5s cast, single-target
    SilentTorrentSmall = 49998, // Helper->self, 4.0s cast, range 17-19 20-degree cone
    SilentTorrentMedium = 49999, // Helper->self, 4.0s cast, range 17-19 40-degree cone
    SilentTorrentLarge = 50000, // Helper->self, 4.0s cast, range 17-19 60-degree cone
    Vacuum = 50001, // VoidVacuum->self, 1.5s cast, range 7 circle

    DeepFreezeRaidwide = 50043, // Boss->self, 5.0+1.0s cast, range 40 circle
    DeepFreezeFlare = 50044, // Helper->players, 6.0s cast, range 40 circle

    AllForNaught = 50010, // Boss->self, 5.0s cast, single-target, intermission

    ProtectiveAuto = 50751, // ProtectiveShadow->player, no cast, single-target
    AggressiveAuto = 50752, // AggressiveShadow->player, no cast, single-target
    SoothingAuto = 50753, // SoothingShadow->player, no cast, single-target
    LoomingAuto = 50016, // LoomingShadow->player, no cast, single-target
    TowerShadowUnk1 = 50012, // AggressiveShadow/ProtectiveShadow/SoothingShadow->self, no cast, single-target
    TowerShadowUnk2 = 49938, // AggressiveShadow/SoothingShadow/ProtectiveShadow/LoomingShadow->self, no cast, single-target
    LoomingEmptinessVisual = 50011, // LoomingShadow->self, 5.0s cast, single-target
    LoomingEmptinessAOE = 49369, // Helper->self, 6.0s cast, range 8 circle
    LoomingEmptinessKB = 49982, // Helper->self, 6.0s cast, range 100 circle
    VoidalTurbulenceCast = 50036, // LoomingShadow->self, 6.0+1.0s cast, single-target
    VoidalTurbulenceProtean = 50038, // Helper->self, no cast, range 60 60-degree cone
    EmptyShadow = 50013, // Helper->self, 7.0s cast, range 6 circle, tower
    DemonEyeCast = 50022, // AggressiveShadow->self, 4.0+1.0s cast, single-target
    DemonEye = 50023, // Helper->self, 5.0s cast, range 20 circle
    CurseOfTheFleshCast = 50024, // SoothingShadow->self, 2.0+1.0s cast, single-target
    CurseOfTheFlesh = 50025, // Helper->player, 3.0s cast, single-target, applies cleansable Disease
    WeightOfNothingCast = 50020, // ProtectiveShadow->self, 4.0+1.0s cast, single-target
    WeightOfNothing = 50021, // Helper->player, 5.0s cast, range 100 width 8 rect, tankbuster
    Nothingness = 50017, // ProtectiveShadow/AggressiveShadow/SoothingShadow->self, 3.0s cast, range 100 width 4 rect

    DrainTouchCast = 50018, // ProtectiveShadow->self, 5.0s cast, single-target, interruptible
    DrainTouch = 50019, // Helper->player, no cast, single-target, lifesteal
    SelfDestruct = 50799, // AggressiveShadow/SoothingShadow->self, 5.0s cast, range 60 circle

    LightlessWorldCast = 50029, // Boss->self, 10.0s cast, single-target
    LightlessWorldInitial = 50030, // Helper->self, no cast, range 40 circle
    LightlessWorldLast = 50031, // Helper->self, no cast, range 40 circle
    Almagest = 49972, // Boss->self, 5.0s cast, range 40 circle

    PassageOfNaught1 = 49985, // YawningVoid1->self, 7.0s cast, range 80 width 16 rect
    PassageOfNaught2 = 49986, // YawningVoid2->self, 6.0s cast, range 80 width 16 rect
    PassageOfNaught3 = 49987, // Helper->self, 6.0s cast, range 80 width 16 rect

    ShroudedHolyCast = 50045, // Boss->self, 5.0+1.0s cast, single-target
    ShroudedHoly = 50046, // Helper->players, 6.0s cast, range 6 circle

    DimensionZeroCast = 50047, // Boss->self, 5.0s cast, single-target
    DimensionZero = 50048, // Boss->self, no cast, range 60 width 8 rect

    NaughtHunts = 49992, // Boss->self, 6.0+1.0s cast, single-target
    EndlessChaseFirst = 48475, // VoidChase->self, 6.0s cast, range 6 circle
    EndlessChaseRest = 49993, // VoidChase->location, no cast, range 6 circle

    AlmagestEnrage = 50050, // Boss->self, 9.0s cast, range 40 circle
    Unk = 50015, // Helper->player, no cast, single-target
    BigBurst1 = 50008, // VoidGazeSmall->self, no cast, range 60 circle
    BigBurst2 = 50009, // VoidGazeBig->self, no cast, range 60 circle
    BigBurstIntermission = 50014, // Helper->self, no cast, range 60 circle, tower fail
    Apeiron = 48478, // Helper->player, no cast, single-target, intermission enrage (if gauge fills)
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // VoidWildChargeSmall/Helper/VoidWildChargeBig/VoidGazeSmall/VoidGazeBig->player, extra=0x0
    DarkResistanceDownII = 3323, // Helper->player, extra=0x0
    ChainsOfCondemnation = 4562, // Boss->player, extra=0x0
    Unk1 = 2234, // none->VoidGazeSmall/VoidGazeBig, extra=0x58/0x4B
    Unk2 = 2056, // none->AggressiveShadow/SoothingShadow/ProtectiveShadow/LoomingShadow, extra=0x46B
    FreezingUp = 3523, // Boss->player, extra=0x0
    DeepFreeze = 4150, // Boss->player, extra=0x0
    Unbecoming = 4882, // none->player, extra=0x0, dot
    GauntletTaken1 = 5357, // none->player, extra=0x0
    GauntletTaken2 = 5358, // none->player, extra=0x0
    GauntletTaken3 = 5359, // none->player, extra=0x0
    GauntletTaken4 = 5360, // none->player, extra=0x0
    GauntletTaken5 = 5361, // none->player, extra=0x0
    GauntletTaken6 = 5362, // none->player, extra=0x0
    GauntletTaken7 = 5363, // none->player, extra=0x0
    GauntletTaken8 = 5364, // none->player, extra=0x0
    GauntletThrown1 = 5365, // none->ProtectiveShadow, extra=0x0
    GauntletThrown2 = 5366, // none->ProtectiveShadow, extra=0x0
    GauntletThrown3 = 5367, // none->SoothingShadow, extra=0x0
    GauntletThrown4 = 5368, // none->SoothingShadow, extra=0x0
    GauntletThrown5 = 5369, // none->AggressiveShadow, extra=0x0
    GauntletThrown6 = 5370, // none->AggressiveShadow, extra=0x0
    GauntletThrown7 = 5371, // none->AggressiveShadow, extra=0x0
    GauntletThrown8 = 5372, // none->AggressiveShadow, extra=0x0
    Petrification = 3007, // Helper->player, extra=0x0, gaze failure
    QuantumEntanglement = 4884, // none->player, extra=0x0, dot
    QuantumNullification = 4883, // none->SoothingShadow, extra=0x0, dot
    Disease = 3943, // Helper->player, extra=0x32
    InEvent = 1268, // none->player, extra=0x0
    SustainedDamage = 4149, // VoidGazeSmall->player, extra=0x1/0x2, from soak failure
}

public enum IconID : uint
{
    NaughtGrowsSmall = 701, // Boss->player, small ball
    NaughtGrowsBig = 702, // Boss->player, big ball
    Flare = 327, // player->self
    MarkerGeneric1 = 172, // player->self
    MarkerGeneric2 = 721, // player->self
    LineBuster = 471, // player->self
    Stack = 318, // player->self
    LineStack = 719, // Boss->player
}

public enum TetherID : uint
{
    GrowsLine = 430, // player->Boss, line
    CircleToSource = 393, // BossVoidTether->Boss, circle, target -> source
    CircleFromSource = 394, // BossVoidTether->Boss, circle, source -> target
    DonutToSource = 395, // BossVoidTether->Boss, donut, target -> source
    DonutFromSource = 396, // BossVoidTether->Boss, donut, source -> target
    VoidChasePlayer = 404, // VoidChase->player, black line
    PlayerChasePlayer = 405, // player->player, chaser pass
    ClockSlow = 406, // VoidGazeSmall/VoidGazeBig->Boss, slow clock
    ClockFast = 407, // VoidGazeSmall/VoidGazeBig->Boss, fast clock
    TowerLine = 284, // AggressiveShadow/ProtectiveShadow/SoothingShadow->player, line
}
