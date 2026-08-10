namespace BossMod.Endwalker.Alliance.A11Byregot;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module) // arena changes excluding hammer phase
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = new ArenaBoundsSquare(24f);
                _aoe = [];
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = new ArenaBoundsSquare(24.5f);
            }
        }
        else if (index == 0x4F && state == 0x00080004u)
        {
            Arena.Bounds = new ArenaBoundsSquare(24.5f);
            Arena.Center = new(0f, 700f);
            AddAOE(WorldState.FutureTime(10.6d));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OrdealOfThunder && Arena.Bounds.Radius > 24f)
        {
            AddAOE(Module.CastFinishAt(spell, 0.9d));
        }
    }

    private void AddAOE(DateTime act)
    {
        var center = Arena.Center;
        var shape = new AOEShapeCustom(center, [new Square(center, 24.5f)], [new Square(center, 24f)]);
        _aoe = [new(shape, center, default, act, shapeDistance: shape.Distance(center, default))];
    }
}
