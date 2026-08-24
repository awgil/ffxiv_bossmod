namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

public enum OID : uint
{
    Knight = 0x30C3, // R2.800, x1
    Warrior = 0x30C7, // R2.800, x1
    Soldier = 0x30CB, // R4.000, x1
    Gunner = 0x30CF, // R4.000, x1
    Helper = 0x233C, // R0.500, x9
    AetherialWard = 0x30C4, // R3.000, spawn during fight (last phase main targets)
    AetherialSphere = 0x30C5, // R2.500, spawn during fight (warrior/knight central sphere)
    SpiritualSphere = 0x30C6, // R1.000, spawn during fight (last phase spheres needing reflect)
    AetherialBolt = 0x30C8, // R0.600, spawn during fight (warrior/knight small bomb)
    AetherialBurst = 0x30C9, // R1.200, spawn during fight (warrior/knight big bomb)
    AuraSphere = 0x30CA, // R1.000, spawn during fight (last phase spheres casting tankbusters)
    SoldierAvatar = 0x30CC, // R4.000, spawn during fight (soldier/gunner jumping clone)
    RagingFlame = 0x30CD, // R1.000-1.500, spawn during fight (solder/gunner small fireball)
    ImmolatingFlame = 0x30CE, // R1.000-3.000, spawn during fight (soldier/gunner big fireball)
    AutomaticTurret = 0x30D0, // R4.000, spawn during fight (soldier/gunner untargetable turret)
    GunTurret = 0x30D1, // R4.000, spawn during fight (soldier/gunner targetable turret)
}

public enum AID : uint
{
    AutoAttackGunner = 22616, // Gunner->player, no cast, single-target
    AutoAttackOthers = 6497, // Soldier/Knight/Warrior->player, no cast, single-target

    BloodAndBoneKnight = 22561, // Knight->self, 5.0s cast, single-target, visual (raidwide)
    BloodAndBoneKnightAOE = 19251, // Helper->self, 5.0s cast, range 60 circle, raidwide
    BloodAndBoneWarrior = 22577, // Warrior->self, 5.0s cast, single-target, visual (raidwide)
    BloodAndBoneWarriorAOE = 19252, // Helper->self, 5.0s cast, range 60 circle, raidwide
    BloodAndBoneSoldier = 22593, // Soldier->self, 5.0s cast, single-target, visual (raidwide)
    BloodAndBoneSoldierAOE = 19253, // Helper->self, 5.0s cast, range 60 circle, raidwide
    QueensShotRaidwide = 22612, // Gunner->self, 5.0s cast, single-target, visual (raidwide)
    QueensShotRaidwideAOE = 19254, // Helper->self, 5.0s cast, range 60 circle, raidwide
    RapidSeverKnight = 22562, // Knight->player, 5.0s cast, single-target, tankbuster
    RapidSeverWarrior = 22578, // Warrior->player, 5.0s cast, single-target, tankbuster
    RapidSeverSoldier = 22594, // Soldier->player, 5.0s cast, single-target, tankbuster
    ShotInTheDark = 22613, // Gunner->player, 5.0s cast, single-target, tankbuster

    RelentlessBatteryWarrior = 22565, // Warrior->self, 5.0s cast, single-target, visual (mechanic start)
    RelentlessBatteryKnight = 22547, // Knight->self, 5.0s cast, single-target, visual (mechanic start)
    SwordOmen = 22548, // Knight->self, 3.0s cast, single-target, visual (buff application)
    ShieldOmen = 22549, // Knight->self, 3.0s cast, single-target, visual (buff application)
    Bombslinger = 23294, // Warrior->self, 3.0s cast, single-target, visual (spawn bombs)
    ReversalOfForces = 22569, // Warrior->self, 4.0s cast, single-target, visual (remove tethers and apply statuses)
    ReversalOfForcesExtra = 23188, // Helper->self, 4.0s cast, single-target, visual (??? cast together with ReversalOfForces)
    OptimalOffensiveSword = 22553, // Knight->location, 7.0s cast, width 5 rect charge
    OptimalOffensiveShield = 22554, // Knight->location, 7.0s cast, width 5 rect charge
    OptimalOffensiveShieldKnockback = 22555, // Helper->self, 7.0s cast, range 60 circle knockback 10
    OptimalOffensiveShieldMoveSphere = 22556, // Helper->AetherialSphere, 7.0s cast, single-target, attract
    UnluckyLot = 22557, // AetherialSphere->self, 1.0s cast, range 20 circle aoe (sphere explosion)
    AboveBoard = 22566, // Warrior->self, 6.0s cast, range 60 circle, visual (throw stuff up)
    AboveBoardExtra = 23410, // Helper->self, 6.0s cast, range 60 circle, visual (??? cast together with AboveBoard)
    LotsCastBigShort = 23405, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallShort = 22568, // AetherialBolt->location, no cast, range 10 circle
    LotsCastBigLong = 22567, // AetherialBurst->location, no cast, range 10 circle visual
    LotsCastSmallLong = 23404, // AetherialBolt->location, no cast, range 10 circle visual
    LotsCastLong = 23479, // Helper->location, 1.2s cast, range 10 circle
    Boost = 22573, // Warrior->self, 4.0s cast, single-target
    WindsOfWeight = 22570, // Warrior->self, 6.0s cast, single-target, visual (wind/gravity aoes)
    WindsOfFate = 22572, // Helper->self, 6.0s cast, range 20 circle (green: small hit if player has reversal)
    WeightOfFortune = 22571, // Helper->self, 6.0s cast, range 20 circle (purple: small hit unless player has reversal)
    OptimalPlaySword = 22550, // Knight->self, 5.0s cast, range 10 circle aoe
    OptimalPlayShield = 22551, // Knight->self, 5.0s cast, range 5-60 donut aoe
    OptimalPlayCone = 22552, // Helper->self, 5.0s cast, range 60 270-degree cone

