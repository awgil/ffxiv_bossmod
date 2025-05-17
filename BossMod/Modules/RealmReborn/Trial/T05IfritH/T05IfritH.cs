namespace BossMod.RealmReborn.Trial.T05IfritH;

public enum OID : uint
{
    Boss = 0xD1, // x4
    InfernalNail = 0xD2, // spawn during fight
    Helper = 0x1B2, // x20
}

public enum AID : uint
{
    AutoAttack = 451, // Boss->player, no cast, range 8+R ?-degree cone cleave
    Incinerate = 1353, // Boss->self, no cast, range 10+R 120-degree cone cleave
    VulcanBurst = 1354, // Boss->self, no cast, range 16+R circle knockback 10
    Eruption = 1355, // Boss->self, 2.2s cast, single-target, visual
    EruptionAOE = 1358, // Helper->location, 3.0s cast, range 8 circle aoe
    CrimsonCyclone = 457, // Boss->self, 3.0s cast, range 38+R width 12 rect aoe
    Sear = 452, // Helper->location, no cast, range 8 circle aoe around boss
    RadiantPlume = 1356, // Boss->self, 2.2s cast, single-target, visual
    RadiantPlumeAOE = 1359, // Helper->location, 3.0s cast, range 8 circle aoe
    Hellfire = 1357, // Boss->self, 2.0s cast, infernal nail 'enrage' (raidwide if killed)
}

class Hints(BossModule module) : BossComponent(module)
{
    private DateTime _nailSpawn;

    public override void AddGlobalHints(GlobalHints hints)
    {
        bool nailsActive = ((T05IfritH)Module).ActiveNails.Any();
        if (_nailSpawn == default && nailsActive)
        {
            _nailSpawn = WorldState.CurrentTime;
        }
        if (_nailSpawn != default && nailsActive)
        {
            hints.Add($"Nail enrage in: {Math.Max(55 - (WorldState.CurrentTime - _nailSpawn).TotalSeconds, 0.0f):f1}s");
        }
    }
}

class Incinerate(BossModule module) : Components.Cleave(module, AID.Incinerate, new AOEShapeCone(15, 60.Degrees()));
class Eruption(BossModule module) : Components.StandardAOEs(module, AID.EruptionAOE, 8);

class CrimsonCyclone(BossModule module) : Components.GenericAOEs(module, AID.CrimsonCyclone)
{
    private readonly List<Actor> _casters = [];

    private static readonly AOEShape _shape = new AOEShapeRect(43, 6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _casters.Select(c => new AOEInstance(_shape, c.Position, c.CastInfo?.Rotation ?? c.Rotation, Module.CastFinishAt(c.CastInfo, 0, WorldState.FutureTime(4))));
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Boss && actor != Module.PrimaryActor && id == 0x008D)
            _casters.Add(actor);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }
}

class RadiantPlume(BossModule module) : Components.StandardAOEs(module, AID.RadiantPlumeAOE, 8);

class T05IfritHStates : StateMachineBuilder
{
    public T05IfritHStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hints>()
            .ActivateOnEnter<Incinerate>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<CrimsonCyclone>()
            .ActivateOnEnter<RadiantPlume>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 59, NameID = 1185)]
public class T05IfritH : BossModule
{
    private readonly IReadOnlyList<Actor> _nails;
    public IEnumerable<Actor> ActiveNails => _nails.Where(n => n.IsTargetable && !n.IsDead);

    public T05IfritH(WorldState ws, Actor primary) : base(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
    {
        _nails = Enemies(OID.InfernalNail);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.InfernalNail => 2,
                OID.Boss => e.Actor == PrimaryActor ? 1 : 0,
                _ => 0,
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var n in ActiveNails)
            Arena.Actor(n, ArenaColor.Enemy);
    }
}
