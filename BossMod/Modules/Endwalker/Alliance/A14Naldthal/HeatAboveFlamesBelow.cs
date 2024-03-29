namespace BossMod.Endwalker.Alliance.A14Naldthal;

class HeatAboveFlamesBelow : Components.GenericAOEs
{
    public List<AOEInstance> _aoes = new();

    private static readonly AOEShapeCircle _shapeOut = new(8);
    private static readonly AOEShapeDonut _shapeIn = new(8, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
            _aoes.Add(new(shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
            _aoes.Clear();
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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
