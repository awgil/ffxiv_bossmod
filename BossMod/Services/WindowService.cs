using DalaMock.Host.Factories;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

public record ToggleWindowMessage(Type WindowType) : MessageBase;
public record OpenWindowMessage(Type WindowType) : MessageBase;
public record CloseWindowMessage(Type WindowType) : MessageBase;

public class WindowService : DisposableMediatorSubscriberBase, IHostedService
{
    readonly WindowSystemFactory windowSystemFactory;
    readonly IUiBuilder uiBuilder;
    readonly IWindowSystem windowSystem;

    public WindowService(IEnumerable<Window> windows, ILogger<DisposableMediatorSubscriberBase> logger, MediatorService mediatorService, WindowSystemFactory windowSystemFactory, IUiBuilder uiBuilder) : base(logger, mediatorService)
    {
        this.windowSystemFactory = windowSystemFactory;
        this.uiBuilder = uiBuilder;
        windowSystem = this.windowSystemFactory.Create("Boss Mod");
        foreach (var window in windows)
            windowSystem.AddWindow(window);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        MediatorService.Subscribe<OpenWindowMessage>(this, OpenWindow);
        MediatorService.Subscribe<ToggleWindowMessage>(this, ToggleWindow);
        MediatorService.Subscribe<CloseWindowMessage>(this, CloseWindow);
        uiBuilder.Draw += Draw;
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        uiBuilder.Draw -= Draw;
        return Task.CompletedTask;
    }

    void OpenWindow(OpenWindowMessage obj)
    {
        if (windowSystem.Windows.FirstOrDefault(c => c.GetType() == obj.WindowType) is { } w)
            w.IsOpen = true;
    }

    void CloseWindow(CloseWindowMessage obj)
    {
        if (windowSystem.Windows.FirstOrDefault(c => c.GetType() == obj.WindowType) is { } w)
            w.IsOpen = false;
    }

    void ToggleWindow(ToggleWindowMessage obj)
    {
        if (windowSystem.Windows.FirstOrDefault(c => c.GetType() == obj.WindowType) is { } w)
            w.IsOpen = !w.IsOpen;
    }

    void Draw()
    {
        windowSystem.Draw();
    }
}
