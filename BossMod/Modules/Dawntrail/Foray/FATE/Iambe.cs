namespace BossMod.Dawntrail.Foray.FATE.Iambe;

public enum OID : uint
{
    Boss = 0x4C41,
    Helper = 0x233C,
    WinsomeSeed = 0x4C43, // R0.240-0.528, x0 (spawn during fight)
    Iambe = 0x4C42, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50855, // Boss->player, no cast, single-target
    DirectSeeding = 48029, // Boss->self, 3.0s cast, single-target
    GardenersHymnCast = 48031, // Boss->self, 2.5s cast, single-target
    GardenersHymn = 48032, // 4C42->location, 6.0s cast, range 5 circle
    Burst = 48033, // 4C43->self, 2.0s cast, range 15 circle
    OdeOfTheUnderfoot = 48037, // Boss->self, 5.0s cast, range 10 circle
    IambicMarch = 48035, // Boss->self, 3.0s cast, range 40 circle
}

public enum SID : uint
{
    ForwardMarch = 5142, // Boss->player, extra=0x0
    AboutFace = 5143, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x2/0x1
    Gen = 5106, // 4C42->4C43, extra=0x1
    Gen1 = 5107, // 4C42->4C43/Boss, extra=0x1
}

class GardenersHymn(BossModule module) : Components.StandardAOEs(module, AID.GardenersHymn, 5.0f);
class OdeOfTheUnderfoot(BossModule module) : Components.StandardAOEs(module, AID.OdeOfTheUnderfoot, 10.0f);

class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly List<Actor> seeds = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.WinsomeSeed)
        {
            seeds.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.WinsomeSeed)
        {
            seeds.Remove(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GardenersHymn)
        {
            foreach (var seed in seeds)
            {
                if (spell.LocXZ.AlmostEqual(seed.Position, 0.5f))
                {
                    aoes.Add(new(new AOEShapeCircle(15.0f), seed.Position));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 0.5f));
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;
}

sealed class IambicMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3.0f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, default, default)
{
    private const float aoeCircle = 10.0f;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
        {
            return;
        }

        var move0 = state.PendingMoves[0];
        var requiredFacing = Angle.FromDirection((actor.Position - Module.PrimaryActor.Position).Normalized()) - move0.dir;
        hints.ForbiddenDirections.Add((requiredFacing + 180.0f.Degrees(), 170.0f.Degrees(), move0.activation));
        var moveDistance = MovementSpeed * move0.duration;
        var unsafeRadius = aoeCircle - moveDistance;
        if (unsafeRadius > 0.0f)
        {
            hints.AddForbiddenZone(ShapeContains.Circle(Module.PrimaryActor.Position, unsafeRadius), move0.activation);
        }
    }
}

class IambeStates : StateMachineBuilder
{
    public IambeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GardenersHymn>()
            .ActivateOnEnter<OdeOfTheUnderfoot>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<IambicMarch>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14765)]
public class Iambe(WorldState ws, Actor primary) : BossModule(ws, primary, new(-175.000f, -500.000f), new ArenaBoundsCircle(40));
