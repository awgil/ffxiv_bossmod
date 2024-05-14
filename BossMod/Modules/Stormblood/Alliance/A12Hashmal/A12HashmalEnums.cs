namespace BossMod.Stormblood.Alliance.A12Hashmal;

public enum OID : uint
{
    Boss = 0x1F83, // R6.000, x?
    CommandTower1 = 0x18D6, // R0.500, x?, mixed types
    LinaMewrilah = 0x23E, // R0.500, x?
    CommandTower2 = 0x1F86, // R7.300, x?
    SandSphere = 0x1F84, // R2.000, x?
    PennantstoneGolem = 0x1F85, // R1.100-3.300, x0 (spawn during fight)

}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    ControlTower1 = 9665, // Boss->self, 3.0s cast, range 60 circle
    ControlTower2 = 9666, // CommandTower1->self, 4.0s cast, range 6 circle
    CommandTower3 = 9660, // Boss->self, 3.0s cast, range 60 circle
    CommandTower4 = 9838, // CommandTower1->self, 5.5s cast, range 10 circle

    EarthHammer = 9675, // Boss->self, 5.0s cast, single-target
    EarthShaker = 9672, // CommandTower1->self, 5.0s cast, range 60+R 60-degree cone
    ExtremeEdge1 = 9680, // Boss->self, 6.0s cast, range 60+R width 36 rect
    ExtremeEdge2 = 9678, // Boss->self, 6.0s cast, range 60+R width 36 rect
    FallingRock = 9683, // CommandTower1->players, 5.0s cast, range 8 circle
    FallingBoulder = 9682, // CommandTower1->players, 5.0s cast, range 6 circle
    Hammerfall = 9676, // CommandTower1->self, 1.0s cast, range 40 circle
    Impact = 9671, // CommandTower1->self, no cast, range 60 circle
    JaggedEdge = 9677, // CommandTower1->location, 3.3s cast, range 8 circle
    Landwaster1 = 9669, // Boss->self, 5.0s cast, single-target
    Landwaster2 = 9670, // CommandTower1->self, 7.0s cast, range 60 circle
    QuakeIV = 9688, // Boss->self, 4.0s cast, range 60 circle
    RockCutter = 9687, // Boss->self/player, 4.0s cast, range 4+R ?-degree cone
    Sanction1 = 9667, // Boss->self, no cast, range 60 circle
    Sanction2 = 9987, // Boss->self, no cast, range 60 circle
    SubmissionTower = 9837, // Boss->self, 6.0s cast, range 60 circle
    Summon = 9684, // Boss->self, 3.0s cast, single-target
    ToDust = 9673, // SandSphere->self, 20.0s cast, range 60 circle
    Towerfall1 = 9668, // CommandTower1->self, no cast, range 40+R width 10 rect
    Towerfall2 = 9674, // CommandTower1->self, 9.0s cast, range 40+R width 16 rect

    GolemAutoAttack = 872, // PennantstoneGolem->player, no cast, single-target
    Demolish = 9686, // PennantstoneGolem->self, 4.0s cast, range 60 circle
    Might = 9685, // PennantstoneGolem->self, no cast, single-target
}

public enum SID : uint
{
    BrinkOfDeath = 44, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    FleshWound = 264, // Boss->player, extra=0x0
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ThatWhichBindsUs = 361, // none->player, extra=0x2
    ReducedRates = 364, // none->player, extra=0x1E

}

public enum IconID : uint
{
    Icon_62 = 62, // player
    Icon_120 = 120, // player
    Icon_230 = 230, // player
}