using DalaMock.Host.Mediator;
using Dalamud.Plugin;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

public class InitWindowService(IDalamudPluginInterface pluginInterface, MediatorService mediatorService) : IHostedService
{
    public async Task StartAsync(CancellationToken token)
    {
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        pluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
    }

    public async Task StopAsync(CancellationToken token)
    {
        pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        pluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
    }

    void ToggleMainUi()
    {
        mediatorService.Publish(new ToggleWindowMessage(typeof(ReplayManagementWindow)));
    }

    void ToggleConfigUi()
    {
        mediatorService.Publish(new ToggleWindowMessage(typeof(ConfigUI)));
    }
}
