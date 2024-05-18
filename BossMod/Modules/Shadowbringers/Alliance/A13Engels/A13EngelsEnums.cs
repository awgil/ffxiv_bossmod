namespace BossMod.Shadowbringers.Alliance.A13Engels;

public enum OID : uint
{
    Boss = 0x2557, // R30.000, x?
    EngelsHelper1 = 0x233C, // R0.500, x?, 523 type
    EngelsHelper2 = 0x2CCF, // R0.500, x?
    MarxHelper1 = 0x2C0B, // R0.700, x?
    MarxHelper2 = 0x2C0C, // R0.700, x?
    MarxL = 0x2C09, // R18.500, x?
    MarxR = 0x2C0A, // R18.500, x?
    Ally2P1 = 0x2CC4, // R2.800, x?
    Ally2P2 = 0x2CCE, // R0.500, x?
    ReverseJointedGoliath = 0x2C07, // R3.600, x?
    SmallBiped = 0x2C08, // R0.960, x?
    Anogg = 0x2C83, // R0.500, x?
    Konogg = 0x2C82, // R0.500, x?
}

public enum AID : uint
{
    AutoAttack3 = 872, // SmallBiped->player, no cast, single-target
    AutoAttack_AutomaticCannon = 18264, // ReverseJointedGoliath->player, no cast, single-target
    AreaBombardment = 18256, // Boss->self, 3.0s cast, single-target
    ArmLaser = 18263, // ReverseJointedGoliath->self, 3.0s cast, range 30 90-degree cone
    AutoAttack1 = 19041, // Ally2P1->self, no cast, range 30 circle
    AutoAttack2 = 19043, // Ally2P1->ReverseJointedGoliath, no cast, single-target
    AutomaticCannon1 = 18257, // Boss->self, no cast, single-target
    AutomaticCannon2 = 18258, // EngelsHelper1->player, no cast, single-target
    CrushingWheel1 = 18248, // Boss->self, no cast, single-target
    CrushingWheel2 = 18251, // MarxL/MarxR->self, 12.0s cast, range 20 width 30 rect
    DemolishStructure1 = 18244, // Boss->self, no cast, single-target
    DemolishStructure2 = 18245, // EngelsHelper1->self, 9.3s cast, range 50 circle
    DiffuseLaser = 18261, // Boss->self, 4.0s cast, range 60 width 60 rect
    EnergyBarrage = 18236, // Boss->self, 3.0s cast, single-target
    EnergyBlast = 18238, // EngelsHelper1->self, no cast, range 75 circle
    EnergyDispersal = 18237, // EngelsHelper1->self, no cast, range 4 circle
    Frack = 18253, // EngelsHelper1->self, no cast, range 15 circle
    GuidedMissile1 = 18229, // Boss->self, 3.5s cast, single-target
    GuidedMissile2 = 18230, // EngelsHelper1->location, 5.0s cast, range 6 circle
    GuidedMissile3 = 18231, // EngelsHelper1->location, no cast, range 6 circle
    IncendiaryBombing1 = 18232, // Boss->self, 5.0s cast, single-target
    IncendiaryBombing2 = 18233, // EngelsHelper1->location, 4.0s cast, range 8 circle
    IncendiarySaturationBombing1 = 18254, // Boss->self, 3.0s cast, single-target
    IncendiarySaturationBombing2 = 18255, // EngelsHelper1->self, 6.0s cast, range 30 width 60 rect
    LaserSight1 = 18234, // Boss->self, 4.1s cast, range 100 width 20 rect
    LaserSight2 = 18235, // EngelsHelper1->self, no cast, range 100 width 20 rect
    MarxActivation = 18600, // Boss->self, 3.0s cast, single-target
    MarxCrush1 = 18246, // Boss->self, 6.0s cast, single-target
    MarxCrush2 = 18247, // EngelsHelper1->self, 6.0s cast, range 15 width 30 rect
    MarxSmash1 = 18214, // Boss->self, 6.0s cast, single-target
    MarxSmash2 = 18215, // Boss->self, 6.0s cast, single-target
    MarxSmash3 = 18216, // EngelsHelper1->self, 1.6s cast, range 60 width 30 rect
    MarxSmash4 = 18217, // EngelsHelper1->self, 1.6s cast, range 60 width 30 rect
    MarxSmash5 = 18218, // Boss->self, 6.0s cast, single-target
    MarxSmash6 = 18219, // EngelsHelper1->self, 1.5s cast, range 30 width 60 rect
    MarxSmash7 = 18220, // Boss->self, no cast, single-target
    MarxSmash8 = 18221, // EngelsHelper1->self, 0.5s cast, range 60 width 30 rect
    MarxSmash9 = 18222, // Boss->self, 6.0s cast, single-target
    MarxSmash10 = 18223, // EngelsHelper1->self, 1.5s cast, range 35 width 60 rect
    MarxSmash11 = 18224, // Boss->self, no cast, single-target
    MarxSmash12 = 18225, // EngelsHelper1->self, 0.5s cast, range 60 width 20 rect
    MarxSmash13 = 18226, // EngelsHelper1->self, 0.5s cast, range 60 width 20 rect
    MarxThrust1 = 18262, // MarxHelper2/MarxHelper1->self, 5.0s cast, single-target
    MarxThrust2 = 18684, // EngelsHelper1->self, 5.5s cast, range 30 width 20 rect
    PrecisionGuidedMissile1 = 18259, // Boss->self, 4.0s cast, single-target
    PrecisionGuidedMissile2 = 18260, // EngelsHelper1->players, 4.0s cast, range 6 circle
    RadiateHeat = 18252, // EngelsHelper1->self, no cast, range 50 circle
    SaturationBombingManeuver1 = 18896, // Ally2P1->self, 4.0s cast, range 75 circle
    SaturationBombingManeuver2 = 19042, // Ally2P2->self, 5.0s cast, range 75 circle
    SaturationBombingManeuver3 = 19044, // Ally2P2->self, 5.0s cast, range 75 circle
    SurfaceMissile1 = 18227, // Boss->self, 3.5s cast, single-target
    SurfaceMissile2 = 18228, // EngelsHelper1->location, 4.0s cast, range 6 circle
    UnknownAbility1 = 19045, // Ally2P1->location, no cast, single-target
    UnknownAbility2 = 18239, // Boss->self, no cast, single-target
    UnknownAbility3 = 18243, // Boss->self, no cast, single-target
    WideAngleDiffuseLaser1 = 18240, // Boss->self, no cast, single-target
    WideAngleDiffuseLaser2 = 18241, // EngelsHelper1->self, no cast, range 60 width 60 rect
}

public enum SID : uint
{
    Burns = 2194, // Boss->player, extra=0x0
    Hover = 1515, // none->Ally2P1, extra=0xFA
}

public enum IconID : uint
{
    Icon198 = 198, // player
    Icon23 = 23, // player
    Nox = 197, // player chasing AOE icon
}
