using Autofac;
using BossMod.Autorotation;
using BossMod.Interfaces;
using DalaMock.Host.Factories;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Extensions;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace BossMod;

public class Plugin : HostedPlugin
{
    public string Name => "Boss Mod";

    private RotationDatabase _rotationDB;
    private WorldState _ws;
    private AIHints _hints;
    private BossModuleManager _bossmod;
    private ZoneModuleManager _zonemod;
    private AIHintsBuilder _hintsBuilder;
    private IMovementOverride _movementOverride;
    private IAmex _amex;
    private IWorldStateSync _wsSync;
    private RotationModuleManager _rotation;
    private AI.AIManager _ai;
    private IPCProvider _ipc;
    private DTRProvider _dtr;
    private SlashCommandProvider _slashCmd;
    private MultiboxManager _mbox;
    private TimeSpan _prevUpdateTime;
    private IHintExecutor _hintExecutor;
    private readonly PackLoader _packs;

    // windows
    private ConfigUI _configUI; // TODO: should be a proper window!
    private BossModuleMainWindow _wndBossmod;
    private BossModuleHintsWindow _wndBossmodHints;
    private ZoneModuleWindow _wndZone;
    private ReplayManagementWindow _wndReplay;
    private UIRotationWindow _wndRotation;
    private AI.AIWindow _wndAI;
    private MainDebugWindow? _wndDebug;
    private IUiBuilder _uiBuilder;

    public unsafe Plugin(
        IDalamudPluginInterface dalamud,
        IPluginLog log,
        IClientState clientState,
        IObjectTable objects,
        ICommandManager commandManager,
        IDataManager dataManager,
        IDtrBar dtrBar,
        ICondition condition,
        IGameGui gameGui,
        IGameConfig gameConfig,
        IChatGui chatGui,
        IKeyState keyState,
        ITextureProvider tex,
        IGameInteropProvider hook,
        ISigScanner scanner
    ) : base(dalamud, log, clientState, objects, commandManager, dataManager, dtrBar, condition, gameGui, gameConfig, chatGui, keyState, tex, hook, scanner)
    {
        Service.Texture = tex;
        Service.PluginInterface = dalamud;
        Service.DtrBar = dtrBar;
        Service.ObjectTable = objects;
        Service.ClientState = clientState;
        Service.Condition = condition;
        Service.GameGui = gameGui;
        Service.GameConfig = gameConfig;
        Service.ChatGui = chatGui;
        Service.KeyState = keyState;
        Service.SigScanner = scanner;
        Service.Hook = hook;
        Service.LuminaGameData = dataManager.GameData;
        Service.Config.Initialize();
        Service.Config.LoadFromFile(dalamud.ConfigFile);

        Service.LogHandlerDebug = (s) => log.Debug(s);
        Service.LogHandlerVerbose = (s) => log.Verbose(s);
        Service.Condition.ConditionChange += OnConditionChanged;
        MultiboxUnlock.Exec();

        ActionDefinitions.Instance.UnlockCheck = QuestUnlocked;
        _packs = new();

        HostedEvents.PluginStopping += Teardown;

        CreateHost();
        Start();
    }

