using Autofac;
using BossMod.Autorotation;
using BossMod.Interfaces;
using BossMod.Network;
using BossMod.ReplayAnalysis;
using BossMod.ReplayVisualization;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
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
    private readonly IEnumerable<ConfigMigration> migrations;
    readonly RotationModuleManager _rotation;
    readonly MultiboxManager _mbox;
    TimeSpan _prevUpdateTime;
    readonly PackLoader _packs;
    private readonly ConfigRoot configRoot;
    readonly PingTracker _ping;
    private readonly IDalamudPluginInterface dalamud;
    private readonly IUiBuilder uiBuilder;
    private readonly ICondition condition;
    private readonly IPluginLog logger;
    private readonly IClientState clientState;
    private readonly IHintExecutor _hintExecutor;

    private readonly ICallGateSubscriber<bool>? _vnavIsReady;
    private readonly ICallGateSubscriber<Vector3, float, bool, bool>? _vnavIsOnMesh;

    private readonly CancellationTokenSource _saveCts = new();

    public TickService(
        MediatorService mediator,
        ILogger<DisposableMediatorSubscriberBase> mLogger,
        IDalamudPluginInterface dalamud,
        IDataManager dataManager,
        ICommandManager commandManager,
        IUiBuilder uiBuilder,
        ICondition condition,
        IPluginLog logger,
        IFileDialogManager dialog,
        IClientState clientState,
        IWindowSystemFactory windowSystemFactory,
        IEnumerable<WindowBase> windows,
        PackLoader packs,
        ConfigRoot configRoot,
        RotationDatabase db,
        IAmex amex,
        WorldState wsGlobal,
        AIHints hintsGlobal,
        BossModuleManager bmm,
        RotationModuleManager rmm,
        ZoneModuleManager zmm,
        AIHintsBuilder builder,
        IWorldStateGameSync wsSync,
        IEnumerable<ConfigMigration> migrations
    ) : base(mLogger, mediator)
    {
        this.dalamud = dalamud;
        this.uiBuilder = uiBuilder;
        this.condition = condition;
        this.logger = logger;
        this.clientState = clientState;
        windowSystem = windowSystemFactory.Create("vbm");

        foreach (var window in windows)
            windowSystem.AddWindow(window);

        dalamud.Create<Service>();
        Service.Mediator = mediator;
        Service.PluginInterface = dalamud;
        Service.LuminaGameData = dataManager.GameData;
        Service.WindowSystem = windowSystem;
        Service.FileDialogManager = dialog;

        // TODO: all of this stuff should be replaced by actual DI, but that will be complicated
        // testing against actual type incurs a dependency on DalaMock.Core, which is 60MB
        Service.IsMock = uiBuilder.GetType().Assembly.FullName!.StartsWith("DalaMock.Core", StringComparison.InvariantCultureIgnoreCase);

        var configDir = dalamud.ConfigDirectory.FullName;
        if (Service.IsMock)
        {
            // bug(?) in dalamock gives us the wrong config dir (top-level directory)
            configDir = Path.Join(configDir, "BossMod");
            // don't save automatically, instead we have a button attached to MainDevWindow (to avoid destructive changes)
        }
        else
        {
            Task.Run(SavePeriodically);

            // requires windows/wine
            MultiboxUnlock.Exec();
        }

        _packs = packs;
        this.configRoot = configRoot;

        _hints = hintsGlobal;
        _ping = new(logger);

        if (!Service.IsMock)
        {
            _vnavIsReady = Service.PluginInterface.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady");
            _vnavIsOnMesh = Service.PluginInterface.GetIpcSubscriber<Vector3, float, bool, bool>("vnavmesh.Query.Mesh.IsPointOnMesh");
        }

        _rotationDB = db;

        _amex = amex;
        _ws = wsGlobal;
        _wsSync = wsSync;
        this.migrations = migrations;
        if (Service.IsMock)
        {
            _movementOverride = new MockMovementOverride();
            //_amex = new MockAmex();
            //_wsSync = new MockWorldStateGameSync();
            _hintExecutor = new MockHintExecutor();

            Service.LuminaGameData.Options.RsvResolver = Service.LuminaRSV.TryGetValue;
        }
        else
        {
            _ping.Run();
            _movementOverride = new MovementOverride(dalamud);
            //_amex = new ActionManagerEx(_ws, _hints, (MovementOverride)_movementOverride);
            //_wsSync = new WorldStateGameSync(_ws, (ActionManagerEx)_amex, _ping);
            _hintExecutor = new HintExecutor(_ws, _movementOverride, _amex, _hints);

            ActionDefinitions.Instance.UnlockCheck = QuestUnlocked;
        }

        _bossmod = bmm;
        _zonemod = zmm;
        _hintsBuilder = builder;
        _rotation = rmm;
        _mbox = new(_rotation, _ws);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var migration in migrations)
            if (configRoot.PreviousAssemblyVersion < migration.Since && configRoot.AssemblyVersion >= migration.Since)
                migration.Execute();

        ZoneModuleRegistry.Reload([], [Assembly.GetExecutingAssembly()]);
        AnalyzerRegistry.Reload([], [Assembly.GetExecutingAssembly()]);

        MigratePlans(dalamud.ConfigDirectory.FullName);
        CopyLibraries(dalamud.AssemblyLocation);

        clientState.Logout += OnLogout;
        clientState.ZoneInit += OnZoneInit;

        uiBuilder.Draw += Update;
        uiBuilder.DisableAutomaticUiHide = true;
        uiBuilder.OpenConfigUi += OpenUi;
        uiBuilder.OpenMainUi += OpenUi;

        condition.ConditionChange += OnConditionChanged;

        if (Service.IsMock)
        {
            _ws.Execute(new ActorState.OpCreate(0x12345678, 0, 0, 0, "xan", 0, ActorType.Player, Class.WAR, 100, new(90, 0, 90, 0), 0.5f, new(500, 600, 100, 10000, 10000), true, true, 0, 0));
            _ws.Execute(new PartyState.OpModify(0, new(0x87654321, 0x12345678, false)));
            //_ws.Execute(new ActorState.OpCreate(0x12345679, (uint)StrikingDummy.OID.Boss, 10, 0, "Striking Dummy", 541, ActorType.Enemy, Class.None, 1, new(100, 0, 100, 0), 1, new(500, 600, 100, 10000, 10000), true, false, 0, 0));
            _ws.IsFrozen = true;
        }

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
            windowSystem.Windows.FirstOrDefault(w => w.GetType() == typeof(ReplayManagementWindow))?.IsOpen = true;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        clientState.Logout -= OnLogout;
        clientState.ZoneInit -= OnZoneInit;

        uiBuilder.Draw -= Update;
        uiBuilder.OpenConfigUi -= OpenUi;
        uiBuilder.OpenMainUi -= OpenUi;

        condition.ConditionChange -= OnConditionChanged;
    }

    private void OnZoneInit(Dalamud.Game.ClientState.ZoneInitEventArgs obj)
    {
        _pauseBitmapGeneration = false;
    }

    void Update()
    {
        var tsStart = DateTime.Now;
        var moveImminent = _movementOverride.IsMoveRequested() && (!_amex.Config.PreventMovingWhileCasting || _movementOverride.IsForceUnblocked());

        Camera.Instance()?.Update();
        _wsSync.Update(_prevUpdateTime);
        _bossmod.Update();
        _zonemod.ActiveModule?.Update();
        _hintsBuilder.Update(_hints, PartyState.PlayerSlot, moveImminent);
        _amex.QueueManualActions();
        _rotation.Update(_amex.AnimationLockDelayEstimate, _movementOverride.IsMoving(), Service.Condition[ConditionFlag.DutyRecorderPlayback]);
        _amex.FinishActionGather();
        _wsSync.UpdateReplayState(_rotation, _movementOverride, _hints);

        Service.IconFont = uiBuilder.FontIcon;
        Service.MonoFont = uiBuilder.FontMono;
        Service.FontAtlas = uiBuilder.FontAtlas;
        var uiHidden = Service.GameGui.GameUiHidden || Service.Condition.Any(ConditionFlag.OccupiedInCutSceneEvent, ConditionFlag.WatchingCutscene78, ConditionFlag.WatchingCutscene);
        if (!uiHidden)
        {
            windowSystem.Draw();
            Service.FileDialogManager.Draw();
        }

        _hintExecutor.Execute();

        if (_vnavIsReady != null && _vnavIsOnMesh != null)
            CreateBitmapIfMissing(_vnavIsReady, _vnavIsOnMesh);

        var detached = windowSystem.Windows.OfType<WindowBase>().Where(w => w.IsDestroyed || w is DetachedWindow { IsOpen: false }).ToList();
        foreach (var d in detached)
        {
            windowSystem.RemoveWindow(d);
            if (d is IDisposable id)
                id.Dispose();
        }

        Camera.Instance()?.DrawWorldPrimitives();
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

    bool _pauseBitmapGeneration;

    void CreateBitmapIfMissing(ICallGateSubscriber<bool> isReady, ICallGateSubscriber<Vector3, float, bool, bool> isPointOnMesh)
    {
        if (_pauseBitmapGeneration || _ws.Party.Player() is not { } player || !Service.Config.Get<DeveloperConfig>().AutoBitmaps)
            return;

        unsafe
        {
            // vnav isn't reliable in housing zones and there's not really a reason to have bitmaps there anyway
            // TODO isn't there a oneliner for this?
            if (FFXIVClientStructs.FFXIV.Client.Game.GameMain.Instance()->CurrentTerritoryIntendedUseId is FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse.HousingIndoor or FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse.HousingOutdoor)
                return;
        }

        // scorched earth approach: if player is moving vertically, assume we can't trust their position
        // this covers jumping and clientpaths, as well as some annoying edge cases that don't show up anywhere else: walking off the boat in ihuykatumu (during which IsJumping() returns false), walking off of any wall-less arena (which puts you into condition 47, not Jumping), etc
        if (player.PosRot.Y != player.PrevPosRot.Y)
            return;

        // try to do nothing if player is in any state that isn't "standing on the ground"
        if (Service.Condition.Any(ConditionFlag.BetweenAreas, ConditionFlag.BetweenAreas51, ConditionFlag.OccupiedInCutSceneEvent, ConditionFlag.OccupiedInQuestEvent, ConditionFlag.InFlight, ConditionFlag.Diving, ConditionFlag.Jumping, (ConditionFlag)47, ConditionFlag.DutyRecorderPlayback))
            return;

        var insertAtFront = false;

        var (entry, data) = _hintsBuilder.Obstacles.Find(player.PosRot.XYZ());
        if (entry != null && data != null)
        {
            var poff = ((player.Position - entry.Origin) / data.PixelSize).Floor();
            var px = (int)poff.X;
            var py = (int)poff.Z;

            var playerDeepInObstacle = px >= 0 && py >= 0 && px < data.Width && py < data.Height && data[px, py] && (px == 0 || data[px - 1, py]) && (py == 0 || data[px, py - 1]) && (px == (data.Width - 1) || data[px + 1, py]) && (py == (data.Height - 1) || data[px, py + 1]);

            if (playerDeepInObstacle)
                // we are probably standing on an isolated landmass that is within the current map's bounding box
                // we should try to generate a new map from player's current position and make it higher priority (assumption is that this area has a smaller bounding box)
                insertAtFront = true;
            else
                // standing in normal part of map, everything is fine
                return;
        }

        try
        {
            if (!isReady.InvokeFunc())
                return;
        }
        catch (IpcNotReadyError ex)
        {
            Service.Logger.Debug(ex, "vnav is not loaded, pausing");
            _pauseBitmapGeneration = true;
            return;
        }

        try
        {
            // if player is standing on a little rock or at the edge of accessible ground, we want to avoid generating a bitmap every frame which just overwrites an existing one
            if (!isPointOnMesh.InvokeFunc(player.PosRot.XYZ(), 2, true))
                return;
        }
        catch (IpcNotReadyError ex)
        {
            Service.Logger.Debug(ex, "vnav is too old, must be >=1.2.3.8 to support auto generation");
            _pauseBitmapGeneration = true;
            return;
        }

        try
        {
            _hintsBuilder.Obstacles.GenerateMap(player.PosRot.XYZ(), 2048, true, insertAtFront);
        }
        catch (Exception ex)
        {
            Service.Logger.Warning(ex, "Unable to generate an obstacle map, pausing");
            _pauseBitmapGeneration = true;
        }
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

    private void CopyLibraries(FileInfo assemblyLocation)
    {
        var modulesDir = _packs.ModuleDir;
        var assemblyDir = assemblyLocation.DirectoryName;

        Directory.CreateDirectory(modulesDir);

        void copy(string filename)
        {
            var src = Path.Join(assemblyDir, filename);
            var dest = Path.Join(modulesDir, filename);
            if (File.Exists(src))
            {
                if (File.Exists(dest) && File.GetCreationTimeUtc(dest) > File.GetCreationTimeUtc(src))
                {
                    Service.PluginLog.Debug($"{dest} is newer than our version, skipping copy");
                    return;
                }

                Service.PluginLog.Debug($"copied {src} to {dest}");
                File.Copy(src, Path.Join(modulesDir, filename), true);

                return;
            }

            Service.PluginLog.Warning($"{src} does not exist, cannot copy");
        }

        copy("BossMod.Autorotation.dll");
        copy("BossMod.Modules.dll");
        copy("BossMod.Ultimate.dll");
    }

    private async Task SavePeriodically()
    {
        var token = _saveCts.Token;

        while (!token.IsCancellationRequested)
        {
            await Task.Delay(30_000, token).ConfigureAwait(true);

            Service.Config.SaveToFile(dalamud.ConfigFile);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _saveCts.Cancel();
        _saveCts.Dispose();
        if (!Service.IsMock)
            Service.Config.SaveToFile(dalamud.ConfigFile);

        GaugeVisualizer.Dispose();
        _rotationDB.Dispose();
        _mbox.Dispose();
        _rotation.Dispose();
        _wsSync.Dispose();
        _amex.Dispose();
        _ping.Dispose();
        _movementOverride.Dispose();
        _hintsBuilder.Dispose();
        _zonemod.Dispose();
        _bossmod.Dispose();
    }
}
