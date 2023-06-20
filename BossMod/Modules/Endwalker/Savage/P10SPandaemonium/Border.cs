namespace BossMod.Endwalker.Savage.P10SPandaemonium
{
    class Border : BossComponent
    {
        public bool LBridgeActive { get; private set; }
        public bool RBridgeActive { get; private set; }

        public static float MainPlatformCenterZ = 100;
        public static WDir MainPlatformHalfSize = new(13, 15);
        public static float SidePlatformOffsetX = 25;
        public static float SidePlayformCenterZ = 85;
        public static WDir SidePlatformHalfSize = new(4, 15);
        public static float BridgeHalfWidth = 1;

        public static bool InsideMainPlatform(BossModule module, WPos pos) => pos.InRect(new(module.Bounds.Center.X, MainPlatformCenterZ), new WDir(1, 0), MainPlatformHalfSize.X, MainPlatformHalfSize.X, MainPlatformHalfSize.Z);

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            DrawPlatform(arena, new(module.Bounds.Center.X, MainPlatformCenterZ), MainPlatformHalfSize);
            DrawPlatform(arena, new(module.Bounds.Center.X + SidePlatformOffsetX, SidePlayformCenterZ), SidePlatformHalfSize);
            DrawPlatform(arena, new(module.Bounds.Center.X - SidePlatformOffsetX, SidePlayformCenterZ), SidePlatformHalfSize);
            if (LBridgeActive)
                DrawBridge(arena, -1);
            if (RBridgeActive)
                DrawBridge(arena, +1);
        }

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID == 0x800375B1 && state is 0x00020001 or 0x00080004)
            {
                switch (index)
                {
                    case 2: RBridgeActive = state == 0x00020001; break;
                    case 3: LBridgeActive = state == 0x00020001; break;
                }
            }
        }

        private void DrawPlatform(MiniArena arena, WPos center, WDir cornerOffset)
        {
            var otherCorner = new WDir(cornerOffset.X, -cornerOffset.Z);
            arena.AddQuad(center - cornerOffset, center - otherCorner, center + cornerOffset, center + otherCorner, ArenaColor.Border);
        }

        private void DrawBridge(MiniArena arena, float dir)
        {
            var p1 = arena.Bounds.Center + new WDir(MainPlatformHalfSize.X * dir, 0);
            var p2 = arena.Bounds.Center + new WDir((SidePlatformOffsetX - SidePlatformHalfSize.X) * dir, 0);
            var o = new WDir(0, 1);
            arena.AddLine(p1 + o, p2 + o, ArenaColor.Border);
            arena.AddLine(p1 - o, p2 - o, ArenaColor.Border);
        }
    }
}
