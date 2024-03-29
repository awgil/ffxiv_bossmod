using ImGuiNET;

namespace BossMod;

public class BossModuleHintsWindow : UIWindow
{
    private BossModuleManager _mgr;

    public BossModuleHintsWindow(BossModuleManager mgr) : base("Boss module hints", false, new(400, 100))
    {
        _mgr = mgr;
        RespectCloseHotkey = false;
    }

    public override void PreOpenCheck()
    {
        IsOpen = _mgr.WindowConfig.HintsInSeparateWindow && _mgr.ActiveModule != null;
        Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (_mgr.WindowConfig.Lock)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
    }

    public override void Draw()
    {
        try
        {
            _mgr.ActiveModule?.Draw(0, PartyState.PlayerSlot, null, true, false);
        }
        catch (Exception ex)
        {
            Service.Log($"Boss module draw-hints crashed: {ex}");
            _mgr.ActiveModule = null;
        }
    }
}
