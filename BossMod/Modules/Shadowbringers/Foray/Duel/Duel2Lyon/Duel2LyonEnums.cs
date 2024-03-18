namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

public enum OID : uint
{
    Boss = 0x2E8D, // R0.60
    Helper = 0x233C, // R0.500
    VermillionFlame = 0x2E8F, //R1.2
    RavenousGaleVoidzone = 0x1E8910 //R0.5
};

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    RagingWinds1 = 20800, // Boss->self, 3,0s cast, single-target
    RagingWinds2 = 20801, // Helper->self, 1,0s cast, range 50 circle
    UnfetteredFerocity = 20813, // Boss->location, no cast, single-target
    HeartOfNature = 20794, // Boss->self, 3,0s cast, range 80 circle
    NaturesPulse1 = 20797, // Helper->self, 4,0s cast, range 10 circle
    NaturesPulse2 = 20798, // Helper->self, 5,5s cast, range 10-20 donut
    NaturesPulse3 = 20799, // Helper->self, 7,0s cast, range 20-30 donut
    TasteOfBlood = 20815, // Boss->self, 4,0s cast, range 40 180-degree cone
    RavenousGale = 20802, // Boss->self, 4,0s cast, single-target
    DynasticFlame1 = 20806, // Boss->player, 3,0s cast, range 5 circle
    DynasticFlame2 = 20807, // Boss->player, no cast, range 5 circle
    SpitefulFlame1 = 20808, // VermillionFlame->self, 1,0s cast, range 10 circle
    SpitefulFlame2 = 20809, // VermillionFlame->player, 3,0s cast, range 80 width 4 rect
    TwinAgonies = 20816, // Boss->player, 6,0s cast, single-target
    WindsPeak1 = 20811, // Boss->self, 3,0s cast, range 5 circle
    WindsPeak2 = 20812, // Helper->self, 4,0s cast, range 50 circle
    NaturesBlood1 = 20795, // Helper->self, 7,5s cast, range 4 circle
    NaturesBlood2 = 20796, // Helper->self, no cast, range 4 circle
    TheKingsNotice = 20810, // Boss->self, 5,0s cast, range 50 circle
    SplittingRage = 20814, // Boss->self, 3,0s cast, range 50 circle
    SkyrendingStrike = 20804, //enrage, 35s cast time
    SkyrendingStrike2 = 20805, //enrage, 0 cast time, repeat incase player survived 1st enrage
};

public enum TetherID : uint
{
    fireorbs = 5, // Boss->player
};

public enum SID : uint
{
    Enaero = 206, // Boss->Boss, extra=0x64
    TemporaryMisdirection = 1422, // Boss->player, extra=0x2D0
};
