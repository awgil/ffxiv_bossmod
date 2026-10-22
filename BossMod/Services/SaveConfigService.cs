using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

public sealed class SaveConfigService(IDalamudPluginInterface dalamud, IPluginLog logger) : IHostedService
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

                logger.Verbose($"Saving config to {dalamud.ConfigFile}");
                Service.Config.SaveToFile(dalamud.ConfigFile);
            }
        }, token);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Source.Cancel();

        await Task.WhenAny(SaverTask); // ensures we aren't running multiple saves at once

        Service.Config.SaveToFile(dalamud.ConfigFile);
    }
}
