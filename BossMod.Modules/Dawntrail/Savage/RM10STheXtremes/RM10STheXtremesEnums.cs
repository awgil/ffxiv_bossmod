namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

public enum OID : uint
{
    RedHot = 0x4B57, // R4.000, x1
    DeepBlue = 0x4B58, // R4.000, x1
    Helper = 0x233C, // R0.500, x20-24, Helper type
    TetherHelper = 0x4AE7, // R1.000, x4
    TheXtremes = 0x4BDF, // R1.000, x2, helper for raidwides
    SickSwell = 0x4B5A, // R1.000, x1
    XtremeAether = 0x4B59, // R1.500, x28
    WateryGraveAttract = 0x4AC0, // R1.000, x4, helper responsible for pulling tower players into bubble (i think)
    WateryGrave = 0x4B5C, // R4.000, x0 (spawn during fight), bubble

    FlameFloater = 0x1EBF30, // fire puddle, range 60 width 8 rect
    FlamePuddle5 = 0x1EBF31, // fire puddle, range 5 circle
    FlamePuddle6 = 0x1EBF32, // fire puddle, range 6 circle
    CutbackBlaze = 0x1EBF33, // fire puddle, range 60 330-degree cone
    FlameCone = 0x1EBFCE, // fire puddle, range 60 45-degree cone
}

public enum AID : uint
{
    AutoAttackRed = 48639, // RedHot->player, no cast, single-target
    AutoAttackBlue = 48640, // DeepBlue->player, no cast, single-target
    JumpRed = 46456, // RedHot->location, no cast, single-target
    JumpBlue = 46457, // DeepBlue->location, no cast, single-target
    Unk1 = 46460, // RedHot->self, no cast, single-target
    Unk2 = 46461, // DeepBlue->self, no cast, single-target
    DiversDareRed = 46520, // RedHot->self, 5.0s cast, range 60 circle
    DiversDareBlue = 46521, // DeepBlue->self, 5.0s cast, range 60 circle
    EpicBrotherhoodRed = 46458, // RedHot->DeepBlue, no cast, single-target
    EpicBrotherhoodBlue = 46459, // DeepBlue->RedHot, no cast, single-target

    HotImpact1 = 46518, // RedHot->players, 5.0s cast, range 6 circle
    HotImpact2 = 46464, // RedHot->players, 5.0s cast, range 6 circle
    FlameFloaterCast = 46522, // RedHot->self, 7.0s cast, single-target
    FlameFloater1 = 46523, // RedHot->location, no cast, range 60 width 8 rect
    FlameFloater2 = 46524, // RedHot->location, no cast, range 60 width 8 rect
    FlameFloater3 = 46525, // RedHot->location, no cast, range 60 width 8 rect
    FlameFloater4 = 46526, // RedHot->location, no cast, range 60 width 8 rect

    AlleyOopInfernoCast = 46528, // RedHot->self, 4.3+0.7s cast, single-target
    AlleyOopInferno = 46529, // Helper->player, 5.0s cast, range 5 circle
    CutbackBlazeCast = 46537, // RedHot->self, 4.3+0.7s cast, single-target
    CutbackBlaze = 46538, // Helper->self, no cast, range 60 330-degree cone
    PyrotationCast = 46530, // RedHot->self, 4.3+0.7s cast, single-target
    PyrotationSpread = 46531, // Helper->players, no cast, range 6 circle

    SickSwellCast = 46539, // DeepBlue->self, 3.0s cast, single-target
    SickSwellAOE = 46540, // Helper->self, 7.0s cast, range 50 width 50 rect
    SickestTakeOffCast = 46541, // DeepBlue->self, 4.0s cast, single-target
    SickestTakeOffAOE = 46542, // Helper->self, 7.0s cast, range 50 width 15 rect
    AwesomeSplash1 = 46543, // Helper->players, no cast, range 5 circle
    AwesomeSlab1 = 46544, // Helper->players, no cast, range 6 circle
    AwesomeSplash2 = 46551, // Helper->player, no cast, range 5 circle
    AwesomeSlab2 = 46552, // Helper->players, no cast, range 6 circle

