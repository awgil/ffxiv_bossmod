namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

public enum OID : uint
{
    Boss = 0x3114, // R7.920, x1
    Helper = 0x233C, // R0.500, x10
    QueensKnight = 0x3115, // R2.800, x1
    QueensWarrior = 0x3118, // R2.800, x1
    QueensSoldier = 0x311C, // R4.000, x1
    QueensGunner = 0x311E, // R4.000, x1
    SoldierAvatar = 0x311D, // R4.000, spawn during fight
    BallLightning = 0x2FB0, // R2.000, spawn during fight
    AutomaticTurret = 0x311F, // R3.000, spawn during fight
    AetherialSphere = 0x3117, // R2.500, spawn during fight
    AetherialBolt = 0x3119, // R0.600, spawn during fight (small bomb)
    AetherialBurst = 0x311A, // R1.200, spawn during fight (big bomb)
    ProtectiveDome = 0x1EB12C, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 23499, // Boss->location, no cast, single-target
    EmpyreanIniquity = 23033, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    EmpyreanIniquityAOE = 23415, // Helper->self, 5.0s cast, range 60 circle raidwide
    NorthswainsGlow = 23027, // Boss->self, 3.0s cast, single-target, visual (three fire lines with aoes on intersections)
    NorthswainsGlowAOE = 23028, // Helper->self, 10.0s cast, range 20 circle aoe
    CleansingSlashFirst = 23029, // Boss->player, 5.0s cast, single-target tankbuster
    CleansingSlashSecond = 23480, // Boss->player, no cast, single-target tankbuster

    QueensWill = 23014, // Boss->self, 5.0s cast, single-target, visual (easy chess start)
    QueensEdict = 23020, // Boss->self, 5.0s cast, single-target, visual (super chess start)
    BeckAndCallToArmsWillKW = 23449, // Boss->self, 5.0s cast, single-target, visual (tether knight & warrior and have them move)
    BeckAndCallToArmsWillSG = 23450, // Boss->self, 5.0s cast, single-target, visual (tether soldier & gunner and have them move)
    BeckAndCallToArmsEdictKW = 23451, // Boss->self, 16.3s cast, single-target, visual (tether knight & warrior and have them move)
    BeckAndCallToArmsEdictSG = 23452, // Boss->self, 8.7s cast, single-target, visual (tether soldier & gunner and have them move)
    EndsKnight = 23015, // QueensKnight->self, 1.0s cast, range 60 width 10 cross
    MeansWarrior = 23016, // QueensWarrior->self, 1.0s cast, range 60 width 10 cross
    EndsSoldier = 23017, // QueensSoldier->self, 1.0s cast, range 60 width 10 cross
    MeansGunner = 23018, // QueensGunner->self, 1.0s cast, range 60 width 10 cross
    QueensJustice = 23019, // Helper->self, no cast, range 60 circle raidwide (hitting players who failed their movement edict)
    GunnhildrsBlades = 23329, // Boss->self, 14.0s cast, single-target, visual (hit players not in safespot)
    GunnhildrsBladesExtra = 23021, // Helper->self, no cast, single-target, visual ?
    GunnhildrsBladesAOE = 23330, // Helper->self, no cast, range 60 circle raidwide on players outside safespot

    SecondPhaseModelChange = 21928, // Boss->self, no cast, single-target, visual
    GodsSaveTheQueen = 23034, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    GodsSaveTheQueenAOE = 21487, // Helper->self, 2.0s cast, range 60 circle raidwide
    UnluckyLotBallLightning = 21942, // BallLightning->self, no cast, range 8 circle reflectable aoe
    MaelstromsBolt = 23023, // Boss->self, no cast, single-target
    MaelstromsBoltAOE = 21486, // Helper->self, 2.0s cast, range 60 circle raidwide (should stay under dome)
    RelentlessPlay = 23036, // Boss->self, 5.0s cast, single-target, visual (mechanic start)

