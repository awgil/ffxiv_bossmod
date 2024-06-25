namespace BossMod.Stormblood.Alliance.A22Belias;

class FireIV(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FireIV));
class Eruption(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Eruption), 8);
class TimeBomb2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TimeBomb2), new AOEShapeCone(60, 45.Degrees()));

class TimeEruption(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.TimeEruptionVisual))
{
    private readonly List<Actor> _castersEruptionAOEFirst = [];
    private readonly List<Actor> _castersEruptionAOESecond = [];

    private static readonly AOEShape _shapeEruptionAOEFirst = new AOEShapeRect(10, 10, 10);
    private static readonly AOEShape _shapeEruptionAOESecond = new AOEShapeRect(10, 10, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersEruptionAOEFirst.Count > 0)
            return _castersEruptionAOEFirst.Select(c => new AOEInstance(_shapeEruptionAOEFirst, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
        else
            return _castersEruptionAOESecond.Select(c => new AOEInstance(_shapeEruptionAOESecond, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
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
        AID.TimeEruptionAOEFirst => _castersEruptionAOEFirst,
        AID.TimeEruptionAOESecond => _castersEruptionAOESecond,
        _ => null
    };
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7223)]
public class A22Belias(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200, -541), new ArenaBoundsSquare(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Gigas))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
