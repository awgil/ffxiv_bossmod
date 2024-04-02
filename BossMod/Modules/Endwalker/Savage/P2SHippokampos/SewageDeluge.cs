namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to sewage deluge mechanic
class SewageDeluge : BossComponent
{
    public enum Corner { None, NW, NE, SW, SE };

    private Corner _blockedCorner = Corner.None;

    private static readonly float _offsetCorner = 9.5f; // not sure
    private static readonly float _cornerHalfSize = 4; // not sure
    private static readonly float _connectHalfWidth = 2; // not sure
    private static readonly float _cornerInner = _offsetCorner - _cornerHalfSize;
    private static readonly float _cornerOuter = _offsetCorner + _cornerHalfSize;
    private static readonly float _connectInner = _offsetCorner - _connectHalfWidth;
    private static readonly float _connectOuter = _offsetCorner + _connectHalfWidth;

    private static readonly WDir[] _corners = { new(), new(-1, -1), new(1, -1), new(-1, 1), new(1, 1) };

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_blockedCorner == Corner.None)
            return;

        // central area + H additionals
        arena.ZoneRect(module.Bounds.Center, new WDir( 1, 0), _connectInner, _connectInner, _cornerInner, ArenaColor.AOE);
        // central V additionals
        arena.ZoneRect(module.Bounds.Center, new WDir(0,  1), _connectInner, -_cornerInner, _cornerInner, ArenaColor.AOE);
        arena.ZoneRect(module.Bounds.Center, new WDir(0, -1), _connectInner, -_cornerInner, _cornerInner, ArenaColor.AOE);
        // outer additionals
        arena.ZoneRect(module.Bounds.Center, new WDir( 1, 0), _cornerOuter, -_connectOuter, _cornerInner, ArenaColor.AOE);
        arena.ZoneRect(module.Bounds.Center, new WDir(-1, 0), _cornerOuter, -_connectOuter, _cornerInner, ArenaColor.AOE);
        arena.ZoneRect(module.Bounds.Center, new WDir(0,  1), _cornerOuter, -_connectOuter, _cornerInner, ArenaColor.AOE);
        arena.ZoneRect(module.Bounds.Center, new WDir(0, -1), _cornerOuter, -_connectOuter, _cornerInner, ArenaColor.AOE);
        // outer area
        arena.ZoneRect(module.Bounds.Center, new WDir( 1, 0), module.Bounds.HalfSize, -_cornerOuter, module.Bounds.HalfSize, ArenaColor.AOE);
        arena.ZoneRect(module.Bounds.Center, new WDir(-1, 0), module.Bounds.HalfSize, -_cornerOuter, module.Bounds.HalfSize, ArenaColor.AOE);
        arena.ZoneRect(module.Bounds.Center, new WDir(0,  1), module.Bounds.HalfSize, -_cornerOuter, _cornerOuter, ArenaColor.AOE);
        arena.ZoneRect(module.Bounds.Center, new WDir(0, -1), module.Bounds.HalfSize, -_cornerOuter, _cornerOuter, ArenaColor.AOE);

        var corner = module.Bounds.Center + _corners[(int)_blockedCorner] * _offsetCorner;
        arena.ZoneRect(corner, new WDir(1, 0), _cornerHalfSize, _cornerHalfSize, _cornerHalfSize, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // inner border
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerInner, -_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerInner, -_connectInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerInner, -_connectInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerInner, -_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_connectInner, -_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_connectInner, +_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerInner, +_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerInner, +_connectInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerInner, +_connectInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerInner, +_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_connectInner, +_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_connectInner, -_cornerInner));
        arena.PathStroke(true, ArenaColor.Border);

        // outer border
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerOuter, -_cornerOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerInner, -_cornerOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerInner, -_connectOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerInner, -_connectOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerInner, -_cornerOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerOuter, -_cornerOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerOuter, -_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_connectOuter, -_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_connectOuter, +_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerOuter, +_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerOuter, +_cornerOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerInner, +_cornerOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(+_cornerInner, +_connectOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerInner, +_connectOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerInner, +_cornerOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerOuter, +_cornerOuter));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerOuter, +_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_connectOuter, +_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_connectOuter, -_cornerInner));
        arena.PathLineTo(module.Bounds.Center + new WDir(-_cornerOuter, -_cornerInner));
        arena.PathStroke(true, ArenaColor.Border);
    }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
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