    public void Teardown()
    {
        _uiBuilder.Draw -= DrawUI;
        Service.Condition.ConditionChange -= OnConditionChanged;
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

    public override HostedPluginOptions ConfigureOptions() => new() { UseMediatorService = true };
    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        ConfigureExtra(containerBuilder);
        containerBuilder.RegisterType<MovementOverride>().AsSelf().SingleInstance();

        containerBuilder.RegisterBuildCallback(scope =>
        {
            var ws = scope.Resolve<WindowSystemFactory>();
            Service.WindowSystem = ws.Create("vbm");

            var dalamud = scope.Resolve<IDalamudPluginInterface>();
            _rotationDB = new(new(dalamud.GetPluginConfigDirectory() + "/autorot"), new(dalamud.AssemblyLocation.DirectoryName! + "/DefaultRotationPresets.json"));

            var wsf = scope.Resolve<IWorldStateFactory>();
            _ws = wsf.Create();
            _hints = new();
            _bossmod = new(_ws);
            _zonemod = new(_ws);
            _hintsBuilder = new(_ws, _bossmod, _zonemod);
            _movementOverride = scope.Resolve<IMovementOverride>();

            var af = scope.Resolve<IAmex.Factory>();
            _amex = af.Invoke(_ws, _hints);

            var wssf = scope.Resolve<IWorldStateSync.Factory>();
            _wsSync = wssf.Invoke(_ws);
            _rotation = new(_rotationDB, _bossmod, _hints);
            _ai = new(_rotation, _amex, _movementOverride);
            _ipc = new(_rotation, _amex, _movementOverride, _ai);
            _dtr = new(_rotation, _ai);
            _slashCmd = new(scope.Resolve<ICommandManager>(), "/vbm");
            _mbox = new(_rotation, _ws);

            var hef = scope.Resolve<IHintExecutor.Factory>();
            _hintExecutor = hef.Invoke(_ws, _hints);

            var replayDir = new DirectoryInfo(dalamud.GetPluginConfigDirectory() + "/replays");
            _configUI = new(Service.Config, _ws, replayDir, _rotationDB);

            _wndBossmod = new(_bossmod, _zonemod);
            _wndBossmodHints = new(_bossmod, _zonemod);
            _wndZone = new(_zonemod);
            _wndReplay = new(_ws, _bossmod, _rotationDB, replayDir);
            _wndRotation = new(_rotation, _amex, () => OpenConfigUI("Autorotation Presets"));
            _wndAI = new(_ai);

            _uiBuilder = scope.Resolve<IUiBuilder>();
            Service.IconFont = _uiBuilder.FontIcon;
            _uiBuilder.DisableAutomaticUiHide = true;
            _uiBuilder.Draw += DrawUI;
            _uiBuilder.OpenMainUi += () => OpenConfigUI();
            _uiBuilder.OpenConfigUi += () => OpenConfigUI();

            _ = new ConfigChangelogWindow();

            OnContainerBuild(scope);
        });
    }
    public override void ConfigureServices(IServiceCollection serviceCollection) { }

    public virtual void OnContainerBuild(ILifetimeScope scope)
    {
        var dalamud = scope.Resolve<IDalamudPluginInterface>();

        Camera.Instance = new();

        // save config on modify
        Service.Config.Modified.Subscribe(() => Task.Run(() => Service.Config.SaveToFile(dalamud.ConfigFile)));

        // debug window, heavily uses CS so only suitable for actual plugin
        _wndDebug = new(_ws, _rotation, _zonemod, _amex, _movementOverride, _hintsBuilder, dalamud) { IsOpen = dalamud.IsDev };

        RegisterSlashCommands();
    }

    public virtual void ConfigureExtra(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterSingletonSelfAndInterfaces<MovementOverride>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<ActionManagerEx>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<WorldStateGameSync>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<WorldStateFactory>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<HintExecutor>();
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
        _slashCmd.AddSubcommand("d").SetSimpleHandler("show debug UI", () => _wndDebug?.OpenAndBringToFront());
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

    private void VbmaiHandler(string _, string __)
    {
        Service.ChatGui.PrintError("/vbmai: Legacy AI mode is deprecated. Please use /vbm cfg AIConfig (args...) instead. This command will be removed in a future update.");
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
        _rotation.Update(_amex.AnimationLockDelayEstimate, _movementOverride.IsMoving(), Service.Condition[ConditionFlag.DutyRecorderPlayback]);
        _ai.Update();
        _amex.FinishActionGather();

        bool uiHidden = Service.GameGui.GameUiHidden || Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.WatchingCutscene];
        if (!uiHidden)
        {
            Service.WindowSystem?.Draw();
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

    private void OnConditionChanged(ConditionFlag flag, bool value)
    {
        Service.Log($"Condition change: {flag}={value}");
    }
}
