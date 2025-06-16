namespace BossMod.Data;

public enum PhantomID : uint
{
    None = 0,
    PhantomGuard = 41588,
    Pray = 41589,
    OccultHeal = 41590,
    Pledge = 41591,
    Rage = 41592,
    DeadlyBlow = 41594,
    PhantomKick = 41595,
    OccultCounter = 41596,
    Counterstance = 41597,
    OccultChakra = 41598,
    PhantomAim = 41599,
    OccultFeatherfoot = 41600,
    OccultFalcon = 41601,
    OccultUnicorn = 41602,
    Mineuchi = 41603,
    Shirahadori = 41604,
    Iainuki = 41605,
    Zeninage = 41606,
    MightyMarch = 41607,
    OffensiveAria = 41608,
    RomeosBallad = 41609,
    HerosRime = 41610,
    BattleBell = 41611,
    Weather = 41612,
    RingingRespite = 41619,
    Suspend = 41620,
    OccultSlowga = 41621,
    OccultDispel = 41622,
    OccultComet = 41623,
    OccultMageMasher = 41624,
    OccultQuick = 41625,
    PhantomFire = 41626,
    HolyCannon = 41627,
    DarkCannon = 41628,
    ShockCannon = 41629,
    SilverCannon = 41630,
    OccultPotion = 41631,
    OccultEther = 41633,
    Revive = 41634,
    OccultElixir = 41635,
    Predict = 41636,
    PhantomJudgment = 41637,
    Cleansing = 41638,
    Blessing = 41639,
    Starfall = 41640,
    Recuperation = 41641,
    PhantomDoom = 41642,
    PhantomRejuvenation = 41643,
    Invulnerability = 41644,
    Steal = 41645,
    OccultSprint = 41646,
    Vigilance = 41647,
    TrapDetection = 41648,
    PilferWeapon = 41649,
    OccultResuscitation = 41650,
    OccultTreasuresight = 41651
}

public enum PhantomSID : uint
{
    None = 0,

    PhantomGuard = 4231, // applied by Phantom Guard to self, 90% damage reduction at max level
    Pray = 4232, // applied by Pray to self, regen (base 5000 hp/s, affected by OC gear)
    EnduringFortitude = 4233, // applied by Pray to allies via knowledge crystal, 10% damage reduction
    Pledge = 4234, // applied by Pledge to target, single-hit invulnerability (2 stacks)

    Rage = 4235, // applied by Rage to self, self-stun and KB immunity
    PentupRage = 4236, // applied by Rage to self, increases potency of Deadly Blow

    PhantomKick = 4237, // applied by Phantom Kick to self, maximum of 3 stacks, 9% damage increase per stack (at max level)
    Counterstance = 4238, // applied by Counterstance to self, +100% parry rate
    Fleetfooted = 4239, // applied by Counterstance to allies via knowledge crystal, +10% movement speed

    PhantomAim = 4240, // applied by Phantom Aim to allies
    DeadlyPhantomAim = 4241, // applied by Phantom Aim to allies
    OccultUnicorn = 4243, // applied by Occult Unicorn to allies, shield

    Shirahadori = 4245, // applied by Shirahadori to self, single hit invulnerability to physical damage

    RomeosBallad = 4244, // applied by Romeo's Ballad to allies via knowledge crystal, phantom XP gain
    MightyMarch = 4246, // applied by Mighty March to allies, regen + increased max HP
    OffensiveAria = 4247, // applied by Offensive Aria to allies, damage increase
    ArcaneStop = 4248, // applied by Romeo's Ballad to targets, stun
    HerosRime = 4249, // applied by Hero's Rime to allies, 10% damage increase and mitigation
    HerosLament = 4250, // ????

    BattleBell = 4251, // applied by Battle Bell to target, target gains Battle's Clangor upon taking damage
    BattlesClangor = 4252, // 5% damage increase per stack, maximum 8 stacks
    BlessedRain = 4253, // applied by Blessed Rain to allies, shield
    MistyMirage = 4254, // applied by Misty Mirage to allies, +40% evasion
    HastyMirage = 4255, // applied by Hasty Mirage to allies, +20% movement speed
    AetherialGain = 4256, // applied by Aetherial Gain to allies, +10% damage
    RingingRespite = 4257, // applied by Ringing Respite to target, 3000hp heal when taking damage
    Suspend = 4258, // applied by Suspend to target, immunity to certain mechanics

    OccultMageMasher = 4259, // applied by Occult Mage Masher to target, -10% magic damage dealt
    OccultQuick = 4260, // applied by Occult Quick to target, +10% haste, -10 seconds to spellcast time
    OccultSwift = 4261, // applied by Occult Quick to target, +100% movement speed

    ResurrectionRestricted = 4262, // target can only be raised by Chemist
    ResurrectionDenied = 4263, // target cannot be raised at all
    SilverSickness = 4264, // applied by Silver Cannon to targets, -10% damage dealt, +5% damage taken
    Stun = 2, // applied by Occult Falcon to target

    PredictionOfJudgment = 4265, // applied by Predict to self
    PredictionOfCleansing = 4266, // applied by Predict to self
    PredictionOfBlessing = 4267, // applied by Predict to self
    PredictionOfStarfall = 4268, // applied by Predict to self
    FalsePrediction = 4269, // applied by Predict to self, 50000hp/s damage over time
    Blessing = 4270, // applied by Blessing to allies, heal + regen
    Recuperation = 4271, // applied by Recuperation to target, AOE cleanse triggered at expiration
    FortifiedRecuperation = 4272, // applied by Recuperation to target, AOE cleanse
    PhantomDoom = 4273, // applied by Phantom Doom to target, instant death
    PhantomRejuvenation = 4274, // applied by Phantom Rejuvenation to target, 90000hp AOE heal
    Invulnerability = 4275, // applied by Invulnerability to target, functions identically to holmgang

    OccultSprint = 4276, // applied by Occult Sprint to self, +100% movement speed
    Vigilance = 4277, // applied by Vigilance to self, grants Foreseen Offense when entering combat
    ForeseenOffense = 4278, // +60% crit
    WeaponPilfered = 4279, // applied by Pilfer Weapon to target, -10% attack power
}
