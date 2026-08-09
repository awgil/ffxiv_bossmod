namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ArenaFeatures)
        {
            if (state == 0x00010002u)
            {
                var shape = Phantom.GetArenaChangeAOE();
                var center = Arena.Center;
                _aoe = [new(shape, center, default, WorldState.FutureTime(4d), shapeDistance: shape.Distance(center, default))];
            }
            else if (state == 0x00080010u)
            {
                _aoe = [];
                Arena.Bounds = new ArenaBoundsRect(23.5f, 24f);
                Arena.Center = new(202f, -370f);
            }
        }
    }
}
