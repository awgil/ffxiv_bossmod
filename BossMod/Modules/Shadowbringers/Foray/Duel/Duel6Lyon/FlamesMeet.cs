namespace BossMod.Shadowbringers.Foray.Duel.Duel6Lyon;

class FlamesMeet(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCross _shape = new(40, 7);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (int i = 0; i < _aoes.Count; i++)
        {
            AOEInstance aoe = _aoes[i];
            if (i == 0)
                aoe.Color = ArenaColor.Danger;
            yield return aoe;
            // Only show the first 2 so it's obvious which one to go to.
            if (i == 1)
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlamesMeet2)
            _aoes.Add(new(_shape, caster.Position));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlamesMeet2 && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}
