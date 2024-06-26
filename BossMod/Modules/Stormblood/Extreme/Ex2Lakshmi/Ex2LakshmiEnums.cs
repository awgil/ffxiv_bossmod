namespace BossMod.Stormblood.Extreme.Ex2Lakshmi;

public enum OID : uint
{
    Boss = 0x1C8E, // R3.500, x1
    DreamingKshatriya = 0x1C90, // R1.000, x2
    Helper = 0x18D6, // R0.500, x16, 523 type
    Actor1ea76c = 0x1EA76C, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 8535, // Boss->player, no cast, single-target

    AlluringArm = 8526, // Boss->self, 3.0s cast, single-target

    BlissfulArrow = 8527, // Helper->player, no cast, single-target
    BlissfulHammer = 8668, // Helper->player, no cast, range 7 circle
    BlissfulSpear = 8529, // Helper->self, no cast, range 40 width 8 cross

    Chanchala = 8520, // Boss->self, 3.0s cast, single-target
    DivineDenial = 8521, // Boss->self, 5.0s cast, range 40 circle

    HandOfBeauty = 8525, // Boss->self, 3.0s cast, single-target
    HandOfGrace = 8524, // Boss->self, 3.0s cast, single-target

    Stotram = 8519, // Boss->self, 3.0s cast, range 40 circle
    ThePallOfLight = 8540, // Boss->player, 5.0s cast, range 7 circle

    ThePathOfLight = 8538, // Boss->self, no cast, range 40+R ?-degree cone

    ThePullOfLight1 = 8542, // Boss->player, 5.0s cast, single-target
    ThePullOfLight2 = 8543, // Boss->player, 5.0s cast, single-target

    InnerDemons = 9613, // DreamingKshatriya->self, 4.0s cast, range 6+R circle
}

public enum SID : uint
{
    TargetRight = 1374, // none->player, extra=0x0
    TargetLeft = 1375, // none->player, extra=0x0
    Chanchala = 1410, // Boss->Helper/Boss, extra=0x0
}

public enum IconID : uint
{
    Icon107 = 107, // player
    Protean = 14, // player
    Icon109 = 109, // player
    Stackmarker = 62, // player
}
