using ImGuiNET;

namespace BossMod;

public class BossModulePlanWindow : UIWindow
{
    private readonly BossModuleManager _mgr;

    public BossModulePlanWindow(BossModuleManager mgr) : base("Cooldown plan", false, new(400, 400))
    {
        _mgr = mgr;
        ShowCloseButton = false;
        RespectCloseHotkey = false;
    }

    public override void PreOpenCheck()
    {
        IsOpen = _mgr.Config.EnableTimerWindow && _mgr.ActiveModule?.PlanConfig != null;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (_mgr.Config.Lock)
            Flags |= ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        if (_mgr.ActiveModule?.PlanExecution == null)
            return;

        if (ImGui.Button("Show timeline"))
        {
            _ = new StateMachineWindow(_mgr.ActiveModule);
        }
        ImGui.SameLine();
        _mgr.ActiveModule.PlanConfig?.DrawSelectionUI(_mgr.ActiveModule.Raid.Player()?.Class ?? Class.None, _mgr.ActiveModule.StateMachine, _mgr.ActiveModule.Info);

        _mgr.ActiveModule.PlanExecution?.Draw(_mgr.ActiveModule.StateMachine); // note: null check again, since plan could've been just deleted
    }
}
