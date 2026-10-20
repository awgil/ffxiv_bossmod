using Autofac;
using BossMod.Autorotation;
using BossMod.Interfaces;
using BossMod.ReplayVisualization;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal class TickService : DisposableMediatorSubscriberBase, IHostedService
{
#nullable disable
    private IWindowSystem windowSystem;
#nullable restore

    readonly AIHints _hints;
    readonly BossModuleManager _bossmod;
    readonly ZoneModuleManager _zonemod;
    readonly AIHintsBuilder _hintsBuilder;
    readonly IMovementOverride _movementOverride;
    readonly IAmex _amex;
    readonly IWorldStateGameSync _wsSync;
    private readonly IFileDialogManager fileDialogManager;
    private readonly ICamera camera;
    readonly RotationModuleManager _rotation;
    TimeSpan _prevUpdateTime;
    private readonly ConfigRoot configRoot;
    private readonly IDalamudPluginInterface dalamud;
    private readonly IUiBuilder uiBuilder;
    private readonly ICondition condition;
    private readonly IPluginLog logger;
    private readonly IClientState clientState;
    private readonly IWindowSystemFactory windowSystemFactory;
    private readonly WindowBase[] windows;
    private readonly IHintExecutor _hintExecutor;

    public TickService(
        MediatorService mediator,
        ILogger<DisposableMediatorSubscriberBase> mLogger,
        IDalamudPluginInterface dalamud,
        IDataManager dataManager,
        IUiBuilder uiBuilder,
        ICondition condition,
        IPluginLog logger,
        IClientState clientState,
        IWindowSystemFactory windowSystemFactory,
        WindowBase[] windows,
        ConfigRoot configRoot,
        IAmex amex,
        WorldState wsGlobal,
        AIHints hintsGlobal,
        BossModuleManager bmm,
        RotationModuleManager rmm,
        ZoneModuleManager zmm,
        AIHintsBuilder builder,
        IWorldStateGameSync wsSync,
        IMovementOverride movementOverride,
        IHintExecutor hintExecutor,
        IFileDialogManager fileDialogManager,
        ICamera camera
    ) : base(mLogger, mediator)
    {
        this.dalamud = dalamud;
        this.uiBuilder = uiBuilder;
        this.condition = condition;
        this.logger = logger;
        this.clientState = clientState;
        this.windowSystemFactory = windowSystemFactory;
        this.windows = windows;

        Service.Mediator = mediator;
        Service.PluginInterface = dalamud;
        Service.LuminaGameData = dataManager.GameData;
        Service.WindowSystem = windowSystem;

        Service.IsMock = uiBuilder.GetType().Assembly.FullName!.StartsWith("DalaMock.Core", StringComparison.InvariantCultureIgnoreCase);

        this.configRoot = configRoot;

        _hints = hintsGlobal;

        _amex = amex;
        _wsSync = wsSync;
        _movementOverride = movementOverride;
        _hintExecutor = hintExecutor;
        this.fileDialogManager = fileDialogManager;
        this.camera = camera;
        _bossmod = bmm;
        _zonemod = zmm;
        _hintsBuilder = builder;
        _rotation = rmm;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        dalamud.Create<Service>();

        windowSystem = windowSystemFactory.Create("vbm");

        foreach (var window in windows)
            windowSystem.AddWindow(window);

        clientState.Logout += OnLogout;

        uiBuilder.Draw += Update;
        uiBuilder.DisableAutomaticUiHide = true;
        uiBuilder.OpenConfigUi += OpenUi;
        uiBuilder.OpenMainUi += OpenUi;

        condition.ConditionChange += OnConditionChanged;

        MediatorService.Subscribe(this, (OpenWindow o) =>
        {
            windowSystem.Windows.FirstOrDefault(w => w.GetType() == o.WindowType)?.IsOpen = true;
        });
        MediatorService.Subscribe(this, (ToggleWindow t) =>
        {
            windowSystem.Windows.FirstOrDefault(w => w.GetType() == t.WindowType)?.IsOpen ^= true;
        });
        MediatorService.Subscribe(this, (CreateWindow w) => windowSystem.AddWindow(w.Window));
        MediatorService.Subscribe(this, (DestroyWindow w) => windowSystem.RemoveWindow(w.Window));

        if (Service.IsMock)
        {
            windowSystem.Windows.OfType<ReplayManagementWindow>().FirstOrDefault()?.OpenAndBringToFront();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        clientState.Logout -= OnLogout;

        uiBuilder.Draw -= Update;
        uiBuilder.OpenConfigUi -= OpenUi;
        uiBuilder.OpenMainUi -= OpenUi;

        condition.ConditionChange -= OnConditionChanged;
    }

    void Update()
    {
        var tsStart = DateTime.Now;
        var moveImminent = _movementOverride.IsMoveRequested() && (!_amex.Config.PreventMovingWhileCasting || _movementOverride.IsForceUnblocked());

        camera.Update();
        _wsSync.Update(_prevUpdateTime);
        _bossmod.Update();
        _zonemod.ActiveModule?.Update();
        _hintsBuilder.Update(_hints, PartyState.PlayerSlot, moveImminent);
        _amex.QueueManualActions();
        _rotation.Update(_amex.AnimationLockDelayEstimate, _movementOverride.IsMoving(), Service.Condition[ConditionFlag.DutyRecorderPlayback]);
        _amex.FinishActionGather();
        _wsSync.UpdateReplayState(_rotation, _movementOverride, _hints, configRoot.Get<PartyRolesConfig>());

        Service.IconFont = uiBuilder.FontIcon;
        Service.MonoFont = uiBuilder.FontMono;
        Service.FontAtlas = uiBuilder.FontAtlas;
        var uiHidden = Service.GameGui.GameUiHidden || Service.Condition.Any(ConditionFlag.OccupiedInCutSceneEvent, ConditionFlag.WatchingCutscene78, ConditionFlag.WatchingCutscene);
        if (!uiHidden)
        {
            windowSystem.Draw();
            fileDialogManager.Draw();
        }

        _hintExecutor.Execute();

        var destroyed = windowSystem.Windows.OfType<WindowBase>().Where(w => w.IsDestroyed).ToList();
        foreach (var d in destroyed)
        {
            Service.PluginLog.Verbose($"Removing from WindowSystem: {d.WindowName}");
            windowSystem.RemoveWindow(d);
            if (d is IDisposable id)
                id.Dispose();
        }

        camera.DrawWorldPrimitives();
        _prevUpdateTime = DateTime.Now - tsStart;
    }

    void OnConditionChanged(ConditionFlag flag, bool value)
    {
        logger.Debug($"Condition change: {flag}={value}");
    }

    void OnLogout(int type, int code)
    {
        _bossmod.ForceUnload($"logout type={type}, code={code}");
    }

    void OpenConfigUI(string tab = "")
    {
        MediatorService.Publish(new OpenSettingsWindow(tab));
    }

    void OpenUi() => OpenConfigUI();

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        GaugeVisualizer.Dispose();
    }
}
