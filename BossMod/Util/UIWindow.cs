using Dalamud.Interface.Windowing;
using ImGuiNET;

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

    protected UIWindow(string name, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None, List<TitleBarButton>? titleBarButtons = null) : base(name, flags)
    {
        DisposeOnClose = detached;
        Size = initialSize;
        SizeCondition = ImGuiCond.FirstUseEver;
        AllowClickthrough = AllowPinning = false; // this breaks uidev
        if (titleBarButtons != null)
        {
            TitleBarButtons = titleBarButtons;
        }
        var existingWindow = Service.WindowSystem!.Windows.FirstOrDefault(w => w.WindowName == WindowName);
        if (existingWindow == null)
        {
            // standard case - just register window in window system
            Service.WindowSystem.AddWindow(this);
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

    // note: it won't be called for a detached window that was never registered...
    protected virtual void Dispose(bool disposing) => Service.WindowSystem?.RemoveWindow(this);
}

// utility: window that uses custom delegate to perform drawing - allows avoiding creating derived classes in simple cases
public class UISimpleWindow(string name, Action draw, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None, List<Window.TitleBarButton>? titleBarButtons = null)
    : UIWindow(name, detached, initialSize, flags, titleBarButtons)
{
    private readonly Action _draw = draw;

    public override void Draw() => _draw();
}
