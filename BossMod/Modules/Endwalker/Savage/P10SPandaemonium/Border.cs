namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class Border(BossModule module) : BossComponent(module)
{
    public bool LBridgeActive { get; private set; }
    public bool RBridgeActive { get; private set; }

    public const float MainPlatformCenterZ = 100;
    public static readonly WDir MainPlatformHalfSize = new(13, 15);
    public const float SidePlatformOffsetX = 25;
    public const float SidePlayformCenterZ = 85;
    public static readonly WDir SidePlatformHalfSize = new(4, 15);
    public const float BridgeHalfWidth = 1;

    public static bool InsideMainPlatform(BossModule module, WPos pos) => pos.InRect(new(module.Center.X, MainPlatformCenterZ), new WDir(1, 0), MainPlatformHalfSize.X, MainPlatformHalfSize.X, MainPlatformHalfSize.Z);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        DrawPlatform(new(Module.Center.X, MainPlatformCenterZ), MainPlatformHalfSize);
        DrawPlatform(new(Module.Center.X + SidePlatformOffsetX, SidePlayformCenterZ), SidePlatformHalfSize);
        DrawPlatform(new(Module.Center.X - SidePlatformOffsetX, SidePlayformCenterZ), SidePlatformHalfSize);
        if (LBridgeActive)
            DrawBridge(-1);
        if (RBridgeActive)
            DrawBridge(+1);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state is 0x00020001 or 0x00080004)
        {
            switch (index)
            {
                case 2: RBridgeActive = state == 0x00020001; break;
                case 3: LBridgeActive = state == 0x00020001; break;
            }
        }
    }

    private void DrawPlatform(WPos center, WDir cornerOffset)
    {
        var otherCorner = new WDir(cornerOffset.X, -cornerOffset.Z);
        Arena.AddQuad(center - cornerOffset, center - otherCorner, center + cornerOffset, center + otherCorner, ArenaColor.Border);
    }

    private void DrawBridge(float dir)
    {
        var p1 = Module.Center + new WDir(MainPlatformHalfSize.X * dir, 0);
        var p2 = Module.Center + new WDir((SidePlatformOffsetX - SidePlatformHalfSize.X) * dir, 0);
        var o = new WDir(0, 1);
        Arena.AddLine(p1 + o, p2 + o, ArenaColor.Border);
        Arena.AddLine(p1 - o, p2 - o, ArenaColor.Border);
    }
}
