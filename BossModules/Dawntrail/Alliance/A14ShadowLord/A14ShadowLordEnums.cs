namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

public enum OID : uint
{
    Boss = 0x4657, // R10.502, x1
    LordlyShadow = 0x4659, // R10.502, x2
    Helper = 0x233C, // R0.500, x40, Helper type
}

public enum AID : uint
{
    AutoAttack = 41747, // Boss->player, no cast, single-target
    Teleport = 40810, // Boss->location, no cast, single-target
    GigaSlashL = 40766, // Boss/LordlyShadow->self, 11.0+1.0s cast, single-target, visual (left -> right)
    GigaSlashR = 40767, // Boss/LordlyShadow->self, 11.0+1.0s cast, single-target, visual (right -> left)
    GigaSlashLAOE1 = 40768, // Helper->self, 12.0s cast, range 60 225-degree cone (left cleave, first in LR sequence)
    GigaSlashRAOE2 = 40769, // Helper->self, 1.0s cast, range 60 270-degree cone (right cleave, second in LR sequence)
    GigaSlashRAOE1 = 40770, // Helper->self, 12.0s cast, range 60 225-degree cone (right cleave, first in RL sequence)
    GigaSlashLAOE2 = 40771, // Helper->self, 1.0s cast, range 60 270-degree cone (left cleave, second in RL sequence)
    UmbraSmashBoss = 40795, // Boss->self, 4.0+0.5s cast, single-target, visual (expanding lines)
    UmbraSmashRepeat = 40796, // Boss->self, no cast, single-target, visual (other cardinals)
    UmbraSmashAOE1 = 40797, // Helper->self, 4.5s cast, range 60 width 10 rect
    UmbraSmashAOE2 = 40798, // Helper->self, 7.2s cast, range 60 width 10 rect
    UmbraSmashAOE3 = 40799, // Helper->self, 9.9s cast, range 60 width 10 rect
    UmbraSmashAOE4 = 40800, // Helper->self, 12.6s cast, range 60 width 10 rect
    UmbraWave = 40801, // Helper->self, 2.0s cast, range 5 width 60 rect
    UmbraSmashClone = 40787, // LordlyShadow->self, 4.5+0.5s cast, single-target, visual (expanding lines)
    UmbraSmashAOEClone = 40788, // Helper->self, 5.0s cast, range 60 width 10 rect
    FlamesOfHatred = 40809, // Boss->self, 5.0s cast, range 100 circle, raidwide
    ImplosionL = 40772, // Boss/LordlyShadow->self, 8.0+1.0s cast, single-target, visual (circle + half-arena cleave right)
    ImplosionR = 40773, // Boss/LordlyShadow->self, 8.0+1.0s cast, single-target, visual (circle + half-arena cleave right)
    ImplosionLargeL = 40774, // Helper->self, 9.0s cast, range 12 180-degree cone
    ImplosionSmallR = 40775, // Helper->self, 9.0s cast, range 12 180-degree cone
    ImplosionLargeR = 40776, // Helper->self, 9.0s cast, range 12 180-degree cone
    ImplosionSmallL = 40777, // Helper->self, 9.0s cast, range 12 180-degree cone

    CthonicFuryStart = 40778, // Boss->self, 7.0s cast, range 100 circle, raidwide + arena transition to platforms
    CthonicFuryEnd = 40779, // Boss->self, 7.0s cast, range 100 circle, raidwide + arena transition to normal
    BurningCourt = 40780, // Helper->self, 7.0s cast, range 8 circle
    BurningMoat = 40781, // Helper->self, 7.0s cast, range 5-15 donut
    BurningKeep = 40782, // Helper->self, 7.0s cast, range 23 width 23 rect
    BurningBattlements = 40783, // Helper->self, 7.0s cast, inverted rect
    DarkNebula = 40784, // Boss->self, 3.0s cast, single-target, visual (knockback)
    DarkNebulaShort = 41532, // Helper->self, 5.0s cast, range 60 width 100 rect knockback 20
    DarkNebulaLong = 40785, // Helper->self, 13.0s cast, range 60 width 100 rect knockback 20
    EchoesOfAgony = 41899, // Boss->self, 8.0s cast, single-target, visual (5-hit stack)
    EchoesOfAgonyAOE = 41900, // Helper->players, no cast, range 5 circle stack

