using BossMod.Config;
using System.IO;

namespace BossMod.Autorotation;

public sealed class RotationDatabase
{
    public readonly PresetDatabase Presets;
    public readonly PlanDatabase Plans;

    public RotationDatabase(PresetsDatabaseRoot root, DefaultPresetsFile file)
    {
        var rootPath = new DirectoryInfo(root.Path);
        var defaults = new FileInfo(file.Path);
        if (!rootPath.Exists)
            rootPath.Create();
        Presets = new(rootPath.FullName + "/presets", defaults);
        Plans = new(rootPath.FullName + "/plans");
    }
}
