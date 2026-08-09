namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance[] _aoe = [];
    private bool startingArena = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void Update()
    {
        if (startingArena && _aoe.Length == 0)
        {
            var features = Module.Enemies((uint)OID.ArenaFeatures);
            var count = features.Count;
            for (var i = 0; i < count; ++i)
            {
                var f = features[i];
                if (f.EventState == default && f.Position.AlmostEqual(new(244f, -129f), 1f))
                {
                    var center = Arena.Center;
                    _aoe = [new(donut, center, default, WorldState.FutureTime(5d), shapeDistance: donut.Distance(center, default))];
                    return;
                }
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x18 && state == 0x00020001u)
        {
            var arena = QueensGuard.GetDefaultArena();
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
            _aoe = [];
            startingArena = false;
        }
    }
}
