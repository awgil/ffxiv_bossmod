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

    public UIWindow(string name, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None) : base(name, flags)
    {
        DisposeOnClose = detached;
        Size = initialSize;
        SizeCondition = ImGuiCond.FirstUseEver;
        AllowClickthrough = AllowPinning = false; // this breaks uidev

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
            throw new Exception($"Failed to register window {name} due to name conflict");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        Service.WindowSystem!.RemoveWindow(this);
    }

    public override void OnClose()
    {
        if (DisposeOnClose)
            Dispose();
    }

    protected virtual void Dispose(bool disposing) { } // note: it won't be called for a detached window that was never registered...
}

// utility: window that uses custom delegate to perform drawing - allows avoiding creating derived classes in simple cases
public class UISimpleWindow : UIWindow
{
    private Action _draw;

    public UISimpleWindow(string name, Action draw, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None) : base(name, detached, initialSize, flags)
    {
        _draw = draw;
    }

    public override void Draw() => _draw();
}
