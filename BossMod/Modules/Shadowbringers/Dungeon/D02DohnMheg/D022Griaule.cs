namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D022Griaule;

public enum OID : uint
{
    Boss = 0x98E, // R=3.18-12.72
    PaintedRoot = 0xF08, // R=1.48
    PaintedSapling = 0xEFB // R=0.9
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    Rake = 10355, // Boss->player, no cast, single-target
    Swinge = 8906, // Boss->self, 4.0s cast, range 50+R 60-degree cone
    Fodder = 8897, // Boss->self, 5.0s cast, single-target
    Tiiimbeeer = 8915, // Boss->self, 6.0s cast, range 50 circle
    FeedingTime = 8899, // PaintedSapling->player/Boss, no cast, single-target
    CoilingIvy = 8901 // Boss->self, 3.0s cast, single-target
}

sealed class FeedingTime(BossModule module) : Components.InterceptTether(module, (uint)AID.FeedingTime)
{
    private DateTime _activation;
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.PaintedSapling)
            _activation = WorldState.FutureTime(10.9d);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
        {
            var source = Module.Enemies((uint)OID.PaintedSapling)[slot];
            var target = Module.PrimaryActor;
            hints.AddForbiddenZone(new SDInvertedRect(target.Position + (target.HitboxRadius + 0.1f) * target.DirectionTo(source), source.Position, 0.5f), _activation);
        }
    }
}

sealed class Tiiimbeeer(BossModule module) : Components.RaidwideCast(module, (uint)AID.Tiiimbeeer);
sealed class Swinge(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Swinge)
        {
            _aoe = [new(new AOEShapeCone(50f + caster.HitboxRadius, 30.Degrees()), caster.Position, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Swinge)
        {
            _aoe = [];
        }
    }
}

sealed class D022GriauleStates : StateMachineBuilder
{
    public D022GriauleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FeedingTime>()
            .ActivateOnEnter<Tiiimbeeer>()
            .ActivateOnEnter<Swinge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8143)]
public sealed class D022Griaule : BossModule
{
    public D022Griaule(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D022Griaule(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(7.156f, -339.132f), 24.5f * CosPI.Pi32th, 32)], [new Rectangle(new(7f, -363.5f), 20f, 1.1f), new Rectangle(new(7f, -315), 20f, 0.75f)]);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.PaintedRoot));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.PaintedRoot => 1,
                _ => 0
            };
        }
    }
}
