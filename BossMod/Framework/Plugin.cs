using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Boss Mod";

        private ConfigRoot _config;
        private CommandManager _commandManager { get; init; }

        private Network _network;
        private WorldStateGame _ws;
        private WorldStateLogger _debugLogger;
        private BossModuleManagerGame _bossmod;
        private Autorotation _autorotation;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface dalamud,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            dalamud.Create<Service>();
            Service.LogHandler = (string msg) => PluginLog.Log(msg);
            Service.LuminaGameData = Service.DataManager.GameData;
            //Service.Device = pluginInterface.UiBuilder.Device;
            Camera.Instance = new();
            Mouseover.Instance = new();

            _config = ConfigRoot.ReadConfig(dalamud);
            var generalCfg = _config.Get<GeneralConfig>();

            _commandManager = commandManager;
            _commandManager.AddHandler("/vbm", new CommandInfo(OnCommand) { HelpMessage = "Show boss mod config UI" });

            _network = new(generalCfg);
            _ws = new(_network);
            _debugLogger = new(_ws, generalCfg, dalamud.ConfigDirectory);
            _bossmod = new(_ws, _config);
            _autorotation = new(_network, _config, _bossmod);

            dalamud.UiBuilder.Draw += DrawUI;
            dalamud.UiBuilder.OpenConfigUi += OpenConfigUI;
        }

        public void Dispose()
        {
            WindowManager.Reset();
            _debugLogger.Dispose();
            _bossmod.Dispose();
            _network.Dispose();
            _autorotation.Dispose();
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
            }
        }

        private void OpenConfigUI()
        {
            var w = WindowManager.CreateWindow("Boss mod config", _config.Draw, () => { }, () => true);
            w.SizeHint = new Vector2(300, 300);
        }

        private void OpenDebugUI()
        {
            var ui = new DebugUI(_ws, _autorotation);
            var w = WindowManager.CreateWindow("Boss mod debug UI", ui.Draw, ui.Dispose, () => true);
            w.SizeHint = new Vector2(300, 200);
        }

        private void DrawUI()
        {
            Camera.Instance?.Update();
            _ws.Update();
            _bossmod.Update();
            _autorotation.Update();

            WindowManager.DrawAll();
        }
    }
}
