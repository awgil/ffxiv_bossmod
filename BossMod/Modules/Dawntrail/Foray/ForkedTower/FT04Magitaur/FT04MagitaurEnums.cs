#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

public enum OID : uint
{
    Boss = 0x46E3, // R7.000, x4
    Helper = 0x233C, // R0.500, x30, Helper type
    _Gen_SagesStaff = 0x46E5, // R1.000, x6
    _Gen_Trap = 0x4767, // R1.000, x1
    _Gen_AssassinsDagger = 0x46E6, // R1.000, x3
    _Gen_UniversalEmpowermentConduit = 0x46F9, // R4.000, x0 (spawn during fight)
    _Gen_AxeEmpowermentConduit = 0x46EC, // R4.000, x0 (spawn during fight)
    _Gen_LanceEmpowermentConduit = 0x46EB, // R4.000, x0 (spawn during fight)
    LuminousLance = 0x46E4,
}

public enum AID : uint
{
    _AutoAttack_ = 41765, // Boss->player, no cast, single-target
    UnsealedAuraCast = 41572, // Boss->self, 4.2+0.8s cast, range 100 circle
    UnsealedAura = 39911, // Helper->self, 5.0s cast, ???
    UnsealAxe = 41537, // Boss->self, 5.0s cast, single-target
    UnsealLance = 41538, // Boss->self, 5.0s cast, single-target
    _Ability_Attack = 41539, // Boss->self, no cast, single-target
    _Ability_Attack1 = 41540, // Helper->player, no cast, single-target
    _Ability_ = 41399, // Boss->location, no cast, single-target
    _Ability_AssassinsDagger = 41568, // Boss->self, 4.7+0.3s cast, single-target
    _Ability_AssassinsDagger1 = 41569, // 46E6->location, 5.0s cast, width 6 rect charge
    _Ability_AssassinsDagger2 = 41570, // 46E6->location, no cast, width 6 rect charge
    CriticalAxeblowCast = 41543, // Boss->self, 5.0+1.1s cast, single-target
    CriticalAxeblowFloor = 41544, // Helper->self, no cast, range 100 circle
    CriticalAxeblowCircle = 41545, // Helper->self, no cast, range 20 circle
    _Ability_AssassinsDagger3 = 41571, // 46E6->location, no cast, width 6 rect charge
    _Ability_1 = 41548, // Boss->location, no cast, single-target
    CriticalLanceblowCast = 41547, // Boss->self, 5.0+1.4s cast, single-target
    CriticalLanceblowPlatform = 41549, // Helper->self, no cast, range 20 width 20 rect
    CriticalLanceblowDonut = 41550, // Helper->self, no cast, range ?-32 donut
    _Ability_ForkedFury = 41573, // Boss->self, 4.5+0.5s cast, single-target
    _Ability_ForkedFury1 = 41574, // Helper->player, 0.5s cast, single-target
    _Ability_AuraBurst = 41562, // Boss->self, 19.0+1.0s cast, single-target
    _Ability_AuraBurst1 = 39909, // Helper->self, 20.0s cast, ???
    _Ability_ArcaneReaction = 41565, // Helper->self, no cast, range 55 width 6 rect
    _Ability_ArcaneRecoil = 41564, // Helper->player, no cast, single-target
    _Ability_SagesStaff = 41566, // Boss->self, 5.0s cast, single-target
    _Ability_ManaExpulsion = 41567, // _Gen_SagesStaff->self, no cast, range 40 width 4 rect
    _Ability_RuneAxe = 41551, // Boss->self, 5.0s cast, single-target
    _Ability_RuinousRune = 41553, // Helper->player, no cast, range 11 circle
    _Ability_Axeglow = 41556, // Helper->self, no cast, range 100 circle
    _Ability_RuinousRune1 = 41552, // Helper->player, no cast, range 5 circle
    _Ability_Axeglow1 = 41555, // Helper->self, no cast, range 20 width 20 rect
    _Ability_CarvingRune = 41554, // Helper->player, no cast, single-target
    _Ability_Attack2 = 41541, // Boss->self, no cast, single-target
    _Ability_Attack3 = 41542, // Helper->player, no cast, single-target
    _Ability_HolyLance = 41557, // Boss->self, 5.0s cast, single-target
    _Ability_2 = 41558, // 46E4->self, no cast, single-target
    _Ability_Lancelight = 41561, // Helper->self, no cast, range 100 circle
    _Ability_Lancelight1 = 41560, // Helper->self, no cast, range 20 width 20 rect
    _Ability_HolyIV = 41559, // Helper->players, no cast, range 6 circle
    _Ability_Holy = 41563, // Boss->self, 19.0+1.0s cast, single-target
    _Ability_Holy1 = 39910, // Helper->self, 20.0s cast, ???
}

public enum SID : uint
{
    _Gen_Unsealed = 4339, // Boss->Boss, extra=0x353 (axe)/0x354 (lance)
    _Gen_PreyLesserAxebit = 4336, // none->player, extra=0x0
    _Gen_PreyGreaterAxebit = 4337, // none->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    _Gen_PreyLancepoint = 4338, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_loc08sp_08a_se_c = 573, // player->self
    _Gen_Icon_tag_ae5m_8s_0v = 345, // player->self
    _Gen_Icon_com_share2i = 100, // player->self
}
