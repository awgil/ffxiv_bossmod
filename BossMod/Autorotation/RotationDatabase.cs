using Dalamud.Plugin;
using System.IO;

namespace BossMod.Autorotation;

public sealed class RotationDatabase
{
    public readonly PresetDatabase Presets;
    public readonly PlanDatabase Plans;

    public RotationDatabase(ConfigRoot cfg, DirectoryInfo rootPath, FileInfo defaultPresets)
    {
        if (!rootPath.Exists)
            rootPath.Create();
        Presets = new(cfg, rootPath.FullName + "/presets", defaultPresets);
        Plans = new(rootPath.FullName + "/plans");
    }

    public RotationDatabase(ConfigRoot cfg, IDalamudPluginInterface pluginInterface) : this(cfg, new DirectoryInfo(pluginInterface.ConfigDirectory.FullName + "/autorot"), new FileInfo(pluginInterface.AssemblyLocation.DirectoryName! + "/DefaultRotationPresets.json")) { }
}