    Nightfall = 41144, // Boss->self, 5.0s cast, single-target, visual (transition)
    TeraSlash = 41145, // Helper->self, no cast, range 100 circle, raidwide
    GigaSlashNightfallLRF = 42020, // Boss->self, 14.0+1.0s cast, single-target, visual (3 cleaves: left > right > front)
    GigaSlashNightfallLRB = 42021, // Boss->self, 14.0+1.0s cast, single-target, visual (3 cleaves: left > right > back)
    GigaSlashNightfallRLF = 42022, // Boss->self, 14.0+1.0s cast, single-target, visual (3 cleaves: right > left > front)
    GigaSlashNightfallRLB = 42023, // Boss->self, 14.0+1.0s cast, single-target, visual (3 cleaves: right > left > back)
    GigaSlashNightfallFAOE3 = 42024, // Helper->self, 1.0s cast, range 60 210-degree cone (front cleave, third in sequence)
    GigaSlashNightfallBAOE3 = 42025, // Helper->self, 1.0s cast, range 60 210-degree cone (back cleave, third in sequence)
    GigaSlashNightfallLAOE1 = 42027, // Helper->self, 15.0s cast, range 60 225-degree cone (left cleave, first in LR sequence)
    GigaSlashNightfallRAOE2 = 42028, // Helper->self, 1.0s cast, range 60 270-degree cone (right cleave, second in LR sequence)
    GigaSlashNightfallRAOE1 = 42029, // Helper->self, 15.0s cast, range 60 225-degree cone (right cleave, first in RL sequence)
    GigaSlashNightfallLAOE2 = 42030, // Helper->self, 1.0s cast, range 60 270-degree cone (left cleave, second in RL sequence)
    ShadowSpawn = 40786, // Boss->self, 3.0s cast, single-target, visual (create clone)
    UnbridledRage = 40807, // Boss->self, 5.0s cast, single-target, visual (line tankbusters)
    UnbridledRageAOE = 40808, // Helper->self, no cast, range 100 width 8 rect
    DarkNova = 41335, // Helper->players, 6.0s cast, range 6 circle spread
    BindingSigil = 40789, // Boss->self, 12.0+1.0s cast, single-target, visual (puddles)
    BindingSigilPreview = 41513, // Helper->self, 1.5s cast, range 9 circle, visual
    SoulBinding = 41514, // Helper->self, 1.0s cast, range 9 circle, apply debuff
    SoulBindingBurst = 41531, // Helper->self, no cast, vuln on debuffed people
    DamningStrikes1 = 40791, // Boss->self, 8.0s cast, single-target, visual (towers)
    DamningStrikes2 = 41054, // Boss->self, 8.7s cast, single-target, visual (towers, much more rare, ??? difference)
    DamningStrikesGrab = 41530, // Helper->player, no cast, single-target, grab target
    DamningStrikesTeleport1 = 40794, // Boss->location, no cast, single-target, teleport before hit
    DamningStrikesTeleport2 = 41815, // Boss->location, no cast, single-target, teleport before hit
    DamningStrikesTeleport3 = 41816, // Boss->location, no cast, single-target, teleport before hit
    DamningStrikesTeleport4 = 42054, // Boss->location, no cast, single-target, teleport before hit
    DamningStrikesTeleport5 = 42055, // Boss->location, no cast, single-target, teleport before hit
    DamningStrikesVisual1 = 40793, // Boss->self, no cast, single-target, visual before hit
    DamningStrikesVisual2 = 42052, // Boss->self, no cast, single-target, visual before hit
    DamningStrikesVisual3 = 42053, // Boss->self, no cast, single-target, visual before hit
    DamningStrikesImpact1 = 40792, // Helper->self, 10.5s cast, range 3 circle, tower 1
    DamningStrikesImpact2 = 41110, // Helper->self, 13.0s cast, range 3 circle, tower 2
    DamningStrikesImpact3 = 41111, // Helper->self, 15.7s cast, range 3 circle, tower 3
    DamningStrikesShockwave = 41112, // Helper->self, no cast, range 100 circle, raidwide with dot if tower is not soaked
    DoomArc = 40806, // Boss->self, 15.0s cast, range 100 circle, raidwide with bleed + damage up
}

public enum IconID : uint
{
    EchoesOfAgony = 545, // player->self
    UnbridledRage = 471, // player->self
    DarkNova = 311, // player->self
}
