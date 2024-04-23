namespace BossMod.Endwalker.Hunt.RankS.NarrowRift;

public enum OID : uint
{
    Boss = 0x35DB, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    EmptyPromiseCircle = 27487, // Boss->self, 5.0s cast, range 10 circle
    EmptyPromiseDonut = 27488, // Boss->self, 5.0s cast, range 6-40 donut
    VanishingRayStart = 27333, // Boss->self, no cast, range 1 circle, visual
    VanishingRay = 27516, // Boss->self, no cast, range 50 width 8 rect
    VanishingRayEnd = 27519, // Boss->self, no cast, single-target, visual
    ContinualMeddlingFR = 27327, // Boss->self, 4.0s cast, range 60 circle, applies forward march/right face debuffs
    ContinualMeddlingFL = 27328, // Boss->self, 4.0s cast, range 60 circle, applies forward march/left face debuffs
    ContinualMeddlingBL = 27329, // Boss->self, 4.0s cast, range 60 circle, applies about face/left face debuffs
    ContinualMeddlingBR = 27330, // Boss->self, 4.0s cast, range 60 circle, applies about face/right face debuffs
    // also meddling: 27325
    EmptyRefrainCircleFirst = 27331, // Boss->self, 12.0s cast, range 10 circle
    EmptyRefrainDonutFirst = 27332, // Boss->self, 13.5s cast, range 6-40 donut
    EmptyRefrainCircleSecond = 27335, // Boss->self, 1.0s cast, range 10 circle
    EmptyRefrainDonutSecond = 27337, // Boss->self, 1.0s cast, range 6-40 donut
}

public enum SID : uint
{
    ForwardMarch = 1958, // Boss->player, extra=0x0
    AboutFace = 1959, // Boss->player, extra=0x0
    LeftFace = 1960, // Boss->player, extra=0x0
    RightFace = 1961, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=1 (forward) / 2 (backward) / 4 (left) / 8 (right)
}

class EmptyPromise(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEShape> _pendingShapes = [];

    private static readonly AOEShapeCircle _shapeCircle = new(10);
    private static readonly AOEShapeDonut _shapeDonut = new(6, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_pendingShapes.Count > 0)
            yield return new(_pendingShapes[0], Module.PrimaryActor.Position, new(), Module.PrimaryActor.CastInfo?.NPCFinishAt ?? WorldState.CurrentTime); // TODO: activation
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor)
            return;
        switch ((AID)spell.Action.ID)
        {
            case AID.EmptyPromiseCircle:
                _pendingShapes.Add(_shapeCircle);
                break;
            case AID.EmptyPromiseDonut:
                _pendingShapes.Add(_shapeDonut);
                break;
            case AID.EmptyRefrainCircleFirst:
                _pendingShapes.Add(_shapeCircle);
                _pendingShapes.Add(_shapeDonut);
                break;
            case AID.EmptyRefrainDonutFirst:
                _pendingShapes.Add(_shapeDonut);
                _pendingShapes.Add(_shapeCircle);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster == Module.PrimaryActor && _pendingShapes.Count > 0)
            _pendingShapes.RemoveAt(0);
    }
}

class VanishingRay(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private static readonly AOEShapeRect _shape = new(50, 4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (caster != Module.PrimaryActor)
            return;
        switch ((AID)spell.Action.ID)
        {
            case AID.VanishingRayStart:
                _activation = WorldState.FutureTime(4);
                break;
            case AID.VanishingRay:
                _activation = new();
                break;
        }
    }
}

class ContinualMeddling(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.CastInfo != null && Module.PrimaryActor.CastInfo.IsSpell() && (AID)Module.PrimaryActor.CastInfo.Action.ID is AID.ContinualMeddlingFR or AID.ContinualMeddlingFL or AID.ContinualMeddlingBL or AID.ContinualMeddlingBR)
            hints.Add("Apply march debuffs");
    }
}

class NarrowRiftStates : StateMachineBuilder
{
    public NarrowRiftStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmptyPromise>()
            .ActivateOnEnter<VanishingRay>()
            .ActivateOnEnter<ContinualMeddling>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 10622)]
public class NarrowRift(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
