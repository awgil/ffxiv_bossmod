namespace BossMod.Endwalker.Savage.P6SHegemone;

public enum OID : uint
{
    Boss = 0x3A37, // x1
    Helper = 0x233C, // x25
    PolySquare = 0x3AD9, // x4 (used for swap tethers)
    Parasitos = 0x3AEB, // x8 (transmission)
    //_Gen_Parasitos = 0x3A38, // x8
    //_Gen_Parasitos = 0x3A39, // x4
};

public enum AID : uint
{
    AutoAttack = 31256, // Boss->player, no cast, single-target
    AutoAttack2 = 31257, // Boss->player, no cast, single-target ???
    Teleport = 30796, // Boss->location, no cast, single-target

    HemitheosDark = 30816, // Boss->self, 5.0s cast, raidwide
    Synergy = 30855, // Boss->self, 6.3s cast, single-target, visual tankbuster
    SynergyAOE1 = 30856, // Helper->player, 7.0s cast, range 5 circle, tankbuster at MT
    SynergyAOE2 = 30857, // Helper->players, 7.0s cast, range 5 circle, tankbuster at OT
    ChelicSynergy = 30858, // Boss->self, 7.0s cast, range 60 60-degree cone tankbuster

    AetherialExchange = 30797, // Boss->self, 3.0s cast, single-target, visual (next mechanic will have swap tethers)
    AethericPolyominoid = 30822, // Boss->self, 4.0s cast, single-target, visual (plus/cross appears)
    PolyominousDark = 30823, // Helper->self, 1.0s cast, range 10 width 10 rect (square explosion)
    PolyominoidSigma = 30824, // Boss->self, 4.0s cast, single-target, visual (plus/cross appears)
    PolySquaresStart = 30804, // Helper->self, no cast, single-target, ??? (follows poly...)

    UnholyDarkness = 30865, // Boss->self, 4.0s cast, single-target, visual (party stacks)
    UnholyDarknessAOE = 30866, // Helper->players, 6.0s cast, range 6 circle shared
    Exocleaver = 30825, // Boss->self, 4.0s cast, single-target, visual
    ExocleaverAOE1 = 30826, // Helper->self, 4.0s cast, range 30 30-degree cone aoe
    ExocleaverAOE2 = 30827, // Helper->self, no cast, range 30 30-degree cone aoe

    PathogenicCells = 30820, // Boss->self, 8.0s cast, single-target, visual
    PathogenicCellsAOE = 30821, // Boss->self, no cast, range 40 90-degree cone aoe, baited on number

    ExchangeOfAgonies = 30828, // Boss->self, 4.0s cast, single-target, visual
    AgoniesUnholyDarkness1 = 30829, // Helper->players, 7.0s cast, range 6 circle shared (stack->stack icon)
    AgoniesUnholyDarkness2 = 30830, // Helper->players, 7.0s cast, range 6 circle shared (circle->stack icon)
    AgoniesUnholyDarkness3 = 31247, // Helper->players, 7.0s cast, range 6 circle shared (during role mechanic)
    AgoniesDarkburst1 = 30832, // Helper->player, 7.0s cast, range 15 circle aoe (circle->circle icon)
    AgoniesDarkburst2 = 30833, // Helper->player, 7.0s cast, range 15 circle aoe (stack->circle icon)
    AgoniesDarkburst3 = 30834, // Helper->player, 7.0s cast, range 15 circle aoe (donut->circle icon)
    AgoniesDarkPerimeter1 = 30835, // Helper->players, 7.0s cast, range 6-15 donut (donut->donut icon)
    AgoniesDarkPerimeter2 = 30837, // Helper->players, 7.0s cast, range 6-15 donut (circle->donut icon)

    ChorosIxouFSFront = 30849, // Boss->self, 4.5s cast, single-target, visual (front then sides)
    ChorosIxouSFFront = 30850, // Boss->self, no cast, single-target
    ChorosIxouSFSides = 30851, // Boss->self, 4.5s cast, single-target, visual (sides then front)
    ChorosIxouFSSides = 30852, // Boss->self, no cast, single-target, visual
    ChorosIxouFSFrontAOE = 30853, // Helper->self, 5.0s cast, range 40 90-degree cone (first front/back)
    ChorosIxouSFSidesAOE = 30854, // Helper->self, 5.0s cast, range 40 90-degree cone (first sides)
    ChorosIxouSFFrontAOE = 31211, // Helper->self, 0.5s cast, range 40 90-degree cone
    ChorosIxouFSSidesAOE = 31212, // Helper->self, 0.5s cast, range 40 90-degree cone

