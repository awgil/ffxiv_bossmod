using Dalamud.Logging;
using ImGuiNET;
using System;

namespace BossMod
{
    class BossModuleManager : IDisposable
    {
        public class BossModuleConfig : ConfigNode
        {
            public bool ShowRaidWarnings = true;

            protected override void DrawContents()
            {
                DrawProperty(ref ShowRaidWarnings, "Show warnings for all raid members");
            }

            protected override string? NameOverride() => "Boss modules settings";
        }

        private WorldState _ws;
        private BossModuleConfig _config;
        private BossModule? _activeModule;
        private WindowManager.Window? _mainWindow;
        private WindowManager.Window? _raidWarnings;

        public BossModuleManager(WorldState ws, ConfigNode settings)
        {
            _ws = ws;
            _config = settings.Get<BossModuleConfig>();

            _ws.CurrentZoneChanged += ZoneChanged;
            ActivateModuleForZone(_ws.CurrentZone);
        }

        public void Dispose()
        {
            _activeModule?.Dispose();
            _ws.CurrentZoneChanged -= ZoneChanged;
        }

        public void Update()
        {
            bool wantRaidWarnings = _activeModule != null && _config.ShowRaidWarnings;
            if (wantRaidWarnings && _raidWarnings == null)
            {
                _raidWarnings = WindowManager.CreateWindow("Raid warnings", () => TryExec(DrawRaidWarnings), () => _raidWarnings = null);
                _raidWarnings.SizeHint = new(100, 100);
                _raidWarnings.MinSize = new(100, 100);
                _raidWarnings.Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            }
            else if (!wantRaidWarnings && _raidWarnings != null)
            {
                _raidWarnings?.Close();
                _raidWarnings = null;
            }

            if (_activeModule != null)
            {
                TryExec(_activeModule.Update);
            }
        }

        public void ActivateModuleForZone(ushort zone)
        {
            _activeModule?.Dispose();
            _activeModule = null;
            _mainWindow?.Close();
            _mainWindow = null;

            switch (zone)
            {
                case 993:
                    _activeModule = new Zodiark(_ws);
                    break;
                case 1003:
                    _activeModule = new P1S(_ws);
                    break;
                case 1005:
                    _activeModule = new P2S(_ws);
                    break;
                case 1007:
                    _activeModule = new P3S(_ws);
                    break;
            }

            if (_activeModule != null)
            {
                _mainWindow = WindowManager.CreateWindow("Boss module", () => TryExec(DrawMainWindow), () => ActivateModuleForZone(0));
                _mainWindow.SizeHint = new(400, 400);
                _mainWindow.MinSize = new(400, 400);
                _mainWindow.Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            }

            Service.Log($"Activated module: {_activeModule?.GetType().ToString() ?? "none"}");
        }

        private void DrawMainWindow()
        {
            _activeModule?.Draw(Camera.Instance?.CameraAzimuth ?? 0, null);
        }

        private void DrawRaidWarnings()
        {
            if (_activeModule == null)
                return;

            // TODO: this should really snap to player in party ui...
            var riskColor = ImGui.ColorConvertU32ToFloat4(0xff00ffff);
            var safeColor = ImGui.ColorConvertU32ToFloat4(0xff00ff00);
            foreach ((int i, var player) in _activeModule.IterateRaidMembers())
            {
                var obj = Service.ObjectTable.SearchById(player.InstanceID);
                if (obj == null)
                    continue;

                var hints = _activeModule.CalculateHintsForRaidMember(i, player);
                if (hints.Count == 0)
                    continue;

                ImGui.Text($"{obj.Name}:");
                foreach ((var hint, bool risk) in hints)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(risk ? riskColor : safeColor, hint);
                }
            }
        }

        private void TryExec(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Boss module crashed");
                _activeModule?.Dispose();
                _activeModule = null;
            }
        }

        private void ZoneChanged(object? sender, ushort zone)
        {
            ActivateModuleForZone(zone);
        }
    }
}
