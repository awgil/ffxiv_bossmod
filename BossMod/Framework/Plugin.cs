using Dalamud.Common;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Reflection;

namespace BossMod
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Boss Mod";

        private ICommandManager _commandManager { get; init; }

        private Network.Logger _network;
        private WorldStateGame _ws;
        private BossModuleManager _bossmod;
        private Autorotation _autorotation;
        private AI.AIManager _ai;
        private AI.Broadcast _broadcast;
        private IPCProvider _ipc;
        private TimeSpan _prevUpdateTime;

        // windows
        private BossModuleMainWindow _wndBossmod;
        private BossModulePlanWindow _wndBossmodPlan;
        private BossModuleHintsWindow _wndBossmodHints;
        private ReplayManagementWindow _wndReplay;
        private MainDebugWindow _wndDebug;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface dalamud,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            var dalamudRoot = dalamud.GetType().Assembly.
                    GetType("Dalamud.Service`1", true)!.MakeGenericType(dalamud.GetType().Assembly.GetType("Dalamud.Dalamud", true)!).
                    GetMethod("Get")!.Invoke(null, BindingFlags.Default, null, Array.Empty<object>(), null);
            var dalamudStartInfo = dalamudRoot?.GetType().GetProperty("StartInfo", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dalamudRoot) as DalamudStartInfo;

            dalamud.Create<Service>();
            Service.LogHandler = (string msg) => Service.Logger.Debug(msg);
            Service.LuminaGameData = Service.DataManager.GameData;
            Service.WindowSystem = new("vbm");
            //Service.Device = pluginInterface.UiBuilder.Device;
            Service.Condition.ConditionChange += OnConditionChanged;
            MultiboxUnlock.Exec();
            Network.IDScramble.Initialize();
            Camera.Instance = new();

            Service.Config.Initialize();
            Service.Config.LoadFromFile(dalamud.ConfigFile);
            Service.Config.Modified += (_, _) => Service.Config.SaveToFile(dalamud.ConfigFile);

            ActionManagerEx.Instance = new(); // needs config

            _commandManager = commandManager;
            _commandManager.AddHandler("/vbm", new CommandInfo(OnCommand) { HelpMessage = "Show boss mod config UI" });

            _network = new(dalamud.ConfigDirectory);
            _ws = new(dalamudStartInfo?.GameVersion?.ToString() ?? "unknown");
            _bossmod = new(_ws);
            _autorotation = new(_bossmod);
            _ai = new(_autorotation);
            _broadcast = new();
            _ipc = new(_autorotation);

            _wndBossmod = new(_bossmod);
            _wndBossmodPlan = new(_bossmod);
            _wndBossmodHints = new(_bossmod);
            _wndReplay = new(_ws, dalamud.ConfigDirectory);
            _wndDebug = new(_ws, _autorotation);

            dalamud.UiBuilder.DisableAutomaticUiHide = true;
            dalamud.UiBuilder.Draw += DrawUI;
            dalamud.UiBuilder.OpenConfigUi += OpenConfigUI;
        }

        public void Dispose()
        {
            Service.Condition.ConditionChange -= OnConditionChanged;
            _wndDebug.Dispose();
            _wndReplay.Dispose();
            _wndBossmodHints.Dispose();
            _wndBossmodPlan.Dispose();
            _wndBossmod.Dispose();
            _ipc.Dispose();
            _bossmod.Dispose();
            _network.Dispose();
            _ai.Dispose();
            _autorotation.Dispose();
            _ws.Dispose();
            ActionManagerEx.Instance?.Dispose();
            _commandManager.RemoveHandler("/vbm");
        }

        private void OnCommand(string cmd, string args)
        {
            Service.Log($"OnCommand: {cmd} {args}");
            var split = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
            {
                OpenConfigUI();
                return;
            }

            switch (split[0])
            {
                case "d":
                    _wndDebug.IsOpen = true;
                    _wndDebug.BringToFront();
                    break;
                case "cfg":
                    var output = Service.Config.ConsoleCommand(new ArraySegment<string>(split, 1, split.Length - 1));
                    foreach (var msg in output)
                        Service.ChatGui.Print(msg);
                    break;
                case "gc":
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    break;
                case "r":
                    _wndReplay.SetVisible(!_wndReplay.IsOpen);
                    break;
            }
        }

        private void OpenConfigUI()
        {
            new UISimpleWindow("Boss mod config", new ConfigUI(Service.Config, _ws).Draw, true, new(300, 300));
        }

        private void DrawUI()
        {
            var tsStart = DateTime.Now;

            Camera.Instance?.Update();
            _ws.Update(_prevUpdateTime);
            _bossmod.Update();
            _autorotation.Update();
            _ai.Update();
            _broadcast.Update();

            bool uiHidden = Service.GameGui.GameUiHidden || Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.WatchingCutscene];
            if (!uiHidden)
            {
                Service.WindowSystem?.Draw();
            }

            Camera.Instance?.DrawWorldPrimitives();
            _prevUpdateTime = DateTime.Now - tsStart;
        }

        private void OnConditionChanged(ConditionFlag flag, bool value)
        {
            Service.Log($"Condition chage: {flag}={value}");
        }
    }
}
