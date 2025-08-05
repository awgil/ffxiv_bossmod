using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using static BossMod.BossModuleConfig;

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
            OnManualClose(_mgr.Config.CloseBehavior);
            IsOpen = true;
        }
    }

    private void OnManualClose(RadarCloseBehavior beh)
    {
        switch (beh)
        {
            case RadarCloseBehavior.Prompt:
                _showClosePopup = true;
                break;

            case RadarCloseBehavior.DisableRadar:
                _mgr.Config.Enable = false;
                break;

            case RadarCloseBehavior.DisableActiveModule:
                if (_mgr.ActiveModule?.Info is { } info)
                {
                    _mgr.Config.DisabledModules.Add(info.ModuleType.ToString());
                    _mgr.ActiveModule = null;
                }
                break;

            case RadarCloseBehavior.DisableActiveModuleCategory:
                if (_mgr.ActiveModule?.Info is { } i)
                {
                    _mgr.Config.DisabledCategories.Add(i.Category);
                    _mgr.ActiveModule = null;
                }
                break;
        }
    }

    private bool _showClosePopup;

    public override void Draw()
    {
        using (var popup = ImRaii.PopupModal("Radar close behavior##radar_close"))
            if (popup)
                DrawRadarClosePopup();

        if (_showClosePopup)
        {
            _showClosePopup = false;
            ImGui.OpenPopup("Radar close behavior##radar_close");
        }

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

    private bool _rememberCloseChoice;
    private RadarCloseBehavior _beh;

    private void DrawRadarClosePopup()
    {
        ImGui.Text("What would you like to do?");

        if (ImGui.RadioButton("Hide the radar window", _beh == RadarCloseBehavior.DisableRadar))
            _beh = RadarCloseBehavior.DisableRadar;
        if (ImGui.RadioButton($"Disable the current module (currently: {_mgr.ActiveModule})", _beh == RadarCloseBehavior.DisableActiveModule))
            _beh = RadarCloseBehavior.DisableActiveModule;
        if (ImGui.RadioButton($"Disable the current category (currently: {_mgr.ActiveModule?.Info?.Category.ToString() ?? "unknown"})", _beh == RadarCloseBehavior.DisableActiveModuleCategory))
            _beh = RadarCloseBehavior.DisableActiveModuleCategory;

        ImGui.Dummy(new(0, 15));

        ImGui.Checkbox("Remember my choice", ref _rememberCloseChoice);

        ImGui.Dummy(new(0, 15));

        if (ImGui.Button("OK"))
        {
            if (_rememberCloseChoice)
            {
                _mgr.Config.CloseBehavior = _beh;
                _mgr.Config.Modified.Fire();
            }
            ImGui.CloseCurrentPopup();
            OnManualClose(_beh);
            _beh = default;
            _rememberCloseChoice = false;
        }
        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            _beh = default;
            _rememberCloseChoice = false;
            ImGui.CloseCurrentPopup();
        }
    }

    private bool ShowZoneModule() => _mgr.Config.ShowGlobalHints && !_mgr.Config.HintsInSeparateWindow && _mgr.ActiveModule?.StateMachine.ActivePhase == null && (_zmm.ActiveModule?.WantDrawHints() ?? false);
}
