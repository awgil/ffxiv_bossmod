using Autofac;
using BossMod.Services;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Extensions;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace BossMod;

public class Plugin : HostedPlugin
{
    public string Name => "Boss Mod";

    public Plugin(IDalamudPluginInterface dalamud, IEnumerable<ISigScanner> maybeSigScanner, IDataManager dataManager) : base(dalamud)
    {
#if LOCAL_CS
        InteropGenerator.Runtime.Resolver.GetInstance.Setup(maybeSigScanner.First().SearchBase, dataManager.GameData.Repositories["ffxiv"].Version, new(dalamud.ConfigDirectory.FullName + "/cs.json"));
        FFXIVClientStructs.Interop.Generated.Addresses.Register();
        InteropGenerator.Runtime.Resolver.GetInstance.Resolve();
#endif
    }

    public override HostedPluginOptions ConfigureOptions() => new() { UseMediatorService = true };
    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterSingletonSelfAndInterfaces<TickService>();
    }
    public override void ConfigureServices(IServiceCollection serviceCollection) { }

    public static string GetStorageDir() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vbm");
}
