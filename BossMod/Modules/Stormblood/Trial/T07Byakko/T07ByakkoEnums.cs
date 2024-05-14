namespace BossMod.Stormblood.Trial.T07Byakko;

public enum OID : uint
{
    Boss = 0x20F7, // R4.300, x1
    Actor1e8ea9 = 0x1E8EA9, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ea87e = 0x1EA87E, // R0.500, x0 (spawn during fight), EventObj type
    AratamaForce = 0x20F9, // R0.700, x0 (spawn during fight)
    Hakutei1 = 0x20F8, // R4.750, x1
    Hakutei2 = 0x2162, // R4.750, x0 (spawn during fight)
    Helper = 0x18D6, // R0.500, x29, mixed types
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    MobAutoAttack = 870, // Hakutei1->player, no cast, single-target

    AnswerOnHigh = 10212, // Boss->self, no cast, single-target

    Aratama1 = 10793, // Helper->location, 2.5s cast, range 4 circle
    Aratama2 = 10818, // Helper->location, no cast, range 2 circle
    Aratama3 = 10824, // AratamaForce->self, no cast, range 2 circle

    Bombogenesis1 = 10215, // Boss->self, no cast, single-target
    Bombogenesis2 = 10811, // Helper->self, no cast, range 6 circle

    Clutch = 10209, // Boss->player, no cast, single-target
    DanceOfTheIncomplete = 9681, // Boss->self, no cast, single-target
    DistantClap = 10800, // Boss->self, 5.0s cast, range ?-25 donut
    FellSwoop = 10829, // Helper->self, no cast, range 100 circle

    FireAndLightning1 = 10796, // Boss->self, 4.0s cast, range 50+R width 20 rect
    FireAndLightning2 = 10801, // Hakutei1->self, 4.0s cast, range 50+R width 20 rect

    HeavenlyStrike = 10797, // Boss->player, 4.0s cast, range 3 circle

    HighestStakes1 = 10210, // Boss->location, 5.0s cast, single-target
    HighestStakes2 = 10806, // Helper->location, no cast, range 6 circle

    HundredfoldHavoc1 = 10808, // Helper->self, 5.0s cast, range 5 circle
    HundredfoldHavoc2 = 10809, // Helper->self, no cast, range 5 circle

    ImperialGuard = 10819, // Hakutei2->self, 3.0s cast, range 40+R width 5 rect

    StateOfShock1 = 10070, // Boss->player, no cast, single-target
    StateOfShock2 = 10208, // Boss->player, 4.0s cast, single-target

    SteelClaw = 10802, // Hakutei1->self, no cast, range 13+R ?-degree cone
    StormPulse = 10799, // Boss->self, 4.0s cast, range 100 circle

    SweepTheLeg1 = 10798, // Boss->self, 4.0s cast, range 24+R 270-degree cone
    SweepTheLeg2 = 10821, // Boss->self, no cast, single-target
    SweepTheLeg3 = 10822, // Helper->self, 5.1s cast, range ?-25 donut

    TheRoarOfThunder = 10827, // Hakutei1->self, 20.0s cast, range 100 circle
    TheVoiceOfThunder = 10825, // Hakutei1->self, no cast, single-target

    UnknownAbility1 = 10071, // Boss->location, no cast, single-target
    UnknownAbility2 = 9679, // Hakutei1->self, no cast, single-target
    UnknownAbility3 = 9822, // Hakutei1->location, no cast, single-target

    UnknownCast = 10794, // Boss->self, no cast, single-target

    UnknownSpell1 = 10222, // Boss->self, no cast, single-target
    UnknownSpell2 = 10817, // Helper->location, no cast, single-target

    UnrelentingAnguish = 10221, // Boss->self, 3.0s cast, single-target
    WhiteHerald = 10828, // Hakutei1->self, no cast, range 50 circle
}

public enum SID : uint
{
    Stun1 = 201, // Boss->player, extra=0x0
    Fetters = 1477, // Boss->player, extra=0xC
    UnrelentingAnguish = 1480, // Boss->Helper/Boss, extra=0x0
    VulnerabilityUp = 202, // AratamaForce->player, extra=0x1/0x2
    DownForTheCount = 783, // Hakutei1->player, extra=0xEC7
    Falling = 1479, // none->player, extra=0x0
    Stun2 = 1513, // none->player, extra=0x1802
    Paralysis = 482, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Icon_62 = 62, // Helper
    Icon_87 = 87, // player
    Icon_101 = 101, // player
}