    ReversalOfForcesExtra = 23361, // Helper->self, 4.0s cast, single-target, visual?
    ReversalOfForces = 23054, // QueensWarrior->self, 4.0s cast, single-target, visual (tethers/icons for players)
    AutomaticTurretRP1 = 23083, // QueensGunner->self, 3.0s cast, single-target, visual (spawn turrets)
    Reading = 23084, // QueensGunner->self, 3.0s cast, single-target, visual (unseen statuses)
    WindsOfWeight = 23055, // QueensWarrior->self, 6.0s cast, single-target, visual (wind/gravity aoes)
    WeightOfFortune = 23056, // Helper->self, 6.0s cast, range 20 circle (purple: small hit unless player has reversal)
    WindsOfFate = 23057, // Helper->self, 6.0s cast, range 20 circle (green: small hit if player has reversal)
    QueensShot = 23085, // QueensGunner->self, 7.0s cast, range 60 circle, visual (unseen resolve)
    QueensShotAOE = 23086, // Helper->self, 7.0s cast, range 60 circle, raidwide that requires directing unseen side to caster
    TurretsTourUnseen = 23087, // AutomaticTurret->self, 3.0s cast, range 50 width 5 rect aoe that requires directing unseen side to caster

    SwordOmen = 23037, // QueensKnight->self, 3.0s cast, single-target, apply sword-bearer status
    ShieldOmen = 23038, // QueensKnight->self, 3.0s cast, single-target, apply shield-bearer status
    DoubleGambit = 23067, // QueensSoldier->self, 5.0s cast, single-target, visual (summon 4 pawns)
    OptimalOffensive = 23043, // QueensKnight->location, 7.0s cast, width 5 rect charge
    OptimalOffensiveKnockback = 23044, // Helper->self, 7.0s cast, range 60 circle knockback 10
    OptimalOffensiveMoveSphere = 23045, // Helper->AetherialSphere, 7.0s cast, single-target, attract
    UnluckyLotAetherialSphere = 23046, // AetherialSphere->self, 1.0s cast, range 20 circle
    JudgmentBladeL = 23025, // Boss->location, 7.0s cast, width 30 rect charge, visual (half arena cleave)
    JudgmentBladeR = 23026, // Boss->location, 7.0s cast, width 30 rect charge, visual (half arena cleave)
    JudgmentBladeLAOE = 23428, // Helper->self, 7.3s cast, range 70 width 30 rect aoe
    JudgmentBladeRAOE = 23429, // Helper->self, 7.3s cast, range 70 width 30 rect
    SecretsRevealed = 23435, // QueensSoldier->self, 5.0s cast, single-target, visual (tether 2 pawns out of 4)
    SecretsRevealedExtra = 23436, // SoldierAvatar->self, no cast, single-target, visual?
    PawnOffReal = 23069, // SoldierAvatar->self, 7.0s cast, range 20 circle aoe
    PawnOffFake = 23070, // SoldierAvatar->self, 7.0s cast, range 20 circle fake aoe

    AutomaticTurretRP3 = 23078, // QueensGunner->self, 3.0s cast, single-target, visual (spawn turrets)
    OptimalPlaySword = 23039, // QueensKnight->self, 5.0s cast, range 10 circle
    OptimalPlayShield = 23040, // QueensKnight->self, 5.0s cast, range 5-60 donut
    OptimalPlayCone = 23041, // Helper->self, 5.0s cast, range 60 270-degree cone
    TurretsTour = 23079, // QueensGunner->self, 5.0s cast, single-target, visual (turret aoes)
    TurretsTourAOE1 = 23080, // Helper->location, 5.0s cast, width 6 rect charge aoe
    TurretsTourAOE2 = 23082, // AutomaticTurret->self, no cast, range 50 width 6 rect
    TurretsTourAOE3 = 23081, // AutomaticTurret->location, no cast, width 6 rect charge

