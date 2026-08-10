namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VerdantTempest && Arena.Bounds.Radius > 26f)
        {
            var center = Arena.Center;
            _aoe = [new(donut, center, default, Module.CastFinishAt(spell, 3.8d), shapeDistance: donut.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x1B && state == 0x00020001u)
        {
            var arena = TrinitySeeker.GetDefaultArena();
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
            _aoe = [];
        }
    }
}
