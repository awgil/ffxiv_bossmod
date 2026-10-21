using BossMod.Services;

namespace BossMod.Autorotation;

public sealed class RotationDatabase(PresetDatabase presets, PlanDatabase plans, ModulePackService loader) : IDisposable
{
    public readonly PresetDatabase Presets = presets;
    public readonly PlanDatabase Plans = plans;

    private readonly EventSubscriptions _subscriptions = new(loader.Modified.Subscribe(() =>
    {
        presets.Load();
        plans.Load();
    }));

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}
