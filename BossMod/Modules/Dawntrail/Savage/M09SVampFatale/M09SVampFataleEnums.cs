namespace BossMod.Dawntrail.Savage.M09SVampFatale;

public enum OID : uint
{
    VampFatale = 0x4ADF, // R4.000, x1
    Helper = 0x233C, // R0.500, x36, Helper 
    VampetteFatale = 0x4C2F, // R1.200, x0 (spawn during fight)
    Coffinmaker = 0x4AE0, // R10.000, x0 (spawn during fight)
    DeadlyDoornail = 0x4AE2, // R3.000, x0 (spawn during fight)
    FatalFlail = 0x4AE1, // R3.000, x0 (spawn during fight)
    Neckbiter = 0x4AF5, // R3.000, x0 (spawn during fight)
    CoffinmakerSmall = 0x4AF6, // R1.000, x0 (spawn during fight)
    CharnelCell = 0x4AF3, // R4.000, x0 (spawn during fight), for healer?
    CharnelCell1 = 0x4AF4, // R4.000, x0 (spawn during fight), for dps?
    CharnelCell2 = 0x4AE3, // R4.000, x0 (spawn during fight), for tanks?
}

public enum AID : uint
{
    KillerVoice = 45956, // VampFatale->self, 5.0s cast, range 60 circle
    HardcoreCast = 45914, // VampFatale->self, 3.0+2.0s cast, single-target
    Hardcore = 45951, // Helper->player, 5.0s cast, range 6 circle
    HardcoreBig = 45952, // Helper->players, 5.0s cast, range 15 circle
    _Ability_VampStomp = 45898, // VampFatale->location, 4.1+0.9s cast, single-target
    VampStomp = 45940, // Helper->self, 5.0s cast, range 10 circle
    VampStompRing = 45940, // Helper->self, no cast, single-target
    BlastBeatPlayer = 45942, // Helper->players, no cast, range 8 circle
    BlastBeatVampette = 45941, // 4C2F->self, 1.0s cast, range 8 circle
    BrutalRainCast = 45917, // VampFatale->self, 3.8+1.2s cast, single-target
    BrutalRain = 45955, // Helper->players, no cast, range 6 circle
    _Ability_1 = 45874, // VampFatale->location, no cast, single-target
    SadisticScreech = 45875, // VampFatale->self, 5.0s cast, single-target
    _Ability_SadisticScreech1 = 45926, // Helper->self, no cast, range 60 circle
    _Ability_DeadWake = 46853, // 4AE0->self, 4.5+0.5s cast, single-target
    DeadWake = 45927, // Helper->self, 5.0s cast, range 10 width 20 rect

    CoffinfillerLong = 45928, // Helper->self, 5.0s cast, range 32 width 5 rect
    CoffinfillerMed = 45929, // Helper->self, 5.0s cast, range 22 width 5 rect
    CoffinfillerShort = 45930, // Helper->self, 5.0s cast, range 12 width 5 rect
    HalfMoonLL = 45943, // Helper->self, 5.0s cast, range 60 180.000-degree cone, Left 1st Left 
    HalfMoonLR = 45944, // Helper->self, 8.0s cast, range 60 180.000-degree cone, Left 1st Right
    HalfMoonBigLL = 45945,
    HalfMoonBigLR = 45946,
    HalfMoonRR = 45947, // Helper->self, 5.0s cast, range 60 180.000-degree cone, Right 1st Right
    HalfMoonRL = 45948, // Helper->self, 8.0s cast, range 60 180.000-degree cone, Right 1st Left
    HalfMoonBigRR = 45949, // Helper->self, 5.0s cast, range 64 180.000-degree cone
    HalfMoonBigRL = 45950, // Helper->self, 8.0s cast, range 64 180.000-degree cone

    CrowdKillCast = 45886, // VampFatale->self, 0.5+4.9s cast, single-target
    CrowdKill = 45933, // Helper->self, no cast, range 60 circle
    FinaleFataleCast = 45888, // VampFatale->self, 5.0s cast, single-target
    FinaleFataleBigCast = 45889, // VampFatale->self, 5.0s cast, single-target
    FinaleFatale = 45935, // Helper->self, no cast, range 60 circle
    FinaleFataleBig = 45936, // Helper->self, no cast, range 60 circle

    PulpingPulse = 45939, // Helper->location, 4.0s cast, range 5 circle
    AetherlettingCast = 45967, // VampFatale->self, 12.3+0.7s cast, single-target
    AetherlettingCone = 45969, // Helper->self, 9.0s cast, range 40 45.000-degree cone
    _Ability_Aetherletting2 = 45968, // VampFatale->self, no cast, single-target
    AetherlettingPuddle = 45970, // Helper->player, 5.0s cast, range 6 circle
    AetherlettingCross = 45971, // Helper->self, 14.0s cast, range 40 width 10 cross

    InsatiableThirstCast = 45892, // VampFatale->self, 2.8+2.2s cast, single-target
    InsatiableThirst = 45938, // Helper->self, no cast, range 60 circle

    GravegrazerBig = 45931, // Helper->self, no cast, range 10 width 5 rect
    GravegrazerSmall = 45932, // Helper->self, no cast, range 5 width 5 rect
    Plummet = 45963, // Helper->self, 7.0s cast, range 3 circle, FatalFlail before spawn
    Electrocution = 46857, // Helper->self, 7.0s cast, range 3 circle, DeadlyDoornail before spawn
    BarbedBurst = 45965, // 4AE1->self, 16.0s cast, range 60 circle, raidwide if FatalFlail not killed

