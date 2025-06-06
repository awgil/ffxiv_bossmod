namespace BossMod.Dawntrail.Foray.ForkedTower.FT01DemonTablet;

public enum OID : uint
{
    Boss = 0x470A,
    Helper = 0x233C,
    DemonSpawn = 0x470D, // R1.200, x6
    SummonedDemon = 0x470B, // R1.500, x0 (spawn during fight)
    SummonedArchdemon = 0x470C, // R2.000, x0 (spawn during fight)

    Meteor = 0x1EBD76,
}

public enum AID : uint
{
    AutoAttackVisual = 41737, // Boss->self, no cast, single-target
    AutoAttack = 41738, // Helper->player, no cast, single-target
    DemonicDarkIICast = 41734, // Boss->self, 5.0s cast, single-target
    DemonicDarkII = 41903, // Helper->self, no cast, ???
    RayOfDangersNear = 41715, // Boss->self, 10.0s cast, single-target
    RayOfExpulsionAfar = 41716, // Boss->self, 10.0s cast, single-target
    LandingBoss = 41812, // Helper->self, 10.5s cast, range 6 width 30 rect
    LandingNear = 43293, // Helper->self, 10.5s cast, range 15 width 30 rect
    LandingKBVisual = 43794, // Helper->self, 10.5s cast, range 30 width 30 rect
    LandingKnockback = 43294, // Helper->self, 10.5s cast, range 30 width 30 rect
    RayOfIgnorance = 41717, // Helper->self, 10.5s cast, range 30 width 30 rect

    OccultChiselCast = 41735, // Boss->self, 8.5s cast, single-target
    OccultChisel = 41736, // Helper->player, 8.5s cast, range 5 circle

    DemonographOfDangersNear = 41718, // Boss->self, 10.0s cast, single-target
    DemonographOfExpulsionAfar = 41719, // Boss->self, 10.0s cast, single-target

    Explosion = 41720, // Helper->self, 15.0s cast, range 4 circle

    RotateRight = 41729, // Boss->self, 8.0s cast, single-target
    RotateLeft = 41730, // Boss->self, 8.0s cast, single-target
    RotationCone = 41731, // Helper->self, 8.8s cast, range 37 90-degree cone
    RotationBoss1 = 41732, // Helper->self, 8.8s cast, range 33 width 3 rect
    RotationBoss2 = 41733, // Helper->self, 8.8s cast, range 33 width 3 rect

    LacunateStream = 41728, // Boss->self, no cast, single-target
    LacunateStreamHelper = 41726, // Helper->self, no cast, range 34 width 30 rect
    LacunateStreamCast = 41725, // Helper->self, 1.0s cast, range 31 width 30 rect

    CometeorOfDangersNear = 41700, // Boss->self, 10.0s cast, single-target
    CometeorOfExpulsionAfar = 41701, // Boss->self, 10.0s cast, single-target

    PortentousCometKBNorth = 41703, // Helper->players, 17.0s cast, range 4 circle
    PortentousCometKBSouth = 41704, // Helper->players, 17.0s cast, range 4 circle
    PortentousCometeor = 41702, // Helper->self, 12.0s cast, range 43 circle

    SummonRect = 41741, // Boss->self, 10.0s cast, range 36 width 30 rect

    AutoAttackDemon = 41811, // SummonedArchdemon->player, no cast, single-target
    Summon = 41705, // Boss->self, 4.0s cast, single-target
    Demonography = 41710, // Boss->self, 4.0s cast, single-target
    GravityOfDangersNear = 41706, // Boss->self, 10.0s cast, single-target
    GravityOfExpulsionAfar = 43521, // Boss->self, 10.0s cast, single-target
    EraseGravity = 41707, // Helper->self, 14.0s cast, range 4 circle
    RestoreGravity = 41708, // Boss->self, 12.0s cast, single-target
    LandingStatue = 41709, // Helper->self, 14.0s cast, range 18 circle

    ExplosionGround = 41711, // Helper->self, 21.0s cast, range 4 circle
    UnmitigatedExplosionGround = 41712, // Helper->self, no cast, single-target
    ExplosionAir = 41713, // Helper->self, 21.0s cast, range 4 circle
    UnmitigatedExplosionAir = 41714, // Helper->self, no cast, single-target
    UnmitigatedExplosion = 43034, // Helper->self, no cast, ???
}

public enum IconID : uint
{
    Tankbuster = 497, // player->self
    StackKBNorth = 574, // player->self
    StackKBSouth = 575, // player->self
}

public enum SID : uint
{
    DefyingGravity = 4353, // Helper->player, extra=0x258, levitate
    CraterLater = 4354, // none->player, extra=0x0, meteor icon
    DarkDefenses = 4355, // none->SummonedArchDemon, extra=0x0, damage down
}

public enum TetherID : uint
{
    DemonTether = 217, // DemonSpawn->Boss
}
