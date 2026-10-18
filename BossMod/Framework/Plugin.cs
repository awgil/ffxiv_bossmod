using Autofac;
using Autofac.Features.AttributeFilters;
using BossMod.Autorotation;
using BossMod.Dev;
using BossMod.ReplayAnalysis;
using BossMod.ReplayVisualization;
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

public class Plugin : HostedPlugin
{
    public string Name => "Boss Mod";

    public Plugin(IDalamudPluginInterface dalamud, ISigScanner sigScanner, IDataManager dataManager) : base(dalamud)
    {
#if LOCAL_CS
        if (sigScanner == null) // nonexistent in mock environment
            return;

        InteropGenerator.Runtime.Resolver.GetInstance.Setup(sigScanner.SearchBase, dataManager.GameData.Repositories["ffxiv"].Version, new(dalamud.ConfigDirectory.FullName + "/cs.json"));
        FFXIVClientStructs.Interop.Generated.Addresses.Register();
        InteropGenerator.Runtime.Resolver.GetInstance.Resolve();
#endif
    }

    public override HostedPluginOptions ConfigureOptions() => new() { UseMediatorService = true };
    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterSingletonSelfAndInterfaces<TickService>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<PackLoader>();

        containerBuilder.RegisterSingletonSelfAndInterfaces<PresetDatabase>().WithAttributeFiltering();
        containerBuilder.RegisterSingletonSelfAndInterfaces<PlanDatabase>().WithAttributeFiltering();

        containerBuilder.RegisterSingletonSelf<RotationModuleRegistry>();
        containerBuilder.RegisterSingletonSelf<RotationDatabase>();
        containerBuilder.RegisterType<RotationModuleManager>().InstancePerLifetimeScope();

        containerBuilder.RegisterSingletonSelf<BossModuleRegistry>();
        containerBuilder.RegisterType<BossModuleManager>().InstancePerLifetimeScope();

        containerBuilder.RegisterType<ZoneModuleManager>().InstancePerLifetimeScope();

        containerBuilder.Register((ctx) =>
        {
            var root = new ConfigRoot(ctx.Resolve<IDalamudPluginInterface>().ConfigFile, ctx.Resolve<BossModuleRegistry>(), ctx.Resolve<JsonSerializerOptions>());
            root.PreviousAssemblyVersion = root.AssemblyVersion;
            root.AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version!;

            Service.Config = root;

            return root;
        }).SingleInstance();
        foreach (var node in Utils.GetDerivedTypes<ConfigNode>(Assembly.GetExecutingAssembly()))
        {
            containerBuilder.Register(s =>
            {
                var root = s.Resolve<ConfigRoot>();
                var method = typeof(ConfigRoot).GetMethod(nameof(ConfigRoot.Get), BindingFlags.Instance | BindingFlags.Public, [])!.MakeGenericMethod(node);
                return method.Invoke(root, [])!;
            }).As(node).SingleInstance();
        }

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
                    new JsonPlanConverter(bmr, rmr)
                }
            };
        }).SingleInstance();

        containerBuilder.RegisterType<ActionQueue>();

        containerBuilder.RegisterType<AIHintsBuilder>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<AIHints>().InstancePerLifetimeScope();

        containerBuilder.RegisterType<ConfigUI>().InstancePerLifetimeScope().WithAttributeFiltering();
        containerBuilder.RegisterSingletonSelf<ModuleViewer>();

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).Where(t => t.IsSubclassOf(typeof(Defs))).As<Defs>().SingleInstance();

        containerBuilder.RegisterSingletonSelf<ActionDefinitions>();

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

        containerBuilder.RegisterGeneric((ctx, prms) =>
        {
            var gd = ctx.Resolve<IDataManager>().GameData;
            var method = typeof(GameData).GetMethod(nameof(GameData.GetExcelSheet))?.MakeGenericMethod(prms);
            return method!.Invoke(gd, [null, null])!;
        }).As(typeof(ExcelSheet<>));

        containerBuilder.RegisterSingletonSelf<BozjaActionID>();

        containerBuilder.RegisterType<AnalysisManager>();
        containerBuilder.RegisterType<ReplayManager.AnalysisEntry>();

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.IsSubclassOf(typeof(WindowBase)))
            .Except<MainDevWindow>()
            .Except<ReplayDetailsWindow>()
            .As<WindowBase>()
            .SingleInstance()
            .WithAttributeFiltering();

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.IsSubclassOf(typeof(DetachedWindow)))
            .As<DetachedWindow>()
            .WithAttributeFiltering();

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.IsSubclassOf(typeof(TestWindow)))
            .As<TestWindow>()
            .WithAttributeFiltering();

        ConfigureExtra(containerBuilder);

        containerBuilder.RegisterBuildCallback(scope =>
        {
            var self = Assembly.GetExecutingAssembly();
            scope.Resolve<ConfigRoot>().Reload([], [self]);
            scope.Resolve<BossModuleRegistry>().Reload([], [self]);
            scope.Resolve<RotationModuleRegistry>().Reload([], [self]);
        });
    }

    public override void ConfigureServices(IServiceCollection serviceCollection) { }

    public virtual void ConfigureExtra(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<ActionManagerEx>().As<IAmex>().SingleInstance();
        containerBuilder.Register(ctx =>
        {
            var dataManager = ctx.Resolve<IDataManager>();
            unsafe
            {
                return new WorldState((ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency, dataManager.GameData.Repositories["ffxiv"].Version);
            }
        }).InstancePerLifetimeScope();

        containerBuilder.Register(ctx => ctx.Resolve<IDalamudPluginInterface>().ConfigDirectory).Keyed<DirectoryInfo>(Keys.ConfigDirectory);

        containerBuilder.RegisterSingletonSelf<MainDebugWindow>().As<WindowBase>();
    }

    public static string GetStorageDir() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vbm");
}
