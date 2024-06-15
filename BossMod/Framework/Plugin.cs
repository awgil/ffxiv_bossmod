using BossMod.Autorotation;
using Dalamud.Common;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.IO;
using System.Reflection;

namespace BossMod;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Boss Mod";

    private ICommandManager CommandManager { get; init; }

    private readonly WorldState _ws;
    private readonly AIHints _hints;
    private readonly BossModuleManager _bossmod;
    private readonly AIHintsBuilder _hintsBuilder;
    private readonly ActionManagerEx _amex;
    private readonly WorldStateGameSync _wsSync;
    private readonly RotationModuleManager _rotation;
    private readonly AutorotationLegacy _autorotation;
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

    public unsafe Plugin(DalamudPluginInterface dalamud, ICommandManager commandManager, ISigScanner sigScanner, IDataManager dataManager)
    {
        if (!dalamud.ConfigDirectory.Exists)
            dalamud.ConfigDirectory.Create();
        var dalamudRoot = dalamud.GetType().Assembly.
                GetType("Dalamud.Service`1", true)!.MakeGenericType(dalamud.GetType().Assembly.GetType("Dalamud.Dalamud", true)!).
                GetMethod("Get")!.Invoke(null, BindingFlags.Default, null, [], null);
        var dalamudStartInfo = dalamudRoot?.GetType().GetProperty("StartInfo", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dalamudRoot) as DalamudStartInfo;
        var gameVersion = dalamudStartInfo?.GameVersion?.ToString() ?? "unknown";
        InteropGenerator.Runtime.Resolver.GetInstance.Setup(sigScanner.SearchBase, gameVersion, new(dalamud.ConfigDirectory.FullName + "/cs.json"));
        FFXIVClientStructs.Interop.Generated.Addresses.Register();
        InteropGenerator.Runtime.Resolver.GetInstance.Resolve();

        dalamud.Create<Service>();
        Service.LogHandler = (string msg) => Service.Logger.Debug(msg);
        Service.LuminaGameData = dataManager.GameData;
        Service.WindowSystem = new("vbm");
        //Service.Device = pluginInterface.UiBuilder.Device;
        Service.Condition.ConditionChange += OnConditionChanged;
        MultiboxUnlock.Exec();
        Network.IDScramble.Initialize();
        Camera.Instance = new();

        var manager = Service.SigScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 0F 28 F0 45 0F 57 C0");
        Service.Log($"foo: {manager:X}");

        Service.Config.Initialize();
        Service.Config.LoadFromFile(dalamud.ConfigFile);
        Service.Config.Modified.Subscribe(() => Service.Config.SaveToFile(dalamud.ConfigFile));

        CommandManager = commandManager;
        CommandManager.AddHandler("/vbm", new CommandInfo(OnCommand) { HelpMessage = "Show boss mod config UI" });

        var rotationRoot = new DirectoryInfo(dalamud.ConfigDirectory.FullName + "/autorot");
        rotationRoot.Create();

        var actionDefs = ActionDefinitions.Instance; // ensure action definitions are initialized
        var qpf = (ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency;
        _ws = new(qpf, gameVersion);
        _hints = new();
        _bossmod = new(_ws);
        _hintsBuilder = new(_ws, _bossmod);
        _amex = new(_ws, _hints);
        _wsSync = new(_ws, _amex);
        _rotation = new(rotationRoot, _bossmod, _hints);
        _autorotation = new(_bossmod, _hints, _amex);
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
        _ai.Dispose();
        _autorotation.Dispose();
        _rotation.Dispose();
        _wsSync.Dispose();
        _amex.Dispose();
        _hintsBuilder.Dispose();
        _bossmod.Dispose();
        ActionDefinitions.Instance.Dispose();
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
            case "ar":
                ParseAutorotationCommands(split);
                break;
        }
    }

    private void OpenConfigUI()
    {
        _ = new UISimpleWindow("Boss mod config", new ConfigUI(Service.Config, _ws, _rotation.Database).Draw, true, new(300, 300));
    }

    private void DrawUI()
    {
        var tsStart = DateTime.Now;

        Camera.Instance?.Update();
        _wsSync.Update(_prevUpdateTime);
        var queue = _amex.StartActionGather();
        _bossmod.Update();
        _hintsBuilder.Update(_hints, PartyState.PlayerSlot);
        _rotation.Update(_hints?.ForcedTarget ?? _ws.Actors.Find(_ws.Party.Player()?.TargetID ?? 0), queue);
        _autorotation.Update(queue);
        _ai.Update(queue);
        _broadcast.Update();
        _amex.FinishActionGather();

        bool uiHidden = Service.GameGui.GameUiHidden || Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.WatchingCutscene];
        if (!uiHidden)
        {
            Service.WindowSystem?.Draw();
        }

        Camera.Instance?.DrawWorldPrimitives();
        _prevUpdateTime = DateTime.Now - tsStart;
    }

    private void ParseAutorotationCommands(string[] cmd)
    {
        switch (cmd.Length > 1 ? cmd[1] : "")
        {
            case "clear":
                Service.Log($"Console: clearing autorotation preset '{_rotation.Preset?.Name ?? "<n/a>"}'");
                _rotation.Preset = null;
                break;
            case "disable":
                Service.Log($"Console: force-disabling from preset '{_rotation.Preset?.Name ?? "<n/a>"}'");
                _rotation.Preset = RotationModuleManager.ForceDisable;
                break;
            case "set":
                ParseAutorotationSetCommand(cmd, false);
                break;
            case "toggle":
                ParseAutorotationSetCommand(cmd, true);
                break;
            default:
                PrintAutorotationHelp();
                break;
        }
    }

    private void ParseAutorotationSetCommand(string[] cmd, bool toggle)
    {
        if (cmd.Length <= 2)
        {
            PrintAutorotationHelp();
        }
        else if (_rotation.Database.Presets.FirstOrDefault(p => p.Name == cmd[2]) is var preset && preset == null)
        {
            Service.ChatGui.PrintError($"Failed to find preset '{cmd[2]}'");
        }
        else
        {
            var newPreset = toggle && _rotation.Preset == preset ? null : preset;
            Service.Log($"Console: {(toggle ? "toggle" : "set")} changes preset from '{_rotation.Preset?.Name ?? "<n/a>"}' to '{newPreset?.Name ?? "<n/a>"}'");
            _rotation.Preset = newPreset;
        }
    }

    private void PrintAutorotationHelp()
    {
        Service.ChatGui.Print("Autorotation commands:");
        Service.ChatGui.Print("* /vbm ar clear - clear current preset; autorotation will do nothing unless plan is active");
        Service.ChatGui.Print("* /vbm ar disable - force disable autorotation; no actions will be executed automatically even if plan is active");
        Service.ChatGui.Print("* /vbm ar set Preset - start executing specified preset");
        Service.ChatGui.Print("* /vbm ar toggle Preset - start executing specified preset unless it's already active; clear otherwise");
    }

    private void OnConditionChanged(ConditionFlag flag, bool value)
    {
        Service.Log($"Condition chage: {flag}={value}");
    }
}
