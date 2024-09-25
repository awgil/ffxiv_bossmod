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

public enum TetherID : uint
{
    GrowthTether = 84 // EFB->Boss/player
}

class FeedingTime(BossModule module) : Components.InterceptTether(module, ActionID.MakeSpell(AID.FeedingTime), (uint)TetherID.GrowthTether)
{
    private DateTime _activation;
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.PaintedSapling)
            _activation = WorldState.FutureTime(10.9f);
    }

    //TODO: consider moving this logic to the component
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
        {
            var source = Module.Enemies(OID.PaintedSapling)[slot].Position;
            var direction = (Module.PrimaryActor.Position - source).Normalized();
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position - (Module.PrimaryActor.HitboxRadius + 0.1f) * Angle.FromDirection(direction).ToDirection(), source, 1), _activation);
        }
    }
}

class Tiiimbeeer(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Tiiimbeeer));
class Swinge(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Swinge)
            _aoe = new(new AOEShapeCone(50 + caster.HitboxRadius, 30.Degrees()), caster.Position, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Swinge)
            _aoe = null;
    }
}

class D022GriauleStates : StateMachineBuilder
{
    public D022GriauleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FeedingTime>()
            .ActivateOnEnter<Tiiimbeeer>()
            .ActivateOnEnter<Swinge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus), Ported by Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8143)]
public class D022Griaule(WorldState ws, Actor primary) : BossModule(ws, primary, new(7.17f, -339), new ArenaBoundsCircle(24.6f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.PaintedRoot), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.PaintedRoot => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
