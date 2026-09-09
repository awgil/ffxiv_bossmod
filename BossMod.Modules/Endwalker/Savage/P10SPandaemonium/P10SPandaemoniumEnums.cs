namespace BossMod.Endwalker.Savage.P10SPandaemonium;

public enum OID : uint
{
    Boss = 0x3F1D, // R35.000, x1
    PandaemoniacPillarCircle = 0x3F1E, // R1.000, x8
    PandaemoniacPillarDonut = 0x3F1F, // R1.000, x8
    PandaemoniacPillarTurret = 0x3F20, // R1.000, x8
    Pillar = 0x3F21, // R0.200, x6
    Helper = 0x233C, // R0.500, x23
    ArcaneSphere = 0x3F22, // R0.700, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 33444, // Boss->player, no cast, single-target
    Ultima = 33445, // Boss->location, 5.0s cast, range 100 circle, raidwide with bleed
    SoulGrasp = 33439, // Boss->self, 5.0s cast, single-target, visual (shared multihit tankbuster)
    SoulGraspAOE = 33440, // Helper->players, no cast, range 4 circle
    DividingWings = 33431, // Boss->self, 3.0s cast, single-target, visual (pillars appear)
    DividingWingsAOE = 33432, // Helper->self, no cast, range 60 120-degree cone
    SteelWeb = 34595, // Boss->self, 3.0s cast, single-target, visual (tethers/shared webs)
    SteelWebAOE = 33407, // Helper->players, no cast, range 6 circle
    PandaemonsHoly = 33446, // Boss->self, 4.0s cast, range 36 circle
    CirclesOfPandaemonium = 33447, // Boss->self, 4.0s cast, range 12-40 donut (origin is off center)
    WickedStep = 33433, // Boss->self, 6.0s cast, single-target, visual (tankbuster towers)
    WickedStepAOE1 = 33434, // Helper->self, 7.1s cast, range 4 circle, knockback 36 (first tower)
    WickedStepAOE2 = 33435, // Helper->self, 9.5s cast, range 4 circle, knockback 36 (second tower)
    WickedStepUnmitigatedExplosion = 33436, // Helper->self, 1.0s cast, range 100 circle (tower failure)
    EntanglingWeb = 34594, // Boss->self, 3.0s cast, single-target, visual (building a bridge)
    EntanglingWebAOE = 33406, // Helper->location, 3.0s cast, range 5 circle
    PandaemoniacPillars = 33408, // Boss->self, 5.0s cast, single-target, visual (towers)
    BuryVisual = 33409, // PandaemoniacPillarCircle/PandaemoniacPillarDonut/PandaemoniacPillarTurret->self, no cast, single-target, visual (pillar appear)
    Bury = 33410, // Helper->self, 6.2s cast, range 2 circle (tower soak)
    BuryUnmitigatedExplosion = 33411, // Helper->self, no cast, range 100 circle (tower failure)
    Imprisonment = 33412, // PandaemoniacPillarCircle->self, no cast, single-target, visual (circle marker)
    ImprisonmentAOE = 33413, // Helper->self, 3.5s cast, range 4 circle
    Cannonspawn = 33414, // PandaemoniacPillarDonut->self, no cast, single-target, visual (donut marker)
    CannonspawnAOE = 33415, // Helper->self, 3.5s cast, range 3-8 donut
    PealOfDamnation = 34736, // PandaemoniacPillarTurret->self, 3.0s cast, range 50 width 7 rect
    Silkspit = 33404, // Boss->self, 3.0s cast, single-target, visual
    SilkspitAOE = 33405, // Helper->player, no cast, range 7 circle
    DaemoniacBonds = 33441, // Boss->self, 3.0s cast, single-target, visual (stack/spread debuffs)
    DaemoniacBondsAOE = 33442, // Helper->players, no cast, range 6 circle spread
    DuodaemoniacBonds = 33443, // Helper->players, no cast, range 4 circle 2-man stack
    TetradaemoniacBonds = 34734, // Helper->players, no cast, range 4 circle 4-man stack
    PandaemoniacMeltdown = 33437, // Boss->self, 5.0s cast, single-target, visual (line stack + 2 spreads)
    PandaemoniacMeltdownTargetSelect = 26708, // Helper->player, no cast, single-target, select target for line stack
    PandaemoniacMeltdownSpread = 34737, // Helper->self, no cast, range 50 width 4 rect
    PandaemoniacMeltdownStack = 33438, // Helper->self, no cast, range 50 width 6 rect
    Touchdown = 33421, // Boss->self, 8.0s cast, single-target, visual
    TouchdownAOE = 33422, // Helper->self, 9.0s cast, range 4 circle (+16 due to area-of-influence-up)
    PandaemoniacTurrets = 34735, // Boss->self, 5.0s cast, single-target, visual (towers)
    PealOfCondemnation = 33416, // PandaemoniacPillarTurret->self, no cast, range 50 width 5 rect, knockback 17
    PandaemoniacRayL = 33417, // Boss->self, 5.0s cast, single-target, visual (left half cleave)
    PandaemoniacRayR = 33419, // Boss->self, 5.0s cast, single-target, visual (right half cleave)
    PandaemoniacRayAOE = 33418, // Helper->self, 5.2s cast, range 30 width 50 rect
    JadePassage = 33420, // ArcaneSphere->self, 1.0s cast, range 80 width 2 rect
    PandaemoniacWeb = 34596, // Boss->self, 3.0s cast, single-target, visual (web wall)
    HarrowingHell = 33423, // Boss->self, 5.0s cast, single-target, visual (multihit wild charge)
    HarrowingHellAOE1 = 33424, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE2 = 33425, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE3 = 33426, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE4 = 33427, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE5 = 34489, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE6 = 34490, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE7 = 34491, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE8 = 34492, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellKnockback = 33428, // Helper->self, 3.9s cast, range 60 width 60 rect
    PartedPlumes = 33429, // Boss->self, 3.0s cast, single-target, visual
    PartedPlumesAOE = 33430, // Helper->self, 4.0s cast, range 50 20-degree cone
}

public enum SID : uint
{
    DaemoniacBonds = 3550, // none->player, extra=0x0
    DuodaemoniacBonds = 3551, // none->player, extra=0x0
    TetradaemoniacBonds = 3696, // none->player, extra=0x0
    DarkResistanceDown = 3323, // PandaemoniacPillarTurret->player, extra=0x0
}

public enum IconID : uint
{
    SoulGrasp = 467, // player
    SteelWeb = 428, // player
    EntanglingWeb = 430, // player
    Silkspit = 427, // player
    PandaemoniacMeltdownSpread = 23, // player
    Order1 = 336, // PandaemoniacPillarTurret
    Order2 = 337, // PandaemoniacPillarTurret
    Order3 = 338, // PandaemoniacPillarTurret
    Order4 = 339, // PandaemoniacPillarTurret
}

public enum TetherID : uint
{
    DividingWings = 242, // Helper->player
    Web = 226, // player/Pillar->player/Pillar
    WebFail = 227, // player->player
}
