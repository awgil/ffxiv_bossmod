namespace BossMod.Autorotation;

public sealed class RotationDatabase(PresetDatabase presets, PlanDatabase plans, PackLoader loader) : IDisposable
{
    public readonly PresetDatabase Presets = presets;
    public readonly PlanDatabase Plans = plans;

    private readonly EventSubscriptions _subscriptions = new(loader.Modified.Subscribe(() =>
    {
        presets.Load();
        plans.Load();
    }));

    /*
    public RotationDatabase(DirectoryInfo rootPath, FileInfo defaultPresets, PackLoader loader)
    {
        if (!rootPath.Exists)
            rootPath.Create();
        Presets = new(rootPath.FullName + "/presets", defaultPresets);
        Plans = new(rootPath.FullName + "/plans");

        _subscriptions = new(
            loader.Modified.Subscribe(() =>
            {
                Presets.Load();
                Plans.Load();
            })
        );
    }
    */

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}