    AlleyOopDoubleDipCast = 46557, // DeepBlue->self, 5.0s cast, single-target
    AlleyOopDoubleDipFirst = 46558, // Helper->self, no cast, range 60 30-degree cone
    AlleyOopDoubleDipRepeat = 46559, // Helper->self, no cast, range 60 15-degree cone
    ReverseAlleyOopCast = 46560, // DeepBlue->self, 5.0s cast, single-target
    ReverseAlleyOopFirst = 46561, // Helper->self, no cast, range 60 ?-degree cone
    ReverseAlleyOopRepeat = 46562, // Helper->self, no cast, range 60 ?-degree cone

    DeepImpactCast = 46519, // DeepBlue->self, 4.9s cast, single-target
    DeepImpact = 44486, // DeepBlue->players, no cast, range 6 circle

    XtremeSpectacularCastRed = 46553, // RedHot->self, 4.0s cast, single-target
    XtremeSpectacularCastBlue = 46554, // DeepBlue->self, 4.0s cast, single-target
    XtremeSpectacularProximity = 46500, // TheXtremes->self, 7.4s cast, range 50 width 40 rect
    XtremeSpectacularRepeat = 46556, // TheXtremes->self, no cast, range 60 circle
    XtremeSpectacularFinal = 47050, // TheXtremes->self, no cast, range 60 circle

    InsaneAir1RedFirst = 47255, // RedHot->self, 5.9+1.5s cast, single-target
    InsaneAir1BlueFirst = 47256, // DeepBlue->self, 5.9+1.5s cast, single-target
    InsaneAir1RedRest = 47257, // RedHot->self, 3.9+1.5s cast, single-target
    InsaneAir1BlueRest = 47258, // DeepBlue->self, 3.9+1.5s cast, single-target

    BlastingSnapBoss = 46575, // RedHot->self, no cast, single-target
    PlungingSnapBoss = 46576, // DeepBlue->self, no cast, single-target
    BlastingSnapAOE = 46577, // Helper->self, no cast, range 60 45-degree cone
    PlungingSnapAOE = 46578, // Helper->self, no cast, range 60 45-degree cone
    ReEntryBlastBoss = 46579, // RedHot->self, no cast, single-target
    ReEntryPlungeBoss = 46580, // DeepBlue->self, no cast, single-target
    ReEntryBlastAOE = 46581, // Helper->self, no cast, range 60 ?-degree cone
    ReEntryPlungeAOE = 46582, // Helper->self, no cast, range 60 ?-degree cone
    VerticalBlastBoss = 46583, // RedHot->self, no cast, single-target
    VerticalPlungeBoss = 46584, // DeepBlue->self, no cast, single-target
    VerticalBlastAOE = 46585, // Helper->players, no cast, range 6 circle
    VerticalPlungeAOE = 46586, // Helper->player, no cast, range 6 circle

    Firesnaking = 45953, // RedHot->self, 5.0s cast, range 60 circle
    Watersnaking = 45954, // DeepBlue->self, 5.0s cast, range 60 circle
    SteamBurst = 46587, // XtremeAether->self, 3.0s cast, range 9 circle
    DeepVarialBoss = 47249, // DeepBlue->location, 5.3+1.0s cast, ???
    DeepVarialAOE = 46547, // Helper->self, 6.8s cast, range 60 120-degree cone
    HotAerialCast = 46532, // RedHot->self, 5.0s cast, single-target
    HotAerialJump = 47389, // RedHot->player, no cast, single-target
    HotAerialSpread1 = 47390, // Helper->player, no cast, range 6 circle
    HotAerialSpread2 = 47391, // Helper->player, no cast, range 6 circle
    HotAerialSpread3 = 47392, // Helper->player, no cast, range 6 circle
    HotAerialSpread4 = 47393, // Helper->players, no cast, range 6 circle

