namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class BindingSigil(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(9);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var deadline = _aoes.Count > 0 ? _aoes[0].Activation.AddSeconds(1) : default;
        return _aoes.TakeWhile(aoe => aoe.Activation < deadline);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BindingSigilPreview)
        {
            _aoes.Add(new(_shape, caster.Position, default, Module.CastFinishAt(spell, 9.6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SoulBinding)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
