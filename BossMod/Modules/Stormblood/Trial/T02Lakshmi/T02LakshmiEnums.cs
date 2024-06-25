namespace BossMod.Stormblood.Trial.T02Lakshmi;

public enum OID : uint
{
    Boss = 0x1E20, // R3.500, x1
    Helper = 0x18D6, // R0.500, x16, 523 type
    Lakshmi2 = 0x1D27, // R1.000, x10
    Lakshmi3 = 0x1E23, // R0.000, x1
    DreamingKshatriya = 0x1E22, // R1.000, x2
    Actor1e24 = 0x1E24, // R5.000, x1 (spawn during fight)
    Vril = 0x1E21, // R1.000, x12 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 8535, // Boss->player, no cast, single-target

    AetherDrain = 9357, // Vril->player, no cast, single-target

    AlluringArm = 9352, // Boss->self, 7.0s cast, single-target

    AlluringEmbrace1 = 9358, // Lakshmi3->self, no cast, range 0 circle
    AlluringEmbrace2 = 9366, // Helper->self, no cast, range 100 circle

    BlissfulArrow1 = 9353, // Helper->player, no cast, single-target
    BlissfulArrow2 = 9354, // Helper->player, no cast, single-target

    BlissfulSpear1 = 9355, // Helper->self, no cast, range 40 width 8 cross
    BlissfulSpear2 = 9356, // Helper->self, no cast, range 40 width 8 cross
    BlissfulSpear3 = 9364, // Helper->player, no cast, range 7 circle
    BlissfulSpear4 = 9365, // Helper->player, no cast, range 7 circle

    Chanchala = 9348, // Boss->self, 3.0s cast, single-target
    DivineDenial = 9349, // Boss->self, 8.0s cast, range 40 circle
    HandOfGrace = 9350, // Boss->self, 7.0s cast, single-target
    HandOfBeauty = 9351, // Boss->self, 7.0s cast, single-target
    Jagadishwari = 9026, // Boss->self, no cast, single-target

    Stotram1 = 9347, // Boss->self, 3.0s cast, range 40 circle
    Stotram2 = 9374, // Boss->self, 3.0s cast, range 40 circle

    ThePallOfLightStack = 9361, // Boss->players, 5.0s cast, range 7 circle

    ThePullOfLightTB1 = 9362, // Boss->player, 5.0s cast, single-target
    ThePullOfLightTB2 = 9363, // Boss->player, 5.0s cast, single-target

    ThePathOfLightProtean = 9377, // Boss->self, no cast, range 40+R ?-degree cone

    Unknown1 = 9305, // Lakshmi3->self, no cast, single-target
    Unknown2 = 9306, // Helper->self, no cast, single-target
}

public enum SID : uint
{
    TargetRight = 1374, // none->player, extra=0x0
    TargetLeft = 1375, // none->player, extra=0x0
    Bleeding = 320, // none->player, extra=0x0
    Seduced = 1389, // none->player, extra=0x17EC
    Chanchala = 1410, // Boss->Helper/Boss, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Protean = 14, // player
    Stackmarker = 62, // player
    Spread1 = 107, // player
    Spread2 = 109, // player
    Tankbuster = 218, // player
}
