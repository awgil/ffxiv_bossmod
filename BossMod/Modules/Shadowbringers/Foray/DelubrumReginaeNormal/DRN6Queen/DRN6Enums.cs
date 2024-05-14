namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN6Queen;

public enum OID : uint
{
    Boss = 0x310B, // R7.920, x?
    Helper = 0x233C, // R0.500, x?, 523 type
    QueensKnight = 0x310C, // R2.800, x?
    QueensWarrior = 0x310D, // R2.800, x?
    QueensSoldier = 0x3110, // R4.000, x?
    QueensGunner = 0x3112, // R4.000, x?
    SoldierAvatar = 0x3111, // R4.000, x?
    AutomaticTurret = 0x3113, // R3.000, x?
    AetherialBolt = 0x310E, // R0.600, x?
    AetherialBurst = 0x310F, // R1.200, x?
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 23499, // Boss->location, no cast, single-target
    EmpyreanIniquityAOE = 22984, // Boss->self, 5.0s cast, range 60 circle raidwide
    NorthswainsGlow = 22979, // Boss->self, 3.0s cast, single-target, visual (three fire lines with aoes on intersections)
    NorthswainsGlowAOE = 22980, // Helper->self, 10.0s cast, range 20 circle aoe
    CleansingSlash = 22981, // Boss->player, 5.0s cast, single-target tankbuster

    QueensWill = 22969, // Boss->self, 5.0s cast, single-target, visual (easy chess start)
    QueensEdict = 22974, // Boss->self, 5.0s cast, single-target, visual (super chess start)
    QueensJustice = 22975, // Helper->self, no cast, range 60 circle raidwide (hitting players who failed their movement edict)

    BeckAndCallToArmsWillKW = 23449, // Boss->self, 5.0s cast, single-target, visual (tether knight & warrior and have them move)

    AboveBoard = 22993, // QueensWarrior->self, 6.0s cast, range 60 circle, visual (throw up)
    AboveBoardExtra = 23437, // Helper->self, 6.0s cast, range 60 circle, visual (???)
    Bombslinger = 23358, // QueensWarrior->self, 3.0s cast, single-target, visual (spawn bombs)
    DoubleGambit = 23001, // QueensSoldier->self, 3.0s cast, single-target, visual (summon 4 pawns)
    GodsSaveTheQueenAOE = 22985, // Boss->self, 5.0s cast, range 60 circle raidwide

    HeavensWrath = 22982, // Boss->self, 3.0s cast, single-target, single-target, visual (preparation for knockback)
    HeavensWrathKnockback = 22983, // Helper->self, 7.0s cast, range 60 width 100 rect, knockback 15

    JudgmentBladeL = 22977, // Boss->location, 5.0s cast, single-target, visual (half arena cleave)
    JudgmentBladeR = 22978, // Boss->location, 5.0s cast, single-target, visual (half arena cleave)
    JudgmentBladeLAOE = 23426, // Helper->self, 5.3s cast, range 70 width 30 rect
    JudgmentBladeRAOE = 23427, // Helper->self, 5.3s cast, range 70 width 30 rect
    LotsCastBigShort = 23431, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallShort = 22995, // AetherialBolt->location, no cast, range 10 circle
    LotsCastBigLong = 22994, // AetherialBurst->location, no cast, range 10 circle visual
    LotsCastSmallLong = 23430, // AetherialBolt->location, no cast, range 10 circle visual

    OptimalPlaySword = 22987, // QueensKnight->self, 5.0s cast, range 10 circle
    OptimalPlayShield = 22989, // QueensKnight->self, 6.0s cast, range ?-60 donut
    OptimalPlayCone = 22990, // Helper->self, 5.0s cast, single-target

    PawnOffReal = 23002, // SoldierAvatar->self, 5.0s cast, range 20 circle
    PawnOffFake = 23003, // SoldierAvatar->self, 5.0s cast, range 20 circle

