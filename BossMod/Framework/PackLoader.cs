using BossMod.Autorotation;
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
            // wtf does this even do, and more importantly, why is it necessary
            if (assemblyName.Name == "BossMod")
                return Assembly.GetExecutingAssembly();

            return base.Load(assemblyName);
        }
    }

    // one context per filepath - this is because AssemblyLoadContext doesn't support unloading individual assemblies, and we don't want to force everything to hot reload when one file changes
    readonly Dictionary<string, LoadContext> _loadContexts = [];
    private readonly FileSystemWatcher _watcher;
    private readonly DeveloperConfig _config = Service.Config.Get<DeveloperConfig>();
    private string _prevDirectory = "";

    public IEnumerable<Assembly> Loaded => _loadContexts.Values.SelectMany(c => c.Assemblies);

    private readonly EventSubscriptions _subscriptions;

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

        _subscriptions = new(
            _config.Modified.ExecuteAndSubscribe(() =>
            {
                var curDirectory = _config.ModulePackDirectory;
                if (_prevDirectory != curDirectory)
                    SetDirectory(curDirectory);
                _prevDirectory = curDirectory;
            })
        );
    }

    public void ForceReload() => SetDirectory(_config.ModulePackDirectory);

    private void SetDirectory(string packDirectory)
    {
        foreach (var ctx in _loadContexts.Values)
        {
            foreach (var asm in ctx.Assemblies)
                Unload(asm);

            ctx.Unload();
        }

        _loadContexts.Clear();

        var builtinsFile = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BossMod.Modules.dll");
        if (Path.Exists(builtinsFile))
            OnCreated(builtinsFile);
        else
            Service.PluginLog.Warning($"Builtin modules are missing, you won't see a whole lot in the UI");

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
        using (var s = File.OpenRead(fullPath))
        {
            raw = new byte[s.Length];
            s.ReadExactly(raw, 0, (int)s.Length);
        }
        var context = _loadContexts[fullPath] = new();
        try
        {
            Load(context.LoadFromStream(new MemoryStream(raw)));
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
        RotationModuleRegistry.UnloadFrom(asm);
        BossModuleRegistry.UnloadFrom(asm);
        ZoneModuleRegistry.UnloadFrom(asm);
    }

    static void Load(Assembly asm)
    {
        RotationModuleRegistry.ScanAssembly(asm);
        BossModuleRegistry.ScanAssembly(asm);
        ZoneModuleRegistry.ScanAssembly(asm);
    }

    public void Dispose()
    {
        _subscriptions.Dispose();

        foreach (var c in _loadContexts.Values)
            c.Unload();
    }
}
