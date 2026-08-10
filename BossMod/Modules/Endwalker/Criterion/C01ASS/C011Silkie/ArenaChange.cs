namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NFizzlingSuds or (uint)AID.SFizzlingSuds && Arena.Bounds.Radius > 20f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 29.5f)], [new Square(center, 20f)]);
            _aoe = [new(shape, center, default, WorldState.FutureTime(3.8d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
    }
}
