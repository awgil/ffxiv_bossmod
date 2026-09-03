using BossMod.Autorotation;
using BossMod.ReplayAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace BossMod;

public sealed class PackLoader : IDisposable
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
    private readonly Dictionary<string, LoadContext> _loadContexts = [];
    private readonly FileSystemWatcher _watcher;
    private readonly MemoryCache _memCache = new(new MemoryCacheOptions());
    private readonly DeveloperConfig _config = Service.Config.Get<DeveloperConfig>();
    private readonly EventSubscription _modified;
    public static readonly string ModuleDir = Path.Join(ReplayHistory.GetStorageDir().FullName, "modules");

    public IEnumerable<Assembly> Loaded => _loadContexts.Values.SelectMany(c => c.Assemblies);

    // fired after all the registries have processed a file change event
    public readonly Event Modified = new();

    public PackLoader()
    {
        _watcher = new()
        {
            NotifyFilter = NotifyFilters.LastWrite // creation/modification
                         | NotifyFilters.FileName // deletion
        };
        _watcher.Renamed += OnFileRenamed;
        _watcher.Deleted += OnChangeEvent;
        _watcher.Changed += OnChangeEvent;
        _watcher.Error += (sender, e) => Service.Log($"Error: {e.GetException()}");
        _watcher.Filter = "*.dll";
        _watcher.IncludeSubdirectories = true;

        if (!Directory.Exists(ModuleDir))
            Directory.CreateDirectory(ModuleDir);

        ReloadFrom(ModuleDir);

        _modified = _config.Modified.ExecuteAndSubscribe(() =>
        {
            _watcher.EnableRaisingEvents = _config.HotReload;
        });
    }

    private void ReloadFrom(string packDirectory)
    {
        if (!Path.Exists(packDirectory))
            return;

        _watcher.Path = packDirectory;

        var dir = new DirectoryInfo(packDirectory);
        foreach (var file in dir.EnumerateFiles())
            if (file.Extension == ".dll")
                OnFileChanged(file.FullName);

        Modified.Fire();
    }

    private void OnChangeEvent(object sender, FileSystemEventArgs args)
    {
        Service.PluginLog.Verbose($"firing {args.ChangeType} {args.FullPath}");
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));
        _memCache.Set(args.FullPath, args, new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove).AddExpirationToken(new CancellationChangeToken(cts.Token)).RegisterPostEvictionCallback(OnCacheEntryRemoved));
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        if (_loadContexts.Remove(e.OldFullPath, out var ctx))
            _loadContexts[e.FullPath] = ctx;
    }

    private void OnCacheEntryRemoved(object key, object? value, EvictionReason reason, object? state)
    {
        if (reason != EvictionReason.TokenExpired || value is not FileSystemEventArgs e)
            return;

        if (e.ChangeType.HasFlag(WatcherChangeTypes.Deleted))
            OnFileDeleted(e.FullPath);
        else
            OnFileChanged(e.FullPath);
    }

    private void OnFileChanged(string fullPath)
    {
        var ctxOld = _loadContexts.Remove(fullPath, out var ctx) ? ctx : null;
        var ctxNew = _loadContexts[fullPath] = new LoadContext();

        try
        {
            using var s = Utils.OpenShareable(fullPath, FileMode.Open);
            var raw = new byte[s.Length];
            s.ReadExactly(raw, 0, (int)s.Length);
            ctxNew.LoadFromStream(new MemoryStream(raw));
        }
        catch (BadImageFormatException ex)
        {
            Service.PluginLog.Warning(ex, $"Unable to hot-reload {fullPath}");
            _loadContexts.Remove(fullPath);
        }

        Service.Config.Reload(ctxOld?.Assemblies ?? [], ctxNew.Assemblies);
        BossModuleRegistry.Reload(ctxOld?.Assemblies ?? [], ctxNew.Assemblies);
        RotationModuleRegistry.Reload(ctxOld?.Assemblies ?? [], ctxNew.Assemblies);
        ZoneModuleRegistry.Reload(ctxOld?.Assemblies ?? [], ctxNew.Assemblies);
        AnalyzerRegistry.Reload(ctxOld?.Assemblies ?? [], ctxNew.Assemblies);

        ctxOld?.Unload();

        Modified.Fire();

        if (_watcher.EnableRaisingEvents)
            Service.Notifications?.AddNotification(new()
            {
                Content = $"Loaded {Path.GetFileName(fullPath)}",
                Type = Dalamud.Interface.ImGuiNotification.NotificationType.Success,
            });
    }

    private void OnFileDeleted(string fullPath)
    {
        if (_loadContexts.Remove(fullPath, out var ctx))
        {
            Service.Config.Reload(ctx.Assemblies, []);
            RotationModuleRegistry.Reload(ctx.Assemblies, []);
            BossModuleRegistry.Reload(ctx.Assemblies, []);
            RotationModuleRegistry.Reload(ctx.Assemblies, []);
            AnalyzerRegistry.Reload(ctx.Assemblies, []);

            ctx.Unload();

            Modified.Fire();
        }
    }

    public void Dispose()
    {
        _modified.Dispose();
        _watcher.Dispose();

        foreach (var c in _loadContexts.Values)
            c.Unload();
    }
}
