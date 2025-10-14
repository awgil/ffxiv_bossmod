namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D40ForgivenNaivety;

public enum OID : uint
{
    Boss = 0x460B, // R8.000, x1
    Helper = 0x233C, // R0.500, x34 (spawn during fight), Helper type
    WhaleIcon = 0x4823, // R1.000, x3
    ForgivenAdulation = 0x460C, // R5.000, x2 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 45130, // Boss/ForgivenAdulation->player, no cast, single-target
    Jump1 = 42122, // Boss/ForgivenAdulation->location, no cast, single-target
    Jump2 = 42143, // Boss->self, no cast, single-target
    BlownBlessingBoss1 = 42123, // Boss->self, 3.0s cast, single-target
    BlownBlessingBoss2 = 42124, // Boss->self, 3.0s cast, single-target
    BlownBlessingAdds1 = 42125, // ForgivenAdulation->self, no cast, single-target
    BlownBlessingAdds2 = 42126, // ForgivenAdulation->self, no cast, single-target
    Unk1 = 42127, // Boss/ForgivenAdulation->self, no cast, single-target
    SaltwaterShot = 42128, // Helper/WhaleIcon->self, 10.0s cast, range 40 circle, 21y knockback
    ShiningShot = 42129, // Helper/WhaleIcon->self, 10.0s cast, range 20 circle
    NearTideCast = 45121, // Boss->self, 6.2+0.8s cast, single-target
    NearTide = 45169, // Helper->self, 7.0s cast, range 13 circle
    FarTideCast = 45122, // Boss->self, 6.2+0.8s cast, single-target
    FarTide = 45170, // Helper->self, 7.0s cast, range 8-60 donut
    Chaser1 = 44047, // Helper->location, 3.0s cast, range 5 circle
    Chaser2 = 42131, // Helper->location, 3.0s cast, range 5 circle
    SelfDestruct = 43289, // ForgivenAdulation->self, 13.0s cast, single-target
}

class ShiningShot(BossModule module) : Components.StandardAOEs(module, AID.ShiningShot, 20, maxCasts: 2);
class SaltwaterShot(BossModule module) : Components.KnockbackFromCastTarget(module, AID.SaltwaterShot, 21)
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
class Adulation(BossModule module) : Components.Adds(module, (uint)OID.ForgivenAdulation, 1, forbidDots: true);
class NearTide(BossModule module) : Components.StandardAOEs(module, AID.NearTide, 13);
class FarTide(BossModule module) : Components.StandardAOEs(module, AID.FarTide, new AOEShapeDonut(8, 60));
class Chaser(BossModule module) : Components.GroupedAOEs(module, [AID.Chaser1, AID.Chaser2], new AOEShapeCircle(5));

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

