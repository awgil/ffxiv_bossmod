#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D40ForgivenNaivety;

public enum OID : uint
{
    Boss = 0x460B, // R8.000, x1
    Helper = 0x233C, // R0.500, x34 (spawn during fight), Helper type
    _Gen_ForgivenAdulation = 0x4823, // R1.000, x3
    _Gen_ForgivenAdulation1 = 0x460C, // R5.000, x2 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 45130, // Boss/460C->player, no cast, single-target
    _Ability_ = 42122, // Boss/460C->location, no cast, single-target
    _Ability_1 = 42143, // Boss->self, no cast, single-target
    _Ability_BlownBlessing = 42124, // Boss->self, 3.0s cast, single-target
    _Ability_BlownBlessing1 = 42126, // 460C->self, no cast, single-target
    _Ability_2 = 42127, // Boss/460C->self, no cast, single-target
    _Ability_ShiningShot = 42129, // Helper/4823->self, 10.0s cast, range 20 circle
    _Ability_NearTide = 45121, // Boss->self, 6.2+0.8s cast, single-target
    _Ability_NearTide1 = 45169, // Helper->self, 7.0s cast, range 13 circle
    _Ability_BlownBlessing2 = 42123, // Boss->self, 3.0s cast, single-target
    _Ability_BlownBlessing3 = 42125, // 460C->self, no cast, single-target
    _Ability_SaltwaterShot = 42128, // 4823/Helper->self, 10.0s cast, range 40 circle
    _Ability_SelfDestruct = 43289, // 460C->self, 13.0s cast, single-target
    _Ability_FarTide = 45122, // Boss->self, 6.2+0.8s cast, single-target
    _Ability_FarTide1 = 45170, // Helper->self, 7.0s cast, range 8-60 donut
    _Ability_Chaser = 44047, // Helper->location, 3.0s cast, range 5 circle
    _Ability_Chaser1 = 42131, // Helper->location, 3.0s cast, range 5 circle
}

class ShiningShot(BossModule module) : Components.StandardAOEs(module, AID._Ability_ShiningShot, 20, maxCasts: 2);
class SaltwaterShot(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Ability_SaltwaterShot, 21)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var srcs = Sources(slot, actor).SkipWhile(s => IsImmune(slot, s.Activation)).ToList();
        if (srcs.Count > 0)
        {
            var safeCone = srcs.Count > 1
                // aim roughly for next caster
                ? ShapeContains.InvertedCone(srcs[0].Origin, 3, (srcs[1].Origin - srcs[0].Origin).ToAngle(), 21.Degrees())
                // just aim inside arena, safe angle is ~51.32 degrees
                : ShapeContains.InvertedCone(srcs[0].Origin, 3, srcs[0].Direction, 51.Degrees());

            hints.AddForbiddenZone(safeCone, srcs[0].Activation);
        }
    }
}
class Adulation(BossModule module) : Components.Adds(module, (uint)OID._Gen_ForgivenAdulation1, 1);
class NearTide(BossModule module) : Components.StandardAOEs(module, AID._Ability_NearTide1, 13);
class FarTide(BossModule module) : Components.StandardAOEs(module, AID._Ability_FarTide1, new AOEShapeDonut(8, 60));
class Chaser(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_Chaser, AID._Ability_Chaser1], new AOEShapeCircle(5));

class ForgivenNaivetyStates : StateMachineBuilder
{
    public ForgivenNaivetyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NearTide>()
            .ActivateOnEnter<FarTide>()
            .ActivateOnEnter<Adulation>()
            .ActivateOnEnter<ShiningShot>()
            .ActivateOnEnter<SaltwaterShot>()
            .ActivateOnEnter<Chaser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1035, NameID = 13977)]
public class ForgivenNaivety(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsCircle(16.5f));

