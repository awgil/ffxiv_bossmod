namespace BossMod.RealmReborn.Alliance.A14Phlegethon;
public enum OID : uint
{
    Boss = 0x938, // R5.000, x?
    PhlegethonHelper = 0x939, // R0.500, x?
    IronClaws = 0x93A, // R2.000, x?
    IronGiant = 0x984, // R3.000, x?
}

public enum AID : uint
{
    AutoAttack1 = 1461, // Boss/IronGiant->player, no cast, single-target

    GreatDivide = 1725, // Boss->self, no cast, range 5+R width 8 rect

    VacuumSlash1 = 1733, // Boss->self, 3.0s cast, single-target
    VacuumSlash2 = 1736, // PhlegethonHelper->self, 3.0s cast, range 80+R ?-degree cone
    MoonfallSlash = 1780, // Boss->self, 3.0s cast, range 10+R 120-degree cone

    AbyssalSlash1 = 1734, // Boss->self, 3.0s cast, single-target //half donut shaped aoes
    AbyssalSlash2 = 1737, // PhlegethonHelper->self, 3.0s cast, range 7+R ?-degree cone
    AbyssalSlash3 = 1739, // PhlegethonHelper->self, 3.0s cast, range 17+R ?-degree cone
    AbyssalSlash4 = 1738, // PhlegethonHelper->self, 3.0s cast, range 12+R ?-degree cone

    MegiddoFlame1 = 1735, // Boss->self, 3.0s cast, single-target
    MegiddoFlame2 = 1741, // PhlegethonHelper->location, 3.0s cast, range 3 circle
    MegiddoFlame3 = 1742, // PhlegethonHelper->location, 3.0s cast, range 4 circle
    MegiddoFlame4 = 1743, // PhlegethonHelper->location, 3.0s cast, range 5 circle
    MegiddoFlame5 = 1744, // PhlegethonHelper->location, 3.0s cast, range 6 circle

    AncientFlare1 = 1730, // Boss->self, 7.0s cast, single-target
    AncientFlare2 = 1748, // Boss->self, no cast, ???

    Quake = 1745, // Boss->self, no cast, ???
    QuakeIII = 1732, // Boss->self, 1.0s cast, ???

    GrandSword = 1785, // IronGiant->self, no cast, range 12+R 120-degree cone
    DeathGrip = 610, // IronClaws->player, no cast, single-target
    AutoAttack2 = 1459, // IronClaws->player, no cast, single-target
}

public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    Seized = 3697, // IronClaws->player, extra=0x0
    Burns = 284, // none->player, extra=0x0
}
