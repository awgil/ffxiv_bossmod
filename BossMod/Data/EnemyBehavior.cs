using System.Globalization;

namespace BossMod.Data;

public static class EnemyBehavior
{
    public record struct Record(uint OID, string Name, float TankDistance, bool CanMove);

    private static readonly Dictionary<uint, Record> AllRecords = [];

    static EnemyBehavior()
    {
        using var s = Utils.LoadResource("BossMod.Data.EnemyBehavior.tsv");
        string? line;
        while ((line = s.ReadLine()) != null)
        {
            if (line.StartsWith('#'))
                continue;

            var fields = line.Split('\t');
            if (fields is not [var oidText, var name, var distanceText, var moveText])
            {
                Service.PluginLog.Warning($"Error parsing EnemyBehavior: malformed line {line}");
                continue;
            }

            var start = 0;
            if (oidText.StartsWith("0x", StringComparison.Ordinal))
                start = 2;

            if (!uint.TryParse(oidText[start..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var oid))
            {
                Service.PluginLog.Warning($"invalid OID value: {oidText}");
                continue;
            }

            if (!float.TryParse(distanceText, out var distance))
            {
                Service.PluginLog.Warning($"invalid TankDistance value: {distanceText}");
                continue;
            }

            if (!bool.TryParse(moveText, out var move))
            {
                Service.PluginLog.Warning($"invalid CanMove value: {moveText}");
                continue;
            }

            AllRecords.Add(oid, new(oid, name, distance, move));
        }
    }

    public static bool TryGet(uint oid, out Record r) => AllRecords.TryGetValue(oid, out r);
}
