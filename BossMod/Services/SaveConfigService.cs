using Dalamud.Plugin;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

public sealed class SaveConfigService(ConfigRoot config, IDalamudPluginInterface dalamud) : IHostedService
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
                await Task.Delay(30_000, token);

                config.SaveToFile(dalamud.ConfigFile);
            }
        }, token);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Source.Cancel();

        await SaverTask; // ensures we aren't running multiple saves at once

        config.SaveToFile(dalamud.ConfigFile);
    }
}