    Transmission = 30817, // Boss->self, 5.0s cast, single-target, visual
    ReekHavoc = 31214, // Parasitos->self, 0.5s cast, range 60 30-degree cone (snake => hits forward)
    ChelicClaw = 31215, // Parasitos->self, 0.5s cast, range 60 30-degree cone (wing => hits backward)

    DarkDome = 30859, // Boss->self, 4.0s cast, single-target, visual
    DarkDomeAOE = 30860, // Helper->location, 4.0s cast, range 5 circle aoe (baited)

    DarkAshes = 30861, // Boss->self, 4.0s cast, single-target, visual (spread)
    DarkAshesAOE = 30862, // Helper->player, 8.0s cast, range 6 circle

    Cachexia = 30838, // Boss->self, 3.0s cast, range 50 circle, visual (applies debuffs)
    Aetheronecrosis = 30839, // Helper->self, no cast, range 8 circle aoe (on timer expire)
    DualPredationFirst = 30840, // Boss->self, 6.0s cast, single-target, visual (first snake/wing bait)
    DualPredationRest = 30841, // Boss->self, no cast, single-target, visual (rest baits)
    GlossalPredation = 30842, // Helper->players, no cast, range 5 circle (snake side)
    ChelicPredation = 30843, // Helper->players, no cast, range 5 circle (wing side)
    PteraIxou = 30844, // Boss->self, 6.0s cast, single-target, visual
    PteraIxouAOESnake = 30845, // Helper->self, no cast, range 30 180-degree cone (snake side)
    PteraIxouAOEWing = 30846, // Helper->self, no cast, range 30 180-degree cone (wing side)
    PteraIxouUnholyDarkness = 30847, // Helper->players, 6.0s cast, range 6 circle (during second mechanic)
    PteraIxouDarkSphere = 30848, // Helper->players, 6.0s cast, range 10 circle (during second mechanic)

    DarkSphere = 30863, // Boss->self, 4.0s cast, single-target, visual
    DarkSphereAOE = 30864, // Helper->players, 6.0s cast, range 10 circle aoe (spread)

    Enrage = 30867, // Boss->self, 10.0s cast
};

public enum SID : uint
{
    GlossalResistanceDown = 3319, // Helper->player, extra=0x0, vulnerable to snake
    ChelicResistanceDown = 3320, // Helper->player, extra=0x0, vulnerable to wing
    Aetheronecrosis = 3321, // none->player, extra=0x0
    Chelomorph = 3315, // none->player, extra=0x0, wing infection
    Glossomorph = 3400, // none->player, extra=0x0, snake infection
    OutOfControlWing = 3362, // none->player, extra=0x0
    OutOfControlSnake = 3316, // none->player, extra=0x0
};

public enum TetherID : uint
{
    AetherialExchange = 202, // player->player
    TransmissionSnake = 208, // player->Boss
    TransmissionWing = 209, // player->Boss
    PolyExchange = 207, // PolySquare->PolySquare
};

public enum IconID : uint
{
    Pathogenic1 = 79, // player
    Pathogenic2 = 80, // player
    Pathogenic3 = 81, // player
    Pathogenic4 = 82, // player
    Pathogenic5 = 83, // player
    Pathogenic6 = 84, // player
    Pathogenic7 = 85, // player
    Pathogenic8 = 86, // player
    AgoniesStackToStack = 355, // player
    AgoniesCircleToCircle = 356, // player
    AgoniesStackToCircle = 357, // player
    AgoniesCircleToStack = 359, // player
    AgoniesCircleToDonut = 360, // player
    AgoniesDonutToCircle = 362, // player
    AgoniesDonutToDonut = 366, // player
    DarkAshes = 101, // player
    UnholyDarkness = 318, // player
    DarkSphere = 328, // player
};
