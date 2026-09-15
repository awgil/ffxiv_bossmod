namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

public enum OID : uint
{
    BossHM = 0x4681, // R3.300, x1
    BossEV = 0x4682, // R3.300, x1
    BossMR = 0x4683, // R3.300, x1
    BossTT = 0x4684, // R3.300, x1
    BossGK = 0x4685, // R3.300, x1
    CloneHM = 0x4686, // R3.300, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x40, Helper type
    TachiGekkoHelper = 0x4649, // R5.250, x1
    ArkShield = 0x4754, // R2.200, x0 (spawn during fight), Part type
    TachiGekkoOrigin1 = 0x1EBCC5, // R0.500, x0 (spawn during fight), EventObj type
    TachiGekkoOrigin2 = 0x1EBCC6, // R0.500, x0 (spawn during fight), EventObj type
    DominionSlashHelper = 0x1EBCC7, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    DecisiveBattleMR = 41057, // BossMR->self, 4.0s cast, make invulnerable to everyone except one alliance
    DecisiveBattleTT = 41058, // BossTT->self, 4.0s cast, make invulnerable to everyone except one alliance
    DecisiveBattleGK = 41059, // BossGK->self, 4.0s cast, make invulnerable to everyone except one alliance
    AutoAttackBoss = 870, // BossTT/BossMR/BossHM->player, no cast, single-target
    AutoAttackGK = 1461, // BossGK->player, no cast, single-target
    AutoAttackEV = 40623, // BossEV->player, no cast, single-target
    TeleportTT = 41060, // BossTT->location, no cast, single-target
    TeleportMR = 41066, // BossMR->location, no cast, single-target
    TeleportEV = 41814, // BossEV->location, no cast, single-target
    TeleportHM = 40617, // BossHM->location, no cast, single-target
    AethersplitTT = 41103, // BossTT->BossEV, no cast, single-target, health transfer?
    AethersplitMR = 41104, // BossMR->BossEV/BossTT, no cast, single-target, health transfer?
    AethersplitGK = 41105, // BossGK->BossTT/BossEV, no cast, single-target, health transfer?
    AethersplitHM = 41106, // BossHM->BossEV/BossMR/BossTT, no cast, single-target, health transfer?
    AethersplitEV = 41107, // BossEV->BossTT, no cast, single-target, health transfer?

    Cloudsplitter = 41078, // BossMR->self, 5.0s cast, single-target, visual (tankbusters)
    CloudsplitterAOE = 41079, // Helper->player, 5.5s cast, range 6 circle tankbuster
    MeikyoShisui = 41080, // BossGK->self, 4.0s cast, single-target, visual (crisscross + out + gaze > cones > in)
    TachiYukikaze = 41081, // Helper->self, 3.0s cast, range 50 width 5 rect (crisscross)
    TachiGekko = 41082, // Helper->self, 7.0s cast, range 50 circle, gaze
    TachiGekkoVisual = 41366, // TachiGekkoHelper->self, 7.0s cast, range 50 circle, visual (???)
    TachiKasha = 41083, // Helper->self, 12.0s cast, range 4+16 circle, out
    ConcertedDissolution = 41084, // Helper->self, 6.0s cast, range 40 ?-degree cone
    LightsChain = 41085, // Helper->self, 8.0s cast, range ?-40 donut
    Meteor = 41098, // BossTT->self, 11.0s cast, single-target, interruptible (heavy raidwide with vuln)
    MeteorAOE = 41099, // Helper->location, no cast, range 100 circle, heavy raidwide with vuln
    HavocSpiral = 41067, // BossMR->self, 5.0+0.5s cast, single-target, visual (rotating cones)
    HavocSpiralFirst = 41070, // Helper->self, 5.5s cast, range 30 30-degree cone
    HavocSpiralRest = 41071, // Helper->self, no cast, range 30 30-degree cone
    SpiralFinish = 41068, // BossMR->self, 11.0+0.5s cast, single-target, visual (knockback)
    SpiralFinishAOE = 41069, // Helper->self, 11.5s cast, range 100 circle, knockback 16
    Dragonfall = 41086, // BossGK->self, 9.0s cast, single-target, visual (party stacks)
    DragonfallAOE = 41087, // BossGK->players, no cast, range 6 circle stack
    Guillotine = 41063, // BossTT->self, 10.5s cast, range 40 ?-degree cone
    GuillotineAOE = 41064, // Helper->self, no cast, range 40 ?-degree cone, hits 1-3
    GuillotineAOELast = 41065, // Helper->self, no cast, range 40 ?-degree cone, hit 4

