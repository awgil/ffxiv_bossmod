using ImGuiNET;
using System;

namespace BossMod
{
    public class ReplayRecorderUI : IDisposable
    {
        private WorldState _ws;
        private ReplayRecorderConfig _config;
        private WindowManager.Window? _wnd;
        private ReplayRecorder? _recorder;
        private string _message = "";

        public ReplayRecorderUI(WorldState ws, ReplayRecorderConfig config)
        {
            _ws = ws;
            _config = config;
            _config.Modified += ApplyConfig;
            ApplyConfig(null, EventArgs.Empty);
        }

        public void Dispose()
        {
            _config.Modified -= ApplyConfig;
            _recorder?.Dispose();
        }

        private void ApplyConfig(object? sender, EventArgs args)
        {
            if (_config.ShowUI && _wnd == null)
            {
                _wnd = WindowManager.CreateWindow($"Replay recorder", Draw, () => _wnd = null, UserClose);
                _wnd.SizeHint = new(300, 200);
                UpdateTitle(_wnd);
            }
            else if (!_config.ShowUI && _wnd != null)
            {
                WindowManager.CloseWindow(_wnd);
            }
        }

        private void Draw()
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
                UpdateTitle(_wnd);
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

        private bool UserClose()
        {
            _config.ShowUI = false;
            _config.NotifyModified();
            return true;
        }

        private void UpdateTitle(WindowManager.Window? wnd)
        {
            if (wnd != null)
            {
                wnd.Title = _recorder != null ? "Recording..." : "Idle";
            }
        }
    }
}
