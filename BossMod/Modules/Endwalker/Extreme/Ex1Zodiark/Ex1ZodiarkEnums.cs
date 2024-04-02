namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

public enum OID : uint
{
    Boss = 0x324D,
    Helper = 0x233C, // x20
    Behemoth = 0x3832, // x2
    Python = 0x3833, // x2
    Bird = 0x3834, // x4
    ExoGreen = 0x358F, // x4
    ExoSquare = 0x3590, // x5
    ExoTri = 0x3591, // x5
    RoilingDarkness = 0x3595, // x4, spawn during first intermission
};

public enum AID : uint
{
    InfernalStream = 21201, // Helper->self, no cast, related to fire line
    AdikiaL = 25513, // Helper->self, first hit
    ParaBehemothVisual = 26191, // Behemoth->self, no cast
    ParaSnakeVisual = 26192, // Python->self, no cast
    ParaBirdVisual = 26193, // Bird->self, no cast
    AstralFlowCW = 26210, // Boss->self
    AstralFlowCCW = 26211, // Boss->self
    Paradeigma = 26559, // Boss->self
    ExoterikosFront = 26560, // Boss->self (front tri/sq - only exo2)
    ExoterikosGeneric = 26561, // Boss->self (side/back tri/sq)
    TrimorphosExoterikos = 26562, // Boss->self (staggered)
    AstralEclipse = 26563, // Boss->self
    TripleEsotericRay = 26564, // Boss->self
    InfernalTorrent = 26592, // Helper->self, no cast, related to fire line
    ParaBirdAOE = 26593, // Helper->self, no cast, [5,15] donut
    ParaBehemothAOE = 26594, // Helper->self, no cast
    ParaSnakeAOE = 26595, // Helper->self, no cast
    EsotericRay = 26596, // ExoGreen->self
    EsotericDyad = 26597, // ExoSquare->self
    EsotericSect = 26598, // ExoTri->self
    ExplosionStars = 26599, // Helper->self (7 concurrent casts)
    AddsEndFail = 26600, // Boss->self
    AddsEndSuccess = 26601, // Boss->self 'Apomnemoneumata'
    PhlegetonAOE = 26602, // Helper->none (3x6 casts)
    Phlegeton = 26603, // Boss->self
    AlgedonTL = 26604, // Boss->self
    AlgedonTR = 26605, // Boss->self
    AlgedonAOE = 26606, // Helper->self
    Ania = 26607, // Boss->self
    Phobos = 26608, // Boss->self
    Adikia = 26609, // Boss->self
    AdikiaR = 26610, // Helper->self, second hit
    Styx = 26611, // Boss->self
    StyxAOE = 26612, // Helper->target, no cast
    Enrage = 26613,
    AniaAOE = 27491, // Helper->MT, starts with Ania, 1 sec longer
    Kokytos = 27744, // Boss->self
    AutoAttack = 27763, // Boss->target, no cast
    ApomnemoneumataLethal = 28026, // Helper->self
    ApomnemoneumataNormal = 28027, // Helper->self
};

public enum SID : uint
{
    TenebrousGrasp = 2832,
}

public enum TetherID : uint
{
    ExoTri = 164, // Boss->ExoTri
    ExoSquare = 171, // Boss->ExoSquare
};

public enum IconID : uint
{
    Styx = 316,
}
