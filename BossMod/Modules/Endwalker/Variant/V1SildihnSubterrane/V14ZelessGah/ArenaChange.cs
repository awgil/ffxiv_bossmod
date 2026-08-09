namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShowOfStrength && Arena.Bounds.Radius > 20f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Rectangle(center, 25f, 30f)], [new Rectangle(center, 15f, 20f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.8d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x14 && state == 0x00080004u)
        {
            Arena.Bounds = new ArenaBoundsRect(15f, 20f);
            _aoe = [];
        }
    }
}
