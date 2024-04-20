using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace BossMod;

public static class Serialization
{
    public static JsonSerializer BuildSerializer()
    {
        var res = new JsonSerializer();
        res.Converters.Add(new StringEnumConverter());
        return res;
    }

    public static void DeserializeFields(this JsonSerializer ser, JObject from, object to)
    {
        foreach (var (f, data) in from)
            ser.DeserializeField(f, data, to);
    }

    public static void DeserializeField(this JsonSerializer ser, string name, JToken? data, object to)
    {
        var field = to.GetType().GetField(name);
        if (field != null)
        {
            var value = data?.ToObject(field.FieldType, ser);
            if (value != null)
            {
                field.SetValue(to, value);
            }
        }
    }

    public static uint? DeserializeHex(this JsonSerializer ser, JToken? from)
    {
        var str = from?.Value<string>();
        return (str != null && str.StartsWith("0x", StringComparison.Ordinal)) ? uint.Parse(str.AsSpan(2), NumberStyles.HexNumber) : null;
    }

    public static uint? DeserializeEnum(this JsonSerializer ser, JToken? from, Type? enumType)
        => (enumType != null && Enum.TryParse(enumType, from?.Value<string>(), out var val) && val != null) ? (uint)val : null;

    public static ActionID? DeserializeActionID(this JsonSerializer ser, JToken? from, Type? aidType)
    {
        var aid = ser.DeserializeEnum(from, aidType);
        if (aid != null)
            return new(ActionType.Spell, aid.Value);
        var uaid = ser.DeserializeHex(from);
        return uaid != null ? new(uaid.Value) : null;
    }

    public static string SerializeActionID(this JsonSerializer ser, ActionID value, Type? aidType)
    {
        var aidStr = value.Type == ActionType.Spell ? aidType?.GetEnumName(value.ID) : null;
        return aidStr ?? $"0x{value.Raw:X}";
    }
}
