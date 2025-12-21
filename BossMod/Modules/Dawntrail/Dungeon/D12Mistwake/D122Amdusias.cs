#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Dungeon.D12Mistwake.D122Amdusias;

public enum OID : uint
{
    Boss = 0x4A77, // R3.960, x1
    Helper = 0x233C, // R0.500, x15, Helper type
    _Gen_Thunderhead = 0x18D6, // R0.500, x2
    _Gen_PoisonCloud = 0x4A78, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 46839, // Boss->player, no cast, single-target
    _Weaponskill_ = 45357, // Boss->location, no cast, single-target
    _Weaponskill_StaticCharge = 45335, // Boss->self, no cast, single-target
    _Weaponskill_ThunderclapConcerto = 45341, // Boss->self, 5.0+0.5s cast, single-target
    _Weaponskill_ThunderclapConcerto1 = 45342, // Helper->self, 5.5s cast, range 40 300-degree cone
    _Weaponskill_StaticCharge1 = 45333, // Boss->self, no cast, single-target
    _Weaponskill_ThunderclapConcerto2 = 45336, // Boss->self, 5.0+0.5s cast, single-target
    _Weaponskill_ThunderclapConcerto3 = 45337, // Helper->self, 5.5s cast, range 40 300-degree cone
    _Weaponskill_BioII = 45344, // Boss->self, 5.0s cast, single-target
    _Weaponskill_BioII1 = 45345, // Helper->location, 5.0s cast, range 20 circle
    _Weaponskill_1 = 45348, // Helper->location, 1.5s cast, width 5 rect charge
    _Weaponskill_GallopingThunder = 45346, // Boss->location, 10.0s cast, single-target
    _Weaponskill_GallopingThunder1 = 45347, // Boss->location, no cast, width 5 rect charge
    _Weaponskill_Burst = 45349, // 4A78->self, 2.5s cast, range 9 circle
    _Weaponskill_ThunderIV = 45350, // Boss->self, 4.4+0.6s cast, single-target
    _Weaponskill_ThunderIV1 = 45351, // Helper->self, 5.0s cast, range 70 circle
    _Weaponskill_Shockbolt = 45355, // Boss->self, 4.4+0.6s cast, single-target
    _Weaponskill_Shockbolt1 = 45356, // Helper->player, 5.0s cast, single-target
    _Weaponskill_Thunder = 45343, // Helper->player, 5.0s cast, range 5 circle
    _Weaponskill_ThunderIII = 45352, // Boss->self, 4.4+0.6s cast, single-target
    _Weaponskill_ThunderIII1 = 45353, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_ThunderIII2 = 45354, // Helper->players, no cast, range 6 circle
}

class ThunderclapConcerto(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ThunderclapConcerto1, AID._Weaponskill_ThunderclapConcerto3], new AOEShapeCone(40, 150.Degrees()));

class D122AmdusiasStates : StateMachineBuilder
{
    public D122AmdusiasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThunderclapConcerto>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1064, NameID = 14271, DevOnly = true)]
public class D122Amdusias(WorldState ws, Actor primary) : BossModule(ws, primary, new(281, -285), new ArenaBoundsCircle(19.5f));

