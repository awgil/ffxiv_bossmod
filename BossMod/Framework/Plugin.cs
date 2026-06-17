using Autofac;
using BossMod.Services;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Extensions;
using Dalamud.Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace BossMod;

public class Plugin : HostedPlugin
{
    public string Name => "Boss Mod";

    public Plugin(IDalamudPluginInterface dalamud) : base(dalamud)
    {
#if LOCAL_CS
        InteropGenerator.Runtime.Resolver.GetInstance.Setup(sigScanner.SearchBase, gameVersion, new(dalamud.ConfigDirectory.FullName + "/cs.json"));
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
}
