namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SuppressiveMagitekRays && Arena.Bounds.Radius > 23f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 29.5f)], [new Square(center, 23f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 1.5d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x31 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(23f);
            _aoe = [];
        }
    }
}
