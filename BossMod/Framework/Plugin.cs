using Autofac;
using Autofac.Features.AttributeFilters;
using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Dev;
using BossMod.Interfaces;
using BossMod.Network;
using BossMod.Pathfinding;
using BossMod.Services;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Extensions;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina;
using Lumina.Excel;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

public enum Keys
{
    PresetDatabase,
    DefaultPresetsFile,
    PlanDatabase,
    PlanManifest,
    ConfigDirectory,
    ReplayDirectory
}

public class Plugin(IDalamudPluginInterface dalamud) : HostedPlugin(dalamud)
{
    public string Name => "Boss Mod";

    public override HostedPluginOptions ConfigureOptions() => new() { UseMediatorService = true };
    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterSingletonSelfAndInterfaces<StartupService>().WithAttributeFiltering();

        ConfigureExtra(containerBuilder);

        // useful filepaths
        var autorotRoot = Path.Join(GetStorageDir(), "autorot");
        containerBuilder.Register(ctx => new FileInfo(Path.Join(ctx.Resolve<IDalamudPluginInterface>().AssemblyLocation.DirectoryName, "DefaultRotationPresets.json")))
            .Keyed<FileInfo>(Keys.DefaultPresetsFile);
        containerBuilder.Register(ctx => new FileInfo(Path.Join(autorotRoot, "presets.db.json")))
            .Keyed<FileInfo>(Keys.PresetDatabase);
        containerBuilder.Register(ctx => new DirectoryInfo(Path.Join(autorotRoot, "plans")))
            .Keyed<DirectoryInfo>(Keys.PlanDatabase);
        containerBuilder.Register(ctx => new FileInfo(Path.Join(autorotRoot, "plans.manifest.json")))
            .Keyed<FileInfo>(Keys.PlanManifest);
        containerBuilder.Register(ctx => new DirectoryInfo(Path.Join(ctx.ResolveKeyed<DirectoryInfo>(Keys.ConfigDirectory).FullName, "replays")))
            .Keyed<DirectoryInfo>(Keys.ReplayDirectory);

        // root config node
        containerBuilder.Register((ctx) =>
        {
            var root = new ConfigRoot(ctx.Resolve<IDalamudPluginInterface>().ConfigFile, ctx.Resolve<BossModuleRegistry>(), ctx.Resolve<JsonSerializerOptions>());
            root.PreviousAssemblyVersion = root.AssemblyVersion;
            root.AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version!;

            //Service.Config = root;

            return root;
        }).SingleInstance().AsSelf().As<IReloadable>();

