using BossMod.Autorotation;
using BossMod.ReplayAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace BossMod;

sealed class PackLoader : IDisposable
{
    class LoadContext() : AssemblyLoadContext(true)
    {
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            //loadcontext has no idea where these are i guess
            if (assemblyName.Name == "BossMod")
                return typeof(PackLoader).Assembly;
            if (assemblyName.Name == "FFXIVClientStructs")
                return typeof(FFXIVClientStructs.FFXIV.Client.Game.Camera).Assembly;
            if (assemblyName.Name == "Lumina")
                return typeof(Lumina.GameData).Assembly;

            return base.Load(assemblyName);
        }
    }

    // one context per filepath - this is because AssemblyLoadContext doesn't support unloading individual assemblies, and we don't want to force everything to hot reload when one file changes
    readonly Dictionary<string, LoadContext> _loadContexts = [];
    private readonly FileSystemWatcher _watcher;
    private readonly DeveloperConfig _config = Service.Config.Get<DeveloperConfig>();
    public static readonly string ModuleDir = Path.Join(ReplayHistory.GetStorageDir().FullName, "modules");

    public IEnumerable<Assembly> Loaded => _loadContexts.Values.SelectMany(c => c.Assemblies);

    public PackLoader()
    {
        _watcher = new()
        {
            NotifyFilter = NotifyFilters.LastWrite // creation/modification
                         | NotifyFilters.FileName // deletion
        };
        _watcher.Created += (sender, e) => OnCreated(e.FullPath);
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += (sender, e) => Service.Log($"Error: {e.GetException()}");
        _watcher.Filter = "*.dll";
        _watcher.IncludeSubdirectories = true;

        if (!Directory.Exists(ModuleDir))
            Directory.CreateDirectory(ModuleDir);

        ReloadFrom(ModuleDir);
    }

    public void ForceReload() => ReloadFrom(_config.ModulePackDirectory);

    private void ReloadFrom(string packDirectory)
    {
        foreach (var ctx in _loadContexts.Values)
        {
            foreach (var asm in ctx.Assemblies)
                Unload(asm);

            ctx.Unload();
        }

        _loadContexts.Clear();

        if (!Path.Exists(packDirectory))
            return;

        _watcher.Path = packDirectory;
        _watcher.EnableRaisingEvents = true;

        var dir = new DirectoryInfo(packDirectory);
        foreach (var file in dir.EnumerateFiles())
            if (file.Extension == ".dll")
                OnCreated(file.FullName);
    }

    void OnCreated(string fullPath)
    {
        Service.Log($"loading assembly from {fullPath}");
        byte[] raw;
        using (var s = Utils.OpenShareable(fullPath))
        {
            raw = new byte[s.Length];
            s.ReadExactly(raw, 0, (int)s.Length);
        }
        var context = _loadContexts[fullPath] = new();
        try
        {
            Load(context.LoadFromStream(new MemoryStream(raw)), fullPath);
        }
        catch (BadImageFormatException e)
        {
            Service.PluginLog.Warning(e, $"Unable to load assembly {fullPath}");
            _loadContexts.Remove(fullPath);
        }
    }

    void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (_loadContexts.TryGetValue(e.FullPath, out var ctx))
        {
            Service.Log($"unloading assembly from {e.FullPath}");

            foreach (var asm in ctx.Assemblies)
                Unload(asm);

            ctx.Unload();

            _loadContexts.Remove(e.FullPath);
        }
    }

    void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (_loadContexts.Remove(e.OldFullPath, out var ctx))
            _loadContexts[e.FullPath] = ctx;
    }

    static void Unload(Assembly asm)
    {
        Service.Config.UnloadFrom(asm);
        RotationModuleRegistry.UnloadFrom(asm);
        BossModuleRegistry.UnloadFrom(asm);
        ZoneModuleRegistry.UnloadFrom(asm);
        AnalyzerRegistry.UnloadFrom(asm);
    }

    static void Load(Assembly asm, string dllPath)
    {
        // TODO: need to rebuild config tree as well
        Service.Config.ScanAssembly(asm);
        RotationModuleRegistry.ScanAssembly(asm);
        BossModuleRegistry.ScanAssembly(asm);
        ZoneModuleRegistry.ScanAssembly(asm);
        AnalyzerRegistry.ScanAssembly(asm);

        Service.Notifications?.AddNotification(new()
        {
            Content = $"Loaded {Path.GetFileName(dllPath)}",
            Type = Dalamud.Interface.ImGuiNotification.NotificationType.Success,
        });
    }

    public void Dispose()
    {
        _watcher.Dispose();

        foreach (var c in _loadContexts.Values)
            c.Unload();
    }
}
