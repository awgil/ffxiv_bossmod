namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class SerpentsTide(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(80, 10);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SerpentsTideVisual)
            _aoes.Add(new(rect, caster.Position, spell.Rotation, spell.NPCFinishAt.AddSeconds(0.15f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.SerpentsTideRectAOE1 or AID.SerpentsTideRectAOE2 or AID.SerpentsTideRectAOE3 or AID.SerpentsTideRectAOE4)
            _aoes.RemoveAt(0);
    }
}
