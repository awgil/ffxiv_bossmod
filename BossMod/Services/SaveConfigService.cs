using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

public sealed class SaveConfigService(IDalamudPluginInterface dalamud, IPluginLog logger) : IHostedService, IDisposable
{
    private CancellationTokenSource Source = new();
    private Task SaverTask = Task.CompletedTask;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Source = new();

        var token = Source.Token;
        SaverTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(30_000, token).ConfigureAwait(true);

                logger.Verbose($"Saving config to {dalamud.ConfigFile}");
                Service.Config.SaveToFile(dalamud.ConfigFile);
            }
        }, token);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Source.CancelAsync().ConfigureAwait(true);

        await Task.WhenAny(SaverTask).ConfigureAwait(true); // ensures we aren't running multiple saves at once

        Service.Config.SaveToFile(dalamud.ConfigFile);
    }

    public void Dispose()
    {
        Source.Dispose();
    }
}