    RelentlessPlay = 23036, // Boss->self, 5.0s cast, single-target, visual (mechanic start)
    ReversalOfForces = 22996, // QueensWarrior->self, 4.0s cast, single-target, visual (tethers/icons for players)
    ReversalOfForcesExtra = 23360, // Helper->self, 4.0s cast, single-target

    SecondPhaseModelChange = 21928, // Boss->self, no cast, single-target, visual
    SecretsRevealed = 23434, // QueensSoldier->self, 5.0s cast, single-target
    SecretsRevealedExtra = 23436, // SoldierAvatar->self, no cast, single-target

    ShieldOmen = 22987, // QueensKnight->self, 3.0s cast, single-target, apply shield-bearer status
    EndsKnight = 22970, // QueensKnight->self, 1.0s cast, range 60 width 10 cross
    EndsSoldier = 22972, // QueensSoldier->self, 1.0s cast, range 60 width 10 cross
    MeansWarrior = 22971, // QueensWarrior->self, 1.0s cast, range 60 width 10 cross
    MeansGunner = 22973, // QueensGunner->self, 1.0s cast, range 60 width 10 cross

    AutomaticTurretRP1 = 23006, // QueensGunner->self, 3.0s cast, single-target
    TurretsTour = 23007, // QueensGunner->self, 5.0s cast, single-target, visual (unseen statuses)
    TurretsTourAOE1 = 23008, // Helper->location, 5.0s cast, width 6 rect charge
    TurretsTourAOE2 = 23010, // AutomaticTurret->self, no cast, range 50 width 6 rect
    TurretsTourAOE3 = 23009, // AutomaticTurret->location, no cast, width 6 rect charge
}

public enum SID : uint
{
    MovementIndicator = 2056, // Boss->QueensKnight/QueensWarrior/Boss/AutomaticTurret, extra=0xE1/0xE2/0xE4/0xE3/0x111
    MovementHeavy = 1141, // none->QueensKnight, extra=0x32
    MovementSprint = 481, // none->QueensWarrior, extra=0x32
    MovementEdictShort2 = 2474, // none->player, extra=0x109
    MovementEdictShort4 = 2476, // none->player, extra=0x10B
    MovementEdictShort3 = 2475, // none->player, extra=0x10A
    MovementEdictLong2 = 2477, // none->player, extra=0x0
    MovementEdictLong3 = 2478, // none->player, extra=0x0
    MovementEdictLong4 = 2479, // none->player, extra=0x0
    YourMove2Squares = 2480, // none->player, extra=0x0
    YourMove4Squares = 2482, // none->player, extra=0x0
    YourMove3Squares = 2481, // none->player, extra=0x0
    MovementInProgress = 2552, // none->player, extra=0x104/0x106/0x105/0x103

    Stun = 149, // none->player, extra=0x0
    ReversalOfForces = 2447, // none->player, extra=0x0
    RightUnseen = 1707, // none->player, extra=0xE9
    LeftUnseen = 1708, // none->player, extra=0xEA
    BackUnseen = 1709, // none->player, extra=0xE8
    ShieldBearer = 2446, // QueensKnight->QueensKnight, extra=0x0
    AboveBoardPlayerLong = 2426, // none->player, extra=0x3E8
    AboveBoardPlayerShort = 2427, // none->player, extra=0x3E8
    AboveBoardBombShort = 2429, // none->AetherialBurst, extra=0x3E8
    AboveBoardBombLong = 2428, // none->AetherialBolt, extra=0x3E8
}

public enum IconID : uint
{
    PhysicalVulnerabilityDown = 136, // player
    MagicVulnerabilityDown = 137, // player
}

public enum TetherID : uint
{
    Tether_2 = 2, // QueensSoldier/QueensGunner/QueensWarrior/QueensKnight->Boss
    ReversalBomb = 16, // AetherialBolt/AetherialBurst->QueensWarrior
    SecretsRevealed = 30, // SoldierAvatar->QueensSoldier
}
