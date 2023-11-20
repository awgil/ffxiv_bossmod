using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod
{
    public class ReplayManager : IDisposable
    {
        private class ReplayEntry : IDisposable
        {
            public string Path;
            public float Progress;
            public CancellationTokenSource Cancel = new();
            public Task<Replay> Replay;
            public ReplayVisualization.ReplayDetailsWindow? Window;
            public bool AutoShowWindow;
            public bool Selected;
            public bool Disposed;

            public ReplayEntry(string path, bool autoShow)
            {
                Path = path;
                AutoShowWindow = autoShow;
                Replay = Task.Run(() => ReplayParserLog.Parse(path, ref Progress, Cancel.Token));
            }

            public void Dispose()
            {
                Window?.Dispose();
                Cancel.Cancel();
                Replay.Wait();
                Replay.Dispose();
                Cancel.Dispose();
                Disposed = true;
            }

            public void Show()
            {
                Window ??= new(Replay.Result);
                Window.IsOpen = true;
                Window.BringToFront();
            }
        }

        private class AnalysisEntry : IDisposable
        {
            public string Identifier;
            public List<ReplayEntry> Replays = new();
            public ReplayAnalysis.AnalysisManager? Analysis;
            public UISimpleWindow? Window;
            public bool Disposed;

            public AnalysisEntry(string identifier)
            {
                Identifier = identifier;
            }

            public void Dispose()
            {
                Window?.Dispose();
                Analysis?.Dispose();
                Disposed = true;
            }

            public void Show()
            {
                Analysis ??= new(Replays.Where(r => r.Replay.IsCompletedSuccessfully && r.Replay.Result.Ops.Count > 0).Select(r => r.Replay.Result).ToList());
                Window ??= new($"Multiple logs: {Identifier}", Analysis.Draw, false, new(1200, 800));
                Window.IsOpen = true;
            }
        }

        private List<ReplayEntry> _replayEntries = new();
        private List<AnalysisEntry> _analysisEntries = new();
        private int _nextAnalysisId;
        private string _path = "";
        private FileDialog? _fileDialog;
        private string _fileDialogStartPath;

        public ReplayManager(string startPath)
        {
            _fileDialogStartPath = startPath;
        }

        public void Dispose()
        {
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

            foreach (var e in _replayEntries)
            {
                using var idScope = ImRaii.PushId(e.Path);

                ImGui.TableNextColumn();
                if (!e.Replay.IsCompleted)
                {
                    ImGui.ProgressBar(e.Progress, new(100, 0));
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
                            e.Show();
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
                    e.Dispose();
                    foreach (var a in _analysisEntries.Where(a => !a.Disposed && a.Replays.Contains(e)))
                        a.Dispose();
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
            bool shouldSelectAll = _replayEntries.Count == 0 || numSelected < _replayEntries.Count;
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
                    var analysis = new AnalysisEntry((++_nextAnalysisId).ToString());
                    analysis.Replays = _replayEntries.Where(e => e.Selected).ToList();
                    _analysisEntries.Add(analysis);
                }
                ImGui.SameLine();
                if (ImGui.Button("Unload selected"))
                {
                    foreach (var e in _replayEntries.Where(e => e.Selected))
                        e.Dispose();
                    foreach (var e in _analysisEntries.Where(e => e.Replays.Any(r => r.Selected)))
                        e.Dispose();
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Unload all"))
            {
                foreach (var e in _replayEntries)
                    e.Dispose();
                foreach (var e in _analysisEntries)
                    e.Dispose();
            }
        }

        private void DrawNewEntry()
        {
            ImGui.InputText("###path", ref _path, 500);
            ImGui.SameLine();
            if (ImGui.Button("..."))
            {
                _fileDialog ??= new("select_log", "Select file or directory", "Log files{.log},All files{.*}", _fileDialogStartPath, "", ".log", 1, false, ImGuiFileDialogFlags.SelectOnly);
                _fileDialog.Show();
            }
            ImGui.SameLine();
            using (ImRaii.Disabled(_path.Length == 0 || _replayEntries.Any(e => e.Path == _path)))
            {
                if (ImGui.Button("Open"))
                {
                    _replayEntries.Add(new(_path, true));
                }
            }
            ImGui.SameLine();
            using (ImRaii.Disabled(_path.Length == 0 || _analysisEntries.Any(e => e.Identifier == _path)))
            {
                if (ImGui.Button("Analyze all"))
                {
                    var analysis = new AnalysisEntry(_path);
                    try
                    {
                        var di = new DirectoryInfo(_path);
                        var pattern = "*.log";
                        if (!di.Exists && (di.Parent?.Exists ?? false))
                        {
                            pattern = di.Name;
                            di = di.Parent;
                        }
                        foreach (var fi in di.EnumerateFiles(pattern, new EnumerationOptions { RecurseSubdirectories = true }))
                        {
                            var r = new ReplayEntry(fi.FullName, false);
                            _replayEntries.Add(r);
                            analysis.Replays.Add(r);
                        }
                    }
                    catch (Exception e)
                    {
                        Service.Log($"Failed to read {_path}: {e}");
                    }
                    if (analysis.Replays.Count > 0)
                        _analysisEntries.Add(analysis);
                }
            }
        }

        private void ConvertLog(Replay r, ReplayLogFormat format)
        {
            if (r.Ops.Count == 0)
                return;

            var player = new ReplayPlayer(r);
            player.WorldState.Frame.Timestamp = r.Ops.First().Timestamp; // so that we get correct name etc.
            using var relogger = new ReplayRecorder(player.WorldState, format, false, new FileInfo(r.Path).Directory!, format.ToString());
            player.AdvanceTo(DateTime.MaxValue, () => { });
        }
    }
}
