using Autofac;
using BossMod;
using BossMod.Mock;
using BossMod.Mocks;
using DalaMock.Core.DI;
using DalaMock.Core.Mocks;
using DalaMock.Core.Windows;
using DalaMock.Shared.Extensions;
using DalaMock.Shared.Interfaces;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.IO;
using System.Reflection;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
sealed class DalamudLibPathAttribute(string path) : Attribute
{
    public string Path { get; } = path;
}

static class Program
{
    // don't move this, we don't want dalamud's LocalPlugin scanner to find it
    private class MockPlugin(
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
        IChatGui chatGui,
        IKeyState keyState,
        ITextureProvider tex,
        IGameInteropProvider hook,
        ITargetManager targetManager
    ) : Plugin(
        dalamud,
        log,
        clientState,
        playerState,
        objects,
        commandManager,
        dataManager,
        dtrBar,
        condition,
        gameGui,
        new MockGameConfig(),
        chatGui,
        keyState,
        tex,
        hook,
        new MockSigScanner(),
        targetManager,
        new MockNotificationManager()
    )
    {
        public override void ConfigureContainer(ContainerBuilder containerBuilder)
        {
            base.ConfigureContainer(containerBuilder);
            containerBuilder.RegisterType<MockWindowSystem>().As<IWindowSystem>();
            containerBuilder.RegisterType<MockFileDialogManager>().As<IFileDialogManager>();
            containerBuilder.RegisterType<MockFont>().As<IFont>().SingleInstance();
        }

        public override void OnContainerBuild(ILifetimeScope scope)
        {
            Service.Config.Modified.Subscribe(() =>
            {
                Service.Log("Config was modified.");
            });

            // replay manager should display by default in dev mode since it's the only useful thing in the plugin
            Service.Config.Get<ReplayManagementConfig>().ShowUI = true;
        }

        public override void ConfigureExtra(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterSingletonSelfAndInterfaces<MockMovementOverride>();
            containerBuilder.RegisterSingletonSelfAndInterfaces<MockActionManagerEx>();
            containerBuilder.RegisterSingletonSelfAndInterfaces<MockWorldStateSync>();
            containerBuilder.RegisterSingletonSelfAndInterfaces<MockWorldStateFactory>();
            containerBuilder.RegisterSingletonSelfAndInterfaces<MockHintExecutor>();
        }
    }

    static readonly string[] SupportedLibs = [
        "Dalamud",
        "FFXIVClientStructs",
        "Lumina",
        "Serilog.Sinks.Console",
        "TerraFX.Interop.Windows"
    ];

    static void Initialize()
    {
        var dalapath = Assembly.GetEntryAssembly()!.GetCustomAttribute<DalamudLibPathAttribute>()!.Path;

        AppDomain.CurrentDomain.AssemblyResolve += delegate (object? sender, ResolveEventArgs args)
        {
            var libName = args.Name.Split(',').FirstOrDefault();
            return libName != null && SupportedLibs.Contains(libName) ? Assembly.LoadFrom(Path.Join(dalapath, $"{libName}.dll")) : null;
        };
    }

    static void RealMain()
    {
        var cnt = new MockContainer();
        var ui = cnt.GetMockUi();
        var loader = cnt.GetPluginLoader();
        var pl = loader.AddPlugin(typeof(MockPlugin));
        loader.StartPlugin(pl);
        ui.Run();
    }

    // OutputType=Exe only in Debug configuration
#pragma warning disable IDE0210 // Convert to top-level statements
    static void Main()
    {
        Initialize();
        RealMain();
    }
#pragma warning restore IDE0210 // Convert to top-level statements
}
