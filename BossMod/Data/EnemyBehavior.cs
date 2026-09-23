using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

namespace BossMod.Data;

public static class EnemyBehavior
{
    public record struct Record(
        [property: TypeConverter(typeof(OIDConverter))]
        uint OID,
        string Name,
        float TankDistance,
        bool CanMove
    );

    private static readonly Dictionary<uint, Record> AllRecords = [];

    static EnemyBehavior()
    {
        using var s = Utils.LoadResource("BossMod.Data.EnemyBehavior.csv");
        using var csv = new CsvReader(s, System.Globalization.CultureInfo.InvariantCulture);
        foreach (var rec in csv.GetRecords<Record>())
            AllRecords.Add(rec.OID, rec);
    }

    public static bool TryGet(uint oid, out Record r) => AllRecords.TryGetValue(oid, out r);
}

class OIDConverter : DefaultTypeConverter
{
    private readonly System.ComponentModel.UInt32Converter converter = new();

    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return text == null ? null : converter.ConvertFromString(text);
    }
}
