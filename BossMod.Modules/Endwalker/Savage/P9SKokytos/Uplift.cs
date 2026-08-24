namespace BossMod.Endwalker.Savage.P9SKokytos;

class Uplift(BossModule module) : Components.StandardAOEs(module, AID.Uplift, new AOEShapeRect(4, 8))
{
    public Angle? WallDirection { get; private set; }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (WallDirection != null)
        {
            for (int i = 0; i < 4; ++i)
            {
                var center = WallDirection.Value + i * 90.Degrees();
                Arena.PathArcTo(Module.Center, Module.Bounds.Radius - 0.5f, (center - 22.5f.Degrees()).Rad, (center + 22.5f.Degrees()).Rad);
                Arena.PathStroke(false, ArenaColor.Border, 2);
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // state 00080004 => remove walls
        if (index is 2 or 3 && state == 0x00020001)
            WallDirection = (index == 2 ? 0 : 45).Degrees();
    }
}