    RelentlessBatteryGunner = 22596, // Gunner->self, 5.0s cast, single-target, visual (mechanic start)
    RelentlessBatterySoldier = 22580, // Soldier->self, 5.0s cast, single-target, visual (mechanic start)
    GreatBallOfFire = 22587, // Soldier->self, 3.0s cast, single-target, visual (create 2 big and 2 small fireballs)
    FoolsGambit = 22588, // Soldier->self, 6.0s cast, single-target, visual (give fireballs transfiguration buff)
    AutomaticTurretGambit = 22602, // Gunner->self, 3.0s cast, single-target, visual (create turrets during gambit mechanics)
    Reading = 22603, // Gunner->self, 3.0s cast, single-target, visual (assign 'x unseen' statuses)
    BurnSmall = 22589, // RagingFlame->self, 1.5s cast, range 10 circle
    BurnBig = 22590, // ImmolatingFlame->self, 1.5s cast, range 18 circle
    QueensShotUnseen = 22604, // Gunner->self, 7.0s cast, single-target, visual (unseen resolve)
    QueensShotUnseenAOE = 22605, // Helper->self, 7.0s cast, range 60 circle, raidwide that requires directing unseen side to caster
    TurretsTourUnseen = 22606, // AutomaticTurret->self, 3.0s cast, range 50 width 5 rect aoe that requires directing unseen side to caster
    GunTurret = 22607, // Gunner->self, 3.0s cast, single-target, visual (create killable turrets)
    HigherPower = 22611, // Gunner->self, 4.0s cast, single-target, visual (grant gun turrets damage up)
    SniperShot = 22608, // GunTurret->self, 8.0s cast, single-target, visual (tankbuster on target at cast end)
    SniperShotAOE = 22609, // GunTurret->player, no cast, single-target, tankbuster
    Explosion = 22610, // GunTurret->self, 15.0s cast, ??? (wipe if allowed to finish)
    FieryPortent = 22591, // Soldier->self, 6.0s cast, range 60 circle, visual (pyretic buff application)
    IcyPortent = 22592, // Soldier->self, 6.0s cast, range 60 circle, visual (move to avoid effect)
    DoubleGambit = 22583, // Soldier->self, 3.0s cast, single-target, visual (create avatars)
    SecretsRevealed = 23407, // Soldier->self, 5.0s cast, single-target, visual (tether 2 of 4 avatars that will be activated)
    SecretsRevealedExtra = 23408, // SoldierAvatar->self, no cast, single-target, visual (untether after secrets revealed?)
    AvatarJump = 22584, // SoldierAvatar->location, no cast, single-target, teleport
    PawnOffReal = 22585, // SoldierAvatar->self, 7.0s cast, range 20 circle aoe
    PawnOffFake = 22586, // SoldierAvatar->self, 7.0s cast, range 20 circle fake aoe
    AutomaticTurretNormal = 22597, // Gunner->self, 3.0s cast, single-target, visual (create turrets in the arena that need to be dodged)
    TurretsTourNormal = 22598, // Gunner->self, 5.0s cast, single-target, visual (tower lines)
    TurretsTourNormalAOE1 = 22599, // Helper->location, 5.0s cast, width 6 rect charge
    TurretsTourNormalAOE2 = 22601, // AutomaticTurret->self, no cast, range 55 width 6 rect
    TurretsTourNormalAOE3 = 22600, // AutomaticTurret->location, no cast, width 6 rect charge

