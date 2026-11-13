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
            if (fields is not [var oidText, var name, var distanceText])
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

            float distance;
            var canMove = true;

            if (distanceText == "-")
            {
                distance = 0;
                canMove = false;
            }
            else if (!float.TryParse(distanceText, out distance))
            {
                Service.PluginLog.Warning($"invalid TankDistance value: {distanceText}");
                continue;
            }

            AllRecords.Add(oid, new(oid, name, distance, canMove));
        }
    }

    public static bool TryGet(uint oid, out Record r) => AllRecords.TryGetValue(oid, out r);
}
