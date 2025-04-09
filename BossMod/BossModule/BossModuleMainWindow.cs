using Dalamud.Interface;
using ImGuiNET;

namespace BossMod;

public class BossModuleMainWindow : UIWindow
{
    private readonly BossModuleManager _mgr;
    private readonly ZoneModuleManager _zmm;

    private const string _windowID = "###Boss module";

    public BossModuleMainWindow(BossModuleManager mgr, ZoneModuleManager zmm) : base(_windowID, false, new(400, 400))
    {
        _mgr = mgr;
        _zmm = zmm;
        RespectCloseHotkey = false;
        TitleBarButtons.Add(new() { Icon = FontAwesomeIcon.WindowClose, IconOffset = new(1), Click = _ => OnClose() });
    }

    public override void PreOpenCheck()
    {
        var showZoneModule = ShowZoneModule();
        IsOpen = _mgr.Config.Enable && (_mgr.LoadedModules.Count > 0 || showZoneModule);
        ShowCloseButton = _mgr.ActiveModule != null && !showZoneModule;
        WindowName = (showZoneModule ? $"Zone module ({_zmm.ActiveModule?.GetType().Name})" : _mgr.ActiveModule != null ? $"Boss module ({_mgr.ActiveModule.GetType().Name})" : "Loaded boss modules") + _windowID;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (_mgr.Config.TrishaMode)
            Flags |= ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;
        if (_mgr.Config.Lock)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
        ForceMainWindow = _mgr.Config.TrishaMode; // NoBackground flag without ForceMainWindow works incorrectly for whatever reason

        if (_mgr.Config.ShowWorldArrows && _mgr.ActiveModule != null && _mgr.WorldState.Party[PartyState.PlayerSlot] is var pc && pc != null)
            DrawMovementHints(_mgr.ActiveModule.CalculateMovementHintsForRaidMember(PartyState.PlayerSlot, pc), pc.PosRot.Y);
    }

    public override void OnOpen()
    {
        Service.Log($"[BMM] Opening main window; there are {_mgr.LoadedModules.Count} loaded modules, active is {_mgr.ActiveModule?.GetType().FullName ?? "<n/a>"}; zone module is {_zmm.ActiveModule?.GetType().FullName ?? "<n/a>"}");
    }

    public override void OnClose()
    {
        Service.Log($"[BMM] Closing main window; there are {_mgr.LoadedModules.Count} loaded modules, active is {_mgr.ActiveModule?.GetType().FullName ?? "<n/a>"}; zone module is {_zmm.ActiveModule?.GetType().FullName ?? "<n/a>"}");
    }

    public override void PostDraw()
    {
        if (!IsOpen)
        {
            // user pressed close button
            OnManualClose();
            IsOpen = true;
        }
    }

    private void OnManualClose()
    {
        switch (_mgr.Config.CloseBehavior)
        {
            case BossModuleConfig.RadarCloseBehavior.Prompt:
                Service.Log($"TODO: implement prompt");
                break;

            case BossModuleConfig.RadarCloseBehavior.DisableRadar:
                _mgr.Config.Enable = false;
                break;

            case BossModuleConfig.RadarCloseBehavior.DisableActiveModule:
                Service.Log($"TODO: implement disable single {_mgr.ActiveModule?.Info}");
                break;

            case BossModuleConfig.RadarCloseBehavior.DisableActiveModuleCategory:
                Service.Log($"TODO: implement disable group {_mgr.ActiveModule?.Info}");
                break;
        }
    }

    public override void Draw()
    {
        if (ShowZoneModule())
        {
            _zmm.ActiveModule?.DrawGlobalHints();
        }
        else if (_mgr.ActiveModule != null)
        {
            try
            {
                _mgr.ActiveModule.Draw(_mgr.Config.RotateArena ? _mgr.WorldState.Client.CameraAzimuth : default, PartyState.PlayerSlot, !_mgr.Config.HintsInSeparateWindow, true);
            }
            catch (Exception ex)
            {
                Service.Log($"Boss module draw crashed: {ex}");
                _mgr.ActiveModule = null;
            }
        }
    }

    private void DrawMovementHints(BossComponent.MovementHints? arrows, float y)
    {
        if (arrows == null || arrows.Count == 0 || Camera.Instance == null)
            return;

        foreach ((var start, var end, uint color) in arrows)
        {
            Vector3 start3 = start.ToVec3(y);
            Vector3 end3 = end.ToVec3(y);
            Camera.Instance.DrawWorldLine(start3, end3, color);
            var dir = Vector3.Normalize(end3 - start3);
            var arrowStart = end3 - 0.4f * dir;
            var offset = 0.07f * Vector3.Normalize(Vector3.Cross(Vector3.UnitY, dir));
            Camera.Instance.DrawWorldLine(arrowStart + offset, end3, color);
            Camera.Instance.DrawWorldLine(arrowStart - offset, end3, color);
        }
    }

    private bool ShowZoneModule() => _mgr.Config.ShowGlobalHints && !_mgr.Config.HintsInSeparateWindow && _mgr.ActiveModule?.StateMachine.ActivePhase == null && (_zmm.ActiveModule?.WantDrawHints() ?? false);
}
