using Autofac;
using Autofac.Features.AttributeFilters;
using BossMod.Autorotation;
using BossMod.Services;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Extensions;
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
    PlanManifest
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
        containerBuilder.RegisterSingletonSelf<PackLoader>();

        containerBuilder.RegisterType<BossModuleManager>().InstancePerLifetimeScope();

        containerBuilder.Register((ctx) =>
        {
            var dalamud = ctx.Resolve<IDalamudPluginInterface>();
            var root = new ConfigRoot(dalamud.ConfigFile);
            root.PreviousAssemblyVersion = root.AssemblyVersion;
            root.AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version!;

            root.Reload([], [Assembly.GetExecutingAssembly()]);

            //Service.Config = root;

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

        containerBuilder.RegisterType<ActionQueue>();
        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).Where(t => t.IsSubclassOf(typeof(Defs))).SingleInstance();

        containerBuilder.RegisterSingletonSelf<PresetDatabase>().WithAttributeFiltering();
        containerBuilder.RegisterSingletonSelf<PlanDatabase>().WithAttributeFiltering();
        containerBuilder.RegisterSingletonSelf<RotationDatabase>();
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

        ConfigureExtra(containerBuilder);
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
    }

    public static string GetStorageDir() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vbm");
}
