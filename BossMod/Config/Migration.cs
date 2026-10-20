using System.Threading.Tasks;

namespace BossMod;

public abstract class ConfigMigration(Version since)
{
    public Version Since => since;
    public abstract Task Execute();

    public bool ShouldExecute(ConfigRoot config) => Since > config.PreviousAssemblyVersion && Since <= config.AssemblyVersion;
}

sealed class BitmapV1Migration(Pathfinding.ObstacleMapManager manager) : ConfigMigration(new(7, 5, 1, 13))
{
    public override async Task Execute()
    {
        Service.Log($"Removing potentially broken bitmaps from {manager.UserRoot.FullName}");
        manager.ClearGenerated();
    }
}
