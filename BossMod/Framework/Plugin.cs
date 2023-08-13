using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;

namespace BossMod
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Boss Mod";

        private CommandManager _commandManager { get; init; }

        private Network _network;
        private WorldStateGame _ws;
        private ReplayRecorderWindow _recorder;
        private BossModuleManagerGame _bossmod;
        private Autorotation _autorotation;
        private AI.AIManager _ai;
        private AI.Broadcast _broadcast;
        private TimeSpan _prevUpdateTime;
        private MainDebugWindow _debugUI;

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
            Service.WindowSystem = new("vbm");
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

            var recorderSettings = Service.Config.Get<ReplayRecorderConfig>();
            recorderSettings.TargetDirectory = dalamud.ConfigDirectory;

            _network = new(dalamud.ConfigDirectory);
            _ws = new(_network);
            _recorder = new(_ws, recorderSettings);
            _recorder.Register();
            _bossmod = new(_ws);
            _autorotation = new(_bossmod);
            _ai = new(_autorotation);
            _broadcast = new();

            _debugUI = new(_ws, _autorotation);
            _debugUI.IsOpen = false;
            _debugUI.Register();

            dalamud.UiBuilder.DisableAutomaticUiHide = true;
            dalamud.UiBuilder.Draw += DrawUI;
            dalamud.UiBuilder.OpenConfigUi += OpenConfigUI;
        }

        public void Dispose()
        {
            Service.Condition.ConditionChange -= OnConditionChanged;
            _debugUI.Unregister();
            WindowManager.Reset();
            _recorder.Unregister();
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
                    _debugUI.OpenAndFocus();
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
            new SimpleActionWindow("Boss mod config", new ConfigUI(Service.Config, _ws).Draw, new(300, 300)).Register();
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
                WindowManager.DrawAll();
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
