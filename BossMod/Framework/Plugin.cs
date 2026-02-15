using Autofac;
using BossMod.Autorotation;
using BossMod.Config;
using BossMod.Interfaces;
using BossMod.ReplayVisualization;
using BossMod.Services;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Classes;
using DalaMock.Shared.Extensions;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.Tasks;

namespace BossMod;

public class Plugin : HostedPlugin
{
    public string Name => "Boss Mod";

    public unsafe Plugin(
        IDalamudPluginInterface dalamud,
        IPluginLog log,
        IClientState clientState,
        IPlayerState playerState,
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
        ISigScanner scanner,
        ITargetManager targetManager,
        INotificationManager notifications,
        IUnlockState unlockState
    ) : base(dalamud, log, clientState, playerState, objects, commandManager, dataManager, dtrBar, condition, gameGui, gameConfig, chatGui, keyState, tex, hook, scanner, targetManager, notifications, unlockState)
    {
        Service.SigScanner = scanner;
        Service.Hook = hook;
        Service.LuminaGameData = dataManager.GameData;
        //Service.Config.Initialize();
        //Service.Config.LoadFromFile(dalamud.ConfigFile);

        Service.LogHandlerDebug = (s) => log.Debug(s);
        Service.LogHandlerVerbose = (s) => log.Verbose(s);
        MultiboxUnlock.Exec();

        //_packs = new();

        CreateHost();
        Start();
    }

    public override HostedPluginOptions ConfigureOptions() => new() { UseMediatorService = true };
    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        ConfigureExtra(containerBuilder);

        void RegisterWindow<T>() where T : Window => containerBuilder.RegisterType<T>().AsSelf().As<Window>();

        // main services
        containerBuilder.RegisterSingletonSelfAndInterfaces<FrameworkUpdateService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<CommandService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<DtrService>();

        containerBuilder.RegisterSingletonSelf<PackLoader>();
        containerBuilder.RegisterSingletonSelf<SlashCommandProvider>();
        containerBuilder.RegisterSingletonSelf<MovementOverride>();
        containerBuilder.RegisterSingletonSelf<AI.AIManager>();
        containerBuilder.RegisterSingletonSelf<Serializer>();
        containerBuilder.RegisterSingletonSelf<RotationDatabase>();
        containerBuilder.RegisterSingletonSelf<ReplayManager>();
        containerBuilder.RegisterSingletonSelf<RotationModuleRegistry>();
        containerBuilder.RegisterSingletonSelf<BossModuleRegistry>();

        containerBuilder.RegisterSingletonSelf<ActionDefinitions>();
        // class definitions
        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => typeof(IDefinitions).IsAssignableFrom(t))
            .AsSelf()
            .As<IDefinitions>()
            .SingleInstance();

        // main non-detached windows
        RegisterWindow<ReplayManagementWindow>();
        RegisterWindow<BossModuleMainWindow>();
        RegisterWindow<BossModuleHintsWindow>();
        RegisterWindow<UIRotationWindow>();
        RegisterWindow<AI.AIWindow>();
        RegisterWindow<ConfigChangelogWindow>();

        // these are instantiated per-worldstate, including one "global" instance corresponding with the "real" world
        containerBuilder.RegisterType<ModuleArgs>();
        containerBuilder.RegisterType<ZoneModuleArgs>();
        containerBuilder.Register(s => s.Resolve<IWorldStateFactory>().Create()).AsSelf().As<WorldState>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<AIHints>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<AIHintsBuilder>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<BossModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<RotationModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ZoneModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ConfigUI>().AsSelf().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ReplayBuilder>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ReplayDetailsWindow>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ReplayManager.ReplayEntry>();

        // global config (ConfigRoot)
        containerBuilder.Register(s =>
        {
            var cf = new ConfigRoot(s.Resolve<Serializer>(), s.Resolve<Lazy<BossModuleRegistry>>());
            cf.Initialize();
            cf.LoadFromFile(s.Resolve<IDalamudPluginInterface>().ConfigFile);
            return cf;
        }).AsSelf().SingleInstance();
        foreach (var configType in Utils.GetDerivedTypes<ConfigNode>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract))
        {
            containerBuilder.Register(s =>
            {
                var root = s.Resolve<ConfigRoot>();
                var method = typeof(ConfigRoot).GetMethod(nameof(ConfigRoot.Get), BindingFlags.Instance | BindingFlags.Public, [])!.MakeGenericMethod(configType);
                return method.Invoke(root, [])!;
            }).As(configType).SingleInstance();
        }

        // derived paths
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

        containerBuilder.RegisterBuildCallback(scope =>
        {
            //Service.ConfigLazy.SetValue(scope.Resolve<ConfigRoot>());

            Service.IconFont = scope.Resolve<IUiBuilder>().FontIcon;

            OnContainerBuild(scope);
        });
    }
    public override void ConfigureServices(IServiceCollection serviceCollection) { }

    public virtual void OnContainerBuild(ILifetimeScope scope)
    {
        var dalamud = scope.Resolve<IDalamudPluginInterface>();

        Camera.Instance = new();

        var cfg = scope.Resolve<ConfigRoot>();
        cfg.Modified.Subscribe(() => Task.Run(() => cfg.SaveToFile(dalamud.ConfigFile)));
    }

    public virtual void ConfigureExtra(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterSingletonSelfAndInterfaces<MovementOverride>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<ActionManagerEx>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<WorldStateGameSync>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<WorldStateFactory>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<HintExecutor>();

        containerBuilder.RegisterSingletonSelf<Dalamud.Interface.ImGuiFileDialog.FileDialogManager>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<DalamudFileDialogManager>();

        containerBuilder.RegisterType<MainDebugWindow>().AsSelf().As<Window>();
    }
}
