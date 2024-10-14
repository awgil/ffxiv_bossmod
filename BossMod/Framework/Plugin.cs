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

    private readonly RotationDatabase _rotationDB;
    private readonly WorldState _ws;
    private readonly AIHints _hints;
    private readonly BossModuleManager _bossmod;
    private readonly ZoneModuleManager _zonemod;
    private readonly AIHintsBuilder _hintsBuilder;
    private readonly MovementOverride _movementOverride;
    private readonly ActionManagerEx _amex;
    private readonly WorldStateGameSync _wsSync;
    private readonly RotationModuleManager _rotation;
    private readonly AI.AIManager _ai;
    private readonly AI.Broadcast _broadcast;
    private readonly IPCProvider _ipc;
    private readonly DTRProvider _dtr;
    private TimeSpan _prevUpdateTime;

    // windows
    private readonly ConfigUI _configUI; // TODO: should be a proper window!
    private readonly BossModuleMainWindow _wndBossmod;
    private readonly BossModuleHintsWindow _wndBossmodHints;
    private readonly ReplayManagementWindow _wndReplay;
    private readonly UIRotationWindow _wndRotation;
    private readonly AI.AIWindow _wndAI;
    private readonly MainDebugWindow _wndDebug;

    public unsafe Plugin(IDalamudPluginInterface dalamud, ICommandManager commandManager, ISigScanner sigScanner, IDataManager dataManager)
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

        Service.Config.Initialize();
        Service.Config.LoadFromFile(dalamud.ConfigFile);
        Service.Config.Modified.Subscribe(() => Service.Config.SaveToFile(dalamud.ConfigFile));

        CommandManager = commandManager;
        CommandManager.AddHandler("/vbm", new CommandInfo(OnCommand) { HelpMessage = "Show boss mod settings UI" });
        CommandManager.AddHandler("/vbmai", new CommandInfo((cmd, args) => ParseAICommands(args.Split(' ', StringSplitOptions.RemoveEmptyEntries)))); //TODO: deprecated

        ActionDefinitions.Instance.UnlockCheck = QuestUnlocked; // ensure action definitions are initialized and set unlock check functor (we don't really store the quest progress in clientstate, for now at least)

        var qpf = (ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency;
        _rotationDB = new(new(dalamud.ConfigDirectory.FullName + "/autorot"), new(dalamud.AssemblyLocation.DirectoryName! + "/DefaultRotationPresets.json"));
        _ws = new(qpf, gameVersion);
        _hints = new();
        _bossmod = new(_ws);
        _zonemod = new(_ws);
        _hintsBuilder = new(_ws, _bossmod, _zonemod);
        _movementOverride = new();
        _amex = new(_ws, _hints, _movementOverride);
        _wsSync = new(_ws, _amex);
        _rotation = new(_rotationDB, _bossmod, _hints);
        _ai = new(_rotation, _amex, _movementOverride);
        _broadcast = new();
        _ipc = new(_rotation, _amex, _movementOverride, _ai);
        _dtr = new(_rotation, _ai);

        var replayDir = new DirectoryInfo(dalamud.ConfigDirectory.FullName + "/replays");
        _configUI = new(Service.Config, _ws, replayDir, _rotationDB);
        _wndBossmod = new(_bossmod, _zonemod);
        _wndBossmodHints = new(_bossmod, _zonemod);
        _wndReplay = new(_ws, _rotationDB, replayDir);
        _wndRotation = new(_rotation, _amex, () => OpenConfigUI("Autorotation Presets"));
        _wndAI = new(_ai);
        _wndDebug = new(_ws, _rotation, _amex, _hintsBuilder, dalamud);

        dalamud.UiBuilder.DisableAutomaticUiHide = true;
        dalamud.UiBuilder.Draw += DrawUI;
        dalamud.UiBuilder.OpenConfigUi += () => OpenConfigUI();

        _ = new ConfigChangelogWindow();
    }

    public void Dispose()
    {
        Service.Condition.ConditionChange -= OnConditionChanged;
        _wndDebug.Dispose();
        _wndAI.Dispose();
        _wndRotation.Dispose();
        _wndReplay.Dispose();
        _wndBossmodHints.Dispose();
        _wndBossmod.Dispose();
        _configUI.Dispose();
        _ipc.Dispose();
        _ai.Dispose();
        _rotation.Dispose();
        _wsSync.Dispose();
        _amex.Dispose();
        _movementOverride.Dispose();
        _hintsBuilder.Dispose();
        _zonemod.Dispose();
        _bossmod.Dispose();
        _dtr.Dispose();
        ActionDefinitions.Instance.Dispose();
        CommandManager.RemoveHandler("/vbm");
        CommandManager.RemoveHandler("/vbmai"); // TODO: deprecated
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
                var output = Service.Config.ConsoleCommand(split.AsSpan(1));
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
            case "ai":
                ParseAICommands(split.AsSpan(1));
                break;
        }
    }

    private void OpenConfigUI(string showTab = "")
    {
        _configUI.ShowTab(showTab);
        _ = new UISimpleWindow("Boss Mod Settings", _configUI.Draw, true, new(300, 300));
    }

    private void DrawUI()
    {
        var tsStart = DateTime.Now;

        var userPreventingCast = _movementOverride.IsMoveRequested() && !_amex.Config.PreventMovingWhileCasting;
        var maxCastTime = userPreventingCast ? 0 : _ai.ForceMovementIn;

        _dtr.Update();
        Camera.Instance?.Update();
        _wsSync.Update(_prevUpdateTime);
        _bossmod.Update();
        _zonemod.ActiveModule?.Update();
        _hintsBuilder.Update(_hints, PartyState.PlayerSlot, maxCastTime);
        _amex.QueueManualActions();
        _rotation.Update(_amex.AnimationLockDelayEstimate, _movementOverride.IsMoving());
        _ai.Update();
        _broadcast.Update();
        _amex.FinishActionGather();
        ExecuteHints();

        bool uiHidden = Service.GameGui.GameUiHidden || Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.WatchingCutscene];
        if (!uiHidden)
        {
            Service.WindowSystem?.Draw();
        }

        Camera.Instance?.DrawWorldPrimitives();
        _prevUpdateTime = DateTime.Now - tsStart;
    }

    private unsafe bool QuestUnlocked(uint link)
    {
        // see ActionManager.IsActionUnlocked
        var gameMain = FFXIVClientStructs.FFXIV.Client.Game.GameMain.Instance();
        return link == 0
            || Service.LuminaRow<Lumina.Excel.GeneratedSheets.TerritoryType>(gameMain->CurrentTerritoryTypeId)?.TerritoryIntendedUse == 31 // deep dungeons check is hardcoded in game
            || FFXIVClientStructs.FFXIV.Client.Game.UI.UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(link);
    }

    private unsafe void ExecuteHints()
    {
        _movementOverride.DesiredDirection = _hints.ForcedMovement;
        // update forced target, if needed (TODO: move outside maybe?)
        if (_hints.ForcedTarget != null)
        {
            var obj = _hints.ForcedTarget.SpawnIndex >= 0 ? FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[_hints.ForcedTarget.SpawnIndex].Value : null;
            if (obj != null && obj->EntityId != _hints.ForcedTarget.InstanceID)
                Service.Log($"[ExecHints] Unexpected new target: expected {_hints.ForcedTarget.InstanceID:X} at #{_hints.ForcedTarget.SpawnIndex}, but found {obj->EntityId:X}");
            FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()->Target = obj;
        }
        foreach (var s in _hints.StatusesToCancel)
        {
            var res = FFXIVClientStructs.FFXIV.Client.Game.StatusManager.ExecuteStatusOff(s.statusId, s.sourceId != 0 ? (uint)s.sourceId : 0xE0000000);
            Service.Log($"[ExecHints] Canceling status {s.statusId} from {s.sourceId:X} -> {res}");
        }
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
                if (cmd.Length <= 2)
                    PrintAutorotationHelp();
                else
                    ParseAutorotationSetCommand(cmd[2], false);
                break;
            case "toggle":
                ParseAutorotationSetCommand(cmd.Length > 2 ? cmd[2] : "", true);
                break;
            case "ui":
                _wndRotation.SetVisible(!_wndRotation.IsOpen);
                break;
            case "help":
            case "?":
            default:
                PrintAutorotationHelp();
                break;
        }
    }

    private void ParseAutorotationSetCommand(string presetName, bool toggle)
    {
        var preset = presetName.Length > 0 ? _rotation.Database.Presets.VisiblePresets.FirstOrDefault(p => p.Name == presetName) : RotationModuleManager.ForceDisable;
        if (preset != null)
        {
            var newPreset = toggle && _rotation.Preset == preset ? null : preset;
            Service.Log($"Console: {(toggle ? "toggle" : "set")} changes preset from '{_rotation.Preset?.Name ?? "<n/a>"}' to '{newPreset?.Name ?? "<n/a>"}'");
            _rotation.Preset = newPreset;
        }
        else
        {
            Service.ChatGui.PrintError($"Failed to find preset '{presetName}'");
        }
    }

    private void PrintAutorotationHelp()
    {
        Service.ChatGui.Print("Autorotation commands:");
        Service.ChatGui.Print("* /vbm ar clear - clear current preset; autorotation will do nothing unless plan is active");
        Service.ChatGui.Print("* /vbm ar disable - force disable autorotation; no actions will be executed automatically even if plan is active");
        Service.ChatGui.Print("* /vbm ar set Preset - start executing specified preset");
        Service.ChatGui.Print("* /vbm ar toggle - force disable autorotation if not already; otherwise clear overrides");
        Service.ChatGui.Print("* /vbm ar toggle Preset - start executing specified preset unless it's already active; clear otherwise");
        Service.ChatGui.Print("* /vbm ar ui - toggle autorotation ui");
    }

    private void ParseAICommands(ReadOnlySpan<string> cmd)
    {
        switch (cmd.Length > 0 ? cmd[0] : "")
        {
            case "on":
                _ai.Enabled = true;
                break;
            case "off":
                _ai.Enabled = false;
                break;
            case "toggle":
                _ai.Enabled ^= true;
                break;
            case "follow":
                if (cmd.Length < 1)
                {
                    PrintAIHelp();
                    return;
                }

                var masterString = cmd.Length > 1 ? $"{cmd[0]} {cmd[1]}" : cmd[0];
                var masterStringIsSlot = masterString.StartsWith("slot", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(masterString.Substring(4, 1)) : 0;
                var master = masterStringIsSlot > 0 ? (masterStringIsSlot - 1, _ws.Party[masterStringIsSlot - 1]) : _ws.Party.WithSlot().FirstOrDefault(x => x.Item2.Name.Equals(masterString, StringComparison.OrdinalIgnoreCase));
                if (master.Item2 is null)
                {
                    Service.ChatGui.PrintError($"[AI] [Follow] Error: can't find {masterString} in our party");
                    return;
                }
                _ai.SwitchToFollow(master.Item1);
                _ai.Enabled = true;
                break;
            case "ui":
                _wndAI.SetVisible(!_wndAI.IsOpen);
                break;
            case "help":
            case "?":
            case "":
                PrintAIHelp();
                break;
            default:
                // TODO: this should really be removed, it's synonym for /vbm cfg AIConfig ...
                List<string> list = [];
                list.Add("AIConfig");
                list.AddRange(cmd);

                if (list.Count == 2)
                {
                    //toggle
                    var result = Service.Config.ConsoleCommand(list.AsSpan());
                    if (bool.TryParse(result[0], out var resultBool))
                    {
                        list.Add((!resultBool).ToString());
                        Service.Config.ConsoleCommand(list.AsSpan());
                    }
                    else
                    {
                        Service.Log($"[AI] Unknown command: {cmd[0]}");
                    }
                }
                else if (list.Count == 3)
                {
                    //set
                    var onOffReplace = list[2].Replace("on", "true", StringComparison.InvariantCultureIgnoreCase).Replace("off", "false", StringComparison.InvariantCultureIgnoreCase);
                    if (list[2] == "on")
                        list[2] = "true";
                    else if (list[2] == "off")
                        list[2] = "false";

                    if (Service.Config.ConsoleCommand(list.AsSpan()).Count > 0)
                        Service.Log($"[AI] Unknown command: {cmd[0]}");
                }
                else
                {
                    Service.Log($"[AI] Unknown command: {cmd[0]}");
                }

                break;
        }
    }

    private void PrintAIHelp()
    {
        Service.ChatGui.Print("AI commands:");
        Service.ChatGui.Print("* /vbm ai on - enable AI mode");
        Service.ChatGui.Print("* /vbm ai off - disable AI mode");
        Service.ChatGui.Print("* /vbm ai toggle - toggle AI mode");
        Service.ChatGui.Print("* /vbm ai follow <name> - enable AI mode and follow party member with specified name");
        Service.ChatGui.Print("* /vbm ai follow slot<N> - enable AI mode and follow party member at slot #N (1-8)");
        Service.ChatGui.Print("* /vbm ai ui - toggle AI ui");
    }

    private void OnConditionChanged(ConditionFlag flag, bool value)
    {
        Service.Log($"Condition chage: {flag}={value}");
    }
}
