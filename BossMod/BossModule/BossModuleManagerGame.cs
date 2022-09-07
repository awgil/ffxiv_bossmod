using Dalamud.Interface;
using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    // class that is responsible for drawing boss modules in game in a separate window
    class BossModuleManagerGame : BossModuleManager
    {
        private WindowManager.Window? _mainWindow;
        private WindowManager.Window? _planWindow;

        public BossModuleManagerGame(WorldState ws)
            : base(ws)
        {
        }

        protected override void RefreshConfigOrModules()
        {
            // create or destroy main window if needed
            if (_mainWindow != null && LoadedModules.Count == 0)
            {
                Service.Log("[BMM] Closing main window, since there are no more loaded modules");
                WindowManager.CloseWindow(_mainWindow);
            }
            else if (_mainWindow == null && LoadedModules.Count > 0)
            {
                Service.Log("[BMM] Creating main window, since there are now loaded modules");
                _mainWindow = WindowManager.CreateWindow("Boss module", DrawMainWindow, MainWindowClosed, MainWindowClosedByUser);
                _mainWindow.SizeHint = new(400, 400);
            }

            // update main window properties
            if (_mainWindow != null)
            {
                _mainWindow.Title = ActiveModule != null ? $"Boss module ({ActiveModule.GetType().Name})" : "Loaded boss modules";
                _mainWindow.Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
                if (WindowConfig.TrishaMode)
                    _mainWindow.Flags |= ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;
                if (WindowConfig.Lock)
                    _mainWindow.Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
            }

            // create or destroy plan window if needed
            bool showPlanWindow = WindowConfig.EnableTimerWindow && ActiveModule?.PlanConfig != null;
            if (_planWindow != null && !showPlanWindow)
            {
                Service.Log("[BMM] Closing plan window");
                WindowManager.CloseWindow(_planWindow);
            }
            else if (_planWindow == null && showPlanWindow)
            {
                Service.Log("[BMM] Opening plan window");
                _planWindow = WindowManager.CreateWindow("Cooldown plan", DrawPlanWindow, PlanWindowClosed, () => true);
                _planWindow.SizeHint = new(400, 400);
            }

            // update plan window properties
            if (_planWindow != null)
            {
                _planWindow.Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
                if (WindowConfig.TrishaMode)
                    _planWindow.Flags |= ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;
                if (WindowConfig.Lock)
                    _planWindow.Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
            }
        }

        private void DrawMainWindow()
        {
            if (ActiveModule != null)
            {
                try
                {
                    BossComponent.MovementHints? movementHints = WindowConfig.ShowWorldArrows ? new() : null;
                    ActiveModule.Draw(WindowConfig.RotateArena ? (Camera.Instance?.CameraAzimuth ?? 0) : 0, PartyState.PlayerSlot, movementHints);
                    DrawMovementHints(movementHints, WorldState.Party.Player()?.PosRot.Y ?? 0);
                }
                catch (Exception ex)
                {
                    Service.Log($"Boss module draw crashed: {ex}");
                    ActiveModule = null;
                }
            }
            else
            {
                foreach (var m in LoadedModules)
                {
                    var oidType = ModuleRegistry.FindByOID(m.PrimaryActor.OID)?.ObjectIDType;
                    var oidName = oidType?.GetEnumName(m.PrimaryActor.OID);
                    if (ImGui.Button($"{m.GetType()} ({m.PrimaryActor.InstanceID:X} '{m.PrimaryActor.Name}' {oidName})"))
                        ActiveModule = m;
                }
            }
        }

        private void DrawMovementHints(BossComponent.MovementHints? arrows, float y)
        {
            if (arrows == null || arrows.Count == 0 || Camera.Instance == null)
                return;

            Camera.Instance.BeginWorldWindow("movement_hints_overlay");
            foreach ((var start, var end, uint color) in arrows)
            {
                Vector3 start3 = new(start.X, y, start.Z);
                Vector3 end3 = new(end.X, y, end.Z);
                Camera.Instance.DrawWorldLine(start3, end3, color);
                var dir = Vector3.Normalize(end3 - start3);
                var arrowStart = end3 - 0.4f * dir;
                var offset = 0.07f * Vector3.Normalize(Vector3.Cross(Vector3.UnitY, dir));
                Camera.Instance.DrawWorldLine(arrowStart + offset, end3, color);
                Camera.Instance.DrawWorldLine(arrowStart - offset, end3, color);
            }
            Camera.Instance.EndWorldWindow();
        }

        private void MainWindowClosed()
        {
            Service.Log("[BMM] Main window closed");
            _mainWindow = null;
        }

        private bool MainWindowClosedByUser()
        {
            if (ActiveModule != null)
            {
                // show module list instead of boss module
                Service.Log("[BMM] Bossmod window closed by user, showing module list instead...");
                ActiveModule = null;
                return false;
            }
            else
            {
                // close main window
                Service.Log("[BMM] Bossmod window closed by user, disabling temporarily");
                return true;
            }
        }

        private void DrawPlanWindow()
        {
            if (ActiveModule?.PlanExecution == null)
                return;

            if (ImGui.Button("Show timeline"))
            {
                var timeline = new StateMachineVisualizer(ActiveModule.StateMachine);
                var w = WindowManager.CreateWindow($"{ActiveModule.GetType().Name} timeline", timeline.Draw, () => { }, () => true);
                w.SizeHint = new(600, 600);
                w.MinSize = new(100, 100);
            }
            ImGui.SameLine();
            ActiveModule.PlanConfig?.DrawSelectionUI(ActiveModule.Raid.Player()?.Class ?? Class.None, ActiveModule.StateMachine);

            ActiveModule.PlanExecution.Draw(ActiveModule.StateMachine);
        }

        private void PlanWindowClosed()
        {
            Service.Log("[BMM] Plan window closed");
            _planWindow = null;
        }
    }
}
