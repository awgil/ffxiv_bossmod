using BossMod.Autorotation;
using Dalamud.Common;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System.IO;
using System.Reflection;

namespace BossMod;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Boss Mod";

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
    private readonly SlashCommandProvider _slashCmd;
    private TimeSpan _prevUpdateTime;
    private DateTime _throttleJump;
    private DateTime _throttleInteract;

    // windows
    private readonly ConfigUI _configUI; // TODO: should be a proper window!
    private readonly BossModuleMainWindow _wndBossmod;
    private readonly BossModuleHintsWindow _wndBossmodHints;
    private readonly ZoneModuleWindow _wndZone;
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
        Service.LogHandlerDebug = (string msg) => Service.Logger.Debug(msg);
        Service.LogHandlerVerbose = (string msg) => Service.Logger.Verbose(msg);
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

        ActionDefinitions.Instance.UnlockCheck = QuestUnlocked; // ensure action definitions are initialized and set unlock check functor (we don't really store the quest progress in clientstate, for now at least)

        var qpf = (ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency;
        _rotationDB = new(new(dalamud.ConfigDirectory.FullName + "/autorot"), new(dalamud.AssemblyLocation.DirectoryName! + "/DefaultRotationPresets.json"));
        _ws = new(qpf, gameVersion);
        _hints = new();
        _bossmod = new(_ws);
        _zonemod = new(_ws);
        _hintsBuilder = new(_ws, _bossmod, _zonemod);
        _movementOverride = new(dalamud);
        _amex = new(_ws, _hints, _movementOverride);
        _wsSync = new(_ws, _amex);
        _rotation = new(_rotationDB, _bossmod, _hints);
        _ai = new(_rotation, _amex, _movementOverride);
        _broadcast = new();
        _ipc = new(_rotation, _amex, _movementOverride, _ai);
        _dtr = new(_rotation, _ai);
        _slashCmd = new(commandManager, "/vbm");

        var replayDir = new DirectoryInfo(dalamud.ConfigDirectory.FullName + "/replays");
        _configUI = new(Service.Config, _ws, replayDir, _rotationDB);
        _wndBossmod = new(_bossmod, _zonemod);
        _wndBossmodHints = new(_bossmod, _zonemod);
        _wndZone = new(_zonemod);
        _wndReplay = new(_ws, _bossmod, _rotationDB, replayDir);
        _wndRotation = new(_rotation, _amex, () => OpenConfigUI("Autorotation Presets"));
        _wndAI = new(_ai);
        _wndDebug = new(_ws, _rotation, _zonemod, _amex, _movementOverride, _hintsBuilder, dalamud);

        dalamud.UiBuilder.DisableAutomaticUiHide = true;
        dalamud.UiBuilder.Draw += DrawUI;
        dalamud.UiBuilder.OpenMainUi += () => OpenConfigUI();
        dalamud.UiBuilder.OpenConfigUi += () => OpenConfigUI();
        RegisterSlashCommands();

        _ = new ConfigChangelogWindow();
    }

    public void Dispose()
    {
        Service.Condition.ConditionChange -= OnConditionChanged;
        _wndDebug.Dispose();
        _wndAI.Dispose();
        _wndRotation.Dispose();
        _wndReplay.Dispose();
        _wndZone.Dispose();
        _wndBossmodHints.Dispose();
        _wndBossmod.Dispose();
        _configUI.Dispose();
        _slashCmd.Dispose();
        _dtr.Dispose();
        _ipc.Dispose();
        _ai.Dispose();
        _rotation.Dispose();
        _wsSync.Dispose();
        _amex.Dispose();
        _movementOverride.Dispose();
        _hintsBuilder.Dispose();
        _zonemod.Dispose();
        _bossmod.Dispose();
        ActionDefinitions.Instance.Dispose();
    }

    private void RegisterSlashCommands()
    {
        _slashCmd.SetSimpleHandler("show boss mod settings UI", () => OpenConfigUI());
        _slashCmd.AddSubcommand("r").SetSimpleHandler("show/hide replay management window", () => _wndReplay.SetVisible(!_wndReplay.IsOpen));
        RegisterAutorotationSlashCommands(_slashCmd.AddSubcommand("ar"));
        RegisterAISlashCommands(_slashCmd.AddSubcommand("ai"));
        _slashCmd.AddSubcommand("cfg").SetComplexHandler("<config-type> <field> [<value>]", "query or modify configuration setting", args =>
        {
            var output = Service.Config.ConsoleCommand(args);
            foreach (var msg in output)
                Service.ChatGui.Print(msg);
            return true;
        });
        _slashCmd.AddSubcommand("d").SetSimpleHandler("show debug UI", _wndDebug.OpenAndBringToFront);
        _slashCmd.AddSubcommand("gc").SetSimpleHandler("execute C# garbage collector", () =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        });

        _slashCmd.Register();
        _slashCmd.RegisterAlias("/vbmai", "ai"); // TODO: deprecated
    }

    private void RegisterAutorotationSlashCommands(SlashCommandHandler cmd)
    {
        void SetOrToggle(Preset preset, bool toggle)
        {
            var newPreset = toggle && _rotation.Preset == preset ? null : preset;
            Service.Log($"Console: {(toggle ? "toggle" : "set")} changes preset from '{_rotation.Preset?.Name ?? "<n/a>"}' to '{newPreset?.Name ?? "<n/a>"}'");
            _rotation.Preset = newPreset;
        }

        void SetOrToggleByName(ReadOnlySpan<char> presetName, bool toggle)
        {
            var preset = _rotation.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
                SetOrToggle(preset, toggle);
            else
                Service.ChatGui.PrintError($"Failed to find preset '{presetName}'");
        }

        cmd.SetSimpleHandler("toggle autorotation ui", () => _wndRotation.SetVisible(!_wndRotation.IsOpen));
        cmd.AddSubcommand("clear").SetSimpleHandler("clear current preset; autorotation will do nothing unless plan is active", () =>
        {
            Service.Log($"Console: clearing autorotation preset '{_rotation.Preset?.Name ?? "<n/a>"}'");
            _rotation.Preset = null;
        });
        cmd.AddSubcommand("disable").SetSimpleHandler("force disable autorotation; no actions will be executed automatically even if plan is active", () =>
        {
            Service.Log($"Console: force-disabling from preset '{_rotation.Preset?.Name ?? "<n/a>"}'");
            _rotation.Preset = RotationModuleManager.ForceDisable;
        });
        cmd.AddSubcommand("set").SetComplexHandler("<preset>", "start executing specified preset", preset =>
        {
            SetOrToggleByName(preset, false);
            return true;
        });
        var toggle = cmd.AddSubcommand("toggle");
        toggle.SetSimpleHandler("force disable autorotation if not already; otherwise clear overrides", () => SetOrToggle(RotationModuleManager.ForceDisable, true));
        toggle.SetComplexHandler("<preset>", "start executing specified preset unless it's already active; clear otherwise", preset =>
        {
            SetOrToggleByName(preset, true);
            return true;
        });
    }

    private void RegisterAISlashCommands(SlashCommandHandler cmd)
    {
        cmd.SetSimpleHandler("toggle AI ui", () => _wndAI.SetVisible(!_wndAI.IsOpen));
        cmd.AddSubcommand("on").SetSimpleHandler("enable AI mode", () => _ai.Enabled = true);
        cmd.AddSubcommand("off").SetSimpleHandler("disable AI mode", () => _ai.Enabled = false);
        cmd.AddSubcommand("toggle").SetSimpleHandler("toggle AI mode", () => _ai.Enabled ^= true);
        cmd.AddSubcommand("follow").SetComplexHandler("<name>/slot<N>", "enable AI mode and follow party member with specified name or at specified slot", masterString =>
        {
            var masterSlot = masterString.StartsWith("slot", StringComparison.OrdinalIgnoreCase) ? int.Parse(masterString[4..]) - 1 : _ws.Party.FindSlot(masterString);
            if (_ws.Party[masterSlot] != null)
            {
                _ai.SwitchToFollow(masterSlot);
                _ai.Enabled = true;
            }
            else
            {
                Service.ChatGui.PrintError($"[AI] [Follow] Error: can't find {masterString} in our party");
            }
            return true;
        });

        // TODO: this should really be removed, it's a weird synonym for /vbm cfg AIConfig ...
        cmd.SetComplexHandler("", "", args =>
        {
            Span<Range> ranges = stackalloc Range[2];
            var numRanges = args.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (numRanges == 1)
            {
                // toggle
                var value = Service.Config.ConsoleCommand($"AIConfig {args}");
                return bool.TryParse(value[0], out var boolValue) && Service.Config.ConsoleCommand($"AIConfig {args} {!boolValue}").Count == 0;
            }
            else if (numRanges == 2)
            {
                // set
                var value = args[ranges[1]];
                if (value.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                    value = "true";
                else if (value.Equals("off", StringComparison.InvariantCultureIgnoreCase))
                    value = "false";
                return Service.Config.ConsoleCommand($"AIConfig {args[ranges[0]]} {value}").Count == 0;
            }
            return false;
        });
    }

    private void OpenConfigUI(string showTab = "")
    {
        _configUI.ShowTab(showTab);
        _ = new UISimpleWindow("Boss Mod Settings", _configUI.Draw, true, new(300, 300));
    }

    private void DrawUI()
    {
        var tsStart = DateTime.Now;
        var moveImminent = _movementOverride.IsMoveRequested() && (!_amex.Config.PreventMovingWhileCasting || _movementOverride.IsForceUnblocked());

        _dtr.Update();
        Camera.Instance?.Update();
        _wsSync.Update(_prevUpdateTime);
        _bossmod.Update();
        _zonemod.ActiveModule?.Update();
        _hintsBuilder.Update(_hints, PartyState.PlayerSlot, moveImminent);
        _amex.QueueManualActions();
        _rotation.Update(_amex.AnimationLockDelayEstimate, _movementOverride.IsMoving());
        _ai.Update();
        _broadcast.Update();
        _amex.FinishActionGather();

        bool uiHidden = Service.GameGui.GameUiHidden || Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.WatchingCutscene];
        if (!uiHidden)
        {
            Service.WindowSystem?.Draw();
        }

        ExecuteHints();

        Camera.Instance?.DrawWorldPrimitives();
        _prevUpdateTime = DateTime.Now - tsStart;
    }

    private unsafe bool QuestUnlocked(uint link)
    {
        // see ActionManager.IsActionUnlocked
        var gameMain = FFXIVClientStructs.FFXIV.Client.Game.GameMain.Instance();
        return link == 0
            || Service.LuminaRow<Lumina.Excel.Sheets.TerritoryType>(gameMain->CurrentTerritoryTypeId)?.TerritoryIntendedUse.RowId == 31 // deep dungeons check is hardcoded in game
            || FFXIVClientStructs.FFXIV.Client.Game.UI.UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(link);
    }

    private unsafe void ExecuteHints()
    {
        _movementOverride.DesiredDirection = _hints.ForcedMovement;
        _movementOverride.MisdirectionThreshold = _hints.MisdirectionThreshold;
        // update forced target, if needed (TODO: move outside maybe?)
        if (_hints.ForcedTarget != null && _hints.ForcedTarget.IsTargetable)
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
        if (_hints.WantJump && _ws.CurrentTime > _throttleJump)
        {
            //Service.Log($"[ExecHints] Jumping...");
            FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->UseAction(FFXIVClientStructs.FFXIV.Client.Game.ActionType.GeneralAction, 2);
            _throttleJump = _ws.CurrentTime.AddMilliseconds(100);
        }

        if (CheckInteractRange(_ws.Party.Player(), _hints.InteractWithTarget))
        {
            // many eventobj interactions "immediately" start some cast animation (delayed by server roundtrip), and if we keep trying to move toward the target after sending the interact request, it will be canceled and force us to start over
            _movementOverride.DesiredDirection = default;

            if (_amex.EffectiveAnimationLock == 0 && _ws.CurrentTime >= _throttleInteract)
            {
                FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()->InteractWithObject(GetActorObject(_hints.InteractWithTarget), false);
                _throttleInteract = _ws.FutureTime(0.1f);
            }
        }
    }

    private unsafe bool CheckInteractRange(Actor? player, Actor? target)
    {
        var playerObj = GetActorObject(player);
        var targetObj = GetActorObject(target);
        if (playerObj == null || targetObj == null)
            return false;

        // treasure chests have no client-side interact range check at all; just assume they use the standard "small" range, seems to be accurate from testing
        if (targetObj->ObjectKind is FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Treasure)
            return player?.DistanceToHitbox(target) <= 2.09f;

        return EventFramework.Instance()->CheckInteractRange(playerObj, targetObj, 1, false);
    }

    private unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GetActorObject(Actor? actor)
    {
        if (actor == null)
            return null;

        var obj = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[actor.SpawnIndex].Value;
        if (obj == null || obj->GetGameObjectId() != actor.InstanceID)
            return null;

        return obj;
    }

    private void OnConditionChanged(ConditionFlag flag, bool value)
    {
        Service.Log($"Condition chage: {flag}={value}");
    }
}
