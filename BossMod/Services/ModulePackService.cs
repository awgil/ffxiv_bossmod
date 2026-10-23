using BossMod.Autorotation;
using BossMod.ReplayAnalysis;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

public sealed class ModulePackService(IDalamudPluginInterface dalamud, IPluginLog logger) : IHostedService, IDisposable
{
    class LoadContext() : AssemblyLoadContext(true)
    {
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // assemblies from parent
            // TODO: doc page says you can return null here to make these load from the parent context, but that doesn't seem to work for me
            if (assemblyName.Name == "BossMod")
                return typeof(ModulePackService).Assembly;
            if (assemblyName.Name == "Dalamud")
                return typeof(IDalamudPluginInterface).Assembly;
            if (assemblyName.Name == "Dalamud.Bindings.ImGui")
                return typeof(ImGuiCol).Assembly;
            if (assemblyName.Name == "FFXIVClientStructs")
                return typeof(FFXIVClientStructs.FFXIV.Client.Game.Camera).Assembly;
            if (assemblyName.Name == "Lumina")
                return typeof(Lumina.GameData).Assembly;
            if (assemblyName.Name == "Lumina.Excel")
                return typeof(Lumina.Excel.Sheets.Action).Assembly;
            if (assemblyName.Name == "System.Data.SQLite")
                return typeof(SQLiteBlob).Assembly;

            return base.Load(assemblyName);
        }
    }

    // one context per filepath - this is because AssemblyLoadContext doesn't support unloading individual assemblies, and we don't want to force everything to hot reload when one file changes
    private readonly Dictionary<string, LoadContext> _loadContexts = [];
    private readonly SensibleFileWatcher _watcher = new(filter: "*.dll", includeSubdirectories: true);
    private readonly EventSubscriptions _modified = new();
    public readonly string ModuleDir = Path.Join(Plugin.GetStorageDir(), dalamud.IsDev ? "modules-dev" : "modules");

    public IEnumerable<Assembly> Loaded => _loadContexts.Values.SelectMany(c => c.Assemblies);

    // fired after all the registries have processed a file change event
    public readonly Event Modified = new();

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        InstallLibraries(dalamud.AssemblyLocation);

        ReloadFrom(ModuleDir);

        _modified.Add(
            _watcher.Renamed.Subscribe(OnFileRenamed),
            _watcher.Changed.Subscribe(f => OnFileChanged(f.FullPath)),
            _watcher.Deleted.Subscribe(f => OnFileDeleted(f.FullPath))
        );
        _watcher.EnableRaisingEvents = true;
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _watcher.EnableRaisingEvents = false;
    }

    private void ReloadFrom(string packDirectory)
    {
        if (!Path.Exists(packDirectory))
            return;

        _watcher.Path = packDirectory;

        var dir = new DirectoryInfo(packDirectory);

        List<Assembly> assembliesOld = [];
        List<Assembly> assembliesNew = [];

        foreach (var file in dir.EnumerateFiles())
            if (file.Extension == ".dll")
            {
                var (o, n) = ReplaceLoadContext(file.FullName);
                assembliesOld.AddRange(o?.Assemblies ?? []);
                assembliesNew.AddRange(n?.Assemblies ?? []);
            }

        ReloadAll(assembliesOld, assembliesNew);

        Modified.Fire();
    }

    private void OnFileRenamed(RenamedEventArgs e)
    {
        if (_loadContexts.Remove(e.OldFullPath, out var ctx))
            _loadContexts[e.FullPath] = ctx;
    }

    private (LoadContext? Old, LoadContext? New) ReplaceLoadContext(string fullPath)
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
            logger.Warning(ex, $"Unable to hot-reload {fullPath}");
            _loadContexts.Remove(fullPath);
            return (ctxOld, null);
        }

        return (ctxOld, ctxNew);
    }

    private void OnFileChanged(string fullPath)
    {
        var (ctxOld, ctxNew) = ReplaceLoadContext(fullPath);

        ReloadAll(ctxOld?.Assemblies ?? [], ctxNew?.Assemblies ?? []);
        ctxOld?.Unload();

        Modified.Fire();
        Service.ShowNotification($"Reloaded {Path.GetFileName(fullPath)}");
    }

    private void OnFileDeleted(string fullPath)
    {
        if (_loadContexts.Remove(fullPath, out var ctx))
        {
            ReloadAll(ctx.Assemblies, []);
            ctx.Unload();

            Modified.Fire();
            Service.ShowNotification($"Unloaded {Path.GetFileName(fullPath)}");
        }
    }

    public void ReloadAll(IEnumerable<Assembly> oldAssemblies, IEnumerable<Assembly> newAssemblies)
    {
        Service.Config.Reload(oldAssemblies, newAssemblies);
        BossModuleRegistry.Reload(oldAssemblies, newAssemblies);
        RotationModuleRegistry.Reload(oldAssemblies, newAssemblies);
        ZoneModuleRegistry.Reload(oldAssemblies, newAssemblies);
        AnalyzerRegistry.Reload(oldAssemblies, newAssemblies);
    }

    private void InstallLibraries(FileInfo assemblyLocation)
    {
        var modulesDir = ModuleDir;
        var assemblyDir = assemblyLocation.DirectoryName;

        Directory.CreateDirectory(modulesDir);

        void copy(string filename)
        {
            var src = Path.Join(assemblyDir, filename);
            var dest = Path.Join(modulesDir, filename);
            if (File.Exists(src))
            {
                if (File.Exists(dest) && File.GetCreationTimeUtc(dest) > File.GetCreationTimeUtc(src))
                {
                    logger.Debug($"{dest} is newer than our version, skipping copy");
                    return;
                }

                logger.Debug($"copied {src} to {dest}");
                File.Copy(src, Path.Join(modulesDir, filename), true);

                return;
            }

            logger.Warning($"{src} does not exist, cannot copy");
        }

        copy("BossMod.Autorotation.dll");
        copy("BossMod.Modules.dll");
        copy("BossMod.Ultimate.dll");
    }

    public void Dispose()
    {
        _modified.Dispose();
        _watcher.Dispose();

        foreach (var c in _loadContexts.Values)
            c.Unload();
    }
}
