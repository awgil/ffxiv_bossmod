namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: consider setting up 'real' bounds to match the borders
// TODO: consider showing some aoe zone outside bounds?..
class Border(BossModule module) : BossComponent(module)
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
    public static readonly WDir PlatformSOffset = SmallPlatformOffset * new WDir(0, 1);
    public static readonly WDir PlatformEOffset = SmallPlatformOffset * 120.Degrees().ToDirection();
    public static readonly WDir PlatformWOffset = SmallPlatformOffset * (-120.Degrees()).ToDirection();
    public static readonly float BridgeStartOffset = MathF.Sqrt(SmallPlatformRadius * SmallPlatformRadius - BridgeHalfWidth * BridgeHalfWidth);
    public static readonly float BridgeCenterOffset = BridgeHalfWidth / 60.Degrees().Tan();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_threePlatforms)
        {
            Arena.AddCircle(Module.Center, LargePlatformRadius, ArenaColor.Border);
        }
        else
        {
            var cs = Module.Center + PlatformSOffset;
            var ce = Module.Center + PlatformEOffset;
            var cw = Module.Center + PlatformWOffset;
            Arena.AddCircle(cs, SmallPlatformRadius, ArenaColor.Border);
            Arena.AddCircle(ce, SmallPlatformRadius, ArenaColor.Border);
            Arena.AddCircle(cw, SmallPlatformRadius, ArenaColor.Border);
            if (_bridgeN)
            {
                DrawBridge(ce, cw, false);
            }
            if (_bridgeE)
            {
                DrawBridge(ce, cs, false);
            }
            if (_bridgeW)
            {
                DrawBridge(cw, cs, false);
            }
            if (_bridgeCenter)
            {
                DrawBridge(cs, Module.Center, true);
                DrawBridge(ce, Module.Center, true);
                DrawBridge(cw, Module.Center, true);
            }
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        switch (index)
        {
            case 0: // small platforms
                switch (state)
                {
                    // 0x00200010 - large platform disappears?
                    // 0x00800040 - small platforms appear?
                    // 0x08000004 - small platforms disappear?
                    case 0x00020001: // this is preparation
                        _threePlatforms = true;
                        break;
                    case 0x02000100: // this is preparation
                        _threePlatforms = false;
                        break;
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

    private void DrawBridge(WPos p1, WPos p2, bool p2center)
    {
        var dir = (p2 - p1).Normalized();
        var p1adj = p1 + dir * BridgeStartOffset;
        var p2adj = p2 - dir * (p2center ? BridgeCenterOffset : BridgeStartOffset);
        var ortho = dir.OrthoL() * BridgeHalfWidth;
        Arena.AddLine(p1adj + ortho, p2adj + ortho, ArenaColor.Border);
        Arena.AddLine(p1adj - ortho, p2adj - ortho, ArenaColor.Border);
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
