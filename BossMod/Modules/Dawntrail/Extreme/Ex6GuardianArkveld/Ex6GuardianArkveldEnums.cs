#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

public enum OID : uint
{
    Boss = 0x48E5,
    Helper = 0x233C,
}

public enum AID : uint
{
    _Weaponskill_Roar = 43950, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_ChainbladeBlow = 43888, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ChainbladeBlow1 = 45077, // Helper->self, 6.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow2 = 45078, // Helper->self, 6.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance = 43892, // Helper->self, 7.2s cast, range 80 width 28 rect
    _Weaponskill_ChainbladeBlow3 = 43893, // Boss->self, no cast, single-target
    _Weaponskill_ChainbladeBlow4 = 43895, // Helper->self, 1.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow5 = 43896, // Helper->self, 1.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance1 = 43897, // Helper->self, 2.2s cast, range 80 width 28 rect
    _Weaponskill_ = 45175, // Boss->location, no cast, single-target
    _Weaponskill_GuardianSiegeflight = 43899, // Boss->location, 5.0s cast, range 40 width 4 rect
    _Weaponskill_GuardianSiegeflight1 = 43900, // Helper->self, 6.5s cast, range 40 width 8 rect
    _Weaponskill_1 = 45125, // Helper->self, 7.2s cast, range 40 width 8 rect
    _Weaponskill_WhiteFlash = 43906, // Helper->players, 8.0s cast, range 6 circle
    _Weaponskill_GuardianResonance = 43901, // Helper->self, 10.0s cast, range 40 width 16 rect
    _Weaponskill_WyvernsRattle = 43939, // Boss->self, no cast, single-target
    _Weaponskill_WyvernsRadiance2 = 43940, // Helper->self, 2.5s cast, range 8 width 40 rect
    _Weaponskill_WyvernsRadiance3 = 43942, // Helper->location, 5.0s cast, range 6 circle
    _Weaponskill_WyvernsRadiance4 = 43941, // Helper->self, 1.0s cast, range 8 width 40 rect
    _Weaponskill_WyvernsRadiance5 = 43904, // Helper->self, 10.0s cast, range 40 width 18 rect
    _Weaponskill_WyvernsRadiance6 = 43905, // Helper->self, 10.0s cast, range 40 width 18 rect
    _Weaponskill_Dragonspark = 43907, // Helper->players, 8.0s cast, range 6 circle
    _Weaponskill_WyvernsSiegeflight1 = 43903, // Helper->self, 6.5s cast, range 40 width 8 rect
    _Weaponskill_2 = 45111, // Helper->self, 7.2s cast, range 40 width 8 rect
    _Weaponskill_WyvernsSiegeflight = 43902, // Boss->location, 5.0s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow6 = 43887, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ChainbladeBlow7 = 43889, // Helper->self, 6.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow8 = 43890, // Helper->self, 6.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance7 = 43891, // Helper->self, 7.2s cast, range 80 width 28 rect
    _Weaponskill_ChainbladeBlow9 = 43894, // Boss->self, no cast, single-target
    _Weaponskill_ChainbladeBlow10 = 45079, // Helper->self, 1.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow11 = 45080, // Helper->self, 1.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance8 = 43898, // Helper->self, 2.2s cast, range 80 width 28 rect
    _AutoAttack_ = 43342, // Boss->player, no cast, single-target
    _Weaponskill_3 = 43908, // Helper->location, 2.5s cast, width 12 rect charge
    _Weaponskill_4 = 45110, // Helper->self, 3.5s cast, range 8 circle
    _Weaponskill_Rush = 43909, // Boss->location, 6.0s cast, width 12 rect charge
    _Weaponskill_WyvernsRadiance9 = 43911, // Helper->self, 7.5s cast, range 8 circle
    _Weaponskill_Rush1 = 43910, // Boss->location, no cast, width 12 rect charge
    _Weaponskill_WyvernsRadiance10 = 43912, // Helper->self, 9.5s cast, range 20-14 donut
    _Weaponskill_WyvernsRadiance11 = 43913, // Helper->self, 11.5s cast, range 14-20 donut
    _Weaponskill_WyvernsRadiance12 = 43914, // Helper->self, 13.5s cast, range 20-26 donut
    _Weaponskill_5 = 43827, // Boss->location, no cast, single-target
    _Weaponskill_WyvernsOuroblade = 43917, // Boss->self, 6.0+1.5s cast, single-target
    _Weaponskill_WyvernsOuroblade1 = 43918, // Helper->self, 7.0s cast, range 40 180-degree cone
    _Weaponskill_WildEnergy = 43932, // Helper->players, 8.0s cast, range 6 circle
    _Weaponskill_SteeltailThrust = 43949, // Boss->self, 4.0s cast, range 60 width 6 rect
    _Weaponskill_SteeltailThrust1 = 44805, // Helper->self, 4.6s cast, range 60 width 6 rect
    _Weaponskill_ChainbladeCharge = 43947, // Boss->self, 6.0s cast, single-target
    _Weaponskill_ChainbladeCharge1 = 43948, // Boss->player, no cast, single-target
    _Weaponskill_ChainbladeCharge2 = 44812, // Helper->location, no cast, range 6 circle
    _Weaponskill_GuardianResonance1 = 43923, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_AethericResonance = 43919, // Boss->self, 9.7+1.3s cast, single-target
    _Weaponskill_GuardianResonance2 = 43920, // Helper->location, 11.0s cast, range 2 circle
    _Weaponskill_GuardianResonance3 = 43921, // Helper->location, 11.0s cast, range 4 circle
    _Weaponskill_GreaterResonance = 43922, // Helper->location, no cast, range 60 circle
    _Weaponskill_6 = 43859, // Boss->self, no cast, single-target
    _Weaponskill_WyvernsVengeance = 43926, // Helper->self, 5.0s cast, range 6 circle
    _Weaponskill_WyvernsOuroblade2 = 43915, // Boss->self, 6.0+1.5s cast, single-target
    _Weaponskill_WyvernsOuroblade3 = 43916, // Helper->self, 7.0s cast, range 40 180-degree cone
    _Weaponskill_WyvernsRadiance13 = 43924, // 48E6->self, 0.5s cast, range 6 circle
    _Weaponskill_WyvernsRadiance14 = 43925, // 48E7->self, 0.5s cast, range 12 circle
    _Weaponskill_WyvernsVengeance1 = 43927, // Helper->location, no cast, range 6 circle
    _Weaponskill_WyvernsRadiance15 = 44809, // Helper->self, 1.0s cast, range 6 circle
    _Weaponskill_WyvernsRadiance16 = 44810, // Helper->self, 1.0s cast, range 12 circle
    _Weaponskill_ForgedFury = 43934, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ForgedFury1 = 43935, // Helper->self, 7.0s cast, range 60 circle
    _Weaponskill_ForgedFury2 = 44792, // Helper->self, 7.8s cast, range 60 circle
    _Weaponskill_ForgedFury3 = 44793, // Helper->self, 10.2s cast, range 60 circle
    _Weaponskill_Roar1 = 45202, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_ClamorousChase = 43958, // Boss->self, 8.0s cast, single-target
    _Weaponskill_ClamorousChase1 = 43959, // Boss->location, no cast, range 6 circle
    _Weaponskill_ClamorousChase2 = 43960, // Helper->self, 1.0s cast, range 60 ?-degree cone
    _Weaponskill_ClamorousChase3 = 43955, // Boss->self, 8.0s cast, single-target
    _Weaponskill_ClamorousChase11 = 43956, // Boss->location, no cast, range 6 circle
    _Weaponskill_ClamorousChase21 = 43957, // Helper->self, 1.0s cast, range 60 ?-degree cone
    _Weaponskill_WyvernsWeal = 43936, // Boss->self, 8.0s cast, single-target
    _Weaponskill_WyvernsWeal1 = 43872, // Boss->self, no cast, single-target
    _Weaponskill_WyvernsWeal2 = 43937, // Helper->self, 2.0s cast, range 60 width 6 rect
    _Weaponskill_WyvernsWeal3 = 43938, // Helper->self, 0.5s cast, range 60 width 6 rect
    _Weaponskill_7 = 43873, // Boss->self, no cast, single-target
    _Weaponskill_WrathfulRattle = 43943, // Boss->self, 1.0+2.5s cast, single-target
    _Weaponskill_WyvernsRadiance17 = 43944, // Helper->self, 3.5s cast, range 40 width 8 rect
    _Weaponskill_WyvernsRadiance18 = 43945, // Helper->self, 1.0s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow12 = 45082, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ChainbladeBlow13 = 45086, // Helper->self, 5.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow14 = 45087, // Helper->self, 5.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance19 = 45088, // Helper->self, 6.2s cast, range 80 width 28 rect
    _Weaponskill_ChainbladeBlow15 = 45089, // Boss->self, no cast, single-target
    _Weaponskill_WyvernsRadiance20 = 43946, // Helper->self, 2.0s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow16 = 45091, // Helper->self, 1.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow17 = 45092, // Helper->self, 1.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance21 = 45093, // Helper->self, 2.2s cast, range 80 width 28 rect
    _Weaponskill_WyvernsOuroblade4 = 45106, // Boss->self, 5.0+1.5s cast, single-target
    _Weaponskill_WyvernsOuroblade5 = 45108, // Helper->self, 6.0s cast, range 40 180-degree cone
    _Weaponskill_SteeltailThrust2 = 45109, // Boss->self, 3.0s cast, range 60 width 6 rect
    _Weaponskill_SteeltailThrust3 = 44806, // Helper->self, 3.6s cast, range 60 width 6 rect
    _Weaponskill_WyvernsRadiance22 = 43933, // Helper->location, 5.0s cast, range 12 circle
    _Weaponskill_ChainbladeBlow18 = 45081, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ChainbladeBlow19 = 45083, // Helper->self, 5.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow20 = 45084, // Helper->self, 5.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance23 = 45085, // Helper->self, 6.2s cast, range 80 width 28 rect
    _Weaponskill_ChainbladeBlow21 = 45090, // Boss->self, no cast, single-target
    _Weaponskill_ChainbladeBlow22 = 45094, // Helper->self, 1.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow23 = 45095, // Helper->self, 1.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance24 = 45096, // Helper->self, 2.2s cast, range 80 width 28 rect
    _Weaponskill_Roar2 = 43951, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_WyvernsVengeance2 = 43952, // Helper->self, 8.0s cast, range 6 circle
    _Weaponskill_WyvernsVengeance3 = 43953, // Helper->location, no cast, range 6 circle
    _Weaponskill_GuardianSiegeflight2 = 45097, // Boss->location, 4.0s cast, range 40 width 4 rect
    _Weaponskill_GuardianSiegeflight3 = 45099, // Helper->self, 5.5s cast, range 40 width 8 rect
    _Weaponskill_8 = 45126, // Helper->self, 6.2s cast, range 40 width 8 rect
    _Weaponskill_GuardianResonance4 = 45100, // Helper->self, 9.0s cast, range 40 width 16 rect
    _Weaponskill_WyvernsSiegeflight2 = 45098, // Boss->location, 4.0s cast, range 40 width 4 rect
    _Weaponskill_WyvernsSiegeflight3 = 45101, // Helper->self, 5.5s cast, range 40 width 8 rect
    _Weaponskill_9 = 45104, // Helper->self, 6.2s cast, range 40 width 8 rect
    _Weaponskill_WyvernsRadiance25 = 45102, // Helper->self, 9.0s cast, range 40 width 18 rect
    _Weaponskill_WyvernsRadiance26 = 45103, // Helper->self, 9.0s cast, range 40 width 18 rect
    _Weaponskill_WyvernsOuroblade6 = 45105, // Boss->self, 5.0+1.5s cast, single-target
    _Weaponskill_WyvernsOuroblade7 = 45107, // Helper->self, 6.0s cast, range 40 180-degree cone
}

public enum IconID : uint
{
    _Gen_Icon_com_share2i = 100, // player->self
    _Gen_Icon_m0074g01ai = 101, // player->self
    _Gen_Icon_m0361trg_b1t = 404, // player->self
    _Gen_Icon_m0361trg_b2t = 405, // player->self
    _Gen_Icon_m0361trg_b3t = 406, // player->self
    _Gen_Icon_m0361trg_b4t = 407, // player->self
    _Gen_Icon_m0361trg_b5t = 408, // player->self
    _Gen_Icon_m0361trg_b6t = 409, // player->self
    _Gen_Icon_m0361trg_b7t = 410, // player->self
    _Gen_Icon_m0361trg_b8t = 411, // player->self
    _Gen_Icon_lockon8_line_1v = 470, // player->self
}
