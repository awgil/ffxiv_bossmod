using BossMod.Services;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace BossMod;

public abstract class UIWindow : Window, IDisposable
{
    public bool DisposeOnClose; // defaults to true for detached windows, false for normal windows
    public bool IsDisposed { get; private set; }

    protected UIWindow(string name, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None, List<TitleBarButton>? titleBarButtons = null) : base(name, flags)
    {
        DisposeOnClose = detached;
        Size = initialSize;
        SizeCondition = ImGuiCond.FirstUseEver;
        if (titleBarButtons != null)
            TitleBarButtons = titleBarButtons;
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
        // once this flag is set, the window will be removed on the next frame
        IsDisposed = true;
    }
}

// utility: window that uses custom delegate to perform drawing - allows avoiding creating derived classes in simple cases
public class UISimpleWindow : UIWindow
{
    private readonly Action drawAction;

    private UISimpleWindow(string name, Action draw, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None, List<TitleBarButton>? titleBarButtons = null) : base(name, detached, initialSize, flags, titleBarButtons)
    {
        drawAction = draw;
    }

    public override void Draw() => drawAction();

    public static UISimpleWindow Create(MediatorService mediator, string name, Action draw, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None, List<TitleBarButton>? titleBarButtons = null)
    {
        var wnd = new UISimpleWindow(name, draw, detached, initialSize, flags, titleBarButtons);
        mediator.Publish(new CreateWindowMessage(wnd));
        return wnd;
    }
}
