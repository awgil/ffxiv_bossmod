using Dalamud.Utility;

namespace BossMod;

public class ZoneModuleWindow : UIWindow
{
    private readonly ZoneModuleManager _zmm;
    private bool _wasOpen;

    public ZoneModuleWindow(ZoneModuleManager zmm) : base("Zone module###Zone module", false, new(400, 400))
    {
        _zmm = zmm;
        RespectCloseHotkey = false;
    }

    public override void PreOpenCheck()
    {
        IsOpen = _zmm.ActiveModule?.WantDrawExtra() ?? false;
        _wasOpen = IsOpen;
        if (IsOpen)
        {
            var title = _zmm.ActiveModule!.WindowName();
            if (title.IsNullOrEmpty())
                title = "Zone module###Zone module";
            WindowName = title;
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
        _zmm.ActiveModule?.DrawExtra();
    }
}
