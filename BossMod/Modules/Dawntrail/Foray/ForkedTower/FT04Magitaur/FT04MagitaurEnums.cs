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
}

public enum AID : uint
{
    _AutoAttack_ = 41765, // Boss->player, no cast, single-target
    _Ability_UnsealedAura = 41572, // Boss->self, 4.2+0.8s cast, range 100 circle
    _Ability_UnsealedAura1 = 39911, // Helper->self, 5.0s cast, ???
    _Ability_Unseal = 41537, // Boss->self, 5.0s cast, single-target
    _Ability_Attack = 41539, // Boss->self, no cast, single-target
    _Ability_Attack1 = 41540, // Helper->player, no cast, single-target
    _Ability_ = 41399, // Boss->location, no cast, single-target
    _Ability_AssassinsDagger = 41568, // Boss->self, 4.7+0.3s cast, single-target
    _Ability_AssassinsDagger1 = 41569, // 46E6->location, 5.0s cast, width 6 rect charge
    _Ability_AssassinsDagger2 = 41570, // 46E6->location, no cast, width 6 rect charge
    _Ability_CriticalAxeblow = 41543, // Boss->self, 5.0+1.1s cast, single-target
    _Ability_CriticalAxeblow1 = 41544, // Helper->self, no cast, range 100 circle
    _Ability_CriticalAxeblow2 = 41545, // Helper->self, no cast, range 20 circle
    _Ability_AssassinsDagger3 = 41571, // 46E6->location, no cast, width 6 rect charge
    _Ability_1 = 41548, // Boss->location, no cast, single-target
    _Ability_CriticalLanceblow = 41547, // Boss->self, 5.0+1.4s cast, single-target
    _Ability_CriticalLanceblow1 = 41549, // Helper->self, no cast, range 20 width 20 rect
    _Ability_CriticalLanceblow2 = 41550, // Helper->self, no cast, range ?-32 donut
    _Ability_ForkedFury = 41573, // Boss->self, 4.5+0.5s cast, single-target
    _Ability_ForkedFury1 = 41574, // Helper->player, 0.5s cast, single-target
    _Ability_AuraBurst = 41562, // Boss->self, 19.0+1.0s cast, single-target
    _Ability_AuraBurst1 = 39909, // Helper->self, 20.0s cast, ???
}
