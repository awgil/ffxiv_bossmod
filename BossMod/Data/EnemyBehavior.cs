namespace BossMod.Data;

public static class EnemyBehavior
{
    public record struct AggroData(uint Territory, uint NameID, float Distance, string EnglishName);

    public static IReadOnlyList<AggroData> Data => _data;

    private static readonly List<AggroData> _data = [];

    static EnemyBehavior()
    {
        using var reader = Utils.LoadResource("BossMod.Data.AggroDistance.dat");
        string? s;
        while ((s = reader.ReadLine()) != null)
        {
            switch (s.Split('='))
            {
                case [var ts, var ns, var ds, var es]:
                    _data.Add(new(uint.Parse(ts), uint.Parse(ns), float.Parse(ds), es));
                    break;

                default:
                    Service.PluginLog.Warning($"Invalid element in aggro data file: {s}");
                    break;
            }
        }
    }

    // distance is between hitboxes
    public static readonly Dictionary<uint, float> TankDistance = new()
    {
        // Twintania/Nael/Bahamut (UCOB)
        [0x1FDF] = 0,
        [0x1FE1] = 0,
        [0x1FE8] = 0,
    };

    public static bool TryGetTankDistance(uint oid, out float distance) => TankDistance.TryGetValue(oid, out distance);

    // TODO check sheets
    public static readonly HashSet<uint> MovementDisabled = [
        0x4B8E,
        0x4CDC,
        0x4DD4,
    ];

    public static bool TryGetAggroDistance(uint territory, uint name, out float distance)
    {
        var ix = _data.FindIndex(d => d.Territory == territory && d.NameID == name);
        if (ix >= 0)
        {
            distance = _data[ix].Distance;
            return true;
        }

        distance = float.MaxValue;
        return false;
    }
}
