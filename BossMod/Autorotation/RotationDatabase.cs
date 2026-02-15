namespace BossMod.Autorotation;

public sealed class RotationDatabase(PresetDatabase presets, PlanDatabase plans)
{
    public readonly PresetDatabase Presets = presets;
    public readonly PlanDatabase Plans = plans;
}
