namespace BossMod.RealmReborn.Alliance.A13KingBehemoth;
public enum OID : uint
{
    Boss = 0x932, // R8.700, x?
    KingBehemothHelper = 0x933, // R0.500, x?
    Charybdis = 0x98C, // R1.000, x0 (spawn during fight)
    IronGiant = 0x935, // R3.000, x?
    Puroboros = 0x934, // R2.400, x?
    Comet = 0x936, // R2.400, x?
}

public enum AID : uint
{
    AutoAttack = 1461, // Boss/IronGiant->player, no cast, single-target
    Comet = 768, // Comet->self, no cast, range 4 circle
    GrandSword = 1785, // IronGiant->self, no cast, range 12+R 120-degree cone
    MagitekRay = 1787, // IronGiant->location, no cast, range 6 circle
    EclipticMeteor = 1756, // Boss->self, 10.0s cast, ???
    CosmicShrapnel = 769, // Comet->self, no cast, range 8 circle
    SelfDestruct = 1789, // Puroboros->self, 3.0s cast, range 6+R circle
    CharybdisVisual = 1751, // Boss->location, 3.0s cast, range 6 circle
    CharybdisAOE = 1752, // Charybdis->location, no cast, range 6 circle

}

public enum SID : uint
{
    Electrocution = 288, // none->player, extra=0x0
    Burns = 267, // IronGiant->Comet/player, extra=0x0
}
