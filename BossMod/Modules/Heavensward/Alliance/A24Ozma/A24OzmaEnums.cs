namespace BossMod.Heavensward.Alliance.A24Ozma;

public enum OID : uint
{
    Boss = 0x1560, // R12.500, x?
    OzmaHelper = 0x1B2, // R0.500, x?, mixed types
    SingularityFragment = 0x164D, // R3.000, x?
    Ozmasphere = 0x164B, // R1.000, x?
    SingularityEcho = 0x164E, // R4.000, x?
    Ozmashade = 0x164A, // R12.500, x?
    SingularityRipple = 0x1650, // R2.100, x?
}

public enum AID : uint
{
    Attack1 = 6192, // OzmaHelper->player, no cast, single-target
    AutoAttack = 872, // SingularityFragment->player, no cast, single-target
    Attack2 = 6193, // OzmaHelper->self, no cast, range 40+R width 4 rect
    Attack3 = 6194, // OzmaHelper->players, no cast, range 4 circle

    AccelerationBomb = 6191, // Boss->self, no cast, ???
    BlackHole = 6144, // Boss->self, 5.0s cast, range 40 circle
    Execration1 = 6184, // Boss->self, no cast, single-target
    ExecrationAOE = 6185, // OzmaHelper->self, no cast, range 40+R width 10 rect
    Holy = 6190, // Boss->self, 4.0s cast, range 50 circle kb 3

    DebrisBurst = 6454, // SingularityFragment->self, no cast, range 40 circle
    Explosion = 6151, // Ozmasphere->self, no cast, range 6 circle
    FlareStar1 = 6149, // Boss->self, no cast, range 22+R circle
    FlareStar2 = 6150, // OzmaHelper->self, no cast, range 34+R circlesss
    MeteorImpact = 6453, // SingularityFragment->self, 4.0s cast, range 20 circle
    Transfiguration1 = 6147, // Boss->self, no cast, single-target
    Transfiguration2 = 6148, // Boss->self, no cast, single-target
    Transfiguration = 6182, // Boss->self, no cast, single-target
    Transfiguration3 = 6183, // Boss->self, no cast, single-target
    Meteor = 6189, // OzmaHelper->location, no cast, range 10 circle
}

public enum SID : uint
{
    AccelerationBomb = 1072, // Boss->player, extra=0x0
    Bleeding = 1074, // OzmaHelper->player, extra=0x1/0x2/0x3
    BrinkOfDeath = 44, // none->player, extra=0x0
    Cube = 1070, // Boss->Boss, extra=0x0
    DamageDown = 696, // SingularityFragment->player, extra=0x0
    Hover = 412, // none->OzmaHelper, extra=0x0
    Invincibility = 1570, // none->player, extra=0x0
    MagicVulnerabilityUp = 60, // OzmaHelper->player, extra=0x0
    MeatAndMead = 360, // none->player, extra=0xA
    Meteor = 6189, // OzmaHelper->location, no cast, range 10 circle
    Minimum = 438, // OzmaHelper->player, extra=0xF
    Poison = 18, // OzmaHelper->player, extra=0x0
    ProperCare = 362, // none->player, extra=0x14
    Pyramid = 1071, // Boss->Boss, extra=0x0
    ReducedRates = 364, // none->player, extra=0x1E
    Sanction = 245, // none->player, extra=0x0
    Slow = 9, // OzmaHelper->player, extra=0x0
    TheHeatOfBattle = 365, // none->player, extra=0xA
    TheRoadTo80 = 1411, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Transfiguration = 6182, // Boss->self, no cast, single-target
    VulnerabilityUp = 202, // OzmaHelper->player, extra=0x1
    Weakness = 43, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_57 = 57, // player
    Icon_62 = 62, // player
    Icon_75 = 75, // player
}