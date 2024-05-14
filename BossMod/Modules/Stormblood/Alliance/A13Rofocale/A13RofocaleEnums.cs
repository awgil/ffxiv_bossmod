namespace BossMod.Stormblood.Alliance.A13Rofocale;

public enum OID : uint
{
    Boss = 0x1FAF, // R5.850, x?
    Rofocale1 = 0x18D6, // R0.500, x?
    Rofocale2 = 0x2026, // R5.850, x?
    LinaMewrilah = 0x23E, // R0.500, x?
    Archaeodemon = 0x1FC3, // R1.950, x?
    Embrace = 0x20B7, // R1.000, x?
    DarkCircle1 = 0x1EA877, // R0.500, x?, EventObj type
    DarkCircle2 = 0x1EA879, // R0.500, x?, EventObj type
    DarkCircle3 = 0x1EA7F4, // R0.500, x?, EventObj type
    DarkCircle4 = 0x1EA873, // R0.500, x?, EventObj type
    DarkCircle5 = 0x1EA87A, // R0.500, x?, EventObj type
    DarkCircle6 = 0x1EA87B, // R0.500, x?, EventObj type
    DarkCircle7 = 0x1EA87D, // R0.500, x?, EventObj type
    DarkCircle8 = 0x1EA87C, // R0.500, x?, EventObj type
    DarkCircle9 = 0x1EA875, // R0.500, x?, EventObj type
    DarkCircle10 = 0x1EA878, // R0.500, x?, EventObj type
    DarkCircle11 = 0x1EA876, // R0.500, x?, EventObj type
    DarkCircle12 = 0x1EA874, // R0.500, x?, EventObj type
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    AutoAttackArchaeodemon = 872, // Archaeodemon->player, no cast, single-target

    CrushHelm1 = 9856, // Boss->self, 3.0s cast, single-target
    CrushHelm2 = 9857, // Rofocale1->player, no cast, single-target
    CrushHelm3 = 9858, // Boss->player, no cast, single-target
    Chariot = 9844, // Boss->players, 3.0s cast, width 10 rect charge

    CryOfVictory1 = 9845, // Boss->self, 3.0s cast, range 60 180-degree cone
    CryOfVictory2 = 10060, // Boss->self, 3.0s cast, range 60 180-degree cone

    CrushWeapon1 = 9859, // Boss->self, 3.5s cast, single-target
    CrushWeapon2 = 9860, // Rofocale1->location, 3.5s cast, range 6 circle

    Trample1 = 9846, // Boss->self, 4.5s cast, range 1 circle
    Trample2 = 9847, // Rofocale1->self, no cast, range 10 width 10 rect
    Trample3 = 9848, // Rofocale1->self, no cast, range ?-15 donut
    Trample4 = 9849, // Rofocale1->self, no cast, range ?-15 donut
    Trample5 = 9850, // Rofocale1->self, no cast, range ?-15 donut
    Trample6 = 9851, // Rofocale1->self, no cast, range 20 width 10 rect
    Trample7 = 9852, // Rofocale1->self, no cast, range ?-15 donut
    Trample8 = 9853, // Rofocale1->self, no cast, range ?-15 donut
    Trample9 = 9854, // Rofocale1->self, no cast, range ?-15 donut
    Trample10 = 9855, // Rofocale1->self, no cast, range 10 width 10 rect

    Maverick1 = 9865, // Rofocale2/Boss->location, 6.0s cast, range 60 width 16 rect
    Maverick2 = 10022, // Rofocale2/Boss->location, 6.0s cast, range 60 width 16 rect

    Unknown = 9868, // Rofocale1->location, no cast, range 10 width 10 rect
    Karma = 9842, // Archaeodemon->self, 3.0s cast, range 30 90-degree cone
    UnholyDarkness = 9843, // Archaeodemon->location, 3.5s cast, range 8 circle
    HeavenlySubjugation1 = 9866, // Boss->self, 3.5s cast, single-target
    HeavenlySubjugation2 = 9867, // Rofocale1->location, no cast, range 60 width 60 rect
    Embrace1 = 9861, // Boss->self, 2.0s cast, single-target
    Embrace2 = 9862, // Rofocale1->location, 3.0s cast, range 3 circle
    Embrace3 = 9863, // Rofocale1->self, no cast, range 7 circle
    PompAndCircumstance = 9869, // Boss->self, 5.0s cast, range 60 circle
    DarkGeas = 9864, // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    PhysicalVulnerabilityUp = 200, // Rofocale1->player, extra=0x1/0x3/0x4
    VulnerabilityUp = 714, // Boss/Rofocale2->player, extra=0x1/0x2/0x3
    DamageDown = 696, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Sprint = 481, // none->Rofocale1/Boss, extra=0xB1
    Stun = 149, // Rofocale1->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    Bind = 564, // Rofocale1->player, extra=0x0
    Bleeding = 320, // none->player, extra=0x0

}

public enum IconID : uint
{
    Icon_381 = 381, // player
    Icon_23 = 23, // player
}
