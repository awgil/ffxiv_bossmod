using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
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
    private DateTime _recordingStart;
    private string _recordingName = "";

    private bool _showCull;
    private List<ReplayFileMeta>? _cullFiles;
    private float _cullMaxDurationSec = 60;
    private string _cullDutyKey = ""; // empty = any
    private string _cullStatus = "";

    private const string _windowID = "###Replay recorder";

    public ReplayManagementWindow(WorldState ws, BossModuleManager bmm, DirectoryInfo logDir, ReplayManager manager) : base(_windowID, false, new(400, 500))
    {
        _ws = ws;
        _logDir = logDir;
        _config = Service.Config.Get<ReplayManagementConfig>();
        _manager = manager;
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
        DrawRecordingStatus();

        if (UIMisc.ToggleButton(!IsRecording() ? "Start recording###rec" : "Stop recording###rec", IsRecording(), 0xff000080))
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

        UIMisc.SameLineOrWrap(UIMisc.ButtonWidth("Open Replay Folder"));
        if (ImGui.Button("Open Replay Folder") && _logDir != null)
            _lastErrorMessage = OpenDirectory(_logDir);

        if (_lastErrorMessage.Length > 0)
            UIMisc.Banner(FontAwesomeIcon.ExclamationTriangle, _lastErrorMessage, UIMisc.ColorDanger);

        if (_recorder != null)
        {
            ImGui.SetNextItemWidth(MathF.Max(ImGui.GetContentRegionAvail().X - UIMisc.ButtonWidth("Add log marker") - ImGui.GetStyle().ItemSpacing.X, 100));
            ImGui.InputTextWithHint("###msg", "Log marker text, e.g. 'wipe here'", ref _message, 1024);
            ImGui.SameLine();
            if (ImGui.Button("Add log marker") && _message.Length > 0)
            {
                _ws.Execute(new WorldState.OpUserMarker(_message));
                _message = "";
            }
        }

        ImGui.SetNextItemOpen(_showCull);
        _showCull = ImGui.CollapsingHeader("Cull old replays");
        if (_showCull)
            DrawCull();

        ImGui.Separator();
        _manager.Draw();
    }

    private void DrawRecordingStatus()
    {
        if (_recorder == null)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled)))
                UIMisc.IconText(FontAwesomeIcon.Circle);
            ImGui.SameLine();
            ImGui.TextDisabled(_config.AutoRecord ? "Not recording (auto-record is on)" : "Not recording");
            return;
        }

        using (ImRaii.PushColor(ImGuiCol.Text, UIMisc.ColorDanger))
            UIMisc.IconText(FontAwesomeIcon.Circle);
        ImGui.SameLine();
        var elapsed = DateTime.Now - _recordingStart;
        var reason = _recordingManual ? "manual" : _recordingDuty ? "duty" : _recordingActiveModules > 0 ? "boss module" : "auto";
        ImGui.TextUnformatted($"Recording {(int)elapsed.TotalMinutes}:{elapsed.Seconds:D2} ({reason})");
        if (_recordingName.Length > 0)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(_recordingName);
        }
    }

    private void DrawCull()
    {
        _cullFiles ??= ReplayFileMeta.Scan(_logDir, _config.ZoneSubfolders);
        var matching = MatchingCullFiles(_cullFiles);
        var totalBytes = _cullFiles.Sum(f => f.File.Exists ? f.File.Length : 0);
        var hasFilter = _cullMaxDurationSec > 0 || _cullDutyKey.Length > 0;

        if (ImGui.Button("Refresh"))
        {
            _cullFiles = ReplayFileMeta.Scan(_logDir, _config.ZoneSubfolders);
            _cullStatus = "";
        }
        ImGui.SameLine();
        using (ImRaii.Disabled(matching.Count == 0 || !hasFilter))
        {
            if (UIMisc.DangerousButton("Delete matching"))
                DeleteMatching(matching);
        }
        if (!hasFilter && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip("Set a duration threshold and/or select a duty");

        ImGui.Text($"Replays: {_cullFiles.Count} files, {totalBytes / (1024.0 * 1024.0):f1} MiB | Matched: {matching.Count}");

        ImGui.TextUnformatted("Shorter than (seconds)");
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        ImGui.InputFloat("###cullDuration", ref _cullMaxDurationSec);
        if (_cullMaxDurationSec < 0)
            _cullMaxDurationSec = 0;

        var dutyKeys = _cullFiles.GroupBy(f => f.DutyKey).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase).Select(g => (g.Key, Count: g.Count())).ToList();
        if (_cullDutyKey.Length > 0 && dutyKeys.All(d => d.Key != _cullDutyKey))
            _cullDutyKey = "";

        var dutyPreview = _cullDutyKey.Length == 0 ? "(any duty)" : $"{_cullDutyKey} ({dutyKeys.First(d => d.Key == _cullDutyKey).Count})";
        ImGui.TextUnformatted("Duty");
        ImGui.SetNextItemWidth(250 * ImGuiHelpers.GlobalScale);
        using (var combo = ImRaii.Combo("###cullDuty", dutyPreview))
        {
            if (combo)
            {
                if (ImGui.Selectable("(any duty)", _cullDutyKey.Length == 0))
                    _cullDutyKey = "";
                foreach (var d in dutyKeys)
                {
                    if (ImGui.Selectable($"{d.Key} ({d.Count})", _cullDutyKey == d.Key))
                        _cullDutyKey = d.Key;
                }
            }
        }

        if (_cullStatus.Length > 0)
            ImGui.Text(_cullStatus);
    }

    private List<ReplayFileMeta> MatchingCullFiles(List<ReplayFileMeta> files)
    {
        return [.. files.Where(f =>
            (_cullMaxDurationSec <= 0 || f.Duration.TotalSeconds < _cullMaxDurationSec) &&
            (_cullDutyKey.Length == 0 || f.DutyKey == _cullDutyKey))];
    }

    private void DeleteMatching(List<ReplayFileMeta> matching)
    {
        var deleted = new List<string>();
        var failed = 0;
        foreach (var m in matching)
        {
            try
            {
                m.File.Delete();
                deleted.Add(m.File.FullName);
            }
            catch (Exception ex)
            {
                ++failed;
                Service.Log($"Failed to delete replay {m.File.FullName}: {ex}");
            }
        }

        _manager.UnloadPaths(deleted);
        _cullFiles = ReplayFileMeta.Scan(_logDir, _config.ZoneSubfolders);
        _cullStatus = failed == 0 ? $"Deleted {deleted.Count} file(s)." : $"Deleted {deleted.Count} file(s), {failed} failed.";
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

        var (z, p) = GetPrefix();
        var parentDir = _config.ZoneSubfolders ? new(Path.Join(_logDir.FullName, z)) : _logDir;

        // if there are too many replays, delete oldest
        if (_config.MaxReplays > 0)
        {
            try
            {
                var replays = parentDir.GetFiles();
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
            _recorder = new(_ws, _config.WorldLogFormat, true, parentDir, prefix + p, _config.Anonymize);
            _recordingStart = DateTime.Now;
            _recordingName = prefix + p;
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

    private unsafe (string Zone, string FullPrefix) GetPrefix()
    {
        string? prefix = null;
        if (_ws.CurrentCFCID != 0)
            prefix = Service.LuminaRow<ContentFinderCondition>(_ws.CurrentCFCID)?.Name.ToString();
        if (_ws.CurrentZone != 0)
            prefix ??= Service.LuminaRow<TerritoryType>(_ws.CurrentZone)?.PlaceName.ValueNullable?.NameNoArticle.ToString();
        prefix ??= "World";
        prefix = Utils.StringToIdentifier(prefix);

        var zone = prefix;

        var player = _ws.Party.Player();
        if (player != null)
        {
            prefix += $"_{player.Class}{player.Level}";
            if (!_config.Anonymize)
                prefix += $"_{player.Name.Replace(" ", null, StringComparison.Ordinal)}";
        }

        var cf = FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder.Instance();
        if (cf->IsUnrestrictedParty)
            prefix += "_U";
        if (cf->IsLevelSync)
            prefix += "_LS";
        if (cf->IsMinimalIL)
            prefix += "_MI";
        if (cf->IsSilenceEcho)
            prefix += "_NE";

        return (zone, prefix);
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
