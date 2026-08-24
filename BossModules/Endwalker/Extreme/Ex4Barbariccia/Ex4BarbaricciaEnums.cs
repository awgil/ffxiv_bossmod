namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

public enum OID : uint
{
    Boss = 0x39CE, // x1
    BarbaricciaShadow = 0x39CF, // x9
    Helper = 0x233C, // x31
    Tangle = 0x3973, // x4, fetters tethers players to one of these
    StiffBreeze = 0x39D0, // spawn during fight, moving circles
    //_Gen_Actor39d4 = 0x39D4, // x1
    //_Gen_Exit = 0x1E850B, // x1, EventObj type
    //_Gen_MagitekArmor = 0x1EB702, // x4, EventObj type
    //_Gen_Actor1e8536 = 0x1E8536, // x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 30072, // Boss->player, no cast, single-target
    Teleport = 30082, // Boss->location, no cast, single-target
    VoidAeroRaidwide = 30064, // Boss->self, 5.0s cast, raidwide
    VoidAeroTankbuster = 30065, // Boss->player, 5.0s cast, range 5 circle aoe tankbuster
    RagingStorm = 30066, // Boss->location, no cast, raidwide + teleport

    SavageBarberyDonut1 = 29796, // Boss->self, 6.0s cast, single-target, visual
    SavageBarberyDonut2 = 29797, // Boss->self, 6.0s cast, single-target, visual
    SavageBarberyDonut3 = 29798, // Boss->self, 6.0s cast, single-target, visual
    SavageBarberyDonut4 = 30067, // Boss->self, 6.0s cast, single-target, visual
    SavageBarberyDonutAOE = 30068, // Helper->self, 7.0s cast, range 6-20 donut
    SavageBarberyDonutSword = 30069, // Helper->self, 9.1s cast, range 20 circle
    SavageBarberyRect1 = 29833, // Boss->self, 6.0s cast, single-target, visual
    SavageBarberyRect2 = 29835, // Boss->self, 6.0s cast, single-target, visual
    SavageBarberyRectAOE = 30074, // Helper->self, 7.0s cast, range 40 width 12 rect
    SavageBarberyRectSword = 30075, // Helper->self, 9.1s cast, range 20 circle
    //BrushWithDeathSavageBarbery = 30116, // Boss->self, no cast, single-target, visual ???
    HairRaidCone = 30076, // Boss->location, 6.0s cast, visual + teleport
    HairRaidConeAOE = 30077, // Helper->self, 8.0s cast, range 40 ?-degree cone
    HairRaidDonut = 30078, // Boss->self, 6.0s cast, single-target, visual
    HairRaidDonutAOE = 30079, // Helper->self, 8.0s cast, range 6-20 donut
    HairSpray = 30118, // Helper->players, 8.0s cast, range 5 circle spread
    DeadlyTwist = 30119, // Helper->players, 8.0s cast, range 6 circle shared

    TeasingTangles1 = 30121, // Boss->self, 4.0s cast, single-target, visual
    TeasingTangles2 = 30122, // Boss->self, no cast, single-target, visual
    Tangle = 30123, // Helper->self, 4.6s cast, range 6 circle aoe
    TangleKnockback = 30124, // Helper->player, no cast, single-target, knockback 9
    TangleFail = 29567, // Helper->player, no cast, single-target, knockback 20 on people leaving tangle zone
    FettersKnockback = 30127, // Helper->player, no cast, single-target, knockback 9 after fetters during entanglement
    Fetters = 30128, // Helper->player, no cast, single-target, applies fetters status
    //BrushWithDeathTangle = 30115, // Boss->self, no cast, single-target, visual ???
    HairFlay = 29715, // Helper->players, 8.0s cast, range 10 circle spread
    Upbraid = 30120, // Helper->players, 8.0s cast, range 3 circle enumeration 2
    SecretBreeze = 30080, // Boss->self, 3.0s cast, single-target, visual
    SecretBreezeAOE = 29717, // Helper->self, 4.0s cast, range 40 45-degree cone aoe
    SecretBreezeProtean = 30081, // Helper->self, no cast, range 40 45-degree cone baited

    CurlingIron = 30130, // Boss->self, 5.0s cast, single-target, visual
    CurlingIronAOE = 30131, // Helper->self, no cast, raidwide
    Voidstrom = 30071, // Helper->self, no cast, range 6 circle, knockback 4 during curling iron
    //Catabasis = 30132, // Boss->self, no cast, single-target, visual ???
    //Catabasis = 29832, // Boss->self, no cast, single-target, visual ???
    Catabasis = 30070, // Helper->self, 8.0s cast, raidwide

