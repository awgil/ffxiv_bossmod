using BossMod.Services;
using System.Diagnostics.CodeAnalysis;

namespace BossMod.Autorotation;

public sealed class RotationDatabase(PresetDatabase presets, PlanDatabase plans, ModulePackService loader) : IDisposable
{
    public readonly PresetDatabase Presets = presets;
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "owned and disposed by the service container")]
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
