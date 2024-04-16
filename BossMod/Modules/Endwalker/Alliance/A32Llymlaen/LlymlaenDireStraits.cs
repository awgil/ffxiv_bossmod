namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class DireStraits(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(80, 40);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
        if (_aoes.Count > 1)
            yield return new(_aoes[1].Shape, _aoes[1].Origin, _aoes[1].Rotation, _aoes[1].Activation, Risky: false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DireStraitTelegraph1)
        {
            _aoes.Add(new(rect, Module.Bounds.Center, spell.Rotation, spell.NPCFinishAt.AddSeconds(5)));
            _aoes.Add(new(rect, Module.Bounds.Center, spell.Rotation + 180.Degrees(), spell.NPCFinishAt.AddSeconds(6.7f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.DireStraitsRectAOE1 or AID.DireStraitsRectAOE2)
            _aoes.RemoveAt(0);
    }
}