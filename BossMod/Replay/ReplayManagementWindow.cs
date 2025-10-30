using BossMod.Autorotation;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
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
    private bool _recordingManual; // recording was started manually, and so should not be stopped automatically
    private bool _recordingDuty; // recording was started automatically because we've entered duty
    private int _recordingActiveModules; // recording was started automatically, because we've activated N modules
    private string _lastErrorMessage = "";

    private const string _windowID = "###Replay recorder";

    public ReplayManagementWindow(WorldState ws, BossModuleManager bmm, RotationDatabase rotationDB, DirectoryInfo logDir) : base(_windowID, false, new(300, 200))
    {
        _ws = ws;
        _logDir = logDir;
        _config = Service.Config.Get<ReplayManagementConfig>();
        _manager = new(rotationDB, logDir.FullName);
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() => IsOpen = _config.ShowUI),
            _ws.CurrentZoneChanged.Subscribe(op => OnZoneChange(op.CFCID)),
            bmm.ModuleActivated.Subscribe(OnModuleActivation),
            bmm.ModuleDeactivated.Subscribe(OnModuleDeactivation)
        );

        if (!OnZoneChange(_ws.CurrentCFCID))
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
            {
                _recordingManual = true;
                StartRecording("");
            }
            else
            {
                StopRecording();
            }
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

    public bool IsRecording() => _recorder != null;

    public override void OnClose()
    {
        SetVisible(false);
    }

    private void UpdateTitle() => WindowName = $"Replay recording: {(_recorder != null ? "in progress..." : "idle")}{_windowID}";

    public bool ShouldAutoRecord => _config.AutoRecord && (_config.AutoARR || !Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.DutyRecorderPlayback]);

    private bool OnZoneChange(uint cfcId)
    {
        if (!ShouldAutoRecord || _recordingManual)
            return false; // don't care

        var isDuty = cfcId != 0;
        if (_recordingDuty == isDuty)
            return false; // don't care
        _recordingDuty = isDuty;

        if (isDuty && !IsRecording())
        {
            StartRecording("");
            return true;
        }

        if (!isDuty && _recordingActiveModules <= 0 && IsRecording())
        {
            StopRecording();
            return true;
        }

        return false;
    }

    private void OnModuleActivation(BossModule m)
    {
        if (!ShouldAutoRecord || _recordingManual)
            return; // don't care

        ++_recordingActiveModules;
        if (!IsRecording())
            StartRecording($"{m.GetType().Name}-");
    }

    private void OnModuleDeactivation(BossModule m)
    {
        if (!ShouldAutoRecord || _recordingManual || _recordingActiveModules <= 0)
            return; // don't care

        --_recordingActiveModules;
        if (_recordingActiveModules <= 0 && !_recordingDuty && IsRecording())
            StopRecording();
    }

    private void StartRecording(string prefix)
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
            _recorder = new(_ws, _config.WorldLogFormat, true, _logDir, prefix + GetPrefix());
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to start recording: {ex}");
        }

        UpdateTitle();
    }

    private void StopRecording()
    {
        _recordingManual = false;
        _recordingDuty = false;
        _recordingActiveModules = 0;
        _recorder?.Dispose();
        _recorder = null;
        UpdateTitle();
    }

    private unsafe string GetPrefix()
    {
        string? prefix = null;
        if (_ws.CurrentCFCID != 0)
            prefix = Service.LuminaRow<ContentFinderCondition>(_ws.CurrentCFCID)?.Name.ToString();
        if (_ws.CurrentZone != 0)
            prefix ??= Service.LuminaRow<TerritoryType>(_ws.CurrentZone)?.PlaceName.ValueNullable?.NameNoArticle.ToString();
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
