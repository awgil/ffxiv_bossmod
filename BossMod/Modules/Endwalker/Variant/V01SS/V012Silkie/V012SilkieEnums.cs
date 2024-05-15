namespace BossMod.Endwalker.Variant.V01SS.V012Silkie;

public enum OID : uint
{
    Boss = 0x39EF, // R6.000, x1
    Helper = 0x233C, // R0.500, x15, 523 type
    EasternEwer = 0x39F1, // R2.400, x5
    SilkenPuff = 0x39F0, // R1.000, x15
    Actor1e9230 = 0x1E9230, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eb76e = 0x1EB76E, // R0.500, x0 (spawn during fight), EventObj type // possibly route 3 brooms
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    CarpetBeater = 30507, // Boss->player, 5.0s cast, single-target tankbuster
    TotalWash = 30508, // Boss->self, 5.0s cast, range 60 circle // raidwide
    DustBlusterKnockback = 30532, // Boss->location, 5.0s cast, range 60 circle // knockback

    BracingSuds = 30517, // Boss->self, 5.0s cast, single-target, applies green status
    ChillingSuds = 30518, // Boss->self, 5.0s cast, single-target, applies blue status
    SoapsUp = 30519, // Boss->self, 4.0s cast, single-target, removes color statuses
    FreshPuff = 30525, // Boss->self, 4.0s cast, single-target, visual (statuses on puffs)
    SoapingSpreeBoss = 30526, // Boss->self, 6.0s cast, single-target, visual
    SoapingSpreePuff = 30529, // SilkenPuff->self, 6.0s cast, single-target, removes color statuses

    ChillingDuster1 = 30520, // Helper->self, 5.0s cast, range 60 width 10 cross
    BracingDuster1 = 30521, // Helper->self, 5.0s cast, range ?-60 donut

    ChillingDuster2 = 30523, // Helper->self, 8.5s cast, range 60 width 10 cross
    BracingDuster2 = 30524, // Helper->self, 8.5s cast, range ?-60 donut

    ChillingDuster3 = 30527, // Helper->self, 7.0s cast, range 60 width 10 cross
    BracingDuster3 = 30528, // Helper->self, 8.5s cast, range ?-60 donut

    SlipperySoap = 30522, // Boss->location, 5.0s cast, width 10 rect charge

    SpotRemover1 = 30530, // Boss->self, 3.5s cast, single-target
    SpotRemover2 = 30531, // Helper->location, 3.5s cast, range 5 circle// persistenet aoe

    SqueakyCleanE = 30509, // Boss->self, 4.5s cast, single-target
    SqueakyCleanAOE1E = 30511, // Helper->self, 6.0s cast, range 60 90-degree cone
    SqueakyCleanAOE2E = 30512, // Helper->self, 7.7s cast, range 60 90-degree cone
    SqueakyCleanAOE3E = 30513, // Helper->self, 9.2s cast, range 60 225-degree cone

    SqueakyCleanW = 30510, // Boss->self, 4.5s cast, single-target
    SqueakyCleanAOE1W = 30514, // Helper->self, 6.0s cast, range 60 90-degree cone
    SqueakyCleanAOE2W = 30515, // Helper->self, 7.7s cast, range 60 90-degree cone
    SqueakyCleanAOE3W = 30516, // Helper->self, 9.2s cast, range 60 225-degree cone

    WashOut1 = 30533, // Boss->self, 8.0s cast, single-target
    WashOutKnockback = 30534, // Helper->self, 8.0s cast, range 60 width 60 rect

    //route 2
    EasternEwers = 30535, // Boss->self, 4.0s cast, single-target
    BrimOver = 30536, // EasternEwer->self, 3.0s cast, range 4 circle
    Rinse = 30537, // Helper->self, no cast, range 4 circle

    //route 4
    PuffAndTumbleRest = 30538, // SilkenPuff->location, 3.0s cast, single-target
    PuffAndTumbleVisual = 30539, // SilkenPuff->location, no cast, single-target
    PuffAndTumble1 = 30540, // Helper->location, 4.6s cast, range 4 circle
    PuffAndTumble2 = 30656, // Helper->location, 1.6s cast, range 4 circle

}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x3
    BracingSudsBoss = 3297, // Boss->Boss, extra=0x0
    ChillingSudsBoss = 3298, // Boss->Boss, extra=0x0
    BracingSudsPuff = 3305, // none->SilkenPuff, extra=0x0
    ChillingSudsPuff = 3306, // none->SilkenPuff, extra=0x0
    Route2Ewer = 2397, // none->EasternEwer, extra=0x1EC

}

public enum IconID : uint
{
    Tankbuster = 218, // player
}
