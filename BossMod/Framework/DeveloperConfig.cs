namespace BossMod;

[ConfigDisplay(Name = "Developer settings", Order = 99)]
public sealed class DeveloperConfig : ConfigNode
{
    [PropertyDisplay("Obstacle maps: source path", tooltip: "Should be <repo root>/BossMod/Pathfinding/ObstacleMaps")]
    public string MapSourcePath = "";

    [PropertyDisplay("Obstacle map auto-generation")]
    public bool AutoBitmaps = true;
}
