using BossMod.Autorotation;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Diagnostics;
using System.IO;

namespace BossMod;

public class ReplayManagementWindow : UIWindow
{
    private readonly WorldState _ws;
    private readonly DirectoryInfo _logDir;
    private readonly ReplayManagementConfig _config;
    private readonly ReplayManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private ReplayRecorder? _recorder;
    private string _message = "";
    private bool _autoRecording;
    private string _lastErrorMessage = "";

    private const string _windowID = "###Replay recorder";

    public ReplayManagementWindow(WorldState ws, RotationDatabase rotationDB, DirectoryInfo logDir) : base(_windowID, false, new(300, 200))
    {
        _ws = ws;
        _logDir = logDir;
        _config = Service.Config.Get<ReplayManagementConfig>();
        _manager = new(rotationDB, logDir.FullName);
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() => IsOpen = _config.ShowUI),
            _ws.CurrentZoneChanged.Subscribe(op => UpdateAutoRecord(op.CFCID))
        );

        if (!UpdateAutoRecord(_ws.CurrentCFCID))
            UpdateTitle();

        RespectCloseHotkey = false;
    }

    protected override void Dispose(bool disposing)
    {
        _recorder?.Dispose();
        _subscriptions.Dispose();
        _manager.Dispose();
        base.Dispose(disposing);
    }

    public void SetVisible(bool vis)
    {
        if (_config.ShowUI != vis)
        {
            _config.ShowUI = vis;
            _config.Modified.Fire();
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
                _ws.Execute(new WorldState.OpUserMarker(_message));
                _message = "";
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Open Replay Folder") && _logDir != null)
            _lastErrorMessage = OpenDirectory(_logDir);

        if (_lastErrorMessage.Length > 0)
        {
            ImGui.SameLine();
            using var color = ImRaii.PushColor(ImGuiCol.Text, 0xff0000ff);
            ImGui.TextUnformatted(_lastErrorMessage);
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

    private void UpdateTitle() => WindowName = $"Replay recording: {(_recorder != null ? "in progress..." : "idle")}{_windowID}";

    private bool UpdateAutoRecord(uint cfcId)
    {
        if (!_config.AutoRecord)
            return false; // don't care

        if (!IsRecording() && _config.AutoRecord && cfcId != 0)
        {
            StartRecording();
            _autoRecording = true;
            return true;
        }

        if (IsRecording() && _autoRecording && cfcId == 0)
        {
            StopRecording();
            _autoRecording = false;
            return true;
        }

        return false;
    }

    private unsafe string GetPrefix()
    {
        string? prefix = null;
        if (_ws.CurrentCFCID != 0)
            prefix = Service.LuminaRow<ContentFinderCondition>(_ws.CurrentCFCID)?.Name.ToString();
        if (_ws.CurrentZone != 0)
            prefix ??= Service.LuminaRow<TerritoryType>(_ws.CurrentZone)?.PlaceName.Value?.NameNoArticle.ToString();
        prefix ??= "World";
        prefix = Utils.StringToIdentifier(prefix);

        var player = _ws.Party.Player();
        if (player != null)
            prefix += $"_{player.Class}{player.Level}_{player.Name.Replace(" ", null, StringComparison.Ordinal)}";

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

    private string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return $"Directory '{dir}' not found.";

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"Failed to open folder '{dir}', open it manually.";
        }
    }
}
