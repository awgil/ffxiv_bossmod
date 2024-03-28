using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.IO;
using System.Linq;

namespace BossMod
{
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
            _config.Modified += ApplyConfig;
            _manager = new(logDir.FullName);
            ApplyConfig(null, EventArgs.Empty);
            UpdateTitle();
            RespectCloseHotkey = false;
        }

        protected override void Dispose(bool disposing)
        {
            _config.Modified -= ApplyConfig;
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
            if (ImGui.Button(_recorder == null ? "Start recording" : "Stop recording"))
            {
                if (_recorder == null)
                {
                    try
                    {
                        _recorder = new(_ws, _config.WorldLogFormat, true, _logDir, $"{GetPrefix()}");
                    }
                    catch (Exception ex)
                    {
                        Service.Log($"Failed to start recording: {ex}");
                    }
                }
                else
                {
                    _recorder.Dispose();
                    _recorder = null;
                }
                UpdateTitle();
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
            if (_recorder == null)
            {
                try
                {
                    if (_config.MaxReplays > 0)
                    {
                        var files = _logDir.GetFiles().OrderBy(f => f.LastWriteTime).ToList();
                        files.Take(files.Count - _config.MaxReplays).ToList().ForEach(f => f.Delete());
                    }
                }
                catch (Exception ex)
                {
                    Service.Log($"Failed to delete old replays: {ex}");
                }

                try
                {
                    _recorder = new(_ws, _config.WorldLogFormat, true, _logDir, $"{GetPrefix()}");
                }
                catch (Exception ex)
                {
                    Service.Log($"Failed to start recording: {ex}");
                }
            }
        }

        public void StopRecording()
        {
            if (_recorder != null)
            {
                _recorder.Dispose();
                _recorder = null;
            }
        }

        public bool IsRecording() => _recorder != null;

        public override void OnClose()
        {
            SetVisible(false);
        }

        private void ApplyConfig(object? sender, EventArgs args) => IsOpen = _config.ShowUI;
        private void UpdateTitle() =>  WindowName = $"Replay recording: {(_recorder != null ? "in progress..." : "idle")}{_windowID}";
        private static unsafe string GetPrefix()
        {
            var prefix = "World";
            var row = Service.LuminaGameData!.GetExcelSheet<TerritoryType>()!.GetRow(Service.ClientState.TerritoryType);
            if (row != null)
            {
                if (row.ContentFinderCondition.Value!.RowId != 0)
                    prefix = row.ContentFinderCondition.Value.Name.RawString;
                else
                    prefix = row.PlaceName.Value?.NameNoArticle.RawString ?? prefix;
            }

            var cf = ContentsFinder.Instance();
            var unrestricted = cf->IsUnrestrictedParty ? "U" : "";
            var levelsync = cf->IsLevelSync ? "LS" : "";
            var minilvl = cf->IsMinimalIL ? "MI" : "";
            var noecho = cf->IsSilenceEcho ? "NE" : "";
            var dfsettings = string.Join(", ", new[] { unrestricted, levelsync, minilvl, noecho }.Where(s => !string.IsNullOrEmpty(s)));
            if (dfsettings != null)
                prefix += $"_{dfsettings}";

            return prefix;
        }
    }
}
