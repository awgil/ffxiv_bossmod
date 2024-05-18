namespace BossMod.Heavensward.Alliance.A33ProtoUltima;
public enum OID : uint
{
    Boss = 0x1967, // R6.000, x?
    ProtoUltimaHelper = 0x196A, // R0.500, x?, mixed types
    ProtoUltimaHelper2 = 0x196C, // R1.500, x?
    AllaganDreadnaught = 0x196B, // R3.000, x?
    ProtoBit = 0x1968, // R0.600, x?
    AetherCollector = 0x1969, // R1.000, x?
}

public enum AID : uint
{
    BossAutoAttack = 7573, // Boss->players, no cast, range 2 circle
    AllaganDreadnaughtAutoAttack = 872, // AllaganDreadnaught->player, no cast, single-target

    AetherBend = 7577, // Boss->self, no cast, range 8+R circle
    AetherialAbsorbption = 7590, // AetherCollector->Boss, no cast, single-target

    AetherialPool = 7587, // ProtoUltimaHelper->self, 3.5s cast, range 32 circle KB 40 TowardsOrigin

    AetherochemicalFlare = 7762, // Boss->self, 5.0s cast, range 40+R circle
    AetherochemicalLaser1 = 7574, // Boss->self, 3.3s cast, range 65+R width 8 rect
    AetherochemicalLaser2 = 7575, // Boss->self, 3.0s cast, range 65+R width 8 rect
    AetherochemicalLaser3 = 7576, // Boss->self, 3.0s cast, range 65+R width 8 rect

    CitadelBuster1 = 7579, // Boss->self, no cast, range 65+R width 10 rect
    CitadelBuster2 = 7595, // ProtoUltimaHelper->self, 2.5s cast, range 65+R width 10 rect

    DiffractiveLaser = 7594, // Boss->self, no cast, range 12+R ?-degree cone
    Eikonizer = 7597, // ProtoUltimaHelper->self, no cast, range 40 circle
    FlareStar = 7588, // ProtoUltimaHelper->self, 7.0s cast, range 31+R circle
    LightPillar = 7578, // Boss->self, no cast, single-target

    Rotoswipe = 4556, // AllaganDreadnaught->self, 3.0s cast, range 8+R 120-degree cone

    Supernova1 = 7581, // Boss->self, 6.0s cast, range 40+R circle
    Supernova2 = 7593, // Boss->self, 60.0s cast, range 40+R circle

    Touchdown = 7580, // Boss->self, no cast, range 6+R circle

    WreckingBall = 4557, // AllaganDreadnaught->location, 4.0s cast, range 8 circle

    UnknownWeaponskill1 = 7584, // Boss->self, no cast, single-target
    UnknownWeaponskill2 = 7585, // Boss->self, no cast, single-target
    UnknownWeaponskill3 = 7586, // Boss->self, no cast, single-target
    UnknownWeaponskill4 = 7596, // ProtoUltimaHelper2->self, no cast, range 8 circle
}

public enum IconID : uint
{
    Icon62 = 62, // ProtoUltimaHelper2
    Nox = 197, // player chasing aoe icon
}

public enum TetherID : uint
{
    Tether12 = 12, // AetherCollector->Boss
}
