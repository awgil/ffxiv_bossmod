using ImGuiNET;
using System;

namespace BossMod
{
    public class ReplayRecorderWindow : UIWindow
    {
        private WorldState _ws;
        private ReplayRecorderConfig _config;
        private ReplayRecorder? _recorder;
        private string _message = "";

        private static string _windowID = "###Replay recorder";

        public ReplayRecorderWindow(WorldState ws, ReplayRecorderConfig config) : base(_windowID, false, new(300, 200))
        {
            _ws = ws;
            _config = config;
            _config.Modified += ApplyConfig;
            ApplyConfig(null, EventArgs.Empty);
            UpdateTitle();
            RespectCloseHotkey = false;
        }

        protected override void Dispose(bool disposing)
        {
            _config.Modified -= ApplyConfig;
            _recorder?.Dispose();
        }

        public override void Draw()
        {
            if (ImGui.Button(_recorder == null ? "Start" : "Stop"))
            {
                if (_recorder == null)
                {
                    try
                    {
                        _recorder = new(_ws, _config);
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
        }

        public override void OnClose()
        {
            Service.Log($"Closing: {_config.ShowUI}");
            if (_config.ShowUI)
            {
                _config.ShowUI = false;
                _config.NotifyModified();
            }
        }

        private void ApplyConfig(object? sender, EventArgs args) => IsOpen = _config.ShowUI;
        private void UpdateTitle() =>  WindowName = $"{(_recorder != null ? "Recording..." : "Idle")}{_windowID}";
    }
}
