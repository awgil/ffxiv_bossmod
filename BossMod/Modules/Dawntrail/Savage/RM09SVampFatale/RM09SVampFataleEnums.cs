namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

public enum OID : uint
{
    Boss = 0x4ADF, // R4.000, x1
    Helper = 0x233C, // R0.500, x36, Helper type
    VampetteFatale = 0x4C2F, // R1.200, x0 (spawn during fight)
    Coffinmaker = 0x4AE0, // R10.000, x0 (spawn during fight)
    DeadlyDoornail = 0x4AE2, // R3.000, x0 (spawn during fight)
    FatalFlail = 0x4AE1, // R3.000, x0 (spawn during fight)
    Neckbiter = 0x4AF5, // R3.000, x0 (spawn during fight)
    CoffinmakerHelper = 0x4AF6, // R1.000, x0 (spawn during fight)
    CharnelCell1 = 0x4AE3, // R4.000, x0 (spawn during fight)
    CharnelCell2 = 0x4AF3, // R4.000, x0 (spawn during fight)
    CharnelCell3 = 0x4AF4, // R4.000, x0 (spawn during fight)

    Electrocution = 0x1EBF1F, // R0.500, x3
}

public enum AID : uint
{
    AutoAttack1 = 48039, // Boss->player, no cast, single-target
    AutoAttack2 = 47048, // Boss->player, no cast, single-target

    KillerVoice = 45956, // Boss->self, 5.0s cast, range 60 circle
    HardcoreCast = 45914, // Boss->self, 3.0+2.0s cast, single-target
    HardcoreSmall = 45951, // Helper->players, 5.0s cast, range 6 circle
    HardcoreBig = 45952, // Helper->players, 5.0s cast, range 15 circle

    VampStompCast = 45898, // Boss->location, 4.1+0.9s cast, single-target
    VampStomp = 45940, // Helper->self, 5.0s cast, range 10 circle
    BatRing = 45900, // Helper->self, no cast, single-target
    BlastBeatSpread = 45942, // Helper->players, no cast, range 8 circle
    BlastBeatBat = 45941, // VampetteFatale->self, 1.0s cast, range 8 circle
    BrutalRainCast = 45917, // Boss->self, 3.8+1.2s cast, single-target
    BrutalRainHit = 45955, // Helper->players, no cast, range 6 circle
    Jump = 45874, // Boss->location, no cast, single-target
    SadisticScreechCast = 45875, // Boss->self, 5.0s cast, single-target
    SadisticScreech = 45926, // Helper->self, no cast, range 60 circle

    DeadWakeCast = 46853, // Coffinmaker->self, 4.5+0.5s cast, single-target
    DeadWake = 45927, // Helper->self, 5.0s cast, range 10 width 20 rect

    HalfMoonCast1 = 45902, // Boss->self, 4.3+0.7s cast, single-target
    HalfMoonCast2 = 45903, // Boss->self, 4.3+0.7s cast, single-target
    HalfMoonCast3 = 45904, // Boss->self, 4.3+0.7s cast, single-target
    HalfMoonCast4 = 45905, // Boss->self, 4.3+0.7s cast, single-target

    HalfMoonShort1 = 45943, // Helper->self, 5.0s cast, range 60 180-degree cone
    HalfMoonLong1 = 45944, // Helper->self, 8.0s cast, range 60 180-degree cone
    HalfMoonShort2 = 45945, // Helper->self, 5.0s cast, range 64 180-degree cone
    HalfMoonLong2 = 45946, // Helper->self, 8.0s cast, range 64 180-degree cone
    HalfMoonShort3 = 45947, // Helper->self, 5.0s cast, range 60 180-degree cone
    HalfMoonLong3 = 45948, // Helper->self, 8.0s cast, range 60 180-degree cone
    HalfMoonShort4 = 45949, // Helper->self, 5.0s cast, range 64 180-degree cone
    HalfMoonLong4 = 45950, // Helper->self, 8.0s cast, range 64 180-degree cone

    CoffinfillerCast = 46854, // Coffinmaker->self, 5.0s cast, single-target
    CoffinfillerLong = 45928, // Helper->self, 5.0s cast, range 32 width 5 rect
    CoffinfillerMedium = 45929, // Helper->self, 5.0s cast, range 22 width 5 rect
    CoffinfillerShort = 45930, // Helper->self, 5.0s cast, range 12 width 5 rect

    CrowdKillCast = 45886, // Boss->self, 0.5+4.9s cast, single-target
    CrowdKill = 45933, // Helper->self, no cast, range 60 circle

    FinaleFataleCast = 45889, // Boss->self, 5.0s cast, single-target
    FinaleFatale = 45936, // Helper->self, no cast, range 60 circle

    PulpingPulse = 45939, // Helper->location, 4.0s cast, range 5 circle

    AetherlettingBoss = 45967, // Boss->self, 12.3+0.7s cast, single-target
    AetherlettingCone = 45969, // Helper->self, 9.0s cast, range 40 45-degree cone
    AetherlettingVisual = 45968, // Boss->self, no cast, single-target
    AetherlettingSpread = 45970, // Helper->players, 5.0s cast, range 6 circle
    AetherlettingGroundIndicator = 45972, // Helper->self, no cast, range 40 width 10 cross
    AetherlettingGround = 45971, // Helper->self, 14.0s cast, range 40 width 10 cross

    InsatiableThirstCast = 45892, // Boss->self, 2.8+2.2s cast, single-target
    InsatiableThirst = 45938, // Helper->self, no cast, range 60 circle

