namespace BossMod.Dawntrail.Alliance.A11Prishe;

public enum OID : uint
{
    Boss = 0x4673, // R6.342, x1
    Helper = 0x233C, // R0.500, x21, Helper type
    JumpPoint = 0x46CF, // R1.200, x1
    LuminousRemnant = 0x4674, // R1.000-1.860, x0 (spawn during fight)
    AsuranFistsTower = 0x1EBCC9, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 40934, // Boss->player, no cast, single-target
    Teleport = 40933, // Boss->location, no cast, single-target
    Banishga = 40935, // Boss->self, 5.0s cast, range 80 circle, raidwide
    KnuckleSandwich1 = 40936, // Boss->location, 12.0+1.0s cast, single-target, visual (small radius out-in)
    KnuckleSandwich1AOE = 40939, // Helper->self, 13.0s cast, range 9 circle
    KnuckleSandwich2 = 40937, // Boss->location, 12.0+1.0s cast, single-target, visual (medium radius out-in)
    KnuckleSandwich2AOE = 40940, // Helper->self, 13.0s cast, range 18 circle
    KnuckleSandwich3 = 40938, // Boss->location, 12.0+1.0s cast, single-target, visual (large radius out-in)
    KnuckleSandwich3AOE = 40941, // Helper->self, 13.0s cast, range 27 circle
    BrittleImpact1 = 40942, // Helper->self, 14.5s cast, range 9-60 donut
    BrittleImpact2 = 40943, // Helper->self, 14.5s cast, range 18-60 donut
    BrittleImpact3 = 40944, // Helper->self, 14.5s cast, range 27-60 donut
    NullifyingDropkick = 40945, // Boss->players, 5.0+1.5s cast, range 6 circle, shared tankbuster
    NullifyingDropkickAOE = 40957, // Helper->players, 6.5s cast, range 6 circle, second hit
    BanishStorm = 40946, // Boss->self, 4.0s cast, single-target, visual (exaflare)
    Banish = 40947, // Helper->self, no cast, range 6 circle exaflare
    Holy = 40962, // Boss->self, 4.0+1.0s cast, single-target, visual (spreads)
    HolyAOE = 40963, // Helper->players, 5.0s cast, range 6 circle spread
    CrystallineThorns = 40948, // Boss->self, 4.0+1.0s cast, single-target, visual (spikes)
    Thornbite = 40949, // Helper->self, no cast, range 80 circle, spikes
    AuroralUppercut1 = 40950, // Boss->self, 11.4+1.6s cast, single-target, visual (short-range knockback)
    AuroralUppercut2 = 40951, // Boss->self, 11.4+1.6s cast, single-target, visual (medium-range knockback)
    AuroralUppercut3 = 40952, // Boss->self, 11.4+1.6s cast, single-target, visual (long-range knockback)
    AuroralUppercutAOE = 40953, // Helper->self, no cast, range 80 circle, knockback
    BanishgaIV = 40954, // Boss->self, 5.0s cast, range 80 circle, raidwide + orbs
    Explosion = 40955, // LuminousRemnant->self, 5.0s cast, range 8 circle, orb explosion
    AsuranFists = 40956, // Boss->self, 6.5+0.5s cast, single-target, visual (tower)
    AsuranFistsAOE1 = 40958, // Helper->self, no cast, range 6 circle, tower hits 1-3
    AsuranFistsAOE2 = 40959, // Helper->self, no cast, range 6 circle, tower hits 4-7
    AsuranFistsAOE3 = 40960, // Helper->self, no cast, range 6 circle, tower hit 8
}

public enum IconID : uint
{
    NullifyingDropkick = 570, // player->self
    Holy = 215, // player->self
}

public enum TetherID : uint
{
    BossJump = 215, // JumpPoint->Boss
    AuroralUppercut = 297, // player->JumpPoint
}
