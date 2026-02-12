using Autofac;
using BossMod.Autorotation;
using BossMod.Interfaces;
using DalaMock.Host.Factories;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal class TickService(
    IAmex amex,
    IMovementOverride movement,
    DTRProvider dtr,
    IWorldStateSync wsSync,
    BossModuleManager bmm,
    ZoneModuleManager zmm,
    AIHintsBuilder hintsBuilder,
    AIHints hints,
    RotationModuleManager autorot,
    AI.AIManager aiManager,
    WindowSystemFactory windowSystem,
    IHintExecutor hintExecutor,
    IUiBuilder uiBuilder,
    ICondition conditions,
    IGameGui gameGui,
    ConfigUI configUI,
    IComponentContext ctx
) : IHostedService
{
    private TimeSpan _prevUpdateTime;
    private readonly IWindowSystem windows = windowSystem.Create("vbm");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Service.WindowSystem = windows;
        foreach (var p in ctx.Resolve<IEnumerable<Window>>())
            Service.Log($"Initializing window {p.GetType().Name}");

        uiBuilder.OpenMainUi += () => configUI.Open();
        uiBuilder.OpenConfigUi += () => configUI.Open();

        uiBuilder.Draw += Update;
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        uiBuilder.Draw -= Update;
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
            windows.Draw();

        hintExecutor.Execute();

        Camera.Instance?.DrawWorldPrimitives();
        _prevUpdateTime = DateTime.Now - tsStart;
    }
}
