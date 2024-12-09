using System.Reflection;

namespace BossMod.Pathfinding;

[ConfigDisplay(Name = "Obstacle map development", Order = 8)]
public sealed class ObstacleMapConfig : ConfigNode
{
    [PropertyDisplay("Developer mode: load obstacle maps from source rather than from plugin distribution")]
    public bool LoadFromSource;

    [PropertyDisplay("Developer mode: source path", tooltip: "Should be <repo root>/BossMod/Pathfinding/ObstacleMaps/maplist.json")]
    public string SourcePath = "";
}

public sealed class ObstacleMapManager : IDisposable
{
    public readonly WorldState World;
    public readonly ObstacleMapDatabase Database = new();
    public string RootPath { get; private set; } = ""; // empty or ends with slash
    private readonly ObstacleMapConfig _config = Service.Config.Get<ObstacleMapConfig>();
    private readonly EventSubscriptions _subscriptions;
    private readonly List<(ObstacleMapDatabase.Entry entry, Bitmap data)> _entries = [];

    public bool LoadFromSource => _config.LoadFromSource;

    public ObstacleMapManager(WorldState ws)
    {
        World = ws;
        _subscriptions = new(
            _config.Modified.Subscribe(ReloadDatabase),
            ws.CurrentZoneChanged.Subscribe(op => LoadMaps(op.Zone, op.CFCID))
        );
        ReloadDatabase();
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public (ObstacleMapDatabase.Entry? entry, Bitmap? data) Find(Vector3 pos) => _entries.FirstOrDefault(e => e.entry.Contains(pos));
    public bool CanEditDatabase() => _config.LoadFromSource;
    public uint ZoneKey(ushort zoneId, ushort cfcId) => ((uint)zoneId << 16) | cfcId;
    public uint CurrentKey() => ZoneKey(World.CurrentZone, World.CurrentCFCID);

    public void ReloadDatabase()
    {
        if (LoadFromSource)
        {
            var dbPath = _config.SourcePath;
            Service.Log($"Loading obstacle database from '{dbPath}'");
            Database.LoadFromFile(dbPath);
            RootPath = dbPath[..(dbPath.LastIndexOfAny(['\\', '/']) + 1)];
        }
        else
        {
            Service.Log($"Loading embedded obstacle database");
            Database.LoadFromAssembly();
        }

        LoadMaps(World.CurrentZone, World.CurrentCFCID);
    }

    public void SaveDatabase()
    {
        if (!_config.LoadFromSource)
            return;
        Database.Save(_config.SourcePath);
        LoadMaps(World.CurrentZone, World.CurrentCFCID);
    }

    private void LoadMaps(ushort zoneId, ushort cfcId)
    {
        _entries.Clear();
        if (Database.Entries.TryGetValue(ZoneKey(zoneId, cfcId), out var entries))
        {
            foreach (var e in entries)
            {
                if (LoadFromSource)
                {
                    var filename = RootPath + e.Filename;
                    try
                    {
                        var bitmap = new Bitmap(filename);
                        _entries.Add((e, bitmap));
                    }
                    catch (Exception ex)
                    {
                        Service.Log($"Failed to load map {filename} for {zoneId}.{cfcId}: {ex}");
                    }
                }
                else
                {
                    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"BossMod.Pathfinding.ObstacleMaps.{e.Filename}");
                    if (stream == null)
                    {
                        Service.Log($"Embedded map missing for {zoneId}.{cfcId}");
                        continue;
                    }
                    try
                    {
                        var bitmap = new Bitmap(stream);
                        _entries.Add((e, bitmap));
                    }
                    catch (Exception ex)
                    {
                        Service.Log($"Failed to load embedded map for {zoneId}.{cfcId}: {ex}");
                    }
                }
            }
        }
    }
}