    Utsusemi = 41088, // BossHM->self, 3.0s cast, single-target, visual (create clones to be kited)
    MightyStrikesClones = 41089, // CloneHM/BossHM->self, 5.0s cast, single-target, gain mighty strikes buff
    CriticalStrikes = 41090, // CloneHM->player, no cast, single-target, heavy damage + knockback 20 if reaches kited target
    CrossReaver = 41091, // BossHM->self, 3.0s cast, single-target, visual (cross)
    CrossReaverAOE = 41092, // Helper->self, 6.0s cast, range 50 width 12 cross
    DominionSlash = 41093, // BossEV->self, 5.0s cast, range 100 circle, raidwide + orbs
    DivineDominion = 41094, // Helper->self, 2.0s cast, range 6 circle, orb explosion when it expires normally
    DivineDominionFail = 40628, // Helper->self, no cast, range 6 circle, orb explosion when it is touched early
    Holy = 41097, // BossEV->self, 5.0s cast, range 100 circle, raidwide
    ProudPalisade = 42056, // BossEV->self, no cast, single-target, visual (create shield)
    MijinGakure = 41100, // BossHM->self, 30.0s cast, range 100 circle, interruptible enrage
    Rampage = 41072, // BossMR->self, 8.0s cast, single-target, visual (charge sequence)
    RampagePreviewCharge = 41073, // Helper->location, 3.0s cast, width 10 rect charge
    RampagePreviewLast = 41074, // Helper->location, 3.0s cast, range 20 circle
    RampageAOECharge = 41075, // BossMR->location, no cast, width 10 rect charge
    RampageJumpLast = 41076, // BossMR->location, no cast, single-target, visual (jump to last location)
    RampageAOELast = 41077, // Helper->location, 0.5s cast, range 20 circle
    ArroganceIncarnate = 41095, // BossEV->self, 5.0s cast, single-target, visual (multi-hit stack)
    ArroganceIncarnateAOE = 41096, // Helper->players, no cast, range 6 circle stack
    MightyStrikesBoss = 41364, // BossHM->self, 5.0s cast, single-target, gain mighty strikes buff
    CriticalReaverRaidwide = 41365, // BossHM->self, no cast, range 100 circle, raidwide
    CriticalReaverEnrage = 41275, // BossHM->self, 10.0s cast, range 100 circle, interruptible enrage
    Raiton = 41109, // BossHM->self, 5.0s cast, range 100 circle, raidwide
}

public enum SID : uint
{
    EpicHero = 4192, // none->player, extra=0x0
    EpicVillain = 4193, // none->BossMR, extra=0x334
    FatedHero = 4194, // none->player, extra=0x0
    FatedVillain = 4195, // none->BossGK, extra=0x335
    VauntedHero = 4196, // none->player, extra=0x0
    VauntedVillain = 4197, // none->BossTT, extra=0x336
    Invincibility = 4410, // none->BossHM, extra=0x0
    Uninterrupted = 4416, // none->BossHM, extra=0x0
}

public enum IconID : uint
{
    Cloudsplitter = 464, // player->self
    RotateCW = 167, // BossMR->self
    RotateCCW = 168, // BossMR->self
    Dragonfall1 = 557, // player->self
    Dragonfall2 = 566, // player->self
    Dragonfall3 = 567, // player->self
    ArroganceIncarnate = 305, // player->self
}

public enum TetherID : uint
{
    DecisiveBattle = 299, // player->BossMR/BossGK/BossTT
    Dragonfall = 249, // player->BossGK
    Utsusemi = 293, // CloneHM->player
}
