using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Threading;

namespace BossMod;

internal sealed class SensibleFileWatcher : IDisposable
{
    public FileSystemWatcher Watcher { get; init; }
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public float Timeout = 1f;

    public Event<RenamedEventArgs> Renamed = new();
    public Event<FileSystemEventArgs> Deleted = new();
    public Event<FileSystemEventArgs> Changed = new();

    public string Path { get => Watcher.Path; set => Watcher.Path = value; }
    public bool EnableRaisingEvents { get => Watcher.EnableRaisingEvents; set => Watcher.EnableRaisingEvents = value; }

    public SensibleFileWatcher(string? path = null, string? filter = null, bool includeSubdirectories = false)
    {
        Watcher = new()
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
        };
        if (path != null)
            Watcher.Path = path;
        if (filter != null)
            Watcher.Filter = filter;
        if (includeSubdirectories)
            Watcher.IncludeSubdirectories = true;

        Watcher.Renamed += (_, args) => Renamed.Fire(args);
        Watcher.Changed += OnChangeEvent;
        Watcher.Deleted += OnChangeEvent;
        Watcher.Error += (sender, e) => Service.Log($"Error: {e.GetException()}");
    }

    private void OnChangeEvent(object sender, FileSystemEventArgs args)
    {
        Service.PluginLog.Verbose($"firing {args.ChangeType} {args.FullPath}");
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(Timeout));
        var opts = new MemoryCacheEntryOptions()
            .SetPriority(CacheItemPriority.NeverRemove)
            .AddExpirationToken(new CancellationChangeToken(cts.Token))
            .RegisterPostEvictionCallback(OnCacheEntryRemoved);
        _cache.Set(args.FullPath, args, opts);
    }

    private void OnCacheEntryRemoved(object key, object? value, EvictionReason reason, object? state)
    {
        if (reason != EvictionReason.TokenExpired || value is not FileSystemEventArgs args)
            return;

        if (args.ChangeType.HasFlag(WatcherChangeTypes.Deleted))
            Deleted.Fire(args);
        else
            Changed.Fire(args);
    }

    public void Dispose() => Watcher.Dispose();
}
