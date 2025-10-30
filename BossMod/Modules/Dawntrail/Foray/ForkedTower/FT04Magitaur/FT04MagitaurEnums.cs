namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

public enum OID : uint
{
    Boss = 0x46E3, // R7.000, x4
    Helper = 0x233C, // R0.500, x30, Helper type
    LuminousLance = 0x46E4,
    SagesStaff = 0x46E5, // R1.000, x6
    AssassinsDagger = 0x46E6, // R1.000, x3
    UniversalEmpowermentConduit = 0x46F9, // R4.000, x0 (spawn during fight)
    LanceEmpowermentConduit = 0x46EB, // R4.000, x0 (spawn during fight)
    AxeEmpowermentConduit = 0x46EC, // R4.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 41765, // Boss->player, no cast, single-target
    UnsealedAuraCast = 41572, // Boss->self, 4.2+0.8s cast, range 100 circle
    UnsealedAura = 39911, // Helper->self, 5.0s cast, ???

    UnsealAxe = 41537, // Boss->self, 5.0s cast, single-target
    UnsealLance = 41538, // Boss->self, 5.0s cast, single-target
    AutoAttackAxeVisual = 41539, // Boss->self, no cast, single-target
    AutoAttackAxe = 41540, // Helper->player, no cast, single-target
    AutoAttackLanceVisual = 41541, // Boss->self, no cast, single-target
    AutoAttackLance = 41542, // Helper->player, no cast, single-target

    Jump1 = 41399, // Boss->location, no cast, single-target
    Jump2 = 41548, // Boss->location, no cast, single-target

    AssassinsDaggerBossCast = 41568, // Boss->self, 4.7+0.3s cast, single-target
    AssassinsDaggerCast = 41569, // 46E6->location, 5.0s cast, width 6 rect charge
    AssassinsDaggerRepeat = 41570, // 46E6->location, no cast, width 6 rect charge
    AssassinsDaggerRepeatFinal = 41571, // 46E6->location, no cast, width 6 rect charge

    CriticalAxeblowCast = 41543, // Boss->self, 5.0+1.1s cast, single-target
    CriticalAxeblowFloor = 41544, // Helper->self, no cast, range 100 circle
    CriticalAxeblowCircle = 41545, // Helper->self, no cast, range 20 circle
    CriticalLanceblowCast = 41547, // Boss->self, 5.0+1.4s cast, single-target
    CriticalLanceblowPlatform = 41549, // Helper->self, no cast, range 20 width 20 rect
    CriticalLanceblowDonut = 41550, // Helper->self, no cast, range ?-32 donut

    ForkedFuryCast = 41573, // Boss->self, 4.5+0.5s cast, single-target
    ForkedFury = 41574, // Helper->player, 0.5s cast, single-target
    AuraBurstCast = 41562, // Boss->self, 19.0+1.0s cast, single-target
    AuraBurst = 39909, // Helper->self, 20.0s cast, ???
    ArcaneReaction = 41565, // Helper->self, no cast, range 55 width 6 rect, triggered by purple canister to nearest target on death, applies Hysteria
    ArcaneRecoil = 41564, // Helper->player, no cast, single-target

    SagesStaff = 41566, // Boss->self, 5.0s cast, single-target
    ManaExpulsion = 41567, // _Gen_SagesStaff->self, no cast, range 40 width 4 rect, line stack on nearest target

    RuneAxe = 41551, // Boss->self, 5.0s cast, single-target
    RuinousRuneSmall = 41552, // Helper->player, no cast, range 5 circle
    RuinousRuneLarge = 41553, // Helper->player, no cast, range 11 circle
    CarvingRune = 41554, // Helper->player, no cast, single-target, 4 sets of 4 hits on random non-debuffed players during Rune Axe
    AxeglowPlatform = 41555, // Helper->self, no cast, range 20 width 20 rect
    AxeglowFloor = 41556, // Helper->self, no cast, range 100 circle

    HolyLance = 41557, // Boss->self, 5.0s cast, single-target
    LanceAppear = 41558, // 46E4->self, no cast, single-target
    HolyIV = 41559, // Helper->players, no cast, range 6 circle
    LancelightPlatform = 41560, // Helper->self, no cast, range 20 width 20 rect
    LancelightFloor = 41561, // Helper->self, no cast, range 100 circle

    HolyCast = 41563, // Boss->self, 19.0+1.0s cast, single-target
    HolyEnrage = 39910, // Helper->self, 20.0s cast, ???
}

public enum SID : uint
{
    Unsealed = 4339, // Boss->Boss, extra=0x353 (axe)/0x354 (lance)
    PreyLesserAxebit = 4336, // none->player, extra=0x0
    PreyGreaterAxebit = 4337, // none->player, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    PreyLancepoint = 4338, // none->player, extra=0x0
}

public enum IconID : uint
{
    RuinousRuneLarge = 573, // player->self
    RuinousRuneSmall = 345, // player->self
    HolyIV = 100, // player->self
}
