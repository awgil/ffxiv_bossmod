namespace BossMod.RealmReborn.Alliance.A12Thanatos;
public enum OID : uint
{
    Boss = 0x92E, // R3.000, x?
    ThanatosHelper = 0x92F, // R0.500, x?
    MagicPot = 0x931, // R0.800, x?
    Nemesis = 0x930, // R2.000, x?
    Sandman = 0x983, // R1.800, x?
}

public enum AID : uint
{
    AutoAttack = 1461, // Boss/Nemesis->player, no cast, single-target
    BlightedGloom = 759, // Boss->self, no cast, range 10+R circle
    BlackCloud = 758, // Boss->location, no cast, range 6 circle
    Cloudscourge = 760, // ThanatosHelper->location, 3.0s cast, range 6 circle
    CrepusculeBlade = 762, // Boss->self, 3.0s cast, range 8+R ?-degree cone
    Knout = 763, // Boss->self, no cast, single-target

    VoidFireII = 1829, // Nemesis->location, 3.0s cast, range 5 circle
    Attack = 1459, // Sandman->MagicPot, no cast, single-target
    TerrorTouch = 1661, // Sandman->MagicPot, no cast, single-target
    AstralLight = 1759, // MagicPot->self, 3.0s cast, range 6+R circle
}

public enum SID : uint
{
    AstralRealignment = 398, // none->player, extra=0x0
    Leaden = 67, // none->MagicPot, extra=0x50

}

public enum TetherID : uint
{
    Tether_17 = 17, // Sandman->MagicPot
    Tether_15 = 15, // MagicPot->Boss
    Tether_4 = 4, // player->MagicPot
}
