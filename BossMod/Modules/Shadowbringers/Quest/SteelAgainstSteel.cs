namespace BossMod.Shadowbringers.Quest.SteelAgainstSteel;

public enum OID : uint
{
    Boss = 0x2A45,
    Helper = 0x233C,
    Fustuarium = 0x2AD8, // R0.500, x1 (spawn during fight)
    CullingBlade = 0x2AD3, // R0.500, x0 (spawn during fight)
    IndustrialForce = 0x2BCE, // R0.500, x0 (spawn during fight)
    TerminusEst = 0x2A46, // R1.000, x0 (spawn during fight)
    CaptiveBolt = 0x2AD7, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    CullingBlade1 = 17553, // CullingBlade->self, 3.5s cast, range 60 30-degree cone
    TheOrder = 17568, // Boss->self, 4.0s cast, single-target
    TerminusEst1 = 17567, // TerminusEst->self, no cast, range 40+R width 4 rect
    CaptiveBolt = 17561, // CaptiveBolt->self, 7.0s cast, range 50+R width 10 rect
    AetherochemicalGrenado = 17575, // 2A47->location, 4.0s cast, range 8 circle
    Exsanguination = 17565, // 2AD6->self, 5.0s cast, range -17 donut
    Exsanguination1 = 17564, // 2AD5->self, 5.0s cast, range -12 donut
    Exsanguination2 = 17563, // 2AD4->self, 5.0s cast, range -7 donut
    DiffractiveLaser = 17574, // 2A48->self, 3.0s cast, range 45+R width 4 rect
    SnakeShot = 17569, // Boss->self, 4.0s cast, range 20 240-degree cone
    ScaldingTank1 = 17558, // Fustuarium->2A4A, 6.0s cast, range 6 circle
    ToTheSlaughter = 17559, // Boss->self, 4.0s cast, range 40 180-degree cone
}

class ScaldingTank(BossModule module) : Components.StackWithCastTargets(module, AID.ScaldingTank1, 6);
class ToTheSlaughter(BossModule module) : Components.StandardAOEs(module, AID.ToTheSlaughter, new AOEShapeCone(40, 90.Degrees()));
class Exsanguination(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Actor, float Inner)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeDonutSector(c.Inner, c.Inner + 5, 90.Degrees()), c.Actor.CastInfo!.LocXZ, c.Actor.Rotation, Module.CastFinishAt(c.Actor.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var radius = (AID)spell.Action.ID switch
        {
            AID.Exsanguination => 12,
            AID.Exsanguination1 => 7,
            AID.Exsanguination2 => 2,
            _ => 0
        };

        if (radius > 0)
            Casters.Add((caster, radius));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Exsanguination or AID.Exsanguination1 or AID.Exsanguination2)
            Casters.RemoveAll(c => c.Actor == caster);
    }
}
class CaptiveBolt(BossModule module) : Components.StandardAOEs(module, AID.CaptiveBolt, new AOEShapeRect(50, 5), maxCasts: 4);
class AetherochemicalGrenado(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalGrenado, 8);
class DiffractiveLaser(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaser, new AOEShapeRect(45, 2));
class SnakeShot(BossModule module) : Components.StandardAOEs(module, AID.SnakeShot, new AOEShapeCone(20, 120.Degrees()));
class CullingBlade(BossModule module) : Components.StandardAOEs(module, AID.CullingBlade1, new AOEShapeCone(60, 15.Degrees()))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // zone rasterization can end up missing the arena center since it only contains the tips of a bunch of very pointy triangles
        if (Casters.FirstOrDefault() is Actor c)
            hints.AddForbiddenZone(ShapeContains.Circle(c.Position, 0.5f), Module.CastFinishAt(c.CastInfo));
    }
}
class TerminusEst(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? Caster;
    private readonly List<Actor> Actors = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TerminusEst)
            Actors.Add(actor);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Caster is Actor c)
            foreach (var t in Actors)
                yield return new AOEInstance(new AOEShapeRect(40, 2), t.Position, t.Rotation, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // check if we already have terminuses out, because he can use this spell for a diff mechanic
        if (spell.Action.ID == (uint)AID.TheOrder && Actors.Count > 0)
            Caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TerminusEst1)
        {
            Actors.Remove(caster);
            // reset for next iteration
            if (Actors.Count == 0)
                Caster = null;
        }
    }
}

class VitusQuoMessallaStates : StateMachineBuilder
{
    public VitusQuoMessallaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CullingBlade>()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<SnakeShot>()
            .ActivateOnEnter<Exsanguination>()
            .ActivateOnEnter<ToTheSlaughter>()
            .ActivateOnEnter<ScaldingTank>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68802, NameID = 8872)]
public class VitusQuoMessalla(WorldState ws, Actor primary) : BossModule(ws, primary, new(-266, -507), new ArenaBoundsCircle(19.5f));