    SpitefulSpirit = 22574, // Warrior->self, 5.0s cast, single-target, visual (summon spheres?)
    StrongpointDefense = 22558, // Knight->self, 5.0s cast, single-target, visual (summon wards?)
    CoatOfArmsFB = 22559, // AetherialWard->self, 4.0s cast, single-target, applies front/back directional parry
    CoatOfArmsLR = 22560, // AetherialWard->self, 4.0s cast, single-target, applies left/right directional parry
    Counterplay = 22773, // Helper->player, no cast, single-target, damage + knockback 5 + stun + ruin if attack hits wrong side
    SpiteCheck = 22575, // AuraSphere->self, 9.0s cast, 'enrage' (does nothing - but if not killed before cast end, sphere will start killing people)
    SpiteCheckEnrage = 23457, // AuraSphere->player, no cast, single-target (lethal damage on random target if sphere lives too long)
    SpiteSmite = 22576, // AuraSphere->player, 5.0s cast, single-target, lethal tankbuster on sphere target that should be invulned
    Fracture = 23445, // SpiritualSphere->self, 2.0s cast, range 6 circle, reflectable aoe
    Burst = 23446, // SpiritualSphere->self, no cast, range 60 circle, raidwide if no one soaked the sphere

    EnrageP1Knight = 22563, // Knight->self, 10.0s cast, wipe (starts when warrior is killed)
    EnrageP1Warrior = 22579, // Warrior->self, 10.0s cast, wipe (starts when knight is killed)
    EnrageP2Soldier = 22595, // Soldier->self, 10.0s cast, wipe (starts when gunner is killed)
    EnrageP2Gunner = 22614, // Gunner->self, 10.0s cast, wipe (starts when soldier is killed)
    EnrageP3Knight = 22793, // Knight->self, 70.0s cast
    EnrageP3Warrior = 22794, // Warrior->self, 70.0s cast
    EnrageP3Soldier = 22795, // Soldier->self, 70.0s cast
    EnrageP3Gunner = 22796, // Gunner->self, 70.0s cast
}

public enum SID : uint
{
    MagicVulnerabilityDown = 812, // none->Warrior/Knight, extra=0x0
    PhysicalVulnerabilityDown = 899, // none->Warrior/Knight, extra=0x0
    SwordBearer = 2445, // Knight->Knight, extra=0x0
    ShieldBearer = 2446, // Knight->Knight, extra=0x0
    ReversalOfForces = 2447, // none->AetherialBurst/AetherialBolt/player, extra=0x0
    AboveBoardPlayerLong = 2426, // none->player, extra=0x3E8
    AboveBoardPlayerShort = 2427, // none->player, extra=0x3E8
    AboveBoardBombLong = 2428, // none->AetherialBurst/AetherialBolt, extra=0x3E8
    AboveBoardBombShort = 2429, // none->AetherialBolt/AetherialBurst, extra=0x3E8
    Boosted = 2448, // Warrior->Warrior, extra=0x0

    Transfiguration = 565, // none->RagingFlame/ImmolatingFlame, extra=0x199
    //_Gen_ = 2056, // none->AutomaticTurret, extra=0xE1
    RightUnseen = 1707, // none->player, extra=0xE9
    LeftUnseen = 1708, // none->player, extra=0xEA
    BackUnseen = 1709, // none->player, extra=0xE8
    //_Gen_DamageUp = 61, // none->GunTurret, extra=0x0
    Pyretic = 960, // Soldier->player, extra=0x0
    DirectionalParry = 680, // AetherialWard->AetherialWard, extra=0x3 (front+back)/0xC (left+right)
    //_Gen_ = 2195, // AetherialWard->AetherialWard, extra=0x101/0x100
}

public enum IconID : uint
{
    Reversal = 255, // player
    SpriteCheck = 23, // player
}

public enum TetherID : uint
{
    PhysicalVulnerabilityDown = 136, // Warrior->Knight
    MagicVulnerabilityDown = 137, // Warrior->Knight
    BossesClose = 12, // Knight->Warrior
    ReversalBomb = 16, // AetherialBurst/AetherialBolt->Warrior
    ReversalPlayer = 135, // player->Warrior
    //_Gen_Tether_101 = 101, // AetherialSphere->Knight
    //_Gen_Tether_54 = 54, // GunTurret->Gunner
    SecretsRevealed = 30, // SoldierAvatar->Soldier
}