        // dynamic resolvers for ConfigNode descendants
        foreach (var node in Utils.GetDerivedTypes<ConfigNode>(Assembly.GetExecutingAssembly()))
        {
            containerBuilder.Register(s =>
            {
                var root = s.Resolve<ConfigRoot>();
                var method = typeof(ConfigRoot).GetMethod(nameof(ConfigRoot.Get), BindingFlags.Instance | BindingFlags.Public, [])!.MakeGenericMethod(node);
                return method.Invoke(root, [])!;
            }).As(node).SingleInstance();
        }

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).Where(t => t.IsSubclassOf(typeof(ConfigMigration))).As<ConfigMigration>().SingleInstance();

        containerBuilder.Register(ctx =>
        {
            var bmr = ctx.Resolve<Lazy<BossModuleRegistry>>();
            var rmr = ctx.Resolve<Lazy<RotationModuleRegistry>>();

            return new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                Converters = {
                    new JsonStringEnumConverter(),
                    new Serialization.JsonTypeConverter(),
                    new JsonPresetConverter(rmr),
                    new JsonPlanConverter(bmr, rmr, ctx.Resolve<IPluginLog>())
                }
            };
        }).SingleInstance();

        containerBuilder.RegisterType<ActionQueue>();

        // hints related stuff
        containerBuilder.RegisterSingletonSelf<ObstacleMapDatabase>();
        containerBuilder.RegisterType<ObstacleMapManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<AIHintsBuilder>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<AIHints>().InstancePerLifetimeScope();

        // ui related stuff
        containerBuilder.RegisterType<ConfigUI>().InstancePerLifetimeScope().WithAttributeFiltering();
        containerBuilder.RegisterSingletonSelf<ModuleViewer>();

        // action definitions stuff
        containerBuilder.RegisterSingletonSelf<BozjaActionID>();
        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).Where(t => t.IsSubclassOf(typeof(Defs))).As<Defs>().SingleInstance();
        containerBuilder.RegisterSingletonSelf<ActionDefinitions>();

        containerBuilder.RegisterGeneric((ctx, prms) =>
        {
            var gd = ctx.Resolve<IDataManager>().GameData;
            var method = typeof(GameData).GetMethod(nameof(GameData.GetExcelSheet))?.MakeGenericMethod(prms);
            return method!.Invoke(gd, [null, null])!;
        }).As(typeof(ExcelSheet<>));

        // stuff related to plans/presets/modules
        // pack service needs to be registered first here
        containerBuilder.RegisterSingletonSelfAndInterfaces<ModulePackService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<PresetDatabase>().WithAttributeFiltering();
        containerBuilder.RegisterSingletonSelfAndInterfaces<PlanDatabase>().WithAttributeFiltering();

        containerBuilder.RegisterSingletonSelfAndInterfaces<RotationModuleRegistry>();
        containerBuilder.RegisterSingletonSelf<RotationDatabase>();
        containerBuilder.RegisterType<RotationModuleManager>().InstancePerLifetimeScope();

        containerBuilder.RegisterSingletonSelfAndInterfaces<BossModuleRegistry>();
        containerBuilder.RegisterType<BossModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ModuleInit>().InstancePerDependency();

        containerBuilder.RegisterSingletonSelfAndInterfaces<ZoneModuleRegistry>();
        containerBuilder.RegisterType<ZoneModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ZoneModuleInit>().InstancePerDependency();

        // persistent main windows
        containerBuilder.RegisterSingletonSelf<BossModuleHintsWindow>().As<WindowBase>();
        containerBuilder.RegisterSingletonSelf<BossModuleMainWindow>().As<WindowBase>();
        containerBuilder.RegisterSingletonSelf<ZoneModuleWindow>().As<WindowBase>();
        containerBuilder.RegisterSingletonSelf<ConfigChangelogWindow>().As<WindowBase>();
        containerBuilder.RegisterSingletonSelf<ConfigWindow>().As<WindowBase>();
        containerBuilder.RegisterSingletonSelf<ReplayManagementWindow>().As<WindowBase>().WithAttributeFiltering();
        containerBuilder.RegisterSingletonSelf<UIRotationWindow>().As<WindowBase>();
        containerBuilder.RegisterSingletonSelf<UIPresetDatabaseEditor>();
        containerBuilder.RegisterSingletonSelf<AIWindow>().As<WindowBase>();

        // utility windows
        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.IsSubclassOf(typeof(TestWindow)))
            .As<TestWindow>()
            .WithAttributeFiltering();

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.IsSubclassOf(typeof(ChangelogNotice)))
            .As<ChangelogNotice>()
            .SingleInstance()
            .WithAttributeFiltering();

        // replay stuff
        containerBuilder.RegisterSingletonSelfAndInterfaces<ReplayManager>().WithAttributeFiltering();
        containerBuilder.RegisterType<ReplayAnalysis.AnalysisManager>();
        containerBuilder.RegisterType<ReplayAnalysis.AnalysisManager.Global>();
        containerBuilder.RegisterType<ReplayManager.AnalysisEntry>();
        containerBuilder.RegisterType<ReplayVisualization.EventList>();
        containerBuilder.RegisterType<ReplayVisualization.ColumnEnemiesCastEvents>();
        containerBuilder.RegisterType<ReplayVisualization.ColumnEnemiesDetails>();
        containerBuilder.RegisterType<ReplayVisualization.ReplayTimelineWindow>();
        containerBuilder.RegisterType<ReplayVisualization.ColumnPlayersDetails>();
        containerBuilder.RegisterType<ReplayVisualization.ColumnPlayerDetails>();
        containerBuilder.RegisterType<ReplayVisualization.ColumnPlayerActions>();
        containerBuilder.RegisterType<ColumnStateMachineBranch>();

        // plugin services
        containerBuilder.RegisterSingletonSelf<HookProvider>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<CommandService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<DtrService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<IpcService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<TickService>();

        // can't be a service since it needs to run before constructors do (otherwise root config will not be populated)
        containerBuilder.RegisterBuildCallback(scope =>
        {
            var self = Assembly.GetExecutingAssembly();
            foreach (var sub in scope.Resolve<IReloadable[]>())
                sub.Reload([], [self]);
            ReplayAnalysis.AnalyzerRegistry.Reload([], [self]);
        });
    }

    public override void ConfigureServices(IServiceCollection serviceCollection) { }

    public virtual void ConfigureExtra(ContainerBuilder containerBuilder)
    {
        // global worldstate
        containerBuilder.Register(ctx =>
        {
            var dataManager = ctx.Resolve<IDataManager>();
            unsafe
            {
                return new WorldState((ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency, dataManager.GameData.Repositories["ffxiv"].Version);
            }
        }).SingleInstance();

        containerBuilder.Register(ctx => ctx.Resolve<IDalamudPluginInterface>().ConfigDirectory).Keyed<DirectoryInfo>(Keys.ConfigDirectory);

        //containerBuilder.RegisterSingletonSelf<MainDebugWindow>().As<WindowBase>();

        containerBuilder.RegisterSingletonSelfAndInterfaces<Camera>();

        // plugin services
        containerBuilder.RegisterSingletonSelfAndInterfaces<PingService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<SaveConfigService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<OpcodeMap>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<PacketInterceptor>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<ActionManagerEx>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<HintExecutor>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<MovementOverride>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<WorldStateGameSync>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<AutoBitmapService>();

#if LOCAL_CS
        containerBuilder.RegisterBuildCallback(ctx =>
        {
            ctx.Resolve<IPluginLog>().Info("initializing address resolver (LOCAL_CS)");

            InteropGenerator.Runtime.Resolver.GetInstance.Setup(
                ctx.Resolve<ISigScanner>().SearchBase,
                ctx.Resolve<IDataManager>().GameData.Repositories["ffxiv"].Version,
                new(ctx.ResolveKeyed<DirectoryInfo>(Keys.ConfigDirectory).FullName + "/cs.json")
            );
            FFXIVClientStructs.Interop.Generated.Addresses.Register();
            InteropGenerator.Runtime.Resolver.GetInstance.Resolve();
        });
#endif
    }

    public static string GetStorageDir() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vbm");
}
