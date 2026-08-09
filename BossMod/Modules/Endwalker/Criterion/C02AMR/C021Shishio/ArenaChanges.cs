namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(20f, 30f);

    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.NEnkyo or (uint)AID.SEnkyo && Arena.Bounds.Radius > 20f)
        {
            var center = Arena.Center;
            AddAOE(new AOEShapeCustom(center, [new Square(center, 24.5f)], [new Square(center, 20f)]));
        }
        else if (id is (uint)AID.NStormcloudSummons or (uint)AID.SStormcloudSummons)
        {
            AddAOE(donut);
        }
        void AddAOE(AOEShape shape)
        {
            var center = Arena.Center;
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.8d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            if (index == 0x34)
            {
                Arena.Bounds = new ArenaBoundsCustom([new Polygon(Arena.Center, 20f, 64)]);
                _aoe = [];
            }
            else if (index == 0x35)
            {
                Arena.Bounds = new ArenaBoundsSquare(20f);
                _aoe = [];
            }
        }
    }
}
