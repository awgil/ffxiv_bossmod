namespace BossMod.RealmReborn.Alliance.A21Scylla;
public enum OID : uint
{
    Boss = 0xBC1, // R3.750, x?
    ShudderingSoul = 0xBC6, // R1.000, x?
    StaffOfEldering = 0xBC2, // R1.000, x?
    ShiveringSoul = 0xBC4, // R1.000, x?
    SmolderingSoul = 0xBC5, // R1.000, x?
    Gomory = 0xBCD, // R0.900, x?
    Acheron = 0xBCE, // R2.500, x?
    Actor1b2 = 0x1B2, // R0.500, x?, 523 type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Topple = 2321, // Boss->self, 3.0s cast, range 3+R circle
    Unholy = 2319, // Boss->location, no cast, range 81 circle
    Firewalker = 2329, // Boss->self, no cast, range 5+R ?-degree cone
    Shudder = 2324, // ShudderingSoul->self, no cast, range 81 circle
    SearingChain = 2330, // StaffOfEldering->self, 3.0s cast, range 60+R width 4 rect
    InfiniteAnguish = 2331, // StaffOfEldering->self, 3.0s cast, range 12 circle
    Shiver = 2323, // ShiveringSoul->self, no cast, ???
    Smolder = 2322, // SmolderingSoul->self, no cast, range 2+R circle
    AncientFlare = 2317, // Boss->self, 10.0s cast, range 81 circle
}

public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    Heavy = 14, // none->ShiveringSoul, extra=0x32
    DeepFreeze = 487, // ShiveringSoul->player, extra=0x1
    FireResistanceUp = 520, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_24 = 24, // player
    Icon_26 = 26, // player
    Icon_25 = 25, // player
}

public enum TetherID : uint
{
    AetherTether = 6, // ShudderingSoul->player
    IceTether = 8, // ShiveringSoul->player
    FireTether = 5, // SmolderingSoul->player
}
