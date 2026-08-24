namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class ForwardBackwardHalf(BossModule module) : Components.GenericAOEs(module, AID.HalfFullShortAOE)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeEdge = new(50, 30, 10);
    private static readonly AOEShapeRect _shapeSide = new(60, 60);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (relevant, front, left) = (AID)spell.Action.ID switch
        {
            AID.ForwardHalfR or AID.ForwardHalfLongR => (true, true, false),
            AID.ForwardHalfL or AID.ForwardHalfLongL => (true, true, true),
            AID.BackwardHalfR or AID.BackwardHalfLongR => (true, false, false),
            AID.BackwardHalfL or AID.BackwardHalfLongL => (true, false, true),
            _ => default
        };
        if (!relevant)
            return;

        var cleaveDir = spell.Rotation + (front ? 180 : 0).Degrees();
        _aoes.Add(new(_shapeEdge, caster.Position, cleaveDir, Module.CastFinishAt(spell)));
        _aoes.Add(new(_shapeSide, caster.Position, cleaveDir + (left ? 90 : -90).Degrees(), Module.CastFinishAt(spell)));
    }
}

class HalfFull(BossModule module) : Components.GenericAOEs(module, AID.HalfFullLongAOE)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeSide = new(60, 60);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HalfFullR or AID.HalfFullL)
        {
            var cleaveDir = spell.Rotation + ((AID)spell.Action.ID == AID.HalfFullL ? 90 : -90).Degrees();
            _aoes.Add(new(_shapeSide, caster.Position, cleaveDir, Module.CastFinishAt(spell)));
        }
    }
}
