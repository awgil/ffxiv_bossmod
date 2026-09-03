using System.IO;

namespace BossMod.Autorotation;

public sealed class RotationDatabase : IDisposable
{
    public readonly PresetDatabase Presets;
    public readonly PlanDatabase Plans;

    private readonly EventSubscriptions _subscriptions;

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

    public void Dispose() => _subscriptions.Dispose();
}
