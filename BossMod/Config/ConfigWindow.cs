using DalaMock.Host.Mediator;

namespace BossMod;

public sealed class ConfigWindow : UIWindow, IMediatorSubscriber
{
    private readonly ConfigUI root;

    public MediatorService MediatorService { get; }

    public ConfigWindow(ConfigUI root) : base("Boss Mod Settings", false, new(300, 300))
    {
        this.root = root;
        MediatorService = Service.Mediator;

        MediatorService.Subscribe<OpenSettingsWindow>(this, c =>
        {
            root.ShowTab(c.Tab);
            OpenAndBringToFront();
        });
        MediatorService.Subscribe<OpenPresetEditor>(this, c =>
        {
            root.ShowPreset(c.Name);
            OpenAndBringToFront();
        });
        MediatorService.Subscribe<ShowConfigNode>(this, c =>
        {
            root.OpenConfigNode(c.NodeType);
            OpenAndBringToFront();
        });
    }

    public override void Draw()
    {
        root.Draw();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            root.Dispose();

        base.Dispose(disposing);
    }
}
