namespace BossMod;

[ConfigDisplay(Name = "Developer settings", Order = 9)]
public sealed class DeveloperConfig : ConfigNode
{
    [PropertyDisplay("Obstacle maps: source path", tooltip: "Should be <repo root>/BossMod/Pathfinding/ObstacleMaps")]
    public string MapSourcePath = "";

    [PropertyDisplay("Obstacle map auto-generation")]
    public bool AutoBitmaps = true;

    [PropertyDisplay("Enable module hot-reloading")]
    public bool HotReload = true; // FIXME: turn off once i launch all the clients
}
