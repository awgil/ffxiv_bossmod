namespace BossMod.Endwalker.Alliance.A10Lions;

class RoaringBlaze(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(50, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RoaringBlazeFirst or AID.RoaringBlazeSecond or AID.RoaringBlazeSolo)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RoaringBlazeFirst or AID.RoaringBlazeSecond or AID.RoaringBlazeSolo)
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
    }
}
