using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    class BossModuleManager : IDisposable
    {
        public class BossModuleConfig : ConfigNode
        {
            public float ArenaScale = 1;
            public bool Enable = true;
            public bool Lock = false;
            public bool RotateArena = true;
            public bool ShowCardinals = false;
            public bool TrishaMode = false;
            public bool ShowRaidWarnings = true;
            public bool ShowWorldArrows = false;
            public bool ShowDemo = false;

            protected override void DrawContents()
            {
                if (ImGui.DragFloat("Arena scale factor", ref ArenaScale, 0.1f, 0.1f, 10, "%.1f", ImGuiSliderFlags.Logarithmic))
                    NotifyModified();
                DrawProperty(ref Enable, "Enable boss modules");
                DrawProperty(ref Lock, "Lock movement and mouse interaction");
                DrawProperty(ref RotateArena, "Rotate map to match camera orientation");
                DrawProperty(ref ShowCardinals, "Show cardinal direction names");
                DrawProperty(ref TrishaMode, "Trisha mode: show only arena without hints and with transparent background");
                DrawProperty(ref ShowRaidWarnings, "Show warnings for all raid members");
                DrawProperty(ref ShowWorldArrows, "Show movement hints in world");
                DrawProperty(ref ShowDemo, "Show boss module demo out of instances (useful for configuring windows)");
            }

            protected override string? NameOverride() => "Boss modules settings";
        }

        private WorldState _ws;
        private BossModuleConfig _config;
        private BossModule? _activeModule;
        private WindowManager.Window? _mainWindow;
        private WindowManager.Window? _raidWarnings;

        public BossModule? ActiveModule => _activeModule;

        public BossModuleManager(WorldState ws, ConfigNode settings)
        {
            _ws = ws;
            _config = settings.Get<BossModuleConfig>();

            _ws.CurrentZoneChanged += ZoneChanged;
            _config.Modified += ConfigChanged;

            ApplyConfigAndZoneChanges();
        }

        public void Dispose()
        {
            _activeModule?.Dispose();
            _ws.CurrentZoneChanged -= ZoneChanged;
            _config.Modified -= ConfigChanged;
        }

        public void Update()
        {
            if (_activeModule != null)
            {
                TryExec(_activeModule.Update);
            }
        }

        public void ApplyConfigAndZoneChanges(ushort? forcedZone = null)
        {
            var zone = forcedZone ?? _ws.CurrentZone;
            var desiredType = _config.Enable ? ZoneModule.TypeForZone(zone) : null;
            if (desiredType == null && _config.Enable && _config.ShowDemo)
                desiredType = typeof(DemoModule);

            // recreate module if needed
            if (forcedZone != null || (_activeModule?.GetType() ?? null) != desiredType)
            {
                _activeModule?.Dispose();
                _activeModule = ZoneModule.CreateModule(desiredType, _ws);
                Service.Log($"Activated module: {_activeModule?.GetType().ToString() ?? "none"}");
            }

            // update module properties
            if (_activeModule != null)
            {
                _activeModule.Arena.ScreenScale = _config.ArenaScale;
                _activeModule.Arena.ShowCardinals = _config.ShowCardinals;
                _activeModule.DrawOnlyArena = _config.TrishaMode;
            }

            // create or destroy main window if needed
            if (_mainWindow != null && _activeModule == null)
            {
                _mainWindow.Close(true);
                _mainWindow = null;
            }
            else if (_mainWindow == null && _activeModule != null)
            {
                _mainWindow = WindowManager.CreateWindow("Boss module", () => TryExec(DrawMainWindow), MainWindowClosedByUser);
                _mainWindow.SizeHint = new(400, 400);
            }

            // update main window properties
            if (_mainWindow != null)
            {
                _mainWindow.Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
                if (_config.TrishaMode)
                    _mainWindow.Flags |= ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;
                if (_config.Lock)
                    _mainWindow.Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
            }

            // create or destroy raid warnings window
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
        }

        private void DrawMainWindow()
        {
            BossModule.MovementHints? movementHints = _config.ShowWorldArrows ? new() : null;
            _activeModule?.Draw(_config.RotateArena ? (Camera.Instance?.CameraAzimuth ?? 0) : 0, movementHints);
            DrawMovementHints(movementHints);
        }

        private void DrawRaidWarnings()
        {
            if (_activeModule == null)
                return;

            // TODO: this should really snap to player in party ui...
            var riskColor = ImGui.ColorConvertU32ToFloat4(0xff00ffff);
            var safeColor = ImGui.ColorConvertU32ToFloat4(0xff00ff00);
            foreach ((int i, var player) in _activeModule.Raid.WithSlot())
            {
                var hints = _activeModule.CalculateHintsForRaidMember(i, player);
                if (hints.Count == 0)
                    continue;

                ImGui.Text($"{player.Name}:");
                foreach ((var hint, bool risk) in hints)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(risk ? riskColor : safeColor, hint);
                }
            }
        }

        private void DrawMovementHints(BossModule.MovementHints? arrows)
        {
            if (arrows == null || arrows.Count == 0 || Camera.Instance == null)
                return;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
            ImGui.Begin("movement_hints_overlay", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            foreach ((var start, var end, uint color) in arrows)
            {
                DrawWorldLine(start, end, color, Camera.Instance);
                var dir = Vector3.Normalize(end - start);
                var arrowStart = end - 0.4f * dir;
                var offset = 0.07f * Vector3.Normalize(Vector3.Cross(Vector3.UnitY, dir));
                DrawWorldLine(arrowStart + offset, end, color, Camera.Instance);
                DrawWorldLine(arrowStart - offset, end, color, Camera.Instance);
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }

        private void DrawWorldLine(Vector3 start, Vector3 end, uint color, Camera camera)
        {
            var p1 = start.ToSharpDX();
            var p2 = end.ToSharpDX();
            if (!GeometryUtils.ClipLineToNearPlane(ref p1, ref p2, camera.ViewProj))
                return;

            p1 = SharpDX.Vector3.TransformCoordinate(p1, camera.ViewProj);
            p2 = SharpDX.Vector3.TransformCoordinate(p2, camera.ViewProj);
            var p1screen = new Vector2(0.5f * camera.ViewportSize.X * (1 + p1.X), 0.5f * camera.ViewportSize.Y * (1 - p1.Y)) + ImGuiHelpers.MainViewport.Pos;
            var p2screen = new Vector2(0.5f * camera.ViewportSize.X * (1 + p2.X), 0.5f * camera.ViewportSize.Y * (1 - p2.Y)) + ImGuiHelpers.MainViewport.Pos;
            ImGui.GetWindowDrawList().AddLine(p1screen, p2screen, color);
            //ImGui.GetWindowDrawList().AddText(p1screen, color, $"({p1.X:f3},{p1.Y:f3},{p1.Z:f3}) -> ({p2.X:f3},{p2.Y:f3},{p2.Z:f3})");
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

        private void MainWindowClosedByUser()
        {
            // temporarily disable, but do not save...
            Service.Log($"Bossmod window closed by user, disabling temporarily");
            _config.Enable = false;
            ApplyConfigAndZoneChanges();
        }

        private void ZoneChanged(object? sender, ushort zone)
        {
            ApplyConfigAndZoneChanges();
        }

        private void ConfigChanged(object? sender, EventArgs args)
        {
            ApplyConfigAndZoneChanges();
        }
    }
}
