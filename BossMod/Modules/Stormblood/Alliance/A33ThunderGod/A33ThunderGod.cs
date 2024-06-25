namespace BossMod.Stormblood.Alliance.A33ThunderGod;

class HallowedBolt(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.HallowedBoltVisual))
{
    private readonly List<Actor> _castersHallowedBoltAOE = [];
    private readonly List<Actor> _castersHallowedBoltDonut = [];

    private static readonly AOEShape _shapeHallowedBoltAOE = new AOEShapeCircle(15);
    private static readonly AOEShape _shapeHallowedBoltDonut = new AOEShapeDonut(15, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersHallowedBoltAOE.Count > 3
            ? _castersHallowedBoltAOE.Select(c => new AOEInstance(_shapeHallowedBoltAOE, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersHallowedBoltDonut.Select(c => new AOEInstance(_shapeHallowedBoltDonut, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.HallowedBoltAOE => _castersHallowedBoltAOE,
        AID.HallowedBoltDonut => _castersHallowedBoltDonut,
        _ => null
    };
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7899)] //7917
public class A33ThunderGod(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -600), arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-612.5f, -578.4f), 10), new Circle(new(-587.5f, -578.4f), 10), new Circle(new(-575, -600), 10), new Circle(new(-587.5f, -621.5f), 10), new Circle(new(-612.5f, -621.5f), 10), new Circle(new(-625, -600), 10), new Donut(new(-600, -600), 20, 27)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.EphemeralKnight), ArenaColor.Enemy);
    }
}
