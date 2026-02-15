using Autofac;
using BossMod.ReplayVisualization;
using DalaMock.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod;

public sealed class ReplayManager : IDisposable
{
    public sealed class ReplayEntry : IDisposable
    {
        public string Path;
        public float Progress;
        public CancellationTokenSource Cancel = new();
        public Task<Replay> Replay;
        public ReplayDetailsWindow? Window;
        public bool AutoShowWindow;
        public bool Selected;
        public bool Disposed;
        public bool Disposing;
        public DateTime? InitialTime;
        private readonly ReplayDetailsWindow.Factory _detailFac;

        public delegate ReplayEntry Factory(string path, bool autoShow, DateTime? initialTime = null);

        public ReplayEntry(
            string path,
            bool autoShow,
            ReplayBuilder.Factory builderFac,
            ReplayDetailsWindow.Factory detailFac,
            DateTime? initialTime
        )
        {
            _detailFac = detailFac;
            Path = path;
            AutoShowWindow = autoShow;
            InitialTime = initialTime;
            Replay = Task.Run(() => ReplayParserLog.Parse(path, ref Progress, builderFac, Cancel.Token));
        }

        public void Dispose()
        {
            Disposing = true;
            Cancel.Cancel();
            Replay.Wait();
            Replay.Dispose();
            Cancel.Dispose();
            Disposed = true;
        }

        public void Show()
        {
            Window ??= _detailFac.Invoke(Replay.Result, InitialTime);
            Window.IsOpen = true;
            Window.BringToFront();
        }
    }

    private sealed record class AnalysisEntry(string Identifier, IEnumerable<ReplayEntry> Replays, BossModuleRegistry Registry, ActionDefinitions Actions) : IDisposable
    {
        public ReplayAnalysis.AnalysisManager? Analysis;
        public UISimpleWindow? Window;
        public bool Disposed;

        public void Dispose()
        {
            Window?.Dispose();
            Analysis?.Dispose();
            Disposed = true;
        }

        public void Show()
        {
            Analysis ??= new([.. Replays.Where(r => r.Replay.IsCompletedSuccessfully && r.Replay.Result.Ops.Count > 0).Select(r => r.Replay.Result)], Registry, Actions);
            Window ??= new($"Multiple logs: {Identifier}", Analysis.Draw, false, new(1200, 800));
            Window.IsOpen = true;
        }
    }

    private readonly ReplayManagementConfig _config;
    private readonly List<(ILifetimeScope scope, ReplayEntry entry)> _replayEntries = [];
    private readonly List<AnalysisEntry> _analysisEntries = [];
    private int _nextAnalysisId;
    private string _path = "";
    private string _fileDialogStartPath;
    private FileDialog? _fileDialog;
    private readonly IFileDialogManager _dialogManager;
    private readonly ActionDefinitions _actions;
    private readonly BossModuleRegistry _bmr;
    private readonly ILifetimeScope _ctx;

    public delegate ReplayManager Factory(string fileDialogStartPath);

    public ReplayManager(
        IFileDialogManager dialogManager,
        ReplayManagementConfig config,
        ActionDefinitions actions,
        BossModuleRegistry bmr,
        ILifetimeScope ctx,
        string fileDialogStartPath
    )
    {
        _fileDialogStartPath = fileDialogStartPath;
        _dialogManager = dialogManager;
        _actions = actions;
        _bmr = bmr;
        _config = config;
        _ctx = ctx;
        RestoreHistory();
    }

    public void Dispose()
    {
        SaveHistory();
        foreach (var e in _analysisEntries)
            e.Dispose();
        foreach (var (cs, _) in _replayEntries)
            cs.Dispose();
    }

    public void Update()
    {
        // remove disposed entries
        _replayEntries.RemoveAll(e => e.entry.Disposed);
        _analysisEntries.RemoveAll(e => e.Disposed);

        // auto-show replay windows that are now ready
        foreach (var (_, e) in _replayEntries)
        {
            if (e.AutoShowWindow && e.Window == null && e.Replay.IsCompletedSuccessfully && e.Replay.Result.Ops.Count > 0)
            {
                e.Show();
            }
        }
        // auto-show analysis windows that are now ready, auto dispose entries that had their windows closed
        foreach (var e in _analysisEntries)
        {
            if (e.Analysis == null && e.Replays.All(r => r.Replay.IsCompleted))
            {
                e.Show();
            }
            if (e.Window != null && !e.Window.IsOpen)
            {
                e.Dispose();
            }
        }
    }

