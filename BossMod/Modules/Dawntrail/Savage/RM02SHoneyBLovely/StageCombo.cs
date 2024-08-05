namespace BossMod.Dawntrail.Savage.RM02SHoneyBLovely;

class StageCombo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeOut = new(7);
    private static readonly AOEShapeDonut _shapeIn = new(7, 30);
    private static readonly AOEShapeCross _shapeCross = new(30, 7);
    private static readonly AOEShapeCone _shapeCone = new(30, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var firstActivation = _aoes.Count > 0 ? _aoes[0].Activation : default;
        return _aoes.TakeWhile(aoe => aoe.Activation == firstActivation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (first, last, firstRot) = (AID)spell.Action.ID switch
        {
            AID.CenterstageCombo => (_shapeIn, _shapeOut, 0.Degrees()),
            AID.OuterstageCombo => (_shapeOut, _shapeIn, 45.Degrees()),
            _ => ((AOEShape?)null, (AOEShape?)null, 0.Degrees())
        };
        if (first != null && last != null)
        {
            var firstActivation = Module.CastFinishAt(spell, 1.2f);
            var lastActivation = Module.CastFinishAt(spell, 7.5f);
            _aoes.Add(new(first, Module.Center, 180.Degrees(), firstActivation));
            _aoes.Add(new(_shapeCone, Module.Center, firstRot, firstActivation));
            _aoes.Add(new(_shapeCone, Module.Center, firstRot + 90.Degrees(), firstActivation));
            _aoes.Add(new(_shapeCone, Module.Center, firstRot + 180.Degrees(), firstActivation));
            _aoes.Add(new(_shapeCone, Module.Center, firstRot - 90.Degrees(), firstActivation));
            _aoes.Add(new(_shapeCross, Module.Center, 180.Degrees(), Module.CastFinishAt(spell, 4.2f)));
            _aoes.Add(new(last, Module.Center, 180.Degrees(), lastActivation));
            _aoes.Add(new(_shapeCone, Module.Center, firstRot + 45.Degrees(), lastActivation));
            _aoes.Add(new(_shapeCone, Module.Center, firstRot + 135.Degrees(), lastActivation));
            _aoes.Add(new(_shapeCone, Module.Center, firstRot - 45.Degrees(), lastActivation));
            _aoes.Add(new(_shapeCone, Module.Center, firstRot - 135.Degrees(), lastActivation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.LacerationOut => _shapeOut,
            AID.LacerationCross => _shapeCross,
            AID.LacerationCone => _shapeCone,
            AID.LacerationIn => _shapeIn,
            _ => null
        };
        if (shape != null)
        {
            ++NumCasts;
            var firstActivation = _aoes.Count > 0 ? _aoes[0].Activation : default;
            var count = _aoes.RemoveAll(aoe => aoe.Shape == shape && aoe.Rotation.AlmostEqual(caster.Rotation, 0.1f) && aoe.Activation == firstActivation);
            if (count != 1)
                ReportError($"Unexpected aoe: {spell.Action} @ {caster.Rotation}deg");
        }
    }
}
