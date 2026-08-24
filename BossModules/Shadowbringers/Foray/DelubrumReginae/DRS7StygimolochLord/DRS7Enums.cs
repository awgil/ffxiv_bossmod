namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

public enum OID : uint
{
    Boss = 0x30B2, // R4.000, x1
    Helper = 0x233C, // R0.500, x17
    StygimolochMonk = 0x30B3, // R4.000, spawn during fight
    BallOfFire = 0x30B4, // R1.000, spawn during fight
    BallOfEarth = 0x30B6, // R1.000, spawn during fight
    HiddenTrap = 0x18D6, // R0.500, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 22490, // Boss->location, no cast, single-target, teleport
    FoeSplitter = 22487, // Boss->player, 5.0s cast, range 9 ?-degree cone tankbuster
    ViciousSwipe = 21842, // Boss->self, no cast, range 8 circle, knockback 15
    ThunderousDischarge = 22482, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    ThunderousDischargeExtra = 22483, // Helper->self, no cast, single-target, visual?
    ThunderousDischargeAOE = 23352, // Helper->self, no cast, range 70 circle raidwide
    RapidBolts = 22477, // Boss->self, 5.0s cast, single-target, visual (spawn voidzones at marked players)
    RapidBoltsAOE = 22478, // Helper->location, no cast, range 5 circle voidzone
    ThousandTonzeSwing = 22488, // Boss->self, 6.0s cast, range 20 circle aoe
    CrushingHoof = 22485, // Boss->location, 5.0s cast, range 60 circle, visual (baited proximity)
    CrushingHoofAOE = 22486, // Helper->self, no cast, range 80 circle with 25? falloff
    Whack = 22479, // Boss->self, 3.0s cast, single-target, visual (cones)
    WhackExtra = 22480, // Boss->self, no cast, range 40 60-degree cone, visual?
    WhackAOE = 22481, // Helper->self, 3.0s cast, range 40 60-degree cone

    AutoAttackMinotaur = 21926, // StygimolochMonk->player, no cast, single-target
    MemoryOfTheLabyrinth = 22467, // Boss->self, 3.0s cast, single-target, visual (add phase start)
    ManaFlame = 22494, // BallOfFire->self, 5.0s cast, range 6 circle reflectable aoe
    Entrapment = 22491, // StygimolochMonk->self, 3.0s cast, single-target, visual (spawn traps in outer ring)
    MassiveExplosion = 22492, // HiddenTrap->location, no cast, range 5 circle
    PoisonTrap = 22493, // HiddenTrap->location, no cast, range 5 circle
    LabyrinthineFate = 22471, // Boss->self, 3.0s cast, single-target, visual (knockback/attract debuffs)
    LabyrinthineFateExtra = 22472, // Helper->self, no cast, range 100 circle, visual
    FatefulWords = 22473, // Boss->self, 5.0s cast, single-target, visual (resolve fate debuffs)
    FatefulWordsAOE = 22474, // Helper->self, no cast, range 100 circle, knockback/attract 6 depending on debuff
    DevastatingBolt = 22468, // Boss->self, 3.0s cast, single-target, visual (alcoves safe)
    DevastatingBoltOuter = 22469, // Helper->self, 4.0s cast, range 25-30 donut
    DevastatingBoltInner = 22470, // Helper->self, 4.0s cast, range 12-17 donut
    RendingBolt = 22475, // Boss->self, 3.0s cast, single-target, visual (electrocution puddles)
    Electrocution = 22476, // Helper->location, 3.0s cast, range 3 circle puddles
}

public enum SID : uint
{
    WanderersFate = 2430, // none->player, extra=0x0
    SacrificesFate = 2431, // none->player, extra=0x0
}

public enum IconID : uint
{
    FoeSplitter = 198, // player
    RapidBolts = 160, // player
}
