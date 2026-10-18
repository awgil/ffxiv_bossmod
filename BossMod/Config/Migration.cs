namespace BossMod;

public abstract class ConfigMigration(Version since)
{
    public Version Since => since;
    public abstract void Execute();
}

sealed class BitmapV1Migration(Pathfinding.ObstacleMapManager manager) : ConfigMigration(new(7, 5, 1, 13))
{
    public override void Execute()
    {
        Service.Log($"Removing potentially broken bitmaps from {manager.UserRoot.FullName}");
        manager.ClearGenerated();
    }
}
