namespace BossMod.Stormblood.Foray.NM.ProvenanceWatcher;

public enum OID : uint
{
    Boss = 0x2686, // R12.000, x1
    Helper1 = 0x2687, // R0.500, x1
    Charybdis = 0x2768, // R1.000, x0 (spawn during fight)
    Helper3 = 0x277B, // R0.500, x0 (spawn during fight)
    Helper4 = 0x277E, // R12.000, x0 (spawn during fight)
    Icicle = 0x2769, // R2.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 14983, // Boss->player, no cast, single-target
    TheScarletPrice = 15001, // Boss->player, 5.0s cast, range 3 circle
    TheScarletWhisper = 15000, // Boss->self, 4.0s cast, range 10+R ?-degree cone
    Reforge = 14987, // Boss->self, 5.0s cast, single-target
    EuhedralSwat = 14992, // Helper4->self, 6.0s cast, range 100 width 26 rect
    Touchdown = 14993, // Boss->self, no cast, range 50 circle
    PillarImpact = 15346, // Icicle->self, 2.5s cast, range 4+R circle
    PillarPierce = 15344, // Icicle->self, 4.0s cast, range 50+R width 10 rect
    Thunderstorm = 15005, // Helper3->location, 3.0s cast, range 5 circle
    IceAndLevin = 14997, // Boss->self, 5.0s cast, single-target
    Chillstorm = 15006, // Helper3->self, no cast, range ?-40 donut
    IceAndWind = 14996, // Boss->self, 5.0s cast, single-target
    Charybdis = 15002, // Helper2->location, no cast, range 6 circle
    HotTailFirst = 14999, // Boss->self, 5.0s cast, range 65+R width 16 rect
    HotTailSecond = 15007, // Boss->self, no cast, range 65+R width 16 rect
    AkhMornFirst = 14994, // Boss->players, 5.0s cast, range 6 circle
    AkhMornRest = 14995, // Boss->players, no cast, range 6 circle
    DiffractiveBreak = 14998, // Boss->self, 4.0s cast, range 40 circle
}

class TheScarletPrice(BossModule module) : Components.BaitAwayCast(module, AID.TheScarletPrice, new AOEShapeCircle(3), true, true);
class TheScarletWhisper(BossModule module) : Components.StandardAOEs(module, AID.TheScarletWhisper, new AOEShapeCone(22, 60.Degrees()));
class EuhedralSwat(BossModule module) : Components.StandardAOEs(module, AID.EuhedralSwat, new AOEShapeRect(100, 13, 20));
class Touchdown(BossModule module) : Components.RaidwideInstant(module, AID.Touchdown, 3.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (Activation == default && (AID)spell.Action.ID == AID.EuhedralSwat)
            Activation = WorldState.FutureTime(Delay);
    }
}
class PillarImpact(BossModule module) : Components.StandardAOEs(module, AID.PillarImpact, new AOEShapeCircle(6.5f));
class PillarPierce(BossModule module) : Components.StandardAOEs(module, AID.PillarPierce, new AOEShapeRect(52.5f, 5));
class Thunderstorm(BossModule module) : Components.StandardAOEs(module, AID.Thunderstorm, 5);
class IceAndLevin(BossModule module) : Components.GenericAOEs(module, AID.Chillstorm)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.IceAndLevin)
            _aoe = new(new AOEShapeDonut(11, 40), Module.PrimaryActor.Position, Activation: Module.CastFinishAt(spell).AddSeconds(1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _aoe = null;
    }
}
class Charybdis(BossModule module) : Components.GenericAOEs(module, AID.Charybdis)
{
    private class C(int CastsLeft, Actor Actor, DateTime Activation) { public int CastsLeft = CastsLeft; public Actor Actor = Actor; public DateTime Activation = Activation; }

    private readonly Dictionary<ulong, C> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Values.Select(c => new AOEInstance(new AOEShapeCircle(6), c.Actor.Position, Activation: c.Activation));

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Charybdis)
            Casters[actor.InstanceID] = new(19, actor, WorldState.FutureTime(4));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && Casters.TryGetValue(caster.InstanceID, out var c))
        {
            if (--c.CastsLeft == 0)
                Casters.Remove(caster.InstanceID);
        }
    }
}
class HotTail(BossModule module) : Components.StandardAOEs(module, AID.HotTailFirst, new AOEShapeRect(77, 8, 77));
class HotTailSecond(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    private AOEInstance? _aoe;
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HotTailFirst)
            _aoe = new(new AOEShapeRect(77, 8, 77), caster.Position, caster.Rotation, WorldState.FutureTime(3.1f));

        if ((AID)spell.Action.ID == AID.HotTailSecond)
            _aoe = null;
    }
}

class AkhMorn(BossModule module) : Components.GenericStackSpread(module)
{
    private int CastsLeft;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMornFirst && WorldState.Actors.Find(spell.TargetID) is { } target)
        {
            Stacks.Add(new(target, 6, activation: Module.CastFinishAt(spell)));
            CastsLeft = 3;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMornRest && --CastsLeft == 0)
            Stacks.Clear();
    }
}

class ProvenanceWatcherStates : StateMachineBuilder
{
    public ProvenanceWatcherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheScarletPrice>()
            .ActivateOnEnter<TheScarletWhisper>()
            .ActivateOnEnter<EuhedralSwat>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<PillarImpact>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<Thunderstorm>()
            .ActivateOnEnter<IceAndLevin>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<HotTail>()
            .ActivateOnEnter<HotTailSecond>()
            .ActivateOnEnter<AkhMorn>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1423, Contributors = "xan", SortOrder = 10)]
public class ProvenanceWatcher(WorldState ws, Actor primary) : BossModule(ws, primary, new(564.0466f, -568.6868f), new ArenaBoundsCircle(51.5f, MapResolution: 1));
