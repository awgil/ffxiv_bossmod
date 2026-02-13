using Autofac;
using BossMod.Autorotation;
using BossMod.Config;
using BossMod.Interfaces;
using BossMod.ReplayVisualization;
using BossMod.Services;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Extensions;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BossMod;

public class Plugin : HostedPlugin
{
    public string Name => "Boss Mod";

    private readonly PackLoader _packs;

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
        //Service.Texture = tex;
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

        _packs = new();

        CreateHost();
        Start();
    }

    public override HostedPluginOptions ConfigureOptions() => new() { UseMediatorService = true };
    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        ConfigureExtra(containerBuilder);

        void RegisterWindow<T>() where T : Window => containerBuilder.RegisterType<T>().AsSelf().As<Window>();

        containerBuilder.RegisterSingletonSelf<MovementOverride>();
        containerBuilder.RegisterSingletonSelf<AI.AIManager>();
        containerBuilder.RegisterSingletonSelf<DTRProvider>();

        containerBuilder.RegisterSingletonSelfAndInterfaces<FrameworkUpdateService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<CommandService>();

        RegisterWindow<ReplayManagementWindow>();
        RegisterWindow<BossModuleMainWindow>();
        RegisterWindow<BossModuleHintsWindow>();
        RegisterWindow<UIRotationWindow>();
        RegisterWindow<AI.AIWindow>();
        RegisterWindow<ConfigChangelogWindow>();

        // one instance is registered for global scope (i.e. real world), and additionally one is registered per replay
        containerBuilder.RegisterType<ModuleArgs>();
        containerBuilder.RegisterType<ZoneModuleArgs>();
        containerBuilder.Register(s => s.Resolve<IWorldStateFactory>().Create()).AsSelf().As<WorldState>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<AIHints>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<AIHintsBuilder>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<BossModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<RotationModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ZoneModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ConfigUI>().AsSelf().InstancePerLifetimeScope();

        // global config (ConfigRoot)
        containerBuilder.Register(s => Service.Config).AsSelf().SingleInstance();
        containerBuilder.Register(s =>
        {
            var dalamud = s.Resolve<IDalamudPluginInterface>();
            var root = dalamud.GetPluginConfigDirectory() + "/replays";
            return new ReplaysRoot(root);
        }).SingleInstance();
        containerBuilder.Register(s =>
        {
            var dalamud = s.Resolve<IDalamudPluginInterface>();
            var root = dalamud.GetPluginConfigDirectory() + "/autorot";
            return new PresetsDatabaseRoot(root);
        }).SingleInstance();
        containerBuilder.Register(s =>
        {
            var dalamud = s.Resolve<IDalamudPluginInterface>();
            var root = dalamud.AssemblyLocation.DirectoryName + "/DefaultRotationPresets.json";
            return new DefaultPresetsFile(root);
        }).SingleInstance();
        containerBuilder.RegisterType<ReplayBuilder>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ReplayDetailsWindow>().InstancePerLifetimeScope();
        containerBuilder.RegisterSingletonSelf<ReplayManager>();
        containerBuilder.RegisterType<ReplayManager.ReplayEntry>();

        containerBuilder.RegisterSingletonSelf<RotationDatabase>();

        containerBuilder.RegisterBuildCallback(OnContainerBuild);
    }
    public override void ConfigureServices(IServiceCollection serviceCollection) { }

    public virtual void OnContainerBuild(ILifetimeScope scope)
    {
        var dalamud = scope.Resolve<IDalamudPluginInterface>();

        Camera.Instance = new();

        ActionDefinitions.Instance.UnlockCheck = QuestUnlocked;

        // save config on modify
        Service.Config.Modified.Subscribe(() => Task.Run(() => Service.Config.SaveToFile(dalamud.ConfigFile)));
    }

    public virtual void ConfigureExtra(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterSingletonSelfAndInterfaces<MovementOverride>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<ActionManagerEx>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<WorldStateGameSync>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<WorldStateFactory>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<HintExecutor>();
        containerBuilder.RegisterType<MainDebugWindow>().AsSelf().As<Window>();
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