    HellInACell = 45973, // VampFatale->self, 3.8+1.2s cast, single-target
    BloodyBondage = 45974, // Helper->self, 5.0s cast, range 4 circle
    _Ability_BloodLash = 45979, // 4AF4->self, no cast, range 4 circle, damage to player inside cage
    _Ability_BloodLash1 = 45978, // 4AF3->self, no cast, range 4 circle
    _Ability_BloodLash2 = 45977, // 4AE3->self, no cast, range 4 circle
    LastLash = 46855, // 4AF4->self, 5.0s cast, ???, damage if cell not killed?

    UltrasonicSpreadCast = 45980, // VampFatale->self, 5.0s cast, single-target
    UltrasonicSpreadTank = 47235, // Helper->players, no cast, range 40 ?-degree cone
    UltrasonicSpreadRest = 45982, // Helper->players, no cast, range 40 ?-degree cone
    UltrasonicAmpCast = 45981, // VampFatale->self, 5.0s cast, single-target
    UltrasonicAmp = 45983, // Helper->players, no cast, range 40 ?-degree cone

    _Spell_UnmitigatedExplosion = 45975, // Helper->self, no cast, range 60 circle, hell in a cell tower missed
    UndeadDeathmatchCast = 45984, // VampFatale->self, 3.8+1.2s cast, single-target
    BloodyBondageUndeadDeathmatch = 45985, // Helper->self, 5.0s cast, range 6 circle
    _Spell_UnmitigatedExplosion1 = 45986, // Helper->self, no cast, range 60 circle, undead deathmatch tower 

    _Ability_Explosion = 45987, // Helper->player, no cast, single-target
    SanguineScratchCast = 45988, // VampFatale->self, 2.3+0.7s cast, single-target
    SanguineScratchFirst = 45989, // Helper->self, 3.0s cast, range 40 30-degree cone
    _Ability_SanguineScratch = 45990, // VampFatale->self, no cast, single-target
    SanguineScratchRest = 45991, // Helper->self, no cast, range 40 30-degree cone
    BreakdownDrop1 = 45992, // VampetteFatale->self, 1.0s cast, range 7 circle
    BreakwingBeat1 = 45993, // VampetteFatale->self, 1.0s cast, range 4-15 donut
    BreakdownDrop2 = 45994, // VampetteFatale->self, 1.0s cast, range 7 circle
    BreakwingBeat2 = 45995, // VampetteFatale->self, 1.0s cast, range 4-15 donut

    FinaleFataleEnrageCast = 45934, // Boss->self, 10.0s cast, single-target
    FinaleFataleEnrage = 45937, // Helper->self, no cast, range 60 circle
}

public enum SID : uint
{
    _Gen_2056 = 2056, // none->VampFatale, extra=0x415/0x41A/0x416/0x417/0x41B/0x418/0x419/0x41C/0x426/0x427
    CurseOfTheBombpyre = 4729, // none->player, extra=0x0
    VampetteDistance = 1957, // none->4C2F, extra=0x25/0x33/0x37/0x4B, vampettes, marks distance from center? all rotate 90 degrees before exploding, 0x4B sanguine scratch 180 degrees
    MagicVulnerabilityUp = 2941, // Helper/4C2F->player, extra=0x0
    Satisfied = 4727, // none->VampFatale, extra=0x2/0x3/0x4/0x6/0x7/0x8/0x9/0xA/0xB/0xC/0xD/0x1/0x5/0x10
    DirectionalDisregard = 3808, // none->VampFatale, extra=0x0
    FleshWound = 2942, // Helper->player, extra=0x0
    _Gen_Electrocution1 = 3073, // none->player, extra=0x0
    _Gen_Electrocution2 = 3074, // none->player, extra=0x0
    HellAwaits = 4730, // Helper->player, extra=0x0, player in cage, lasts until after 2nd towers resolve
    HellInACell1 = 4731, // none->player, extra=0x32, HellInACell1 can only attack cage with HeelOfTheCell1 etc.
    HellInACell2 = 4732, // none->player, extra=0x32
    HellInACell3 = 4733, // none->player, extra=0x32
    HellInACell4 = 4734, // none->player, extra=0x32
    HellInACell5 = 4735, // none->player, extra=0x32
    HellInACell6 = 4736, // none->player, extra=0x32
    HellInACell7 = 4737, // none->player, extra=0x32
    HeelOfTheCell1 = 4739, // none->4AE3, extra=0x0
    HeelOfTheCell2 = 4740, // none->4AF3, extra=0x0
    HeelOfTheCell3 = 4741, // none->4AF4, extra=0x0
    HeelOfTheCell4 = 4742, // none->4AF4, extra=0x0
    HeelOfTheCell5 = 4743, // none->4AF4, extra=0x0
    HeelOfTheCell6 = 4744, // none->4AF3, extra=0x0
    HeelOfTheCell7 = 4745, // none->4AF4, extra=0x0
}

public enum IconID : uint
{
    TankLockon = 468, // player->self
    ShareMulti = 305, // player->self
    SpreadLockon = 652, // player->self
}

public enum TetherID : uint
{
    Chains = 353, // CharnelCell1/CharnelCell/CharnelCell2/player->player/VampetteFatale
}

