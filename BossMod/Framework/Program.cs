#if DEBUG
using BossMod;
using DalaMock.Core.Mocks;
using DalaMock.Core.Plugin;
using DalaMock.Shared.Interfaces;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using System.Threading;

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
    }

    static readonly string[] SupportedLibs = [
        "Dalamud",
        "Dalamud.Common",
        "FFXIVClientStructs",
        "Lumina",
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
            return libName != null && SupportedLibs.Contains(libName) ? Assembly.LoadFrom(Path.Join(dalapath, $"{libName}.dll")) : null;
        };
    }

    static void RealMain()
    {
        // FIXME once fixed in dalamock (video subsystem has to init, otherwise it can't figure out how many monitors we have and always spawns the window in the same place)
        Veldrid.Sdl2.Sdl2Native.SDL_Init(Veldrid.Sdl2.SDLInitFlags.Video);

        var cnt = new MockContainer();
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