    GravegrazerBig = 45931, // Helper->self, no cast, range 10 width 5 rect
    GravegrazerSmall = 45932, // Helper->self, no cast, range 5 width 5 rect
    Plummet = 45963, // Helper->self, 7.0s cast, range 3 circle
    Electrocution = 46857, // Helper->self, 7.0s cast, range 3 circle
    BarbedBurst = 45965, // FatalFlail->self, 16.0s cast, range 60 circle
    MassiveImpact = 45964, // Helper->self, no cast, range 60 circle, flail tower miss

    FinaleFataleCast2 = 45888, // Boss->self, 5.0s cast, single-target
    FinaleFatale2 = 45935, // Helper->self, no cast, range 60 circle

    HellInACell = 45973, // Boss->self, 3.8+1.2s cast, single-target
    BloodyBondageSolo = 45974, // Helper->self, 5.0s cast, range 4 circle
    NaughtyKnot = 45976, // Helper->player, no cast, single-target
    BloodLash1 = 45977, // CharnelCell1->self, no cast, range 4 circle
    BloodLash2 = 45978, // CharnelCell2->self, no cast, range 4 circle
    BloodLash3 = 45979, // CharnelCell3->self, no cast, range 4 circle
    UltrasonicSpreadCast = 45980, // Boss->self, 5.0s cast, single-target
    UltrasonicSpreadSmall = 45982, // Helper->players, no cast, range 40 45-degree cone (protean)
    UltrasonicSpreadTank = 47235, // Helper->players, no cast, range 40 100-degree cone (tankbuster)
    UltrasonicAmpCast = 45981, // Boss->self, 5.0s cast, single-target
    UltrasonicAmp = 45983, // Helper->players, no cast, range 40 100-degree cone (shared)
    LastLash = 46855, // CharnelCell1/CharnelCell2/CharnelCell3->self, 5.0s cast, enrage
    UnmitigatedExplosion = 45975, // Helper->self, no cast, range 60 circle, cell tower miss

    UndeadDeathmatch = 45984, // Boss->self, 3.8+1.2s cast, single-target
    BloodyBondageParty = 45985, // Helper->self, 5.0s cast, range 6 circle
    Explosion = 45987, // Helper->player, no cast, single-target, shared tower miss?
    SanguineScratchCast = 45988, // Boss->self, 2.3+0.7s cast, single-target
    SanguineScratchFirst = 45989, // Helper->self, 3.0s cast, range 40 30-degree cone
    SanguineScratchBossRepeat = 45990, // Boss->self, no cast, single-target
    SanguineScratchRepeat = 45991, // Helper->self, no cast, range 40 30-degree cone
    BreakdownDrop1 = 45992, // VampetteFatale->self, 1.0s cast, range 7 circle
    BreakwingBeat1 = 45993, // VampetteFatale->self, 1.0s cast, range 8-15 donut
    BreakdownDrop2 = 45994, // VampetteFatale->self, 1.0s cast, range 7 circle
    BreakwingBeat2 = 45995, // VampetteFatale->self, 1.0s cast, range 8-15 donut

    FinaleFataleEnrageCast = 45934, // Boss->self, 10.0s cast, single-target
    FinaleFataleEnrage = 45937, // Helper->self, no cast, range 60 circle
}

public enum SID : uint
{
    Unk2056 = 2056, // none->Boss/Neckbiter/VampetteFatale, extra=0x415/0x41A/0x417/0x443/0x41B/0x418/0x426/0x427/0x41C/0x419/0x41D
    CurseOfTheBombpyre = 4729, // none->player, extra=0x0, explodes when touching bat ring
    Unk1957 = 1957, // none->VampetteFatale, extra=0x37/0x33/0x25/0x4B
    MagicVulnerabilityUp = 2941, // Helper/VampetteFatale->player, extra=0x0
    DirectionalDisregard = 3808, // none->Boss, extra=0x0
    Satisfied = 4727, // none->Boss, extra=0x1-0x10
    MoreThanSatisfied = 4728, // none->Boss, extra=0x0
    HellAwaits = 4730, // Helper->player, extra=0x0, kills player if they take two cell towers

    // player needs matching status to damage the add
    HellInACell1 = 4731, // none->player, extra=0x32
    HellInACell2 = 4732, // none->player, extra=0x32
    HellInACell3 = 4733, // none->player, extra=0x32
    HellInACell4 = 4734, // none->player, extra=0x32
    HellInACell5 = 4735, // none->player, extra=0x32
    HellInACell6 = 4736, // none->player, extra=0x32
    HellInACell7 = 4737, // none->player, extra=0x32
    HellInACell8 = 4738, // none->player, extra=0x32
    HeelOfTheCell1 = 4739, // none->CharnelCell1, extra=0x0
    HeelOfTheCell2 = 4740, // none->CharnelCell2, extra=0x0
    HeelOfTheCell3 = 4741, // none->CharnelCell3, extra=0x0
    HeelOfTheCell4 = 4742, // none->CharnelCell3, extra=0x0
    HeelOfTheCell5 = 4743, // none->CharnelCell1, extra=0x0
    HeelOfTheCell6 = 4744, // none->CharnelCell2, extra=0x0
    HeelOfTheCell7 = 4745, // none->CharnelCell3, extra=0x0
    HeelOfTheCell8 = 4746, // none->CharnelCell3, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 468, // player->self
    ShareMulti = 305, // player->self
    Aetherletting = 652, // player->self
}

public enum TetherID : uint
{
    ShortTether = 353, // CharnelCell1/CharnelCell3/CharnelCell2->player, player->VampetteFatale
    LongTether = 354, // player->VampetteFatale
}
