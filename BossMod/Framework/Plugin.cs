using Dalamud.Game;
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
        private InputOverride _inputOverride;
        private Autorotation _autorotation;
        private AI.AIManager _ai;
        private AI.Broadcast _broadcast = new();
        private TimeSpan _prevUpdateTime;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface dalamud,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            dalamud.Create<Service>();
            Service.LogHandler = (string msg) => PluginLog.Log(msg);
            Service.LuminaGameData = Service.DataManager.GameData;
            //Service.Device = pluginInterface.UiBuilder.Device;
            MultiboxUnlock.Exec();
            Camera.Instance = new();
            Mouseover.Instance = new();

            Service.Config.LoadFromFile(dalamud.ConfigFile);
            Service.Config.Modified += (_, _) => Service.Config.SaveToFile(dalamud.ConfigFile);

            _commandManager = commandManager;
            _commandManager.AddHandler("/vbm", new CommandInfo(OnCommand) { HelpMessage = "Show boss mod config UI" });

            _network = new(dalamud.ConfigDirectory);
            _ws = new(_network);
            _debugLogger = new(_ws, dalamud.ConfigDirectory);
            _bossmod = new(_ws);
            _inputOverride = new();
            _autorotation = new(_network, _bossmod, _inputOverride);
            _ai = new(_ws, _inputOverride, _autorotation);

            Service.Framework.Update += Update;
            dalamud.UiBuilder.Draw += DrawUI;
            dalamud.UiBuilder.OpenConfigUi += OpenConfigUI;
        }

        public void Dispose()
        {
            WindowManager.Reset();
            _debugLogger.Dispose();
            _bossmod.Dispose();
            _network.Dispose();
            _ai.Dispose();
            _autorotation.Dispose();
            _inputOverride.Dispose();
            _commandManager.RemoveHandler("/vbm");
            Service.Framework.Update -= Update;
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
            var ui = new DebugUI(_ws, _autorotation, _inputOverride);
            var w = WindowManager.CreateWindow("Boss mod debug UI", ui.Draw, ui.Dispose, () => true);
            w.SizeHint = new Vector2(300, 200);
        }

        private void Update(Framework fwk)
        {
            _broadcast.Update();
        }

        private void DrawUI()
        {
            var tsStart = DateTime.Now;

            Camera.Instance?.Update();
            _ws.Update(_prevUpdateTime);
            _bossmod.Update();

            // TODO: ai and autorotation are currently somewhat tightly coupled, unfortunately
            _autorotation.UpdatePotentialTargets();
            var target = _ai.UpdateBeforeRotation();
            _autorotation.Update(target);
            _ai.UpdateAfterRotation(target);

            WindowManager.DrawAll();

            _prevUpdateTime = DateTime.Now - tsStart;
        }
    }
}
