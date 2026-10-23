#if DEBUG
using Autofac;
using BossMod;
using BossMod.Dev;
using BossMod.Interfaces;
using BossMod.Services;
using DalaMock.Core.Mocks;
using DalaMock.Core.Plugin;
using DalaMock.Shared.Extensions;
using DalaMock.Shared.Interfaces;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.IO;
using System.Reflection;
using static DalaMock.Core.Fonts.MockDefaultFontConfig;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
sealed class DalamudLibPathAttribute(string path) : Attribute
{
    public string Path { get; } = path;
}

static class Program
{
    private class MockPlugin(MockReplacementContainer mrc, IDalamudPluginInterface dalamud) : Plugin(dalamud)
    {
        public override IReplacementContainer ReplacementContainer { get; } = mrc;

        public override void ConfigureExtra(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterBuildCallback(scope =>
            {
                Service.IsMock = true;

                scope.Resolve<IDataManager>().GameData.Options.RsvResolver = Service.LuminaRSV.TryGetValue;
            });

            // ConfigDirectory is supposed to be plugin-scoped, but dalamock has a bug and returns the top level pluginConfigs dir instead
            containerBuilder.Register(ctx => new DirectoryInfo(Path.Join(ctx.Resolve<IDalamudPluginInterface>().ConfigDirectory.FullName, "BossMod"))).Keyed<DirectoryInfo>(Keys.ConfigDirectory);

            containerBuilder.Register(_ =>
            {
                var ws = new WorldState(0, "unknown");

                ws.Execute(new ActorState.OpCreate(0x12345678, 0, 0, 0, "Player", 0, ActorType.Player, Class.WAR, 100, new(90, 0, 90, 0), 0.5f, new(500, 600, 100, 10000, 10000), true, true, 0, 0));
                ws.Execute(new PartyState.OpModify(0, new(0x87654321, 0x12345678, false)));

                return ws;
            }).SingleInstance();

            containerBuilder.RegisterSingletonSelfAndInterfaces<MockWorldStateGameSync>();
            containerBuilder.RegisterSingletonSelfAndInterfaces<MockAmex>();
            containerBuilder.RegisterSingletonSelfAndInterfaces<MockMovementOverride>();
            containerBuilder.RegisterSingletonSelfAndInterfaces<MockHintExecutor>();
            containerBuilder.RegisterSingletonSelfAndInterfaces<MockCamera>();

            containerBuilder.RegisterSingletonSelf<MainDevWindow>();
        }
    }

    static readonly string[] SupportedLibs = [
        "Dalamud",
        "Dalamud.Common",
        "FFXIVClientStructs",
        "Lumina",
        "NativeFileDialogSharp",
        "Serilog",
        "Serilog.Sinks.Console",
        "TerraFX.Interop.Windows"
    ];

    // bit of a hack to not have to specify Private=false for every single dependency since they will end up in the output dir and interfere with dalamud if this is loaded as a normal plugin
    static void Initialize()
    {
        var dalapath = Assembly.GetEntryAssembly()!.GetCustomAttribute<DalamudLibPathAttribute>()!.Path;

        AppDomain.CurrentDomain.AssemblyResolve += delegate (object? sender, ResolveEventArgs args)
        {
            var libName = args.Name.Split(',').FirstOrDefault();

#if LOCAL_CS
            if (libName == "FFXIVClientStructs")
            {
                var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Assembly.LoadFrom(Path.Join(root, "FFXIVClientStructs.dll"));
            }
#endif

            return libName != null && SupportedLibs.Contains(libName) ? Assembly.LoadFrom(Path.Join(dalapath, $"{libName}.dll")) : null;
        };

        // FIXME once dalamock is no longer broken, bundled cimgui is loaded alongside dalamud version and causes segfault
        var exe = new FileInfo(Assembly.GetExecutingAssembly()!.Location);
        var cim = Path.Join(exe.DirectoryName, "cimgui.dll");
        if (File.Exists(cim))
            File.Delete(cim);
    }

    static void RealMain()
    {
        var cnt = new MockContainer(new()
        {
            GamePathString = "E:\\SteamLibrary\\steamapps\\common\\FINAL FANTASY XIV Online\\game\\sqpack",
            PluginSavePathString = "C:\\Users\\me\\AppData\\Roaming\\XIVLauncher\\pluginConfigs",
            DefaultFont = new()
            {
                Kind = FontKind.DalamudDefault,
                SizePt = 14
            }
        });
        var ui = cnt.GetMockUi();
        var loader = cnt.GetPluginLoader();
        var pl = loader.AddPlugin(typeof(MockPlugin));
        loader.StartPlugin(pl).Wait();
        ui.Run();
    }

    static void Main()
    {
        Initialize();
        RealMain();
    }
}
#endif
