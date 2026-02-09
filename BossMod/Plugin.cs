using Autofac;
using BossMod.Autorotation;
using BossMod.Services;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Extensions;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;

namespace BossMod;

public class Plugin : HostedPlugin
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
    private readonly IPCProvider _ipc;
    private readonly DTRProvider _dtr;
    private readonly SlashCommandProvider _slashCmd;
    private readonly MultiboxManager _mbox;
    private TimeSpan _prevUpdateTime;
    private DateTime _throttleJump;
    private DateTime _throttleInteract;
    private DateTime _throttleFateSync;
    private readonly PackLoader _packs;

    // windows
    private readonly ConfigUI _configUI; // TODO: should be a proper window!
    private readonly BossModuleMainWindow _wndBossmod;
    private readonly BossModuleHintsWindow _wndBossmodHints;
    private readonly ZoneModuleWindow _wndZone;
    private readonly ReplayManagementWindow _wndReplay;
    private readonly UIRotationWindow _wndRotation;
    private readonly AI.AIWindow _wndAI;
    private readonly MainDebugWindow _wndDebug;

    public unsafe Plugin(IDalamudPluginInterface dalamud, IPluginLog pluginLog, ICommandManager cmd) : base(dalamud, pluginLog, cmd)
    {
        CreateHost();
        Start();

        /*
        if (!dalamud.ConfigDirectory.Exists)
            dalamud.ConfigDirectory.Create();
        var dalamudRoot = dalamud.GetType().Assembly.
                GetType("Dalamud.Service`1", true)!.MakeGenericType(dalamud.GetType().Assembly.GetType("Dalamud.Dalamud", true)!).
                GetMethod("Get")!.Invoke(null, BindingFlags.Default, null, [], null);
        var dalamudStartInfo = dalamudRoot?.GetType().GetProperty("StartInfo", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dalamudRoot) as DalamudStartInfo;
        var gameVersion = dalamudStartInfo?.GameVersion?.ToString() ?? "unknown";

#if LOCAL_CS
        InteropGenerator.Runtime.Resolver.GetInstance.Setup(sigScanner.SearchBase, gameVersion, new(dalamud.ConfigDirectory.FullName + "/cs.json"));
        FFXIVClientStructs.Interop.Generated.Addresses.Register();
        InteropGenerator.Runtime.Resolver.GetInstance.Resolve();
#endif

        dalamud.Create<Service>();
        Service.IsDev = dalamud.IsDev;
        Service.LogHandlerDebug = (string msg) => Service.Logger.Debug(msg);
        Service.LogHandlerVerbose = (string msg) => Service.Logger.Verbose(msg);
        Service.LuminaGameData = dataManager.GameData;
        Service.WindowSystem = new("vbm");
        //Service.Device = pluginInterface.UiBuilder.Device;
        Service.Condition.ConditionChange += OnConditionChanged;
        Service.IconFont = UiBuilder.IconFont;
        MultiboxUnlock.Exec();
        Camera.Instance = new();

        Service.Config.Initialize();
        Service.Config.LoadFromFile(dalamud.ConfigFile);
        Service.Config.Modified.Subscribe(() => Task.Run(() => Service.Config.SaveToFile(dalamud.ConfigFile)));

        ActionDefinitions.Instance.UnlockCheck = QuestUnlocked; // ensure action definitions are initialized and set unlock check functor (we don't really store the quest progress in clientstate, for now at least)

        _packs = new();

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
        _ipc = new(_rotation, _amex, _movementOverride, _ai);
        _dtr = new(_rotation, _ai);
        _slashCmd = new(commandManager, "/vbm");
        _mbox = new(_rotation, _ws);

        var replayDir = new DirectoryInfo(dalamud.ConfigDirectory.FullName + "/replays");
        _configUI = new(Service.Config, _ws, replayDir, _rotationDB);
        _wndBossmod = new(_bossmod, _zonemod);
        _wndBossmodHints = new(_bossmod, _zonemod);
        _wndZone = new(_zonemod);
        _wndReplay = new(_ws, _bossmod, _rotationDB, replayDir);
        _wndRotation = new(_rotation, _amex, () => OpenConfigUI("Autorotation Presets"));
        _wndAI = new(_ai);
        _wndDebug = new(_ws, _rotation, _zonemod, _amex, _movementOverride, _hintsBuilder, dalamud) { IsOpen = Service.IsDev };

        dalamud.UiBuilder.DisableAutomaticUiHide = true;
        dalamud.UiBuilder.Draw += DrawUI;
        dalamud.UiBuilder.OpenMainUi += () => OpenConfigUI();
        dalamud.UiBuilder.OpenConfigUi += () => OpenConfigUI();
        RegisterSlashCommands();

        _ = new ConfigChangelogWindow();
        */
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
        containerBuilder.RegisterSingletonSelfAndInterfaces<ConfigRoot>();
        containerBuilder.Register(s =>
        {
            var plugin = s.Resolve<IDalamudPluginInterface>();
            var replayDir = new DirectoryInfo(plugin.ConfigDirectory.FullName + "/replays");
            return new ConfigUI(s.Resolve<ConfigRoot>(), s.Resolve<WorldState>(), replayDir, null);
        }).AsSelf().SingleInstance();

        containerBuilder.RegisterSingletonSelfAndInterfaces<SlashCommandService>();

        containerBuilder.RegisterType<RotationDatabase>().AsSelf().SingleInstance()
            .WithParameter(
                (pi, ctx) => pi.Name == "rootPath",
                (pi, ctx) => ctx.Resolve<IDalamudPluginInterface>().ConfigDirectory.FullName + "/autorot"
            )
            .WithParameter(
                (pi, ctx) => pi.Name == "defaultPresets",
                (pi, ctx) => ctx.Resolve<IDalamudPluginInterface>().AssemblyLocation.DirectoryName! + "/DefaultRotationPresets.json"
            );
        containerBuilder.RegisterSingletonSelf<AIHints>();

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.Name.EndsWith("Window"))
            .As<Window>()
            .AsSelf()
            .AsImplementedInterfaces();
        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.Name.EndsWith("Manager"))
            .AsSelf();

        containerBuilder.Register(s => new WorldState(0, "foo")).AsSelf().SingleInstance();

        containerBuilder.RegisterBuildCallback(scope =>
        {
            var pi = scope.Resolve<IDalamudPluginInterface>();
            var configUI = scope.Resolve<ConfigUI>();

            pi.UiBuilder.OpenConfigUi += () => configUI.Open();
            pi.UiBuilder.OpenMainUi += () => configUI.Open();
        });
    }

    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
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
        _movementOverride.DesiredSpinDirection = _hints.SpinDirection;

        var targetSystem = FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance();
        SetTarget(_hints.ForcedTarget, &targetSystem->Target);
        SetTarget(_hints.ForcedFocusTarget, &targetSystem->FocusTarget);

        foreach (var s in _hints.StatusesToCancel)
        {
            var res = FFXIVClientStructs.FFXIV.Client.Game.StatusManager.ExecuteStatusOff(s.statusId, s.sourceId != 0 ? (uint)s.sourceId : 0xE0000000);
            Service.Log($"[ExecHints] Canceling status {s.statusId} from {s.sourceId:X} -> {res}");
        }
        if (_hints.WantJump && _ws.CurrentTime > _throttleJump)
        {
            //Service.Log($"[ExecHints] Jumping...");
            FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->UseAction(FFXIVClientStructs.FFXIV.Client.Game.ActionType.GeneralAction, 2);
            _throttleJump = _ws.FutureTime(0.1f);
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

        HandleFateSync();
    }

    private unsafe void SetTarget(Actor? target, FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject** targetPtr)
    {
        if (target == null || !target.IsTargetable)
            return;

        var obj = GetActorObject(target);

        // 50 in-game units is the maximum distance before nameplates stop rendering (making the mob effectively untargetable)
        // targeting a mob that isn't visible is bad UX
        if (_ws.Party.Player() is { } player)
        {
            var distSq = (player.PosRot.XYZ() - target.PosRot.XYZ()).LengthSquared();
            if (distSq < 2500)
                *targetPtr = obj;
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

    private unsafe void HandleFateSync()
    {
        var fm = FateManager.Instance();
        var fate = fm->CurrentFate;
        if (fate == null)
            return;

        var shouldDoSomething = _hints.WantFateSync switch
        {
            AIHints.FateSync.Enable => !fm->IsSyncedToFate(fate),
            AIHints.FateSync.Disable => fm->IsSyncedToFate(fate),
            _ => false
        };

        if (shouldDoSomething && _ws.CurrentTime >= _throttleFateSync)
        {
            fm->LevelSync();
            _throttleFateSync = _ws.FutureTime(0.5f);
        }
    }

    private unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GetActorObject(Actor? actor)
    {
        if (actor == null)
            return null;

        var obj = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[actor.SpawnIndex].Value;
        if (obj == null)
            return null;

        if (obj->EntityId != actor.InstanceID)
            Service.Log($"[ExecHints] Unexpected actor: expected {actor.InstanceID:X} at #{actor.SpawnIndex}, but found {obj->EntityId:X}");

        return obj;
    }

    private void OnConditionChanged(ConditionFlag flag, bool value)
    {
        Service.Log($"Condition change: {flag}={value}");
    }
}
