using DalaMock.Host.Mediator;

namespace BossMod;

public sealed class ConfigWindow : UIWindow, IMediatorSubscriber
{
    private readonly ConfigUI root;

    public MediatorService MediatorService { get; }

    public ConfigWindow(ConfigUI root, MediatorService mediator) : base("Boss Mod Settings", false, new(300, 300))
    {
        this.root = root;
        MediatorService = mediator;

        MediatorService.Subscribe<OpenConfigMessage>(this, c =>
        {
            root.ShowTab(c.Tab);
            IsOpen = true;
        });
        MediatorService.Subscribe<OpenPresetEditorMessage>(this, c =>
        {
            root.ShowPreset(c.Name);
            IsOpen = true;
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
