using BossMod.Autorotation;
using DalaMock.Host.Mediator;
using Dalamud.Utility;

namespace BossMod;

public class ZoneModuleWindow : WindowBase
{
    private readonly ZoneModuleManager _zmm;
    private readonly RotationModuleManager _rmm;
    private readonly MediatorService mediator;
    private bool _wasOpen;

    public ZoneModuleWindow(ZoneModuleManager zmm, RotationModuleManager rmm, MediatorService mediator) : base("Zone module###Zone module", new(400, 400))
    {
        _zmm = zmm;
        _rmm = rmm;
        this.mediator = mediator;
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
        _zmm.ActiveModule?.DrawWindow(mediator, _rmm);
    }
}
