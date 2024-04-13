namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to sewage deluge mechanic
class SewageDeluge(BossModule module) : BossComponent(module)
{
    public enum Corner { None, NW, NE, SW, SE }

    private Corner _blockedCorner = Corner.None;

    private static readonly float _offsetCorner = 9.5f; // not sure
    private static readonly float _cornerHalfSize = 4; // not sure
    private static readonly float _connectHalfWidth = 2; // not sure
    private static readonly float _cornerInner = _offsetCorner - _cornerHalfSize;
    private static readonly float _cornerOuter = _offsetCorner + _cornerHalfSize;
    private static readonly float _connectInner = _offsetCorner - _connectHalfWidth;
    private static readonly float _connectOuter = _offsetCorner + _connectHalfWidth;

    private static readonly WDir[] _corners = { new(), new(-1, -1), new(1, -1), new(-1, 1), new(1, 1) };

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_blockedCorner == Corner.None)
            return;

        // central area + H additionals
        Arena.ZoneRect(Module.Bounds.Center, new WDir( 1, 0), _connectInner, _connectInner, _cornerInner, ArenaColor.AOE);
        // central V additionals
        Arena.ZoneRect(Module.Bounds.Center, new WDir(0,  1), _connectInner, -_cornerInner, _cornerInner, ArenaColor.AOE);
        Arena.ZoneRect(Module.Bounds.Center, new WDir(0, -1), _connectInner, -_cornerInner, _cornerInner, ArenaColor.AOE);
        // outer additionals
        Arena.ZoneRect(Module.Bounds.Center, new WDir( 1, 0), _cornerOuter, -_connectOuter, _cornerInner, ArenaColor.AOE);
        Arena.ZoneRect(Module.Bounds.Center, new WDir(-1, 0), _cornerOuter, -_connectOuter, _cornerInner, ArenaColor.AOE);
        Arena.ZoneRect(Module.Bounds.Center, new WDir(0,  1), _cornerOuter, -_connectOuter, _cornerInner, ArenaColor.AOE);
        Arena.ZoneRect(Module.Bounds.Center, new WDir(0, -1), _cornerOuter, -_connectOuter, _cornerInner, ArenaColor.AOE);
        // outer area
        Arena.ZoneRect(Module.Bounds.Center, new WDir( 1, 0), Module.Bounds.HalfSize, -_cornerOuter, Module.Bounds.HalfSize, ArenaColor.AOE);
        Arena.ZoneRect(Module.Bounds.Center, new WDir(-1, 0), Module.Bounds.HalfSize, -_cornerOuter, Module.Bounds.HalfSize, ArenaColor.AOE);
        Arena.ZoneRect(Module.Bounds.Center, new WDir(0,  1), Module.Bounds.HalfSize, -_cornerOuter, _cornerOuter, ArenaColor.AOE);
        Arena.ZoneRect(Module.Bounds.Center, new WDir(0, -1), Module.Bounds.HalfSize, -_cornerOuter, _cornerOuter, ArenaColor.AOE);

        var corner = Module.Bounds.Center + _corners[(int)_blockedCorner] * _offsetCorner;
        Arena.ZoneRect(corner, new WDir(1, 0), _cornerHalfSize, _cornerHalfSize, _cornerHalfSize, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // inner border
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerInner, -_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerInner, -_connectInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerInner, -_connectInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerInner, -_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_connectInner, -_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_connectInner, +_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerInner, +_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerInner, +_connectInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerInner, +_connectInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerInner, +_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_connectInner, +_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_connectInner, -_cornerInner));
        MiniArena.PathStroke(true, ArenaColor.Border);

        // outer border
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerOuter, -_cornerOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerInner, -_cornerOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerInner, -_connectOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerInner, -_connectOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerInner, -_cornerOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerOuter, -_cornerOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerOuter, -_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_connectOuter, -_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_connectOuter, +_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerOuter, +_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerOuter, +_cornerOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerInner, +_cornerOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(+_cornerInner, +_connectOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerInner, +_connectOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerInner, +_cornerOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerOuter, +_cornerOuter));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerOuter, +_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_connectOuter, +_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_connectOuter, -_cornerInner));
        Arena.PathLineTo(Module.Bounds.Center + new WDir(-_cornerOuter, -_cornerInner));
        MiniArena.PathStroke(true, ArenaColor.Border);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        // 800375A2: we typically get two events for index=0 (global) and index=N (corner)
        // state 00200010 - "prepare" (show aoe that is still harmless)
        // state 00020001 - "active" (dot in center/borders, oneshot in corner)
        // state 00080004 - "finish" (reset)
        if (index > 0 && index < 5)
        {
            if (state == 0x00200010)
                _blockedCorner = (Corner)index;
            else if (state == 0x00080004)
                _blockedCorner = Corner.None;
        }
    }
}
