using Autofac.Features.AttributeFilters;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal sealed class StartupService(IPluginLog logger, IDalamudPluginInterface dalamud, ModulePackService packService, [KeyFilter(Keys.ConfigDirectory)] DirectoryInfo pluginConfigDir, ConfigRoot configGlobal, IEnumerable<ConfigMigration> migrations) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(migrations.Where(m => m.ShouldExecute(configGlobal)).Select(m => m.Execute()));

        // install modules before filesystem watcher starts up so we don't trigger multiple modify events on plugin load
        MigratePlans();
        InstallModules();
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    void MigratePlans()
    {
        var source = Path.Join(pluginConfigDir.FullName, "autorot");

        // no autorot dir means fresh install
        if (!Path.Exists(source))
        {
            logger.Verbose($"[Migrator] No configs, nothing to do.");
            return;
        }

        var destination = Path.Join(Plugin.GetStorageDir(), "autorot");

        if (File.Exists(Path.Join(destination, ".migrate-ok")))
        {
            logger.Verbose($"[Migrator] Nothing to do.");
            return;
        }

        Utils.CopyRecursive(source, destination);

        File.Create(Path.Join(destination, ".migrate-ok")).Dispose();

        logger.Debug($"[Migrator] Done.");
    }

    void InstallModules()
    {
        var modulesDir = packService.ModuleDir;
        var assemblyDir = dalamud.AssemblyLocation.DirectoryName;

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
}