    public void Draw()
    {
        DrawNewEntry();
        DrawEntries();
        DrawEntriesOperations();

        if (_fileDialog?.Draw() ?? false)
        {
            if (_fileDialog.GetIsOk())
            {
                _path = _fileDialog.GetResults().FirstOrDefault() ?? "";
                _fileDialogStartPath = _fileDialog.GetCurrentPath();
            }
            _fileDialog.Hide();
            _fileDialog = null;
        }
    }

    private void DrawEntries()
    {
        using var table = ImRaii.Table("entries", 3);
        if (!table)
            return;
        ImGui.TableSetupColumn("op", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("unload", ImGuiTableColumnFlags.WidthFixed, 50);

        foreach (var (cs, e) in _replayEntries)
        {
            using var idScope = ImRaii.PushId(e.Path);

            ImGui.TableNextColumn();
            if (!e.Replay.IsCompleted)
            {
                ImGui.ProgressBar(e.Progress, new Vector2(100, 0));
            }
            else if (e.Replay.IsFaulted || e.Replay.Result.Ops.Count == 0)
            {
                ImGui.TextUnformatted("(failed)");
            }
            else
            {
                if (ImGui.Button("Actions...", new(100, 0)))
                    ImGui.OpenPopup("ctx");
                using var popup = ImRaii.Popup("ctx");
                if (popup)
                {
                    if (ImGui.MenuItem("Show"))
                    {
                        e.Show();
                        SaveHistory();
                    }
                    if (ImGui.MenuItem("Convert to verbose"))
                        ConvertLog(e.Replay.Result, ReplayLogFormat.TextVerbose);
                    if (ImGui.MenuItem("Convert to short text"))
                        ConvertLog(e.Replay.Result, ReplayLogFormat.TextCondensed);
                    if (ImGui.MenuItem("Convert to uncompressed binary"))
                        ConvertLog(e.Replay.Result, ReplayLogFormat.BinaryUncompressed);
                    if (ImGui.MenuItem("Convert to compressed binary"))
                        ConvertLog(e.Replay.Result, ReplayLogFormat.BinaryCompressed);
                }
            }

            ImGui.TableNextColumn();
            if (ImGui.Button(e.Replay.IsCompleted ? "Unload" : "Cancel", new(50, 0)))
            {
                cs.Dispose();
                foreach (var a in _analysisEntries.Where(a => !a.Disposed && a.Replays.Contains(e)))
                    a.Dispose();
                SaveHistory();
            }

            ImGui.TableNextColumn();
            ImGui.Checkbox($"{e.Path}", ref e.Selected);
        }
    }

    private void DrawEntriesOperations()
    {
        if (_replayEntries.Count == 0)
            return;

        var numSelected = _replayEntries.Count(e => e.entry.Selected);
        bool shouldSelectAll = _replayEntries.Count == 0 || numSelected < _replayEntries.Count;
        if (ImGui.Button(shouldSelectAll ? "Select all" : "Unselect all", new(80, 0)))
        {
            foreach (var e in _replayEntries)
                e.entry.Selected = shouldSelectAll;
        }
        using (ImRaii.Disabled(numSelected == 0))
        {
            ImGui.SameLine();
            if (ImGui.Button("Analyze selected"))
            {
                _analysisEntries.Add(new((++_nextAnalysisId).ToString(), [.. _replayEntries.Select(e => e.entry).Where(e => e.Selected)], _bmr, _actions));
            }
            ImGui.SameLine();
            if (ImGui.Button("Unload selected"))
            {
                foreach (var (cs, e) in _replayEntries)
                    if (e.Selected)
                        cs.Dispose();
                foreach (var e in _analysisEntries.Where(e => e.Replays.Any(r => r.Selected)))
                    e.Dispose();
                SaveHistory();
            }
        }
        ImGui.SameLine();
        if (ImGui.Button("Unload all"))
        {
            foreach (var (cs, _) in _replayEntries)
                cs.Dispose();
            foreach (var e in _analysisEntries)
                e.Dispose();
            SaveHistory();
        }
    }

    private (ILifetimeScope scope, ReplayEntry entry) CreateEntry(string path, bool autoOpen, DateTime? initialTime = null)
    {
        var scope = _ctx.BeginLifetimeScope();
        var entry = scope.Resolve<ReplayEntry.Factory>().Invoke(path, autoOpen, initialTime);
        return (scope, entry);
    }

    private void DrawNewEntry()
    {
        ImGui.InputText("###path", ref _path, 500);
        ImGui.SameLine();
        if (ImGui.Button("..."))
        {
            _dialogManager.OpenFileDialog("Select file or directory", "log", (isOk, files) =>
            {
                if (isOk)
                    _path = files[0];
            }, 1, _fileDialogStartPath);
        }
        ImGui.SameLine();
        using (ImRaii.Disabled(_path.Length == 0 || _replayEntries.Any(e => e.entry.Path == _path)))
        {
            if (ImGui.Button("Open"))
            {
                CleanPath();
                _replayEntries.Add(CreateEntry(_path, true));
                SaveHistory();
            }
        }
        ImGui.SameLine();
        using (ImRaii.Disabled(_path.Length == 0 || _analysisEntries.Any(e => e.Identifier == _path)))
        {
            if (ImGui.Button("Analyze all"))
            {
                CleanPath();
                var replays = LoadAll(_path);
                if (replays.Count > 0)
                    _analysisEntries.Add(new(_path, replays.Select(e => e.entry), _bmr, _actions));
            }
        }
        ImGui.SameLine();
        using (ImRaii.Disabled(_path.Length == 0))
        {
            if (ImGui.Button("Load all"))
            {
                CleanPath();
                LoadAll(_path);
                SaveHistory();
            }
        }
    }

    private void CleanPath()
    {
        if (_path.StartsWith('"') && _path.EndsWith('"'))
            _path = _path[1..^1];
    }

    private List<(ILifetimeScope scope, ReplayEntry entry)> LoadAll(string path)
    {
        try
        {
            var res = new List<(ILifetimeScope, ReplayEntry)>();
            var di = new DirectoryInfo(path);
            var pattern = "*.log";
            if (!di.Exists && (di.Parent?.Exists ?? false))
            {
                pattern = di.Name;
                di = di.Parent;
            }
            foreach (var fi in di.EnumerateFiles(pattern, new EnumerationOptions { RecurseSubdirectories = true }))
            {
                var ix = _replayEntries.FindIndex(e => e.entry.Path == fi.FullName);
                if (ix < 0)
                {
                    ix = _replayEntries.Count;
                    _replayEntries.Add(CreateEntry(fi.FullName, false));
                }
                res.Add(_replayEntries[ix]);
            }
            return res;
        }
        catch (Exception e)
        {
            Service.Log($"Failed to read {path}: {e}");
            return [];
        }
    }

    private void ConvertLog(Replay r, ReplayLogFormat format)
    {
        if (r.Ops.Count == 0)
            return;

        var player = new ReplayPlayer(r);
        player.WorldState.Frame.Timestamp = r.Ops[0].Timestamp; // so that we get correct name etc.
        using var relogger = new ReplayRecorder(player.WorldState, format, false, new FileInfo(r.Path).Directory!, format.ToString());
        player.AdvanceTo(DateTime.MaxValue, () => { });
    }

    private void SaveHistory()
    {
        if (!RememberReplays)
            return;
        _config.ReplayHistory = [.. _replayEntries.Select(e => e.entry).Where(r => !r.Disposing).Select(r => new ReplayMemory(r.Path, r.Window?.IsOpen ?? false, r.Window?.CurrentTime ?? default))];
        _config.Modified.Fire();
    }

    private void RestoreHistory()
    {
        if (!RememberReplays)
            return;
        foreach (var memory in _config.ReplayHistory)
            _replayEntries.Add(CreateEntry(memory.Path, memory.IsOpen, _config.RememberReplayTimes ? memory.PlaybackPosition : null));
    }

    private bool RememberReplays => _config.RememberReplays;
}
