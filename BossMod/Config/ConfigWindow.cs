using DalaMock.Host.Mediator;

namespace BossMod;

public sealed class ConfigWindow : WindowBase, IMediatorSubscriber, IDisposable
{
    private readonly ConfigUI root;

    public MediatorService MediatorService { get; }

    public ConfigWindow(ConfigUI root, MediatorService mediator) : base("Boss Mod Settings", new(300, 300))
    {
        this.root = root;
        MediatorService = mediator;

        mediator.Subscribe(this, (OpenSettingsWindow c) =>
        {
            root.ShowTab(c.Tab);
            OpenAndBringToFront();
        });
        mediator.Subscribe(this, (OpenPresetEditor c) =>
        {
            root.ShowPreset(c.Name);
            OpenAndBringToFront();
        });
        mediator.Subscribe(this, (ShowConfigNode c) =>
        {
            root.OpenConfigNode(c.NodeType);
            OpenAndBringToFront();
        });
    }

    public override void Draw()
    {
        root.Draw();
    }

    void IDisposable.Dispose()
    {
        root.Dispose();
    }
}
