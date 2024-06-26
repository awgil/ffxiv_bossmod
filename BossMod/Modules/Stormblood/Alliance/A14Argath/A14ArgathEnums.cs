namespace BossMod.Stormblood.Alliance.A14Argath;

public enum OID : uint
{
    LinaMewrilah = 0x23E, // R0.500, x?
    ArgathThadalfus = 0x18D6, // R0.500, x?, 523 type
    Boss = 0x1F97, // R5.000, x?
    Heartless = 0x1F98, // R1.000, x?
    Shade = 0x1F99, // R0.500, x?
    ShardOfEmptiness = 0x1F9A, // R1.600, x?
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/Shade->player, no cast, single-target
    Coldblood1 = 9765, // Boss->self, no cast, single-target
    Coldblood2 = 9767, // Boss->self, 8.0s cast, range 66 circle

    CripplingBlow = 9773, // Boss->player, 3.0s cast, single-target
    CrushWeapon1 = 9768, // Boss->self, 3.0s cast, single-target
    CrushWeapon2 = 10003, // ArgathThadalfus->location, 3.5s cast, range 6 circle

    DarkUltima = 9760, // Boss->self, 2.5s cast, range 80+R circle
    EmptySoul = 9759, // Boss->self, 5.0s cast, single-target
    FireIV = 9774, // Boss->self, 4.0s cast, range 80+R circle
    GnawingDread = 9761, // Boss->self, 5.0s cast, single-target
    Heartless = 9778, // Heartless->self, 4.0s cast, range 60+R width 11 rect
    JudgmentBlade = 9769, // Boss->self, 3.0s cast, single-target

    MaskOfLies = 9753, // Boss->self, no cast, single-target
    MaskOfTruth = 9754, // Boss->self, no cast, single-target

    RailOfTheRat = 9764, // ArgathThadalfus->self, no cast, range 5 circle
    RoyalBlood = 9758, // Boss->self, 5.0s cast, single-target
    Soulfix = 9770, // Boss->self, 4.0s cast, range 12 circle
    TheWord = 9376, // ArgathThadalfus->self, no cast, range 100 circle
    Trepidation = 9762, // Boss->self, 3.0s cast, single-target
    UnholySacrifice = 9776, // Shade->self, 5.0s cast, range 60+R circle
    Unknown1 = 9995, // Boss->location, no cast, ???
    Unknown2 = 9777, // ShardOfEmptiness->Boss, no cast, single-target

    Unrelenting1 = 9771, // Boss->self, 2.5s cast, single-target
    Unrelenting2 = 9772, // ArgathThadalfus->self, 3.0s cast, range 60+R ?-degree cone

    Coldblood = 9766, // Boss->self, 12.0s cast, range ?-66 donut
}

public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
    Unnerved = 1426, // Boss->player, extra=0x1/0x2
    DivineCommandmentFlee = 1424, // none->player, extra=0x0
    DivineCommandmentTurn = 1425, // none->player, extra=0x0
    TheUpholder = 1446, // Boss->ArgathThadalfus/Boss, extra=0x0
    Bleeding = 642, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    TheDeceiver = 1445, // Boss->ArgathThadalfus/Boss, extra=0x0
    TemporaryMisdirection = 1422, // none->player, extra=0x2D0
    Craven = 1421, // Boss->player, extra=0x4E
    Transfiguration = 1459, // none->player, extra=0x4D
    BrinkOfDeath = 44, // none->player, extra=0x0

}

public enum IconID : uint
{
    Icon381 = 381, // player
    Icon123 = 123, // player
    Icon124 = 124, // player
    Icon125 = 125, // player
}

public enum TetherID : uint
{
    Tether17 = 17, // Shade->player
    Tether1 = 1, // Shade->Shade
}
