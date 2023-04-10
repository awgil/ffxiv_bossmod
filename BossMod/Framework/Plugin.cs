using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace BossMod
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Boss Mod";

        private CommandManager _commandManager { get; init; }

        private Network _network;
        private WorldStateGame _ws;
        private WorldStateLogger _debugLogger;
        private BossModuleManagerGame _bossmod;
        private Autorotation _autorotation;
        private AI.AIManager _ai;
        private AI.Broadcast _broadcast;
        private TimeSpan _prevUpdateTime;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface dalamud,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            dalamud.Create<Service>();
#if DEBUG
            Service.LogHandler = (string msg) => PluginLog.Log(msg);
#else
            Service.LogHandler = (string msg) => PluginLog.Debug(msg);
#endif
            Service.LuminaGameData = Service.DataManager.GameData;
            //Service.Device = pluginInterface.UiBuilder.Device;
            Service.Condition.ConditionChange += OnConditionChanged;
            MultiboxUnlock.Exec();
            Camera.Instance = new();
            Mouseover.Instance = new();

            Service.Config.Initialize();
            Service.Config.LoadFromFile(dalamud.ConfigFile);
            Service.Config.Modified += (_, _) => Service.Config.SaveToFile(dalamud.ConfigFile);

            ActionManagerEx.Instance = new(); // needs config

            _commandManager = commandManager;
            _commandManager.AddHandler("/vbm", new CommandInfo(OnCommand) { HelpMessage = "Show boss mod config UI" });

            _network = new(dalamud.ConfigDirectory);
            _ws = new(_network);
            _debugLogger = new(_ws, dalamud.ConfigDirectory);
            _bossmod = new(_ws);
            _autorotation = new(_bossmod);
            _ai = new(ActionManagerEx.Instance.InputOverride, _autorotation);
            _broadcast = new();

            dalamud.UiBuilder.DisableAutomaticUiHide = true;
            dalamud.UiBuilder.Draw += DrawUI;
            dalamud.UiBuilder.OpenConfigUi += OpenConfigUI;
        }

        public void Dispose()
        {
            Service.Condition.ConditionChange -= OnConditionChanged;
            WindowManager.Reset();
            _debugLogger.Dispose();
            _bossmod.Dispose();
            _network.Dispose();
            _ai.Dispose();
            _autorotation.Dispose();
            Mouseover.Instance?.Dispose();
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
                    OpenDebugUI();
                    break;
                case "cfg":
                    var output = Service.Config.ConsoleCommand(new ArraySegment<string>(split, 1, split.Length - 1));
                    foreach (var msg in output)
                        Service.ChatGui.Print(msg);
                    break;
            }
        }

        private void OpenConfigUI()
        {
            var ui = new ConfigUI(Service.Config, _ws);
            var w = WindowManager.CreateWindow("Boss mod config", ui.Draw, () => { }, () => true);
            w.SizeHint = new Vector2(300, 300);
        }

        private void OpenDebugUI()
        {
            var ui = new DebugUI(_ws, _autorotation, ActionManagerEx.Instance!.InputOverride);
            var w = WindowManager.CreateWindow("Boss mod debug UI", ui.Draw, ui.Dispose, () => true);
            w.SizeHint = new Vector2(300, 200);
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
                WindowManager.DrawAll();

            Camera.Instance?.DrawWorldPrimitives();
            _prevUpdateTime = DateTime.Now - tsStart;
        }

        private void OnConditionChanged(ConditionFlag flag, bool value)
        {
            Service.Log($"Condition chage: {flag}={value}");
        }
    }
}
