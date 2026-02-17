using BossMod.Services;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace BossMod;

// windows are automatically registered/unregistered with global window system on construction/disposal
// there are two common kinds of windows: 'normal' and 'detached'
// normal windows are expected to be managed by some class:
// - typically created and disposed together with an owner
// - show/hide state is decoupled from lifetime (window can be closed by ui interaction, then reopened again later)
// - expected to have globally unique name (violating this throws an exception)
// detached windows are 'fire and forget'
// - created when some event (e.g. user interaction) happens and not owned by the creator (can even outlive it)
// - initially opened, closing automatically disposes it
// - if another window with same name already exists, it is opened and focused instead; this can be detected by IsOpen being false after base class constructor runs
public abstract class UIWindow : Window, IDisposable
{
    public bool DisposeOnClose; // defaults to true for detached windows, false for normal windows

    protected readonly MediatorService MediatorService;

    protected UIWindow(MediatorService mediator, string name, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None, List<TitleBarButton>? titleBarButtons = null) : base(name, flags)
    {
        MediatorService = mediator;
        DisposeOnClose = detached;
        Size = initialSize;
        SizeCondition = ImGuiCond.FirstUseEver;
        if (titleBarButtons != null)
        {
            TitleBarButtons = titleBarButtons;
        }
        mediator.Publish(new CreateWindowMessage(this, detached));
        /*
        var existingWindow = wsys.Windows.FirstOrDefault(w => w.WindowName == WindowName);
        if (existingWindow == null)
        {
            // standard case - just register window in window system
            wsys.AddWindow(this);
            IsOpen = detached;
        }
        else if (detached)
        {
            // this is not an error - just ensure existing window is focused (note that IsOpen is left as false)
            existingWindow.IsOpen = true;
            existingWindow.BringToFront();
        }
        else
        {
            // this is an error
            throw new InvalidOperationException($"Failed to register window {name} due to name conflict");
        }
        */
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public override void OnClose()
    {
        if (DisposeOnClose)
            Dispose();
    }

    public void OpenAndBringToFront()
    {
        IsOpen = true;
        BringToFront();
    }

    // note: it won't be called for a detached window that was never registered...
    protected virtual void Dispose(bool disposing)
    {
        try
        {
            MediatorService.Publish(new DestroyWindowMessage(WindowName));
        }
        catch (ObjectDisposedException)
        {
            // TODO: fix this, it means we are disposing the window after mediator service teardown
        }
    }
}

// utility: window that uses custom delegate to perform drawing - allows avoiding creating derived classes in simple cases
public class UISimpleWindow(MediatorService mediator, string name, Action draw, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None, List<Window.TitleBarButton>? titleBarButtons = null)
    : UIWindow(mediator, name, detached, initialSize, flags, titleBarButtons)
{
    public override void Draw() => draw();
}
