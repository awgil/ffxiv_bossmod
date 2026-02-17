using Autofac;
using BossMod;
using BossMod.Mock;
using BossMod.Mocks;
using BossMod.Testing;
using DalaMock.Core.DI;
using DalaMock.Core.Mocks;
using DalaMock.Core.Windows;
using DalaMock.Shared.Extensions;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface.Windowing;
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
        IGameConfig gameConfig,
        IChatGui chatGui,
        IKeyState keyState,
        ITextureProvider tex,
        IGameInteropProvider hook,
        ISigScanner scanner,
        ITargetManager targetManager,
        INotificationManager notifications,
        IUnlockState unlockState
    ) : Plugin(dalamud, log, clientState, playerState, objects, commandManager, dataManager, dtrBar, condition, gameGui, gameConfig, chatGui, keyState, tex, hook, scanner, targetManager, notifications, unlockState)
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
            var isMockConfig = dalamud.ConfigFile.FullName.Contains("DalaMock", StringComparison.Ordinal);
            configGlobal.Modified.Subscribe(() =>
            {
                logger.Info("Config was modified.");
                if (isMockConfig)
                    configGlobal.SaveToFile(dalamud.ConfigFile);
                else
                    logger.Info("Using real config file. Saving is disabled.");
            });

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

            containerBuilder.RegisterSingletonSelf<MainTestWindow>().As<Window>();
            containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.IsSubclassOf(typeof(TestWindow)) && !t.IsAbstract)
                .As<TestWindow>()
                .AsSelf()
                .SingleInstance();
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
        var cnt = new MockContainer(serviceReplacements: new()
        {
            { typeof(ISigScanner), typeof(MockSigScanner) },
            { typeof(IGameConfig), typeof(MockGameConfig) },
            { typeof(INotificationManager), typeof(MockNotificationManager) },
            { typeof(IUnlockState), typeof(MockUnlockState) }
        });
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
