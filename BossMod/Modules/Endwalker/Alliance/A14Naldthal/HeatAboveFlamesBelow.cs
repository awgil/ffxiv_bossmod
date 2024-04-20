namespace BossMod.Endwalker.Alliance.A14Naldthal;

class HeatAboveFlamesBelow(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeOut = new(8);
    private static readonly AOEShapeDonut _shapeIn = new(8, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
            _aoes.Add(new(shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
            _aoes.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
            ++NumCasts;
    }

    private AOEShape? ShapeForAction(ActionID action) => (AID)action.ID switch
    {
        AID.FlamesOfTheDeadReal => _shapeIn,
        AID.LivingHeatReal => _shapeOut,
        _ => null
    };
}
