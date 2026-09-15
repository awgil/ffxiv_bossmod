namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class VoidStardust(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos pos, DateTime activation)> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in _aoes.Skip(NumCasts + 2).Take(10))
            yield return new(_shape, aoe.pos, default, aoe.activation);
        foreach (var aoe in _aoes.Skip(NumCasts).Take(2))
            yield return new(_shape, aoe.pos, default, aoe.activation, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.VoidStardustFirst:
                _aoes.Add((caster.Position, Module.CastFinishAt(spell)));
                break;
            case AID.VoidStardustRestVisual:
                _aoes.Add((caster.Position, Module.CastFinishAt(spell, 2.9f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.VoidStardustFirst or AID.VoidStardustRestAOE)
            ++NumCasts;
    }
}

class AbyssalQuasar(BossModule module) : Components.StackWithCastTargets(module, AID.AbyssalQuasar, 3, 2);
