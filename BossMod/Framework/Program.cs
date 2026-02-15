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
    // don't make this externally visible since dalamud's LocalPlugin scan function will find it
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
        new MockNotificationManager(),
        new MockUnlockState()
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
            var configGlobal = scope.Resolve<ConfigRoot>();
            var logger = scope.Resolve<IPluginLog>();
            var dalamud = scope.Resolve<IDalamudPluginInterface>();
            var isMockConfig = dalamud.ConfigFile.FullName.Contains("DalaMock");
            if (isMockConfig)
                configGlobal.Modified.Subscribe(() => configGlobal.SaveToFile(dalamud.ConfigFile));
            else
                configGlobal.Modified.Subscribe(() => logger.Info("Config was modified. Saving is disabled."));

            // always show replays in dev mode, since it's the only useful thing we can do
            configGlobal.Get<ReplayManagementConfig>().ShowUI = true;
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
