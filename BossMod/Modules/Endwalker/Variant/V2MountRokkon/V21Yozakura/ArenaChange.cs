namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V21Yozakura;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GloryNeverlasting && Arena.Bounds.Radius > 20f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 22.5f)], [new Square(center, 20f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 3.7d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is 0x35 or 0x36 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
    }
}
