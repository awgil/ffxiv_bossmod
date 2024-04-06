namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class ToTheLast : Components.GenericAOEs
{
    private static readonly AOEShapeRect rect = new(80, 5);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
        if (_aoes.Count > 1)
            yield return new(_aoes[1].Shape, _aoes[1].Origin, _aoes[1].Rotation, _aoes[1].Activation, risky: false);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ToTheLastTelegraph)
            _aoes.Add(new(rect, caster.Position, spell.Rotation, spell.NPCFinishAt.AddSeconds(5 + 1.9f + _aoes.Count)));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.ToTheLastAOE)
            _aoes.RemoveAt(0);
    }
}
