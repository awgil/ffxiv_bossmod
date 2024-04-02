using Dalamud.Interface;
using ImGuiNET;

namespace BossMod;

public class BossModuleMainWindow : UIWindow
{
    private BossModuleManager _mgr;

    private static string _windowID = "###Boss module";

    public BossModuleMainWindow(BossModuleManager mgr) : base(_windowID, false, new(400, 400))
    {
        _mgr = mgr;
        RespectCloseHotkey = false;
        TitleBarButtons.Add(new() { Icon = FontAwesomeIcon.Cog, IconOffset = new(1), Click = _ => OpenModuleConfig() });
    }

    public override void PreOpenCheck()
    {
        IsOpen = _mgr.LoadedModules.Count > 0;
        ShowCloseButton = _mgr.ActiveModule != null;
        WindowName = (_mgr.ActiveModule != null ? $"Boss module ({_mgr.ActiveModule.GetType().Name})" : "Loaded boss modules") + _windowID;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (_mgr.WindowConfig.TrishaMode)
            Flags |= ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;
        if (_mgr.WindowConfig.Lock)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
        ForceMainWindow = _mgr.WindowConfig.TrishaMode; // NoBackground flag without ForceMainWindow works incorrectly for whatever reason
    }

    public override void OnOpen()
    {
        Service.Log($"[BMM] Opening main window; there are {_mgr.LoadedModules.Count} loaded modules, active is {_mgr.ActiveModule?.GetType().FullName ?? "<n/a>"}");
    }

    public override void OnClose()
    {
        Service.Log($"[BMM] Closing main window; there are {_mgr.LoadedModules.Count} loaded modules, active is {_mgr.ActiveModule?.GetType().FullName ?? "<n/a>"}");
    }

    public override void PostDraw()
    {
        if (!IsOpen)
        {
            // user pressed close button - deactivate current module and show module list instead
            // show module list instead of boss module
            Service.Log("[BMM] Bossmod window closed by user, showing module list instead...");
            _mgr.ActiveModule = null;
            IsOpen = true;
        }
    }

    public override void Draw()
    {
        if (_mgr.ActiveModule != null)
        {
            try
            {
                BossComponent.MovementHints? movementHints = _mgr.WindowConfig.ShowWorldArrows ? new() : null;
                _mgr.ActiveModule.Draw(_mgr.WindowConfig.RotateArena ? (Camera.Instance?.CameraAzimuth ?? 0) : 0, PartyState.PlayerSlot, movementHints, !_mgr.WindowConfig.HintsInSeparateWindow, true);
                DrawMovementHints(movementHints, _mgr.WorldState.Party.Player()?.PosRot.Y ?? 0);
            }
            catch (Exception ex)
            {
                Service.Log($"Boss module draw crashed: {ex}");
                _mgr.ActiveModule = null;
            }
        }
        else
        {
            foreach (var m in _mgr.LoadedModules)
            {
                var oidType = ModuleRegistry.FindByOID(m.PrimaryActor.OID)?.ObjectIDType;
                var oidName = oidType?.GetEnumName(m.PrimaryActor.OID);
                if (ImGui.Button($"{m.GetType()} ({m.PrimaryActor.InstanceID:X} '{m.PrimaryActor.Name}' {oidName})"))
                    _mgr.ActiveModule = m;
            }
        }
    }

    private void DrawMovementHints(BossComponent.MovementHints? arrows, float y)
    {
        if (arrows == null || arrows.Count == 0 || Camera.Instance == null)
            return;

        foreach ((var start, var end, uint color) in arrows)
        {
            Vector3 start3 = new(start.X, y, start.Z);
            Vector3 end3 = new(end.X, y, end.Z);
            Camera.Instance.DrawWorldLine(start3, end3, color);
            var dir = Vector3.Normalize(end3 - start3);
            var arrowStart = end3 - 0.4f * dir;
            var offset = 0.07f * Vector3.Normalize(Vector3.Cross(Vector3.UnitY, dir));
            Camera.Instance.DrawWorldLine(arrowStart + offset, end3, color);
            Camera.Instance.DrawWorldLine(arrowStart - offset, end3, color);
        }
    }

    private void OpenModuleConfig()
    {
        if (_mgr.ActiveModule?.Info?.ConfigType != null)
            new BossModuleConfigWindow(_mgr.ActiveModule.Info, _mgr.WorldState);
    }
}
