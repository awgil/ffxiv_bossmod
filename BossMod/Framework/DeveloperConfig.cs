namespace BossMod;

[ConfigDisplay(Name = "Developer settings", Order = 9)]
public sealed class DeveloperConfig : ConfigNode
{
    [PropertyDisplay("Module packs: source directory")]
    public string ModulePackDirectory = "";

    [PropertyDisplay("Obstacle maps: load from source")]
    public bool MapLoadFromSource;

    [PropertyDisplay("Obstacle maps: source path", tooltip: "Should be <repo root>/BossMod/Pathfinding/ObstacleMaps/maplist.json")]
    public string MapSourcePath = "";
}
