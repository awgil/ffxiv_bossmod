using System.IO;
using System.Reflection;

namespace BossMod.Data;

public static class AggroDistance
{
    public record struct AggroData(uint Territory, uint NameID, float Distance, string EnglishName);

    public static IReadOnlyList<AggroData> Data => _data;

    private static readonly List<AggroData> _data = [];

    static AggroDistance()
    {
        var contents = Assembly.GetExecutingAssembly().GetManifestResourceStream("BossMod.Data.AggroDistance.dat")!;
        using var reader = new StreamReader(contents);
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

    public static bool TryGet(uint territory, uint name, out float distance)
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
