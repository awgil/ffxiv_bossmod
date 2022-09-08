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

        private const float _largePlatformRadius = 20;
        private const float _smallPlatformRadius = 10;
        private const float _smallPlatformOffset = 16.5f;
        private const float _bridgeHalfWidth = 4;
        private static WDir _platformSOffset = _smallPlatformOffset * new WDir(0, 1);
        private static WDir _platformEOffset = _smallPlatformOffset * 120.Degrees().ToDirection();
        private static WDir _platformWOffset = _smallPlatformOffset * (-120.Degrees()).ToDirection();
        private static float _bridgeStartOffset = MathF.Sqrt(_smallPlatformRadius * _smallPlatformRadius - _bridgeHalfWidth * _bridgeHalfWidth);
        private static float _bridgeCenterOffset = _bridgeHalfWidth / 60.Degrees().Tan();

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!_threePlatforms)
            {
                arena.AddCircle(module.Bounds.Center, _largePlatformRadius, ArenaColor.Border);
            }
            else
            {
                var cs = module.Bounds.Center + _platformSOffset;
                var ce = module.Bounds.Center + _platformEOffset;
                var cw = module.Bounds.Center + _platformWOffset;
                arena.AddCircle(cs, _smallPlatformRadius, ArenaColor.Border);
                arena.AddCircle(ce, _smallPlatformRadius, ArenaColor.Border);
                arena.AddCircle(cw, _smallPlatformRadius, ArenaColor.Border);
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
            var p1adj = p1 + dir * _bridgeStartOffset;
            var p2adj = p2 - dir * (p2center ? _bridgeCenterOffset : _bridgeStartOffset);
            var ortho = dir.OrthoL() * _bridgeHalfWidth;
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
