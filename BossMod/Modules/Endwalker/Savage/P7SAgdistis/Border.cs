using System;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    // TODO: consider setting up 'real' bounds to match the borders
    // TODO: consider showing some aoe zone outside bounds?..
    class Border : BossComponent
    {
        private bool _threePlatforms;
        private bool _bridgeN;
        private bool _bridgeE;
        private bool _bridgeW;
        private bool _bridgeCenter;

        public const float LargePlatformRadius = 20;
        public const float SmallPlatformRadius = 10;
        public const float SmallPlatformOffset = 16.5f;
        public const float BridgeHalfWidth = 4;
        public static WDir PlatformSOffset { get; } = SmallPlatformOffset * new WDir(0, 1);
        public static WDir PlatformEOffset { get; } = SmallPlatformOffset * 120.Degrees().ToDirection();
        public static WDir PlatformWOffset { get; } = SmallPlatformOffset * (-120.Degrees()).ToDirection();
        public static float BridgeStartOffset { get; } = MathF.Sqrt(SmallPlatformRadius * SmallPlatformRadius - BridgeHalfWidth * BridgeHalfWidth);
        public static float BridgeCenterOffset { get; } = BridgeHalfWidth / 60.Degrees().Tan();

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!_threePlatforms)
            {
                arena.AddCircle(module.Bounds.Center, LargePlatformRadius, ArenaColor.Border);
            }
            else
            {
                var cs = module.Bounds.Center + PlatformSOffset;
                var ce = module.Bounds.Center + PlatformEOffset;
                var cw = module.Bounds.Center + PlatformWOffset;
                arena.AddCircle(cs, SmallPlatformRadius, ArenaColor.Border);
                arena.AddCircle(ce, SmallPlatformRadius, ArenaColor.Border);
                arena.AddCircle(cw, SmallPlatformRadius, ArenaColor.Border);
                if (_bridgeN)
                {
                    DrawBridge(arena, ce, cw, false);
                }
                if (_bridgeE)
                {
                    DrawBridge(arena, ce, cs, false);
                }
                if (_bridgeW)
                {
                    DrawBridge(arena, cw, cs, false);
                }
                if (_bridgeCenter)
                {
                    DrawBridge(arena, cs, module.Bounds.Center, true);
                    DrawBridge(arena, ce, module.Bounds.Center, true);
                    DrawBridge(arena, cw, module.Bounds.Center, true);
                }
            }
        }

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID != 0x800375A7)
                return;
            switch (index)
            {
                case 0: // small platforms
                    switch (state)
                    {
                        case 0x00020001: // this is preparation
                            _threePlatforms = true;
                            break;
                        case 0x02000100: // this is preparation
                            _threePlatforms = false;
                            break;
                        // 0x00200010 - large platform disappears?
                        // 0x00800040 - small platforms appear?
                        // 0x08000004 - small platforms disappear?
                    }
                    break;
                case 1: // bridge N
                    BridgeEnvControl(ref _bridgeN, state);
                    break;
                case 2: // bridge ?
                    BridgeEnvControl(ref _bridgeE, state);
                    break;
                case 3: // bridge ?
                    BridgeEnvControl(ref _bridgeW, state);
                    break;
                case 6: // bridge center
                    BridgeEnvControl(ref _bridgeCenter, state);
                    break;
            }
        }

        private void DrawBridge(MiniArena arena, WPos p1, WPos p2, bool p2center)
        {
            var dir = (p2 - p1).Normalized();
            var p1adj = p1 + dir * BridgeStartOffset;
            var p2adj = p2 - dir * (p2center ? BridgeCenterOffset : BridgeStartOffset);
            var ortho = dir.OrthoL() * BridgeHalfWidth;
            arena.AddLine(p1adj + ortho, p2adj + ortho, ArenaColor.Border);
            arena.AddLine(p1adj - ortho, p2adj - ortho, ArenaColor.Border);
        }

        private void BridgeEnvControl(ref bool bridge, uint state)
        {
            switch (state)
            {
                case 0x00020001: // bridge appears
                    bridge = true;
                    break;
                // 0x00200010: // bridge starts to disappear
                case 0x00800004: // bridge disappears
                    bridge = false;
                    break;
            }
        }
    }
}
