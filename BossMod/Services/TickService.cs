using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Dev;
using BossMod.Interfaces;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal class TickService : DisposableMediatorSubscriberBase, IHostedService
{
    readonly IWindowSystem windowSystem;

    readonly RotationDatabase _rotationDB;
    readonly WorldState _ws;
    readonly AIHints _hints;
    readonly BossModuleManager _bossmod;
    readonly ZoneModuleManager _zonemod;
    readonly AIHintsBuilder _hintsBuilder;
    readonly IMovementOverride _movementOverride;
    readonly IAmex _amex;
    readonly IWorldStateGameSync _wsSync;
    readonly RotationModuleManager _rotation;
    readonly IPCProvider _ipc;
    readonly DTRProvider _dtr;
    readonly SlashCommandProvider _slashCmd;
    readonly MultiboxManager _mbox;
    TimeSpan _prevUpdateTime;
    readonly PackLoader _packs;
    private readonly IDalamudPluginInterface dalamud;
    private readonly IUiBuilder uiBuilder;
    private readonly ICondition condition;
    private readonly IPluginLog logger;
    private readonly IHintExecutor _hintExecutor;

    private readonly BossModuleMainWindow _wndBossmod;
    private readonly BossModuleHintsWindow _wndBossmodHints;
    private readonly ZoneModuleWindow _wndZone;
    private readonly ReplayManagementWindow _wndReplay;
    private readonly UIRotationWindow _wndRotation;
    private readonly AIWindow _wndAI;
    private readonly MainDebugWindow? _wndDebug;

    private readonly ConfigUI _configUI; // TODO: should be a proper window!

    private readonly EventSubscription? _onConfigSave;

    public unsafe TickService(
        MediatorService mediator,
        ILogger<DisposableMediatorSubscriberBase> mLogger,
        IDalamudPluginInterface dalamud,
        IDataManager dataManager,
        ICommandManager commandManager,
        IUiBuilder uiBuilder,
        ICondition condition,
        IPluginLog logger,
        IFileDialogManager dialog,
        IChatGui chat,
        IWindowSystemFactory windowSystemFactory
    ) : base(mLogger, mediator)
    {
        this.dalamud = dalamud;
        this.uiBuilder = uiBuilder;
        this.condition = condition;
        this.logger = logger;
        windowSystem = windowSystemFactory.Create("vbm");

        dalamud.Create<Service>();
        Service.PluginInterface = dalamud;
        Service.LuminaGameData = dataManager.GameData;
        Service.WindowSystem = windowSystem;
        Service.FileDialogManager = dialog;

        // TODO: all of this stuff should be replaced by actual DI, but that will be complicated
        // testing against actual type incurs a dependency on DalaMock.Core, which is 60MB
        Service.IsMock = uiBuilder.GetType().Assembly.FullName!.StartsWith("DalaMock.Core", StringComparison.InvariantCultureIgnoreCase);

        Service.Config.Initialize();
        Service.Config.LoadFromFile(dalamud.ConfigFile);

        _packs = new();
        _hints = new();

        var configDir = dalamud.ConfigDirectory.FullName;
        if (Service.IsMock)
        {
            // bug(?) in dalamock gives us the wrong config dir (top-level directory)
            configDir = Path.Join(configDir, "BossMod");
            // don't save automatically, instead we have a button attached to MainDevWindow (to avoid destructive changes)
        }
        else
        {
            _onConfigSave = Service.Config.Modified.Subscribe(() => Task.Run(() => Service.Config.SaveToFile(dalamud.ConfigFile)));

            // requires windows/wine
            MultiboxUnlock.Exec();
        }

        _rotationDB = new(new(Path.Join(configDir, "autorot")), new(dalamud.AssemblyLocation.DirectoryName! + "/DefaultRotationPresets.json"));

        if (Service.IsMock)
        {
            _ws = new(0, "unknown");
            _movementOverride = new MockMovementOverride();
            _amex = new MockAmex();
            _wsSync = new MockWorldStateGameSync();
            _hintExecutor = new MockHintExecutor();
        }
        else
        {
            _ws = new((ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency, dataManager.GameData.Repositories["ffxiv"].Version);
            _movementOverride = new MovementOverride(dalamud);
            _amex = new ActionManagerEx(_ws, _hints, (MovementOverride)_movementOverride);
            _wsSync = new WorldStateGameSync(_ws, (ActionManagerEx)_amex);
            _hintExecutor = new HintExecutor(_ws, _movementOverride, _amex, _hints);

            Camera.Instance = new();

            ActionDefinitions.Instance.UnlockCheck = QuestUnlocked;
        }

        _bossmod = new(_ws);
        _zonemod = new(_ws);
        _hintsBuilder = new(_ws, _bossmod, _zonemod);
        _rotation = new(_rotationDB, _bossmod, _hints);
        _ipc = new(_rotation, _hintsBuilder.Obstacles);
        _dtr = new(_rotation);
        _slashCmd = new(commandManager, "/vbm");
        _mbox = new(_rotation, _ws);

        var replayDir = new DirectoryInfo(Path.Join(configDir, "replays"));
        _configUI = new(Service.Config, _ws, replayDir, _rotationDB);

        _wndBossmod = new BossModuleMainWindow(_bossmod, _zonemod);
        _wndBossmodHints = new BossModuleHintsWindow(_bossmod, _zonemod);
        _wndZone = new ZoneModuleWindow(_zonemod);
        _wndReplay = new ReplayManagementWindow(_ws, _bossmod, _rotationDB, replayDir);
        _wndRotation = new UIRotationWindow(_rotation, _amex, () => OpenConfigUI("Autorotation Presets"));
        _wndAI = new AIWindow(_rotation);

        if (Service.IsMock)
        {
            Service.Config.Get<ReplayManagementConfig>().ShowUI = _wndReplay.IsOpen = true;
            _ = new MainDevWindow(dalamud) { IsOpen = true };
        }
        else
        {
            _wndDebug = new MainDebugWindow(_ws, _rotation, _zonemod, (ActionManagerEx)_amex, (MovementOverride)_movementOverride, _hintsBuilder, dalamud)
            {
                IsOpen = dalamud.IsDev
            };
        }

        RegisterSlashCommands();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        uiBuilder.Draw += UiDraw;
        uiBuilder.DisableAutomaticUiHide = true;
        uiBuilder.OpenConfigUi += OpenUi;
        uiBuilder.OpenMainUi += OpenUi;

        condition.ConditionChange += OnConditionChanged;

        _ = new ConfigChangelogWindow();

        if (Service.IsMock)
        {
            _ws.Execute(new ActorState.OpCreate(0x12345678, 0, 0, 0, "xan", 0, ActorType.Player, Class.WAR, 100, new(1, 1, 1, 0), 0.5f, new(500, 600, 100, 10000, 10000), true, true, 0, 0));
            _ws.Execute(new PartyState.OpModify(0, new(0x87654321, 0x12345678, false, "xan")));
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        uiBuilder.Draw -= UiDraw;
        uiBuilder.OpenConfigUi -= OpenUi;
        uiBuilder.OpenMainUi -= OpenUi;

        condition.ConditionChange -= OnConditionChanged;
    }

    void UiDraw()
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
        _rotation.Update(_amex.AnimationLockDelayEstimate, _movementOverride.IsMoving(), Service.Condition[ConditionFlag.DutyRecorderPlayback]);
        _amex.FinishActionGather();

        Service.IconFont = uiBuilder.FontIcon;
        var uiHidden = Service.GameGui.GameUiHidden || Service.Condition.Any(ConditionFlag.OccupiedInCutSceneEvent, ConditionFlag.WatchingCutscene78, ConditionFlag.WatchingCutscene);
        if (!uiHidden)
        {
            windowSystem.Draw();
            Service.FileDialogManager.Draw();
        }

        _hintExecutor.Execute();

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

    void OnConditionChanged(ConditionFlag flag, bool value)
    {
        logger.Debug($"Condition change: {flag}={value}");
    }

    void OpenConfigUI(string tab = "")
    {
        _configUI.ShowTab(tab);
        _ = new UISimpleWindow("Boss Mod Settings", _configUI.Draw, true, new(300, 300));
    }

    void OpenUi() => OpenConfigUI();

    void RegisterSlashCommands()
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
        if (_wndDebug != null)
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
        void SetOrToggle(Preset preset, bool toggle, bool exclusive)
        {
            if (toggle)
            {
                var verb = _rotation.Presets.Contains(preset) ? "disables" : "enables";
                Service.Log($"Console: toggle {verb} preset '{preset.Name}'");
                _rotation.Toggle(preset, exclusive);
            }
            else
            {
                Service.Log($"Console: set activates preset '{preset.Name}'");
                _rotation.Activate(preset, exclusive);
            }
        }

        void SetOrToggleByName(ReadOnlySpan<char> presetName, bool toggle, bool exclusive)
        {
            var preset = _rotation.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
                SetOrToggle(preset, toggle, exclusive);
            else
                Service.ChatGui.PrintError($"Failed to find preset '{presetName}'");
        }

        void ClearByName(ReadOnlySpan<char> presetName)
        {
            var preset = _rotation.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
            {
                Service.Log($"Console: unset deactivates preset '{preset.Name}'");
                _rotation.Deactivate(preset);
            }
            else
                Service.ChatGui.PrintError($"Failed to find preset '{presetName}'");
        }

        cmd.SetSimpleHandler("toggle autorotation ui", () => _wndRotation.SetVisible(!_wndRotation.IsOpen));
        cmd.AddSubcommand("clear").SetSimpleHandler("clear current preset; autorotation will do nothing unless plan is active", () =>
        {
            Service.Log($"Console: clearing autorotation preset(s) '{_rotation.PresetNames}'");
            _rotation.Clear();
        });
        cmd.AddSubcommand("disable").SetSimpleHandler("force disable autorotation; no actions will be executed automatically even if plan is active", () =>
        {
            Service.Log($"Console: force-disabling from presets '{_rotation.PresetNames}'");
            _rotation.SetForceDisabled();
        });
        cmd.AddSubcommand("set").SetComplexHandler("<preset>", "start executing specified preset, and deactivate others", preset =>
        {
            SetOrToggleByName(preset, false, true);
            return true;
        });
        var toggle = cmd.AddSubcommand("toggle");
        toggle.SetSimpleHandler("force disable autorotation if not already; otherwise clear overrides", () => SetOrToggle(RotationModuleManager.ForceDisable, true, true));
        toggle.SetComplexHandler("<preset>", "start executing specified preset unless it's already active; clear otherwise", preset =>
        {
            SetOrToggleByName(preset, true, true);
            return true;
        });

        cmd.AddSubcommand("activate").SetComplexHandler("<preset>", "add specified preset to active list", preset =>
        {
            SetOrToggleByName(preset, false, false);
            return true;
        });
        cmd.AddSubcommand("deactivate").SetComplexHandler("<preset>", "remove specified preset from active list", preset =>
        {
            ClearByName(preset);
            return true;
        });
        cmd.AddSubcommand("togglemulti").SetComplexHandler("<preset>", "if specified preset is in active list, disable it, otherwise enable it", preset =>
        {
            SetOrToggleByName(preset, true, false);
            return true;
        });
    }

    private void RegisterAISlashCommands(SlashCommandHandler cmd)
    {
        var aiConfig = Service.Config.Get<AIConfig>();
        cmd.SetSimpleHandler("toggle multibox ui", () =>
        {
            aiConfig.DrawUI ^= true;
            aiConfig.Modified.Fire();
        });
        cmd.AddSubcommand("on").SetSimpleHandler("enable AI mode", () =>
        {
            aiConfig.Enabled = true;
            aiConfig.Modified.Fire();
        });
        cmd.AddSubcommand("off").SetSimpleHandler("disable AI mode", () =>
        {
            aiConfig.Enabled = false;
            aiConfig.Modified.Fire();
        });
        cmd.AddSubcommand("toggle").SetSimpleHandler("toggle AI mode", () =>
        {
            aiConfig.Enabled ^= true;
            aiConfig.Modified.Fire();
        });
        cmd.AddSubcommand("follow").SetComplexHandler("<name>/slot<N>", "enable multibox mode and follow party member with specified name or at specified slot", masterString =>
        {
            var masterSlot = masterString.StartsWith("slot", StringComparison.OrdinalIgnoreCase) ? int.Parse(masterString[4..]) - 1 : _ws.Party.FindSlot(masterString);
            if (_ws.Party[masterSlot] != null)
                _wndAI.SetSlot(masterSlot);
            else
                Service.ChatGui.PrintError($"[MB] [Follow] Error: can't find {masterString} in our party");
            return true;
        });

        // TODO: this should really be removed, it's a weird synonym for /vbm cfg AIConfig ...
        cmd.SetComplexHandler("", "", args =>
        {
            Service.Log($"vbmai args: {args}");
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

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _onConfigSave?.Dispose();
        _wndDebug?.Dispose();
        _wndAI.Dispose();
        _wndRotation.Dispose();
        _wndReplay.Dispose();
        _wndZone.Dispose();
        _wndBossmodHints.Dispose();
        _wndBossmod.Dispose();
        _configUI.Dispose();
        _packs.Dispose();
        _mbox.Dispose();
        _slashCmd.Dispose();
        _dtr.Dispose();
        _ipc.Dispose();
        _rotation.Dispose();
        _wsSync.Dispose();
        _amex.Dispose();
        _movementOverride.Dispose();
        _hintsBuilder.Dispose();
        _zonemod.Dispose();
        _bossmod.Dispose();
    }
}
