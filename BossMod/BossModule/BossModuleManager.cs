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
            public bool RotateArena = true;
            public bool ShowRaidWarnings = true;
            public bool ShowWorldArrows = false;

            protected override void DrawContents()
            {
                DrawProperty(ref RotateArena, "Rotate map to match camera orientation");
                DrawProperty(ref ShowRaidWarnings, "Show warnings for all raid members");
                DrawProperty(ref ShowWorldArrows, "Show movement hints in world");
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
                case 1009:
                    _activeModule = new P4S(_ws);
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

        private void ZoneChanged(object? sender, ushort zone)
        {
            ActivateModuleForZone(zone);
        }
    }
}
