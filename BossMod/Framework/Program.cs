using BossMod;
using DalaMock.Core.Plugin;
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
    private class MockPlugin(IDalamudPluginInterface dalamud, ICommandManager cmd, IDataManager data) : Plugin(dalamud, cmd, data);

    static readonly string[] SupportedLibs = [
        "Dalamud",
        "Dalamud.Common",
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
        loader.StartPlugin(pl).Wait();
        ui.Run();
    }

    static void Main()
    {
        Initialize();
        RealMain();
    }
}
