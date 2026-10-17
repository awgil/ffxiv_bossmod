using BossMod.Autorotation;
using Dalamud.Utility;

namespace BossMod;

public class ZoneModuleWindow : UIWindow
{
    private readonly ZoneModuleManager _zmm;
    private readonly RotationModuleManager _rmm;
    private bool _wasOpen;

    public ZoneModuleWindow(ZoneModuleManager zmm, RotationModuleManager rmm) : base("Zone module###Zone module", false, new(400, 400))
    {
        _zmm = zmm;
        _rmm = rmm;
        RespectCloseHotkey = false;
    }

    public override void PreOpenCheck()
    {
        var desc = _zmm.ActiveModule?.GetWindowDescription();
        IsOpen = desc != null;
        _wasOpen = IsOpen;
        if (desc is { } d)
        {
            var title = d.Name;
            if (title.IsNullOrEmpty())
                title = "Zone module###Zone module";
            WindowName = title;
            Flags = d.Flags;
        }
    }

    public override void PostDraw()
    {
        // user closed window
        if (_wasOpen && !IsOpen)
            _zmm.ActiveModule?.OnWindowClose();
    }

    public override void Draw()
    {
        _zmm.ActiveModule?.DrawWindow(_rmm);
    }
}