    Bombslinger = 23359, // QueensWarrior->self, 3.0s cast, single-target, visual (spawn bombs)
    HeavensWrath = 23030, // Boss->self, 3.0s cast, single-target, visual (preparation for knockback)
    HeavensWrathKnockback = 23031, // Helper->self, 5.0s cast, range 60 width 100 rect, knockback 15
    HeavensWrathVisual = 23032, // Helper->self, 5.0s cast, single-target, visual (knockback)
    FieryPortent = 23073, // QueensSoldier->self, 6.0s cast, range 60 circle, visual (pyretic buff application)
    IcyPortent = 23074, // QueensSoldier->self, 6.0s cast, range 60 circle, visual (move to avoid effect)
    AboveBoard = 23051, // QueensWarrior->self, 6.0s cast, range 60 circle, visual (throw up)
    AboveBoardExtra = 23438, // Helper->self, 6.0s cast, range 60 circle, visual (???)
    LotsCastBigShort = 23433, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallShort = 23053, // AetherialBolt->location, no cast, range 10 circle
    LotsCastBigLong = 23052, // AetherialBurst->location, no cast, range 10 circle visual
    LotsCastSmallLong = 23432, // AetherialBolt->location, no cast, range 10 circle visual
    LotsCastLong = 23478, // Helper->location, 1.2s cast, range 10 circle

    SoftEnrageW = 23062, // QueensWarrior->self, 5.0s cast, range 60 circle, visual (raidwide)
    SoftEnrageWAOE = 23412, // Helper->self, 5.0s cast, range 60 circle, raidwide
    SoftEnrageK = 23048, // QueensKnight->self, 5.0s cast, range 60 circle, visual (raidwide)
    SoftEnrageKAOE = 23411, // Helper->self, 5.0s cast, range 60 circle, raidwide
    SoftEnrageG = 23093, // QueensGunner->self, 5.0s cast, range 60 circle, visual (raidwide)
    SoftEnrageGAOE = 23414, // Helper->self, 5.0s cast, range 60 circle, raidwide
    SoftEnrageS = 23075, // QueensSoldier->self, 5.0s cast, range 60 circle, visual (raidwide)
    SoftEnrageSAOE = 23413, // Helper->self, 5.0s cast, range 60 circle, raidwide
}

public enum SID : uint
{
    MovementIndicator = 2056, // Boss->QueensKnight/QueensSoldier/QueensWarrior/QueensGunner/Boss/AutomaticTurret, extra=0xE1 (no indicator)/0xE2 (move 1 square)/0xE3 (move 2 squares)/0xE4 (move 3 squares)/0x111
    MovementHeavy = 1141, // none->QueensKnight, extra=0x32 (move 1 square)
    MovementSprint = 481, // none->QueensSoldier/QueensWarrior/QueensGunner/QueensKnight, extra=0x0 (move 2 squares)/0x32 (move 3 squares)
    MovementEdictShort2 = 2474, // none->player, extra=0x0
    MovementEdictShort3 = 2475, // none->player, extra=0x0
    MovementEdictShort4 = 2476, // none->player, extra=0x0
    MovementEdictLong2 = 2477, // none->player, extra=0x0
    MovementEdictLong3 = 2478, // none->player, extra=0x0
    MovementEdictLong4 = 2479, // none->player, extra=0x0
    YourMove2Squares = 2480, // none->player, extra=0x0
    YourMove3Squares = 2481, // none->player, extra=0x0
    YourMove4Squares = 2482, // none->player, extra=0x0
    MovementInProgress = 2552, // none->player, extra=0x105/0x106/0x104/0x103
    Stun = 149, // none->player, extra=0x0
    ReversalOfForces = 2447, // none->player, extra=0x0
    RightUnseen = 1707, // none->player, extra=0xE9
    LeftUnseen = 1708, // none->player, extra=0xEA
    BackUnseen = 1709, // none->player, extra=0xE8
    ShieldBearer = 2446, // QueensKnight->QueensKnight, extra=0x0
    AboveBoardPlayerLong = 2426, // none->player, extra=0x3E8
    AboveBoardPlayerShort = 2427, // none->player, extra=0x3E8
    AboveBoardBombLong = 2428, // none->AetherialBurst/AetherialBolt, extra=0x3E8
    AboveBoardBombShort = 2429, // none->AetherialBolt/AetherialBurst, extra=0x3E8
}