    DeepAerialBoss = 46563, // DeepBlue->location, 5.0s cast, single-target
    DeepAerialTower = 46564, // Helper->self, 6.0s cast, range 6 circle
    UnmitigatedExplosion = 46565, // Helper->self, no cast, range 60 circle, bubble tower if missed
    BubbleAbsorb = 46570, // WateryGrave->player, no cast, single-target
    XtremeWaveRedFirst = 46533, // RedHot->self, 4.9s cast, single-target
    XtremeWaveBlueFirst = 46534, // DeepBlue->self, 4.9s cast, single-target
    XtremeWaveRedRest = 46535, // RedHot->self, 4.9s cast, single-target
    XtremeWaveBlueRest = 46536, // DeepBlue->self, 4.9s cast, single-target
    XtremeWaveRedRect = 46545, // RedHot->location, no cast, range 60 width 8 rect
    XtremeWaveBlueRect = 46546, // DeepBlue->location, no cast, range 60 width 8 rect
    ScathingSteam = 44487, // WateryGrave->self, 1.0s cast, range 60 circle
    ImpactZone1 = 46571, // WateryGrave->self, no cast, range 60 circle, bubble enrage
    ImpactZone2 = 46572, // WateryGrave->self, no cast, range 60 circle, same thing, not sure why there are two (46573 is not another copy of this action)

    FlameFloaterSplitCast = 46548, // RedHot->self, 5.0s cast, single-target
    FlameFloaterSplitRect = 46527, // RedHot->location, no cast, range 60 width 8 rect
    FreakyPyrotationCast = 46486, // RedHot->self, 4.3+0.7s cast, single-target
    FreakyPyrotationStack = 46487, // Helper->player, no cast, range 6 circle

    XtremeFiresnaking = 46510, // RedHot->self, 5.0s cast, range 60 circle
    XtremeWatersnaking = 46511, // DeepBlue->self, 5.0s cast, range 60 circle

    InsaneAir2RedFirst = 46566, // RedHot->self, 6.9+1.5s cast, single-target
    InsaneAir2BlueFirst = 46567, // DeepBlue->self, 6.9+1.5s cast, single-target
    InsaneAir2RedRest = 46568, // RedHot->self, 6.9+1.5s cast, single-target
    InsaneAir2BlueRest = 46569, // DeepBlue->self, 6.9+1.5s cast, single-target
    Bailout1 = 46512, // Helper->players, 1.0s cast, range 15 circle
    Bailout2 = 46513, // Helper->players, 1.0s cast, range 15 circle

    OverTheFallsRed = 46588, // RedHot->self, 9.0s cast, range 60 circle, enrage
    OverTheFallsBlue = 46589, // DeepBlue->self, 9.0s cast, range 60 circle
}

public enum SID : uint
{
    FirstInLine = 3004, // none->player, extra=0x0
    SecondInLine = 3005, // none->player, extra=0x0
    ThirdInLine = 3006, // none->player, extra=0x0
    FourthInLine = 3451, // none->player, extra=0x0
    FireResistanceDownII = 2937, // RedHot/Helper->player, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper/RedHot/DeepBlue->player, extra=0x0
    Unk2056 = 2056, // none->DeepBlue, extra=0x3ED/0x3EE/0x3EF/0x3F0/0x435
    Firesnaking = 4974, // none->player, extra=0x0
    Watersnaking = 4975, // none->player, extra=0x0
    Burns1 = 3065, // none->player, extra=0x0
    Burns2 = 3066, // none->player, extra=0x0
    Stun = 2656, // none->player, extra=0x0
    WateryGrave = 4829, // none->player, extra=0x12C, bind + immunity to tether aoes during phase
    VulnerabilityDown = 929, // none->WateryGrave, extra=0x0, applied by blue tether hitting bubble
    XtremeFiresnaking = 4827, // none->player, extra=0x0
    XtremeWatersnaking = 4828, // none->player, extra=0x0
}

public enum IconID : uint
{
    TankbusterShare = 259, // player->self
    BubbleTetherBlue = 635, // player->self
    BubbleTetherRed = 636, // player->self
    FreakyPyrotation = 659, // player->self
    FireSpread = 660, // player->self
    Pyrotation = 666, // player->self
}

public enum TetherID : uint
{
    StretchedTether = 17, // TetherHelper->player
    UnstretchedTether = 57, // TetherHelper->player
    SickSwell = 372, // DeepBlue->TetherHelper
    FlameFloaterShort = 378, // TetherHelper/player->player
    FlameFloaterLong = 379, // player/TetherHelper->player/TetherHelper
}
