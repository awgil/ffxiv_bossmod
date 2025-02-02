namespace BossMod;

public class ZoneModuleWindow : UIWindow
{
    private readonly ZoneModuleManager _zmm;

    public ZoneModuleWindow(ZoneModuleManager zmm) : base("Zone module###Zone module", false, new(400, 400))
    {
        _zmm = zmm;
        RespectCloseHotkey = false;
    }

    public override void PreOpenCheck()
    {
        IsOpen = _zmm.ActiveModule?.WantWindow ?? false;
    }

    public override void Draw()
    {
        _zmm.ActiveModule?.DrawExtra();
    }
}

