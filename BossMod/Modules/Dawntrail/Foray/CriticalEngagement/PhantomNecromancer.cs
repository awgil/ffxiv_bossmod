namespace BossMod.Dawntrail.Foray.CriticalEngagement.PhantomNecromancer;

public enum OID : uint
{
    Boss = 0x4BC1, // R4.000, x1
    Helper = 0x233C, // R0.500, x3, Helper type
    PhantomNecromancer = 0x4C75, // R1.000, x1
    LongDeadExplorer = 0x4BC2, // R1.000, x0 (spawn during fight)
    LongDeadPirate = 0x4BC3, // R2.600, x0 (spawn during fight)
}

public enum AID : uint
{
    DeathWall = 47173, // PhantomNecromancer->self, no cast, ???
    AutoAttack = 50761, // Boss->player, no cast, single-target
    DarkII = 47181, // Boss->self, 5.0s cast, range 50 width 50 rect
    RiseOfTheFallen = 47174, // Boss->self, 3.0s cast, single-target
    ExplosionSmall = 47175, // LongDeadExplorer->self, 2.0s cast, range 8 circle
    ExplosionBig = 47176, // LongDeadPirate->self, 4.0s cast, range 80 width 7 cross
    DarkFlareCast = 47182, // Boss->self, 5.0s cast, single-target
    DarkFlare = 47183, // Helper->self, no cast, ???
    ArcaneRevelation = 47179, // Boss->self, 3.0s cast, single-target
    Necrosurge = 47180, // Helper->self, 7.0s cast, range 70 width 12 rect
}

class DarkII(BossModule module) : Components.StandardAOEs(module, AID.DarkII, new AOEShapeRect(50, 25));
class DarkFlare(BossModule module) : Components.RaidwideCastDelay(module, AID.DarkFlareCast, AID.DarkFlare, 0.9f);
class Necrosurge(BossModule module) : Components.StandardAOEs(module, AID.Necrosurge, new AOEShapeRect(70, 6));

// TODO: it might be better to show both imminent and next set of aoes, but that will be quite noisy, and who is actually doing this content manually anyway?
class LongDeadExploder(BossModule module) : Components.GenericAOEs(module, AID.ExplosionSmall)
{
    readonly List<(Actor actor, DateTime explosion)> _actors = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _actors.Select(a => new AOEInstance(new AOEShapeCircle(8), a.actor.Position, a.actor.Rotation, a.explosion)).TakeSpan(TimeSpan.FromSeconds(1));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.LongDeadExplorer && id == 0x11D4)
            _actors.Add((actor, WorldState.FutureTime(7.1f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var ix = _actors.FindIndex(a => a.actor == caster);
            if (ix >= 0)
                _actors.Ref(ix).explosion = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _actors.RemoveAll(a => a.actor == caster);
    }
}

class LongDeadPirate(BossModule module) : Components.GenericAOEs(module, AID.ExplosionBig)
{
    readonly List<(Actor actor, DateTime explosion)> _actors = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _actors.Select(a => new AOEInstance(new AOEShapeCross(80, 3.5f), a.actor.Position, default, a.explosion));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.LongDeadPirate && id == 0x11D1)
            _actors.Add((actor, WorldState.FutureTime(9.1f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var ix = _actors.FindIndex(a => a.actor == caster);
            if (ix == 0)
                _actors.Ref(ix).explosion = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _actors.RemoveAll(a => a.actor == caster);
    }
}

class PhantomNecromancerStates : StateMachineBuilder
{
    public PhantomNecromancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<DarkFlare>()
            .ActivateOnEnter<Necrosurge>()
            .ActivateOnEnter<LongDeadExploder>()
            .ActivateOnEnter<LongDeadPirate>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14512)]
public class PhantomNecromancer(WorldState ws, Actor primary) : CEModule(ws, primary, new(224, -860), new ArenaBoundsSquare(20));
