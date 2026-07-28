namespace BossMod.Endwalker.Savage.P7SAgdistis;

[SkipLocalsInit]
sealed class Border(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly WPos circleCenterNW = new(85.71058f, 91.75f), circleCenterS = new(100f, 116.5f), circleCenterNE = new(114.28942f, 91.75f), arenaCenter = new(100f, 100f);
    private readonly Polygon[] circles = [new(circleCenterNW, 10f, 48), new(circleCenterS, 10f, 48), new(circleCenterNE, 10f, 48)];
    private readonly RectangleSE[] centerBridge = [new(arenaCenter, circleCenterNW, 4f), new(arenaCenter, circleCenterS, 4f), new(arenaCenter, circleCenterNE, 4f)];
    private readonly List<RectangleSE> activeBridges = [with(3)];
    private readonly List<RectangleSE> disappearingBridges = [with(3)];
    private readonly RectangleSE[] bridgeN = [new(circleCenterNW, circleCenterNE, 4f)];
    private readonly RectangleSE[] bridgeEandW = [new(circleCenterNW, circleCenterS, 4f), new(circleCenterNE, circleCenterS, 4f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case 0: // small platforms
                switch (state)
                {
                    // 0x00200010 - large platform disappears?
                    // 0x00800040 - small platforms appear?
                    // 0x08000004 - small platforms disappear?
                    case 0x00020001u: // small plattforms appear preparation
                        _aoe = [new(new AOEShapeCustom([new Square(Arena.Center, 20f)], circles), arenaCenter, activation: WorldState.FutureTime(6.8d))];
                        break;
                    case 0x00800040u: // small platforms appear
                        _aoe = [];
                        var arena = new ArenaBoundsCustom(circles);
                        Arena.Bounds = arena;
                        Arena.Center = arena.Center;
                        break;
                    case 0x02000100u: // small plattforms disappear prep
                        _aoe = [new(new AOEShapeCustom([new DonutV(arenaCenter, 20f, 24.5f, 128)]), Arena.Center, activation: WorldState.FutureTime(6.8d))];
                        break;
                    case 0x08000004u: // large platform appears
                        _aoe = [];
                        var arena2 = new ArenaBoundsCustom([new Polygon(arenaCenter, 20f, 128)]);
                        Arena.Bounds = arena2;
                        Arena.Center = arena2.Center;
                        break;
                }
                break;
            case 1: // bridge N
                switch (state)
                {
                    case 0x00020001u: // bridge appears
                        AddBridges(bridgeN);
                        break;
                    case 0x00200010u: // bridge starts to disappear
                        AddBridgesWarning(bridgeN);
                        break;
                    case 0x00800004u: // bridge disappears
                        RemoveBridges(bridgeN);
                        break;
                }
                break;
            case 2: // bridge E, index 3 is bridge W, but they always get added/removed together
                switch (state)
                {
                    case 0x00020001u: // bridge appears
                        AddBridges(bridgeEandW);
                        break;
                    case 0x00200010u: // bridge starts to disappear
                        AddBridgesWarning(bridgeEandW);
                        break;
                    case 0x00800004u: // bridge disappears
                        RemoveBridges(bridgeEandW);
                        break;
                }
                break;
            case 6: // bridge center
                switch (state)
                {
                    case 0x00020001u: // bridge appears
                        AddBridges(centerBridge);
                        break;
                    case 0x00200010u: // bridge starts to disappear
                        AddBridgesWarning(centerBridge);
                        break;
                    case 0x00800004u: // bridge disappears
                        RemoveBridges(centerBridge);
                        break;
                }
                break;
                void AddBridges(RectangleSE[] bridges)
                {
                    var count = bridges.Length;
                    for (var i = 0; i < count; ++i)
                    {
                        activeBridges.Add(bridges[i]);
                    }
                    Arena.Bounds = new ArenaBoundsCustom([.. circles, .. activeBridges]);
                }
                void AddBridgesWarning(RectangleSE[] bridges)
                {
                    var count = bridges.Length;
                    for (var i = 0; i < count; ++i)
                    {
                        disappearingBridges.Add(bridges[i]);
                    }
                    _aoe = [new(new AOEShapeCustom(disappearingBridges, circles), Arena.Center, activation: WorldState.FutureTime(5.7d))];
                }
                void RemoveBridges(RectangleSE[] bridges)
                {
                    var count = bridges.Length;
                    for (var i = 0; i < count; ++i)
                    {
                        activeBridges.Remove(bridges[i]);
                    }
                    _aoe = [];
                    disappearingBridges.Clear();
                    Arena.Bounds = new ArenaBoundsCustom([.. circles, .. activeBridges]);
                }
        }
    }
}
