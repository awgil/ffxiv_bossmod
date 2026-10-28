using Autofac;
using Autofac.Features.AttributeFilters;
using BossMod.Autorotation;
using BossMod.Interfaces;
using BossMod.Pathfinding;
using BossMod.Services;
using DalaMock.Host.Hosting;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Extensions;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;

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
        containerBuilder.RegisterSingletonSelfAndInterfaces<ModulePackService>();

        ConfigureExtra(containerBuilder);

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

        // lower level stuff
        containerBuilder.Register(ctx => new ReplayVisualization.GaugeVisualizer(ctx.Resolve<IUiBuilder>().FontAtlas)).SingleInstance();
        containerBuilder.Register(_ => Serialization.BuildSerializationOptions()).SingleInstance();
        containerBuilder.Register(scope =>
        {
            var dalamud = scope.Resolve<IDalamudPluginInterface>();

            var configRoot = new ConfigRoot(dalamud.ConfigFile);
            configRoot.PreviousAssemblyVersion = configRoot.AssemblyVersion;
            configRoot.AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version!;

            return configRoot;
        }).SingleInstance();

        // global plugin state stuff
        containerBuilder.RegisterSingletonSelfAndInterfaces<PresetDatabase>().WithAttributeFiltering();
        containerBuilder.RegisterSingletonSelfAndInterfaces<PlanDatabase>().WithAttributeFiltering();
        containerBuilder.RegisterSingletonSelf<RotationDatabase>();
        containerBuilder.RegisterSingletonSelf<ObstacleMapManager>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<ReplayManager>().WithAttributeFiltering();

        // everything defined in this section is created on demand during replay parsing/viewing
        containerBuilder.RegisterType<BossModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<RotationModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<ZoneModuleManager>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<AIHintsBuilder>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<AIHints>().InstancePerLifetimeScope();

        // framework update stuff
        containerBuilder.RegisterSingletonSelfAndInterfaces<DtrService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<IpcService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<TickService>().WithAttributeFiltering();

        containerBuilder.RegisterBuildCallback(scope =>
        {
            var dalamud = scope.Resolve<IDalamudPluginInterface>();

            dalamud.Create<Service>();

            Service.IsDev = dalamud.IsDev;

            Service.Config = scope.Resolve<ConfigRoot>();
            Service.LuminaGameData = scope.Resolve<IDataManager>().GameData;
            Service.Mediator = scope.Resolve<MediatorService>();
            Service.GaugeVisualizer = scope.Resolve<ReplayVisualization.GaugeVisualizer>();
            Service.UiBuilder = scope.Resolve<IUiBuilder>();
            Service.PluginInterface = dalamud;

            // register builtin config nodes
            scope.Resolve<ModulePackService>().ReloadAll([], [Assembly.GetExecutingAssembly()]);

            Service.Camera = scope.Resolve<ICamera>();

            Service.WindowSystem = scope.Resolve<IWindowSystemFactory>().Create("vbm");
        });
    }
    public override void ConfigureServices(IServiceCollection serviceCollection) { }

    public static string GetStorageDir() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vbm");

    public virtual void ConfigureExtra(ContainerBuilder containerBuilder)
    {
        containerBuilder.Register(ctx => ctx.Resolve<IDalamudPluginInterface>().ConfigDirectory).Keyed<DirectoryInfo>(Keys.ConfigDirectory);

        containerBuilder.RegisterSingletonSelfAndInterfaces<ActionManagerEx>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<MovementOverride>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<WorldStateGameSync>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<HintExecutor>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<Camera>();

        containerBuilder.RegisterSingletonSelfAndInterfaces<AutoBitmapService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<PingService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<SaveConfigService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<SlashCommandService>();

        containerBuilder.RegisterSingletonSelf<MainDebugWindow>();

        // global worldstate
        containerBuilder.Register(scope =>
        {
            unsafe
            {
                return new WorldState(
                    (ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency,
                    scope.Resolve<IDataManager>().GameData.Repositories["ffxiv"].Version
                );
            }
        }).SingleInstance();

        containerBuilder.RegisterBuildCallback(ctx =>
        {
            unsafe
            {
                MultiboxUnlock.Exec(ctx.Resolve<IPluginLog>());
            }
        });

#if LOCAL_CS
        containerBuilder.RegisterBuildCallback(ctx =>
        {
            var sigScanner = ctx.Resolve<ISigScanner>();
            var dataManager = ctx.Resolve<IDataManager>();
            var dalamud = ctx.Resolve<IDalamudPluginInterface>();
            var logger = ctx.Resolve<IPluginLog>();

            logger.Info("LOCAL_CS: initializing our custom address resolver");

            InteropGenerator.Runtime.Resolver.GetInstance.Setup(sigScanner.SearchBase, dataManager.GameData.Repositories["ffxiv"].Version, new(dalamud.ConfigDirectory.FullName + "/cs.json"));
            FFXIVClientStructs.Interop.Generated.Addresses.Register();
            InteropGenerator.Runtime.Resolver.GetInstance.Resolve();
        });
#endif
    }
}
