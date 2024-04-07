using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.IO;

namespace BossMod;

public class ReplayManagementWindow : UIWindow
{
    private WorldState _ws;
    private DirectoryInfo _logDir;
    private ReplayManagementConfig _config;
    private ReplayManager _manager;
    private ReplayRecorder? _recorder;
    private string _message = "";

    private static string _windowID = "###Replay recorder";

    public ReplayManagementWindow(WorldState ws, DirectoryInfo logDir) : base(_windowID, false, new(300, 200))
    {
        _ws = ws;
        _logDir = logDir;
        _config = Service.Config.Get<ReplayManagementConfig>();
        _manager = new(logDir.FullName);

        _ws.CurrentZoneChanged += OnZoneChanged;
        _config.Modified += OnConfigChanged;
        if (!UpdateAutoRecord(_ws.CurrentCFCID))
            UpdateTitle();

        RespectCloseHotkey = false;
        IsOpen = _config.ShowUI;
    }

    protected override void Dispose(bool disposing)
    {
        _config.Modified -= OnConfigChanged;
        _ws.CurrentZoneChanged -= OnZoneChanged;
        _recorder?.Dispose();
        _manager.Dispose();
    }

    public void SetVisible(bool vis)
    {
        if (_config.ShowUI != vis)
        {
            _config.ShowUI = vis;
            _config.NotifyModified();
        }
    }

    public override void PreOpenCheck()
    {
        _manager.Update();
    }

    public override void Draw()
    {
        if (ImGui.Button(!IsRecording() ? "Start recording" : "Stop recording"))
        {
            if (!IsRecording())
                StartRecording();
            else
                StopRecording();
        }

        if (_recorder != null)
        {
            ImGui.InputText("###msg", ref _message, 1024);
            ImGui.SameLine();
            if (ImGui.Button("Add log marker") && _message.Length > 0)
            {
                _ws.Execute(new WorldState.OpUserMarker() { Text = _message });
                _message = "";
            }
        }

        ImGui.Separator();
        _manager.Draw();
    }

    public void StartRecording()
    {
        if (IsRecording())
            return; // already recording

        // if there are too many replays, delete oldest
        if (_config.MaxReplays > 0)
        {
            try
            {
                var replays = _logDir.GetFiles();
                replays.SortBy(f => f.LastWriteTime);
                foreach (var f in replays.Take(replays.Length - _config.MaxReplays))
                    f.Delete();
            }
            catch (Exception ex)
            {
                Service.Log($"Failed to delete old replays: {ex}");
            }
        }

        try
        {
            _recorder = new(_ws, _config.WorldLogFormat, true, _logDir, GetPrefix());
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to start recording: {ex}");
        }

        UpdateTitle();
    }

    public void StopRecording()
    {
        _recorder?.Dispose();
        _recorder = null;
        UpdateTitle();
    }

    public bool IsRecording() => _recorder != null;

    public override void OnClose()
    {
        SetVisible(false);
    }

    private void UpdateTitle() =>  WindowName = $"Replay recording: {(_recorder != null ? "in progress..." : "idle")}{_windowID}";

    private bool UpdateAutoRecord(uint cfcId)
    {
        if (!IsRecording() && _config.AutoRecord && cfcId != 0)
        {
            StartRecording();
            return true;
        }

        if (IsRecording() && _config.AutoStop && cfcId == 0)
        {
            StopRecording();
            return true;
        }

        return false;
    }

    private void OnConfigChanged() => IsOpen = _config.ShowUI;
    private void OnZoneChanged(WorldState.OpZoneChange op) => UpdateAutoRecord(op.CFCID);

    private unsafe string GetPrefix()
    {
        string? prefix = null;
        if (_ws.CurrentCFCID != 0)
            prefix ??= Service.LuminaRow<ContentFinderCondition>(_ws.CurrentCFCID)?.Name.ToString();
        if (_ws.CurrentZone != 0)
            prefix ??= Service.LuminaRow<TerritoryType>(_ws.CurrentZone)?.PlaceName.Value?.NameNoArticle.ToString();
        prefix ??= "World";
        prefix = Utils.StringToIdentifier(prefix);

        var player = _ws.Party.Player();
        if (player != null)
            prefix += $"_{player.Class}{player.Level}_{player.Name.Replace(" ", null)}";

        var cf = FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder.Instance();
        if (cf->IsUnrestrictedParty)
            prefix += "_U";
        if (cf->IsLevelSync)
            prefix += "_LS";
        if (cf->IsMinimalIL)
            prefix += "_MI";
        if (cf->IsSilenceEcho)
            prefix += "_NE";

        return prefix;
    }
}
