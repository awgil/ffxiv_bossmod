using Autofac;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.IO;
using System.Text.Json;
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
        public ReplayVisualization.ReplayDetailsWindow? Window;
        public bool AutoShowWindow;
        private readonly ILifetimeScope scope;
        private readonly MediatorService mediator;
        public bool Selected;
        public bool Disposed;
        public bool Disposing;
        public DateTime? InitialTime;

        public ReplayEntry(string path, bool autoShow, ILifetimeScope scope, MediatorService mediator, DateTime? initialTime = null)
        {
            Path = path;
            AutoShowWindow = autoShow;
            this.scope = scope;
            this.mediator = mediator;
            InitialTime = initialTime;
            Replay = Task.Run(() => ReplayParserLog.Parse(path, scope.Resolve<BossModuleConfig>(), scope.Resolve<BossModuleRegistry>(), scope.Resolve<JsonSerializerOptions>(), scope.Resolve<ModuleInit.Factory>(), ref Progress, Cancel.Token));
        }

        public void Dispose()
        {
            Disposing = true;
            Window?.IsDestroyed = true;
            Cancel.Cancel();
            Replay.Wait();
            Replay.Dispose();
            Cancel.Dispose();
            Disposed = true;
        }

        public void Show()
        {
            Window ??= mediator.CreateWindow<ReplayVisualization.ReplayDetailsWindow>(new(Replay.Result, InitialTime, scope, mediator));
            Window.IsOpen = true;
            Window.BringToFront();
        }
    }

    public sealed record class AnalysisEntry(string Identifier, List<ReplayEntry> Replays, ReplayAnalysis.AnalysisManager.Factory AnalysisFactory, MediatorService Mediator) : IDisposable
    {
        public delegate AnalysisEntry Factory(string Identifier, List<ReplayEntry> Replays);

        public ReplayAnalysis.AnalysisManager? Analysis;
        public UISimpleWindow? Window;
        public bool Disposed;

        public void Dispose()
        {
            if (Window != null)
                Mediator.Publish(new DestroyWindow(Window));
            Window?.Dispose();
            Analysis?.Dispose();
            Disposed = true;
        }

        public void Show()
        {
            Analysis ??= AnalysisFactory.Invoke([.. Replays.Where(r => r.Replay.IsCompletedSuccessfully && r.Replay.Result.Ops.Count > 0).Select(r => r.Replay.Result)]);
            if (Window == null)
            {
                Window = new($"Multiple logs: {Identifier}", Analysis.Draw, false, new(1200, 800));
                Mediator.Publish(new CreateWindow(Window));
            }
            Window.IsOpen = true;
        }
    }

    private readonly List<ReplayEntry> _replayEntries = [];
    private readonly List<AnalysisEntry> _analysisEntries = [];
    private readonly AnalysisEntry.Factory _afac;
    private readonly ReplayHistory _replayHistory;
    private int _nextAnalysisId;
    private string _path = "";
    private string _fileDialogStartPath;
    private readonly ILifetimeScope container;
    private readonly MediatorService mediator;
    private bool _convertAnonymize;

    public ReplayManager(string fileDialogStartPath, ILifetimeScope container, MediatorService mediator)
    {
        _fileDialogStartPath = fileDialogStartPath;
        _afac = container.Resolve<AnalysisEntry.Factory>();
        this.container = container;
        this.mediator = mediator;
        _replayHistory = ReplayHistory.Load();
        RestoreHistory();
    }

    public void Dispose()
    {
        SaveHistory();
        foreach (var e in _analysisEntries)
            e.Dispose();
        foreach (var e in _replayEntries)
            e.Dispose();
    }

    public void Update()
    {
        // remove disposed entries
        _replayEntries.RemoveAll(e => e.Disposed);
        _analysisEntries.RemoveAll(e => e.Disposed);

        // auto-show replay windows that are now ready
        foreach (var e in _replayEntries)
            if (e.AutoShowWindow && e.Window == null && e.Replay.IsCompletedSuccessfully && e.Replay.Result.Ops.Count > 0)
                e.Show();

        // auto-show analysis windows that are now ready, auto dispose entries that had their windows closed
        foreach (var e in _analysisEntries)
        {
            if (e.Analysis == null && e.Replays.All(r => r.Replay.IsCompleted))
                e.Show();

            if (e.Window != null && !e.Window.IsOpen)
                e.Dispose();
        }
    }

    public void Draw()
    {
        DrawNewEntry();
        DrawEntries();
        DrawEntriesOperations();
    }

    public void UnloadPaths(IEnumerable<string> paths)
    {
        var set = paths.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (set.Count == 0)
            return;

        foreach (var e in _replayEntries.Where(e => !e.Disposed && set.Contains(e.Path)))
            e.Dispose();
        foreach (var a in _analysisEntries.Where(a => !a.Disposed && a.Replays.Any(r => set.Contains(r.Path))))
            a.Dispose();
        SaveHistory();
    }

    private void DrawEntries()
    {
        using var table = ImRaii.Table("entries", 3);
        if (!table)
            return;
        ImGui.TableSetupColumn("op", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("unload", ImGuiTableColumnFlags.WidthFixed, 50);

        foreach (var e in _replayEntries)
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
                        ConvertLog(e.Replay.Result, ReplayLogFormat.TextVerbose, _convertAnonymize);
                    if (ImGui.MenuItem("Convert to short text"))
                        ConvertLog(e.Replay.Result, ReplayLogFormat.TextCondensed, _convertAnonymize);
                    if (ImGui.MenuItem("Convert to uncompressed binary"))
                        ConvertLog(e.Replay.Result, ReplayLogFormat.BinaryUncompressed, _convertAnonymize);
                    if (ImGui.MenuItem("Convert to compressed binary"))
                        ConvertLog(e.Replay.Result, ReplayLogFormat.BinaryCompressed, _convertAnonymize);
                    ImGui.Separator();
                    ImGuiP.PushItemFlag(ImGuiItemFlags.SelectableDontClosePopup, true);
                    if (ImGui.MenuItem("Anonymize replay during conversion", _convertAnonymize))
                        _convertAnonymize = !_convertAnonymize;
                    ImGuiP.PopItemFlag();
                }
            }

            ImGui.TableNextColumn();
            if (ImGui.Button(e.Replay.IsCompleted ? "Unload" : "Cancel", new(50, 0)))
            {
                e.Dispose();
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

        var numSelected = _replayEntries.Count(e => e.Selected);
        var shouldSelectAll = _replayEntries.Count == 0 || numSelected < _replayEntries.Count;
        if (ImGui.Button(shouldSelectAll ? "Select all" : "Unselect all", new(80, 0)))
        {
            foreach (var e in _replayEntries)
                e.Selected = shouldSelectAll;
        }
        using (ImRaii.Disabled(numSelected == 0))
        {
            ImGui.SameLine();
            if (ImGui.Button("Analyze selected"))
            {
                _analysisEntries.Add(_afac.Invoke((++_nextAnalysisId).ToString(), [.. _replayEntries.Where(e => e.Selected)]));
            }
            ImGui.SameLine();
            if (ImGui.Button("Unload selected"))
            {
                foreach (var e in _replayEntries.Where(e => e.Selected))
                    e.Dispose();
                foreach (var e in _analysisEntries.Where(e => e.Replays.Any(r => r.Selected)))
                    e.Dispose();
                SaveHistory();
            }
        }
        ImGui.SameLine();
        if (ImGui.Button("Unload all"))
        {
            foreach (var e in _replayEntries)
                e.Dispose();
            foreach (var e in _analysisEntries)
                e.Dispose();
            SaveHistory();
        }
    }

    private void DrawNewEntry()
    {
        ImGui.InputText("###path", ref _path, 500);
        ImGui.SameLine();
        if (UIMisc.IconButton(Dalamud.Interface.FontAwesomeIcon.File))
        {
            // FIXME
            var suffixFilter = Service.IsMock ? "log" : ".log";

            Service.FileDialogManager.OpenFileDialog("Select file", suffixFilter, (c, p) =>
            {
                if (c)
                {
                    _path = p[0];
                    _fileDialogStartPath = new FileInfo(_path).Directory!.FullName;
                }
            }, 1, _fileDialogStartPath);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Open file");
        ImGui.SameLine();
        if (UIMisc.IconButton(Dalamud.Interface.FontAwesomeIcon.FolderOpen))
        {
            Service.FileDialogManager.OpenFolderDialog("Select directory", (c, p) =>
            {
                if (c)
                {
                    _path = p;
                    _fileDialogStartPath = p;
                }
            }, _fileDialogStartPath);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Open folder");
        ImGui.SameLine();
        using (ImRaii.Disabled(_path.Length == 0 || _replayEntries.Any(e => e.Path == _path)))
        {
            if (ImGui.Button("Open"))
            {
                CleanPath();
                _replayEntries.Add(new(_path, true, container, mediator));
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
                    _analysisEntries.Add(_afac.Invoke(_path, replays));
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

    private List<ReplayEntry> LoadAll(string path)
    {
        try
        {
            var res = new List<ReplayEntry>();
            var di = new DirectoryInfo(path);
            var pattern = "*.log";
            if (!di.Exists && (di.Parent?.Exists ?? false))
            {
                pattern = di.Name;
                di = di.Parent;
            }
            foreach (var fi in di.EnumerateFiles(pattern, new EnumerationOptions { RecurseSubdirectories = true }))
            {
                var r = _replayEntries.Find(e => e.Path == fi.FullName);
                if (r == null)
                {
                    r = new ReplayEntry(fi.FullName, false, container, mediator);
                    _replayEntries.Add(r);
                }
                res.Add(r);
            }
            return res;
        }
        catch (Exception e)
        {
            Service.Log($"Failed to read {path}: {e}");
            return [];
        }
    }

    private void ConvertLog(Replay r, ReplayLogFormat format, bool anonymize)
    {
        if (r.Ops.Count == 0)
            return;

        var (scope, player) = ReplayPlayer.Create(r, container);
        using var _ = scope;
        player.WorldState.Frame.Timestamp = r.Ops[0].Timestamp; // so that we get correct name etc.
        using var relogger = new ReplayRecorder(player.WorldState, format, false, new FileInfo(r.Path).Directory!, format.ToString(), anonymize);
        player.AdvanceTo(DateTime.MaxValue, () => { });
    }

    private void SaveHistory()
    {
        _replayHistory.History = [.. _replayEntries.Where(r => !r.Disposing).Select(r => new ReplayMemory(r.Path, r.Window?.IsOpen ?? false, r.Window?.CurrentTime ?? default))];
        try
        {
            _replayHistory.Save();
        }
        catch (IOException ex)
        {
            Service.PluginLog.Warning(ex, "Unable to save replay history");
        }
    }

    private void RestoreHistory()
    {
        if (!Service.IsMock)
            return;

        foreach (var rp in _replayHistory.History)
            _replayEntries.Add(new(rp.Path, rp.IsOpen, container, mediator, rp.PlaybackPosition));
    }
}
