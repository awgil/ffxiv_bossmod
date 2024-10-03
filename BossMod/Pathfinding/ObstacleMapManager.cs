namespace BossMod.Pathfinding;

[ConfigDisplay(Name = "Obstacle map development", Order = 7)]
public sealed class ObstacleMapConfig : ConfigNode
{
    [PropertyDisplay("Developer mode: load obstacle maps from source rather than from plugin distribution")]
    public bool LoadFromSource;

    [PropertyDisplay("Developer mode: source path", tooltip: "Should be <repo root>/BossMod/Pathfinding/ObstacleMaps/maplist.json")]
    public string SourcePath = "";
}

public sealed class ObstacleMapManager : IDisposable
{
    private readonly ObstacleMapConfig _config = Service.Config.Get<ObstacleMapConfig>();
    private readonly WorldState _ws;
    private readonly EventSubscriptions _subscriptions;
    private readonly ObstacleMapDatabase _db = new();
    private readonly List<(ObstacleMapDatabase.Entry entry, Bitmap data)> _entries = [];
    private string _root = ""; // empty or ends with slash

    public ObstacleMapManager(WorldState ws)
    {
        _ws = ws;
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

    private void ReloadDatabase()
    {
        var dbPath = _config.LoadFromSource ? _config.SourcePath : ""; // TODO: load from near assembly instead
        _db.Load(dbPath);
        _root = dbPath[..(dbPath.LastIndexOf('/') + 1)];
        LoadMaps(_ws.CurrentZone, _ws.CurrentCFCID);
    }

    private void LoadMaps(ushort zoneId, ushort cfcId)
    {
        _entries.Clear();
        var key = ((uint)zoneId << 16) | cfcId;
        if (_db.Entries.TryGetValue(key, out var entries))
        {
            foreach (var e in entries)
            {
                var filename = _root + e.Filename;
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
        }
    }
}
