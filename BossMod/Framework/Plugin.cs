using Dalamud.Common;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Reflection;

namespace BossMod;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Boss Mod";

    private ICommandManager CommandManager { get; init; }

    private readonly WorldState _ws;
    private readonly WorldStateGameSync _wsSync;
    private readonly BossModuleManager _bossmod;
    private readonly Autorotation _autorotation;
    private readonly AI.AIManager _ai;
    private readonly AI.Broadcast _broadcast;
    private readonly IPCProvider _ipc;
    private TimeSpan _prevUpdateTime;

    // windows
    private readonly BossModuleMainWindow _wndBossmod;
    private readonly BossModulePlanWindow _wndBossmodPlan;
    private readonly BossModuleHintsWindow _wndBossmodHints;
    private readonly ReplayManagementWindow _wndReplay;
    private readonly MainDebugWindow _wndDebug;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface dalamud,
        [RequiredVersion("1.0")] ICommandManager commandManager)
    {
        var dalamudRoot = dalamud.GetType().Assembly.
                GetType("Dalamud.Service`1", true)!.MakeGenericType(dalamud.GetType().Assembly.GetType("Dalamud.Dalamud", true)!).
                GetMethod("Get")!.Invoke(null, BindingFlags.Default, null, [], null);
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
        Service.Config.Modified.Subscribe(() => Service.Config.SaveToFile(dalamud.ConfigFile));

        BozjaInterop.Instance = new();
        ActionManagerEx.Instance = new(); // needs config

        CommandManager = commandManager;
        CommandManager.AddHandler("/vbm", new CommandInfo(OnCommand) { HelpMessage = "Show boss mod config UI" });

        _ws = new(Utils.FrameQPF(), dalamudStartInfo?.GameVersion?.ToString() ?? "unknown");
        _wsSync = new(_ws);
        _bossmod = new(_ws);
        _autorotation = new(_bossmod);
        _ai = new(_autorotation);
        _broadcast = new();
        _ipc = new(_autorotation);

        _wndBossmod = new(_bossmod);
        _wndBossmodPlan = new(_bossmod);
        _wndBossmodHints = new(_bossmod);
        _wndReplay = new(_ws, new(dalamud.ConfigDirectory.FullName + "/replays"));
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
        _ai.Dispose();
        _autorotation.Dispose();
        _wsSync.Dispose();
        ActionManagerEx.Instance?.Dispose();
        BozjaInterop.Instance?.Dispose();
        CommandManager.RemoveHandler("/vbm");
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
        _ = new UISimpleWindow("Boss mod config", new ConfigUI(Service.Config, _ws).Draw, true, new(300, 300));
    }

    private void DrawUI()
    {
        var tsStart = DateTime.Now;

        Camera.Instance?.Update();
        _wsSync.Update(_prevUpdateTime);
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
