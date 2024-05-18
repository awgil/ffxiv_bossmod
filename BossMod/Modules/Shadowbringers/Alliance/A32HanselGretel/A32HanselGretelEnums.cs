namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

public enum OID : uint
{
    Ally2B = 0x31A8, // R0.512, x?
    Anogg = 0x31AA, // R0.500, x?
    HanselGretel = 0x18D6, // R0.500, x?
    Helper = 0x233C, // R0.500, x?, 523 type
    Gretel = 0x31A4, // R7.000, x?
    Hansel = 0x31A5, // R7.000, x?
    MagicBullet = 0x31A7, // R1.000, x?
    MagicalConfluence = 0x31A6, // R1.000, x?
}

public enum AID : uint
{
    BladeFlurry1 = 23788, // Ally2B->Hansel/Boss, no cast, single-target
    BladeFlurry2 = 23789, // Ally2B->Hansel/Boss, no cast, single-target
    BossAutoAttack = 870, // Hansel/Boss->player, no cast, single-target
    DancingBlade = 23790, // Ally2B->Hansel/Boss, no cast, width 2 rect charge
    BalancedEdge = 23791, // Ally2B->self, 2.0s cast, range 5 circle
    UpgradedLance1 = 23656, // Boss->self, 4.0s cast, single-target
    UpgradedShield1 = 23659, // Hansel->self, 4.0s cast, single-target
    Repay = 23664, // Gretel->player, no cast, single-target
    Wail1 = 23671, // Hansel->self, 5.0s cast, range 50 circle
    CripplingBlow1 = 23672, // Boss->player, 5.0s cast, single-target
    WhirlingAssault = 23792, // Ally2B->self, 2.0s cast, range 40 width 4 rect
    Wail2 = 23670, // Boss->self, 5.0s cast, range 50 circle
    CripplingBlow2 = 23673, // Hansel->player, 5.0s cast, single-target
    TandemAssaultBloodySweep1 = 25016, // Boss->self, 5.0s cast, single-target
    TandemAssaultBloodySweep2 = 25017, // Hansel->self, 5.0s cast, single-target
    UnknownWeaponskill1 = 23678, // Boss->location, no cast, single-target
    UnknownWeaponskill2 = 23679, // Hansel->location, no cast, single-target
    BloodySweep1 = 23636, // Boss->self, 8.0s cast, single-target
    BloodySweep2 = 23637, // Hansel->self, 8.0s cast, single-target
    BloodySweep3 = 23661, // Gretel->self, 8.6s cast, range 50 width 25 rect
    BloodySweep4 = 23660, // Gretel->self, 8.6s cast, range 50 width 25 rect
    UpgradedShield2 = 23657, // Boss->self, 4.0s cast, single-target
    UpgradedLance2 = 23658, // Hansel->self, 4.0s cast, single-target
    SeedOfMagicAlpha1 = 23674, // Boss->self, 5.0s cast, single-target
    RiotOfMagic1 = 23675, // Hansel->self, 5.0s cast, single-target
    SeedOfMagicAlpha2 = 23649, // Gretel->players, 5.0s cast, range 5 circle
    RiotOfMagic2 = 23651, // Gretel->players, 5.0s cast, range 5 circle
    TandemAssaultPassingLance1 = 25020, // Boss->self, 5.0s cast, single-target
    TandemAssaultPassingLance2 = 25021, // Hansel->self, 5.0s cast, single-target
    UnknownAbility = 18683, // Ally2B->location, no cast, single-target
    PassingLance1 = 23652, // Boss->self, 8.0s cast, single-target
    PassingLance2 = 23653, // Hansel->self, 8.0s cast, single-target
    PassingLance3 = 23654, // HanselGretel->self, 8.4s cast, range 50 width 24 rect
    Explosion = 23655, // MagicBullet->self, 1.0s cast, range 4 width 50 rect
    Tandem1 = 23640, // Boss->self, no cast, single-target
    Tandem2 = 23641, // Hansel->self, no cast, single-target
    Transference1 = 23794, // Gretel->location, no cast, single-target
    Transference2 = 23793, // Gretel->location, no cast, single-target
    BloodySweep5 = 23638, // Boss->self, 13.0s cast, single-target
    BloodySweep6 = 23639, // Hansel->self, 13.0s cast, single-target
    BloodySweep7 = 23663, // Gretel->self, 13.6s cast, range 50 width 25 rect
    BloodySweep8 = 23662, // Gretel->self, 13.6s cast, range 50 width 25 rect
    WanderingTrail1 = 23642, // Boss->self, 5.0s cast, single-target
    WanderingTrail2 = 23643, // Hansel->self, 5.0s cast, single-target
    TandemAssaultBreakthrough1 = 25018, // Boss->self, 5.0s cast, single-target
    TandemAssaultBreakthrough2 = 25019, // Hansel->self, 5.0s cast, single-target
    Impact = 23644, // Gretel->self, no cast, range 3 circle
    Breakthrough1 = 21939, // HanselGretel->self, 9.0s cast, range 53 width 32 rect
    Breakthrough2 = 23645, // Boss->location, 9.0s cast, single-target
    Breakthrough3 = 23646, // Hansel->location, 9.0s cast, single-target
    UnevenFooting = 23647, // HanselGretel->self, 9.3s cast, range 50 circle
    HungryLance1 = 23665, // Boss->self, 5.0s cast, range 40 120-degree cone
    HungryLance2 = 23666, // Hansel->self, 5.0s cast, range 40 120-degree cone
    SeedOfMagicBeta1 = 23676, // Boss->self, 5.0s cast, single-target
    SeedOfMagicBeta2 = 23677, // Hansel->self, 5.0s cast, single-target
    Lamentation = 23667, // Gretel->self, 8.0s cast, range 50 circle
    SeedOfMagicBeta3 = 23669, // Gretel->location, 5.0s cast, range 5 circle
}

public enum SID : uint
{
    StrongOfSpear = 2537, // none->Boss/Hansel, extra=0x0
    UnknownStatus = 2056, // none->Boss/Hansel, extra=0x122/0x125/0x124/0x123/0x11F/0x121
    StrongOfShield = 2538, // none->Hansel/Boss, extra=0x0
    DirectionalParry = 680, // none->Hansel/Boss, extra=0xE
    VulnerabilityUp = 1789, // Gretel/HanselGretel/MagicBullet->player, extra=0x1/0x2
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    StrongerTogether = 2539, // none->Boss/Hansel, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Icon96 = 96, // player
    Icon62 = 62, // player
}

public enum TetherID : uint
{
    Tether1 = 1, // Hansel->Boss
    Tether151 = 151, // Boss->Hansel
}
