namespace BossMod.Endwalker.Alliance.A31Thaliak;

public enum OID : uint
{
    Boss = 0x404C, // R9.496, x1
    ThaliakClone = 0x404D, // R9.496, x1
    Helper = 0x233C, // R0.500, x44, 523 type
};

public enum AID : uint
{
    AutoAttack = 35036, // Thaliak->player, no cast, single-target
    Teleport = 35035, // Thaliak->location, no cast, single-target

    Katarraktes = 35025, // Thaliak->self, 5.0s cast, single-target
    KatarraktesAOE = 35034, // ThaliakHelper->self, 5.7s cast, range 70 circle 

    Hieroglyphika = 35023, // Thaliak->self, 5.0s cast, single-target
    HieroglyphikaRect = 35024, // ThaliakHelper->self, 3.0s cast, range 12 width 12 rect

    Thlipsis = 35032, // Thaliak->self, 4.0s cast, single-target, stack marker
    ThlipsisStack = 35033, // ThaliakHelper->players, 6.0s cast, range 6 circle

    Hydroptosis = 35028, // Thaliak->self, 4.0s cast, single-target, spread markers, inflicts Water resistance down making overlap lethal.
    HydroptosisSpread = 35029, // ThaliakHelper->player, 5.0s cast, range 6 circle

    Rhyton = 35030, // Thaliak->self, 5.0s cast, single-target, line AOE tankbusters
    RhytonHelper = 35031, // ThaliakHelper->players, no cast, range 70 width 6 rect

    Rheognosis = 35012, // Thaliak->self, 5.0s cast, single-target
    RheognosisPetrine = 35013, // Thaliak->self, 5.0s cast, single-target
    RheognosisPetrineHelper = 35014, // ThaliakClone->self, no cast, single-target
    RheognosisKnockback = 35015, // ThaliakHelper->self, 3.0s cast, range 48 width 48 rect, knockback 25, dir forward
    RheognosisCrashExaflare = 35016, // ThaliakHelper->self, no cast, range 10 width 24 rect

    Tetraktys = 35017, // Thaliak->self, 6.0s cast, single-target
    TetraBlueTriangles = 35018, // ThaliakHelper->self, 1.8s cast, triangle 16
    TetraGreenTriangles = 35019, // ThaliakHelper->self, 1.8s cast, triangle 32

    TetraktuosKosmos = 35020, // Thaliak->self, 4.0s cast, single-target
    TetraktuosKosmosTri = 35022, // ThaliakHelper->self, 2.9s cast, triangle 16
    TetraktuosKosmosRect = 35021, // ThaliakHelper->self, 2.9s cast, range 30 width 16 rect

    LeftBank = 35026, // Thaliak->self, 5.0s cast, range 60 180-degree cone
    LeftBank2 = 35884, // Thaliak->self, 22.0s cast, range 60 180-degree cone
    RightBank = 35027, // Thaliak->self, 5.0s cast, range 60 180-degree cone
    RightBank2 = 35885, // Thaliak->self, 22.0s cast, range 60 180-degree cone 
};

public enum SID : uint
{
    Bleeding = 2088, // ThaliakHelper->player, extra=0x0
    WaterResistanceDownII = 1025, // ThaliakHelper->player, extra=0x0
    SustainedDamage = 2935, // ThaliakHelper->player, extra=0x0
    DownForTheCount = 783, // ThaliakHelper->player, extra=0xEC7
    Inscribed = 3732, // none->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
};

public enum IconID : uint
{
    Stackmarker = 318, // player
    HydroptosisSpread = 139, // player
    RhytonBuster = 471, // player
    ClockwiseHieroglyphika = 487, // HieroglyphikaIndicator
    CounterClockwiseHieroglyphika = 490, // HieroglyphikaIndicator
};
