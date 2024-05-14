namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN3QueensGuard;
public enum OID : uint
{
    Helper = 0x233C, // R0.500, x9, 523 type
    Warrior = 0x30BC, // R2.800, x1
    Knight = 0x30BA, // R2.800, x1
    Soldier = 0x30BF, // R4.000, x1
    Gunner = 0x30C1, // R4.000, x1
    SoldierAvatar = 0x30C0, // R4.000, x0 (spawn during fight)
    AetherialBurst = 0x30BE, // R1.200, x0 (spawn during fight)
    AetherialBolt = 0x30BD, // R0.600, x0 (spawn during fight)
    AetherialWard = 0x30BB, // R3.000, x0 (spawn during fight)
    GunTurret = 0x30C2, // R4.000, x?
}

public enum AID : uint
{
    AutoAttackOthers = 6497, // Soldier/Knight/Warrior->player, no cast, single-target
    AutoAttackGunner = 22615, // Gunner->player, no cast, single-target
    DoubleGambit = 22533, // Soldier->self, 3.0s cast, single-target
    SecretsRevealed = 23406, // Soldier->self, 5.0s cast, single-target, visual (tether 2 of 4 avatars that will be activated)
    SecretsRevealedExtra = 23408, // SoldierAvatar->self, no cast, single-target, visual (untether after secrets revealed?)
    ReversalOfForces = 22527, // Warrior->self, 4.0s cast, single-target, visual (remove tethers and apply statuses)
    ReversalOfForcesExtra = 23187, // Helper->self, 4.0s cast, single-target, visual (??? cast together with ReversalOfForces)
    PawnOffFake = 22535, // SoldierAvatar->self, 5.0s cast, range 20 circle fake aoe
    PawnOffReal = 22534, // SoldierAvatar->self, 5.0s cast, range 20 circle aoe
    Bombslinger = 23293, // Warrior->self, 3.0s cast, single-target
    AvatarJump = 22584, // SoldierAvatar->location, no cast, single-target

    AboveBoardExtra = 23409, // Helper->self, 6.0s cast, range 60 circle
    AboveBoard = 22524, // Warrior->self, 6.0s cast, range 60 circle

    LotsCastBigShort = 23403, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallShort = 22526, // AetherialBolt->location, no cast, range 10 circle
    LotsCastBigLong = 22525, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallLong = 23402, // AetherialBolt->location, no cast, range 10 circle

    RapidSeverKnight = 22523, // Knight->player, 5.0s cast, single-target, tankbuster
    RapidSeverWarrior = 22529, // Warrior->player, 5.0s cast, single-target, tankbuster
    RapidSeverSoldier = 22537, // Soldier->player, 5.0s cast, single-target, tankbuster
    BloodAndBoneWarriorAOE = 22530, // Warrior->self, 60.0s cast, range 60 circle
    BloodAndBoneKnightAOE = 22521, // Knight->self, 60.0s cast, range 60 circle
    BloodAndBoneSoldierAOE = 22538, // Soldier->self, 60.0s cast, range 60 circle

    ShieldOmen = 22513, // Knight->self, 3.0s cast, single-target
    SwordOmen = 22512, // Knight->self, 3.0s cast, single-target

    OptimalPlay = 22516, // Helper->self, 5.0s cast, single-target
    OptimalPlayShield = 22515, // Knight->self, 6.0s cast, range ?-60 donut
    OptimalPlaySword = 22514, // Knight->self, 6.0s cast, range 10 circle

    AutomaticTurret = 22539, // Gunner->self, 3.0s cast, single-target
    Reading = 22540, // Gunner->self, 5.0s cast, single-target
    TurretsTour2 = 22541, // Helper->location, 5.0s cast, width 6 rect charge
    TurretsTour3 = 22542, // GunTurret->location, no cast, width 6 rect charge
    TurretsTour4 = 22543, // GunTurret->self, no cast, range 50 width 6 rect

    QueensShotUnseen = 22544, // Gunner->self, 5.0s cast, range 60 circle
    QueensShotRaidwideAOE = 22546, // Gunner->self, 60.0s cast, range 60 circle
    ShotInTheDark = 22545, // Gunner->player, 5.0s cast, single-target, tankbuster
    StrongpointDefense = 22517, // Knight->self, 5.0s cast, single-target
    CoatOfArmsLR = 22519, // AetherialWard->self, 4.0s cast, single-target
    CoatOfArmsFB = 22518, // AetherialWard->self, 4.0s cast, single-target
    Counterplay = 22520, // Helper->player, no cast, single-target

    BloodAndBone = 22522, // Knight->self, 5.0s cast, range 60 circle
}

public enum SID : uint
{
    AboveBoardBombLong = 2428, // none->AetherialBolt/AetherialBurst, extra=0x3E8
    AboveBoardBombShort = 2429, // none->AetherialBurst/AetherialBolt, extra=0x3E8
    AboveBoardPlayerLong = 2426, // none->player, extra=0x3E8
    DirectionalParry = 680, // AetherialWard->AetherialWard, extra=0xC/0x3
    Doom = 2519, // AetherialBolt/Helper->player, extra=0x0
    DutiesAsAssigned = 2415, // none->player, extra=0x0
    ProperCare = 362, // none->player, extra=0x14
    RayOfFortitude = 2625, // none->player, extra=0xA/0x2/0x1
    RayOfSuccor = 2627, // none->player, extra=0x2/0xA/0x1
    RayOfValor = 2626, // none->player, extra=0x9/0x8/0xA/0x6/0x2
    ReducedRates = 364, // none->player, extra=0x1E
    ReversalOfForces = 2447, // none->AetherialBolt/AetherialBurst, extra=0x0
    ShieldBearer = 2446, // Knight->Knight, extra=0x0
    Stun = 149, // Helper->player, extra=0x0
    SwordBearer = 2445, // Knight->Knight, extra=0x0
    TheEcho = 42, // none->player, extra=0x0
    TheHeatOfBattle = 365, // none->player, extra=0xA
    Transcendent = 418, // none->player, extra=0x0
    TwiceComeRuin = 2485, // Knight/AetherialBolt/Helper->player, extra=0x1
    GunTurrentStatus = 2056, // none->GunTurret, extra=0xE1
    WardStatus = 2195, // AetherialWard->AetherialWard, extra=0x101/0x100
    AllyStatus = 2160, // none->30B7, extra=0x2129
    Weakness = 43, // none->player, extra=0x0
}

public enum TetherID : uint
{
    ReversalBomb = 16, // AetherialBolt/AetherialBurst->Warrior
    SecretsRevealed = 30, // SoldierAvatar->Soldier
}
