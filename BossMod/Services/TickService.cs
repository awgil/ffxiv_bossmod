using BossMod.AI;
using BossMod.Autorotation;
using DalaMock.Core.Mocks.MockServices;
using DalaMock.Host.Factories;
using DalaMock.Host.Mediator;
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
    readonly AI.AIManager _ai;
    readonly IPCProvider _ipc;
    readonly DTRProvider _dtr;
    readonly SlashCommandProvider _slashCmd;
    readonly MultiboxManager _mbox;
    TimeSpan _prevUpdateTime;
    DateTime _throttleJump;
    DateTime _throttleInteract;
    DateTime _throttleFateSync;
    DateTime _throttleLeaveDuty;
    readonly PackLoader _packs;
    private readonly IDalamudPluginInterface dalamud;
    private readonly IUiBuilder uiBuilder;
    private readonly ICondition condition;
    private readonly IPluginLog logger;

    private readonly ConfigUI _configUI; // TODO: should be a proper window!

    public unsafe TickService(
        MediatorService mediator,
        ILogger<DisposableMediatorSubscriberBase> mLogger,
        IDalamudPluginInterface dalamud,
        ICommandManager commandManager,
        IUiBuilder uiBuilder,
        ICondition condition,
        IPluginLog logger,
        WindowSystemFactory windowSystemFactory
    ) : base(mLogger, mediator)
    {
        this.dalamud = dalamud;
        this.uiBuilder = uiBuilder;
        this.condition = condition;
        this.logger = logger;
        windowSystem = windowSystemFactory.Create("vbm");

        _packs = new();
        _hints = new();
        _rotationDB = new(new(dalamud.ConfigDirectory.FullName + "/autorot"), new(dalamud.AssemblyLocation.DirectoryName! + "/DefaultRotationPresets.json"));

        var isMock = uiBuilder is MockUiBuilder;

        if (isMock)
        {
            _ws = new(0, "unknown");
            _movementOverride = new MockMovementOverride();
            _amex = new MockAmex();
            _wsSync = new MockWorldStateGameSync();
        }
        else
        {
            _ws = new((ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency, File.ReadAllText("ffxivgame.ver"));
            _movementOverride = new MovementOverride(dalamud);
            _amex = new ActionManagerEx(_ws, _hints, (MovementOverride)_movementOverride);
            _wsSync = new WorldStateGameSync(_ws, (ActionManagerEx)_amex);
        }

        _bossmod = new(_ws);
        _zonemod = new(_ws);
        _hintsBuilder = new(_ws, _bossmod, _zonemod);
        _rotation = new(_rotationDB, _bossmod, _hints);
        _ai = new(_rotation, _amex, _movementOverride);
        _ipc = new(_rotation, _hintsBuilder.Obstacles);
        _dtr = new(_rotation, _ai);
        _slashCmd = new(commandManager, "/vbm");
        _mbox = new(_rotation, _ws);

        var replayDir = new DirectoryInfo(dalamud.ConfigDirectory.FullName + "/replays");
        _configUI = new(Service.Config, _ws, replayDir, _rotationDB);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Service.WindowSystem = windowSystem;

        uiBuilder.Draw += Update;
        uiBuilder.DisableAutomaticUiHide = true;
        uiBuilder.OpenConfigUi += OpenUi;
        uiBuilder.OpenMainUi += OpenUi;

        condition.ConditionChange += OnConditionChanged;

        _ = new BossModuleMainWindow(_bossmod, _zonemod);
        _ = new BossModuleHintsWindow(_bossmod, _zonemod);
        _ = new ZoneModuleWindow(_zonemod);
        var replayDir = new DirectoryInfo(dalamud.ConfigDirectory.FullName + "/replays");
        _ = new ReplayManagementWindow(_ws, _bossmod, _rotationDB, replayDir);
        _ = new UIRotationWindow(_rotation, _amex, () => OpenConfigUI("Autorotation Presets"));
        _ = new AIWindow(_ai);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        uiBuilder.Draw -= Update;
        uiBuilder.OpenConfigUi -= OpenUi;
        uiBuilder.OpenMainUi -= OpenUi;

        condition.ConditionChange -= OnConditionChanged;
    }

    void Update()
    {
        windowSystem.Draw();
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
}
