namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class DireStraits : Components.GenericAOEs
{
    private static readonly AOEShapeRect rect = new(80, 40);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_aoes.Count == 2)
        {
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
            yield return new(_aoes[1].Shape, _aoes[1].Origin, _aoes[1].Rotation, _aoes[1].Activation, risky: false);
        }
        if (_aoes.Count == 1)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DireStraitTelegraph1)
        {
            _aoes.Add(new(rect, module.Bounds.Center, spell.Rotation, spell.NPCFinishAt.AddSeconds(5)));
            _aoes.Add(new(rect, module.Bounds.Center, spell.Rotation + 180.Degrees(), spell.NPCFinishAt.AddSeconds(6.7f)));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.DireStraitsRectAOE1 or AID.DireStraitsRectAOE2)
            _aoes.RemoveAt(0);
    }
}