    BrutalRush = 30083, // Boss->player, no cast, single-target, rushes 1-3
    BrutalRushLast = 30084, // Boss->player, no cast, single-target, rush 4
    BrutalGust = 30085, // BarbaricciaShadow->self, 2.0s cast, range 40 width 4 rect

    //BoulderBreak = 29570, // Boss->self, no cast, single-target, visual ???
    BoulderBreak = 29571, // Helper->players, 5.0s cast, range 6 circle shared tankbuster

    WarningGaleVisual = 30086, // Boss->self, no cast, single-target, visual ???
    WarningGale = 30087, // Helper->self, 5.0s cast, range 6 circle aoe in center
    WindingGaleVisual1 = 30088, // BarbaricciaShadow->location, no cast, single-target, teleport ???
    WindingGale = 29830, // Helper->self, 5.0s cast, range 9?-11 donut (spiral arm, 180 degree donut offset by ~outer radius?)
    WindingGaleCharge = 30089, // Helper->location, 2.5s cast, width 4 rect charge aoe
    WindingGaleChargeVisual = 30090, // BarbaricciaShadow->location, no cast, width 4 rect charge, visual
    WindingGaleVisual2 = 30091, // BarbaricciaShadow->location, no cast, single-target, visual
    //_Gen_BrushWithDeathWindingGale = 30117, // Boss->self, no cast, single-target, visual ???
    BoulderVisual = 30108, // Boss->self, no cast, single-target, visual ???
    Boulder = 30109, // Helper->location, 4.0s cast, range 10 circle puddle
    BrittleBoulder = 30110, // Helper->players, 3.0s cast, range 5 circle spread
    TornadoChainInner = 30092, // Helper->self, 4.0s cast, range 11 circle aoe
    TornadoChainOuter = 30093, // Helper->self, 6.5s cast, range 11-20 donut aoe
    TornadoChainVisualInner = 30094, // Boss->self, no cast, single-target, visual
    TornadoChainVisualOuter = 30095, // Boss->self, no cast, single-target, visual

    KnuckleDrumVisual = 30103, // Boss->self, no cast, single-target
    KnuckleDrum = 30104, // Helper->self, no cast, raidwide
    KnuckleDrumLast = 30105, // Helper->self, no cast, raidwide

    BlowAwayRaidwide = 30101, // Boss->self, no cast, raidwide
    BlowAwayPuddle = 30102, // Helper->self, 4.0s cast, range 6 circle puddle
    BoldBoulderVisual = 30106, // Boss->self, no cast, single-target, visual
    BoldBoulder = 30107, // Helper->players, 8.0s cast, flare with 20? falloff
    ImpactTeleport = 30111, // Boss->location, no cast, single-target, teleport
    ImpactAOE = 30112, // Helper->self, 6.2s cast, range 6 circle aoe (in center)
    ImpactKnockback = 30113, // Helper->self, 6.2s cast, range 20 circle knockback 6
    Trample = 30114, // Boss->players, no cast, range 6 circle shared

    BlusteryRuler = 30096, // Helper->self, 5.0s cast, range 6 circle aoe (in center)
    BlusteryRulerVisual = 30097, // Boss->self, no cast, single-target, visual
    Tousle = 30098, // StiffBreeze->self, no cast, range 2 circle (when circle is clipped)
    DryBlowsRaidwide = 30099, // Boss->self, no cast, raidwide
    DryBlowsPuddle = 30100, // Helper->location, 3.0s cast, range 3 circle puddle

    IronOut = 30133, // Boss->self, no cast, single-target, visual
    IronOutAOE = 29781, // Helper->self, no cast, raidwide
    Entanglement = 30125, // Boss->self, 4.0s cast, single-target, visual

    Maelstrom = 30142, // Boss->self, 9.0s cast, enrage
}

public enum TetherID : uint
{
    BrutalRush = 17, // player->Boss
    Tangle = 199, // player->Tangle
    Entanglement = 210, // player->player
}

public enum IconID : uint
{
    Trample = 100, // player
    BoldBoulder = 346, // player
    EntanglementCircle = 367, // player
    EntanglementTriangle = 368, // player
    EntanglementSquare = 369, // player
    EntanglementCross = 370, // player
    //_Gen_Icon_345 = 345, // player
    //_Gen_Icon_343 = 343, // player
    //_Gen_Icon_347 = 347, // player
    //_Gen_Icon_372 = 372, // player
    //_Gen_Icon_259 = 259, // player
    //_Gen_Icon_365 = 365, // player
    //_Gen_Icon_371 = 371, // player
}
