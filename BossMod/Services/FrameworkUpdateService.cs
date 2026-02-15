using BossMod.Autorotation;
using BossMod.Interfaces;
using DalaMock.Host.Factories;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal class FrameworkUpdateService(
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
    ConfigUI configUI,
    IDalamudPluginInterface dalamud,
    Lazy<IEnumerable<Window>> windows
) : IHostedService
{
    private TimeSpan _prevUpdateTime;
    private readonly IWindowSystem windowSystem = windowSystemFactory.Create("vbm");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Service.WindowSystem = windowSystem;
        // TODO: UIWindow constructor expects Service.WindowSystem to be set
        _ = windows.Value;

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
