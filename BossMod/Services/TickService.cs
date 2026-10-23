using Autofac.Features.AttributeFilters;
using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Dev;
using BossMod.Interfaces;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal sealed class TickService(
    ILogger<DisposableMediatorSubscriberBase> mLogger,
    MediatorService mediator,
    [KeyFilter(Keys.ConfigDirectory)] DirectoryInfo configDir,
    [KeyFilter(Keys.ReplayDirectory)] DirectoryInfo replayDir,
    IUiBuilder uiBuilder,
    ICondition condition,
    IPluginLog logger,
    IClientState clientState,
    IFileDialogManager fileDialogManager,
    IAmex amex,
    IMovementOverride movementOverride,
    IWorldStateGameSync wsSync,
    IHintExecutor hintExecutor,
    BossModuleManager bossmod,
    RotationDatabase rotationDB,
    RotationModuleManager rotation,
    ZoneModuleManager zonemod,
    ReplayManager replayManager,
    WorldState worldstateGlobal,
    AIHintsBuilder hintsBuilder,
    AIHints hintsGlobal,
    IEnumerable<MainDevWindow> devWindow,
    IEnumerable<MainDebugWindow> debugWindow
) : DisposableMediatorSubscriberBase(mLogger, mediator), IHostedService
{
#nullable disable
    private BossModuleMainWindow _wndBossmod;
    private BossModuleHintsWindow _wndBossmodHints;
    private ZoneModuleWindow _wndZone;
    private ReplayManagementWindow _wndReplay;
    private UIRotationWindow _wndRotation;
    private AIWindow _wndAI;
    private ConfigWindow _wndConfig;
    private ConfigChangelogWindow _wndChangelog;
#nullable restore

    private TimeSpan _prevUpdateTime;
    private readonly DtrConfig _uiConfig = Service.Config.Get<DtrConfig>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        MigratePlans(configDir.FullName);

        _wndConfig = new(new(worldstateGlobal, replayDir, rotationDB));
        _wndBossmod = new BossModuleMainWindow(bossmod, zonemod, hintsGlobal);
        _wndBossmodHints = new BossModuleHintsWindow(bossmod, zonemod, hintsGlobal);
        _wndZone = new ZoneModuleWindow(zonemod, rotation);
        _wndReplay = new ReplayManagementWindow(worldstateGlobal, bossmod, replayDir, replayManager);
        _wndRotation = new UIRotationWindow(rotation, amex);
        _wndAI = new AIWindow(rotation, MediatorService);
        _wndChangelog = new();

        if (Service.IsMock)
            Service.Config.Get<ReplayManagementConfig>().ShowUI = _wndReplay.IsOpen = true;

        devWindow.FirstOrDefault()?.IsOpen = true;

        if (Service.Config.Get<DeveloperConfig>().DebugWindowOpen)
            debugWindow.FirstOrDefault()?.IsOpen = true;

        Version cutoff = new(7, 5, 1, 13);
        if (Service.Config.PreviousAssemblyVersion < cutoff && Service.Config.AssemblyVersion >= cutoff)
        {
            Service.Log($"Removing potentially broken bitmaps from {hintsBuilder.Obstacles.UserRoot.FullName}");
            hintsBuilder.Obstacles.ClearGenerated();
        }

        // TODO: this should be in worldstate, but how?
        clientState.Logout += OnLogout;

        uiBuilder.Draw += Update;
        uiBuilder.DisableAutomaticUiHide = true;
        uiBuilder.OpenConfigUi += OpenUi;
        uiBuilder.OpenMainUi += OpenUi;

        condition.ConditionChange += OnConditionChanged;

        MediatorService.Subscribe<OpenWindow>(this, o =>
        {
            Service.WindowSystem.Windows.FirstOrDefault(w => w.GetType() == o.WindowType)?.IsOpen = true;
        });
        MediatorService.Subscribe<ToggleWindow>(this, t =>
        {
            Service.WindowSystem.Windows.FirstOrDefault(w => w.GetType() == t.WindowType)?.IsOpen ^= true;
        });
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
        var moveImminent = movementOverride.IsMoveRequested() && (!amex.Config.PreventMovingWhileCasting || movementOverride.IsForceUnblocked());

        Service.Camera.Update();
        wsSync.Update(_prevUpdateTime);
        bossmod.Update();
        zonemod.ActiveModule?.Update();
        hintsBuilder.Update(hintsGlobal, PartyState.PlayerSlot, moveImminent);
        amex.QueueManualActions();
        rotation.Update(amex.AnimationLockDelayEstimate, movementOverride.IsMoving(), Service.Condition[ConditionFlag.DutyRecorderPlayback]);
        amex.FinishActionGather();
        wsSync.UpdateReplayState(rotation, movementOverride, hintsGlobal);

        var uiHidden = _uiConfig.HideWithGameUi && (Service.GameGui.GameUiHidden || Service.Condition.Any(ConditionFlag.OccupiedInCutSceneEvent, ConditionFlag.WatchingCutscene78, ConditionFlag.WatchingCutscene));
        if (!uiHidden)
        {
            Service.WindowSystem.Draw();
            fileDialogManager.Draw();
        }

        hintExecutor.Execute();

        _prevUpdateTime = DateTime.Now - tsStart;
    }

    void OnConditionChanged(ConditionFlag flag, bool value)
    {
        logger.Debug($"Condition change: {flag}={value}");
    }

    void OnLogout(int type, int code)
    {
        bossmod.ForceUnload($"logout type={type}, code={code}");
    }

    void OpenConfigUI(string tab = "")
    {
        MediatorService.Publish(new OpenSettingsWindow(tab));
    }

    void OpenUi() => OpenConfigUI();

    private static void MigratePlans(string pluginConfigDir)
    {
        var source = Path.Join(pluginConfigDir, "autorot");

        // no autorot dir means fresh install
        if (!Path.Exists(source))
        {
            Service.PluginLog.Verbose($"[Migrator] No configs, nothing to do.");
            return;
        }

        var destination = Path.Join(Plugin.GetStorageDir(), "autorot");

        if (File.Exists(Path.Join(destination, ".migrate-ok")))
        {
            Service.PluginLog.Verbose($"[Migrator] Nothing to do.");
            return;
        }

        Utils.CopyRecursive(source, destination);

        File.Create(Path.Join(destination, ".migrate-ok"));

        Service.Log($"[Migrator] Done.");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _wndChangelog.Dispose();
        _wndAI.Dispose();
        _wndRotation.Dispose();
        _wndReplay.Dispose();
        _wndZone.Dispose();
        _wndBossmodHints.Dispose();
        _wndBossmod.Dispose();
        _wndConfig.Dispose();
    }
}
