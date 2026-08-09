namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GloryOfBozja && Arena.Bounds.Radius > 25f)
        {
            var shape = TrinityAvowed.GetArenaChangeAOE();
            var center = Arena.Center;
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.7d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            if (index == 0x11)
            {
                Arena.Bounds = new ArenaBoundsSquare(25f);
                _aoe = [];
            }
            else if (index == 0x12)
            {
                Arena.Bounds = new ArenaBoundsRect(5f, 25f);
                Arena.Center = new(-292f, -82f);
                _aoe = [];
            }
            else if (index == 0x13)
            {
                Arena.Bounds = new ArenaBoundsRect(5f, 25f);
                Arena.Center = new(-252f, -82f);
                _aoe = [];
            }
        }
        else if (state == 0x00080004u && index is 0x12 or 0x13)
        {
            Arena.Bounds = new ArenaBoundsSquare(25f);
            Arena.Center = new(-272f, -82f);
        }
    }
}
