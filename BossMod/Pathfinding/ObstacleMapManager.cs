using System.IO;
using System.Reflection;

namespace BossMod.Pathfinding;

public sealed class ObstacleMapManager : IDisposable
{
    public readonly WorldState World;
    public readonly ObstacleMapDatabase Database = new();
    public string RootPath { get; private set; } = ""; // empty or ends with slash
    private readonly DeveloperConfig _config = Service.Config.Get<DeveloperConfig>();
    private readonly EventSubscriptions _subscriptions;
    private readonly List<(ObstacleMapDatabase.Entry entry, Bitmap data)> _entries = [];

    public bool LoadFromSource => _config.MapLoadFromSource;

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
    public bool CanEditDatabase() => _config.MapLoadFromSource;
    public uint ZoneKey(ushort zoneId, ushort cfcId) => ((uint)zoneId << 16) | cfcId;
    public uint CurrentKey() => ZoneKey(World.CurrentZone, World.CurrentCFCID);

    public void ReloadDatabase()
    {
        Service.Log($"Loading obstacle database from {(_config.MapLoadFromSource ? _config.MapSourcePath : "<embedded>")}");
        RootPath = _config.MapLoadFromSource ? _config.MapSourcePath[..(_config.MapSourcePath.LastIndexOfAny(['\\', '/']) + 1)] : "";

        try
        {
            using var dbStream = _config.MapLoadFromSource ? File.OpenRead(_config.MapSourcePath) : GetEmbeddedResource("maplist.json");
            Database.Load(dbStream);
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to load obstacle database: {ex}");
            Database.Entries.Clear();
        }

        LoadMaps(World.CurrentZone, World.CurrentCFCID);
    }

    public void SaveDatabase()
    {
        if (!_config.MapLoadFromSource)
            return;
        Database.Save(_config.MapSourcePath);
        LoadMaps(World.CurrentZone, World.CurrentCFCID);
    }

    private void LoadMaps(ushort zoneId, ushort cfcId)
    {
        _entries.Clear();
        if (Database.Entries.TryGetValue(ZoneKey(zoneId, cfcId), out var entries))
        {
            foreach (var e in entries)
            {
                try
                {
                    using var eStream = _config.MapLoadFromSource ? File.OpenRead(RootPath + e.Filename) : GetEmbeddedResource(e.Filename);
                    var bitmap = new Bitmap(eStream);
                    _entries.Add((e, bitmap));
                }
                catch (Exception ex)
                {
                    Service.Log($"Failed to load map {e.Filename} from {(_config.MapLoadFromSource ? RootPath : "<embedded>")} for {zoneId}.{cfcId}: {ex}");
                }
            }
        }
    }

    private Stream GetEmbeddedResource(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream($"BossMod.Pathfinding.ObstacleMaps.{name}") ?? throw new InvalidDataException($"Missing embedded resource {name}");
}
