namespace BossMod.Endwalker.Alliance.A36Eulogia;

class FirstBlush : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shape = new(120, 12.5f);
    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FirstBlush1 or AID.FirstBlush2 or AID.FirstBlush3 or AID.FirstBlush4)
            _aoes.Add(new(_shape, caster.Position, caster.Rotation, spell.NPCFinishAt));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FirstBlush1 or AID.FirstBlush2 or AID.FirstBlush3 or AID.FirstBlush4)
            _aoes.RemoveAt(0);
    }
}