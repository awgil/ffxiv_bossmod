using BossMod.Autorotation;
using BossMod.Interfaces;
using DalaMock.Host.Factories;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal class FrameworkUpdateService(
    MediatorService mediator,
    ILogger<DisposableMediatorSubscriberBase> mLogger,
    IAmex amex,
    IMovementOverride movement,
    IPluginLog logger,
    DtrService dtr,
    IWorldStateSync wsSync,
    BossModuleManager bmm,
    ZoneModuleManager zmm,
    AIHintsBuilder hintsBuilder,
    AIHints hints,
    RotationModuleManager autorot,
    AI.AIManager aiManager,
    WindowSystemFactory windowSystemFactory,
    IHintExecutor hintExecutor,
    IUiBuilder uiBuilder,
    ICondition conditions,
    IGameGui gameGui,
    ConfigUI configUI
) : DisposableMediatorSubscriberBase(mLogger, mediator), IHostedService
{
    private TimeSpan _prevUpdateTime;
    private readonly IWindowSystem windowSystem = windowSystemFactory.Create("vbm");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        MediatorService.Subscribe<CreateWindowMessage>(this, CreateWindow);
        MediatorService.Subscribe<DestroyWindowMessage>(this, DestroyWindow);

        uiBuilder.OpenMainUi += OpenUi;
        uiBuilder.OpenConfigUi += OpenUi;
        uiBuilder.Draw += Update;

        conditions.ConditionChange += OnConditionChanged;
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        conditions.ConditionChange -= OnConditionChanged;

        uiBuilder.OpenMainUi -= OpenUi;
        uiBuilder.OpenConfigUi -= OpenUi;
        uiBuilder.Draw -= Update;
    }

    void CreateWindow(CreateWindowMessage msg)
    {
        var wnd = msg.Window;
        var detached = msg.Detached;
        var existing = windowSystem.Windows.FirstOrDefault(w => w.WindowName == wnd.WindowName);
        if (existing == null)
        {
            windowSystem.AddWindow(wnd);
            wnd.IsOpen = detached;
        }
        else if (detached)
        {
            existing.IsOpen = true;
            existing.BringToFront();
        }
        else
        {
            throw new InvalidOperationException($"Failed to register window {wnd.WindowName} due to name conflict");
        }
    }

    void DestroyWindow(DestroyWindowMessage msg)
    {
        var wnd = windowSystem.Windows.FirstOrDefault(w => w.WindowName == msg.WindowName);
        if (wnd != null)
            windowSystem.RemoveWindow(wnd);
        else
            throw new InvalidOperationException($"No window named {msg.WindowName} found in WindowSystem.");
    }

    void OpenUi() => configUI.Open();

    void OnConditionChanged(ConditionFlag flag, bool value)
    {
        logger.Debug($"Condition change: {flag}={value}");
    }

    void Update()
    {
        var tsStart = DateTime.Now;
        var moveImminent = movement.IsMoveRequested() && (!amex.Config.PreventMovingWhileCasting || movement.IsForceUnblocked());

        dtr.Update();
        Camera.Instance?.Update();
        wsSync.Update(_prevUpdateTime);
        bmm.Update();
        zmm.ActiveModule?.Update();
        hintsBuilder.Update(hints, PartyState.PlayerSlot, moveImminent);
        amex.QueueManualActions();
        autorot.Update(amex.AnimationLockDelayEstimate, movement.IsMoving(), conditions[ConditionFlag.DutyRecorderPlayback]);
        aiManager.Update();
        amex.FinishActionGather();

        var uiHidden = gameGui.GameUiHidden || conditions.Any(ConditionFlag.OccupiedInCutSceneEvent, ConditionFlag.WatchingCutscene78, ConditionFlag.WatchingCutscene);
        if (!uiHidden)
            windowSystem.Draw();

        hintExecutor.Execute();

        Camera.Instance?.DrawWorldPrimitives();
        _prevUpdateTime = DateTime.Now - tsStart;
    }
}
