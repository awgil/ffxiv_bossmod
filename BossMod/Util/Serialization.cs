using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

public static class Serialization
{
    public class JsonTypeConverter : JsonConverter<Type>
    {
        public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Type.GetType(reader.GetString() ?? "");
        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options) => writer.WriteStringValue(value.FullName);
        public override Type ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Type.GetType(reader.GetString() ?? "")!;
        public override void WriteAsPropertyName(Utf8JsonWriter writer, Type value, JsonSerializerOptions options) => writer.WritePropertyName(value.FullName!);
    }

    public static JsonSerializerOptions BuildSerializationOptions() => new()
    {
        IncludeFields = true,
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters = { new JsonStringEnumConverter(), new JsonTypeConverter() }
    };

    public static JsonDocument ReadJson(string path)
    {
        using var fstream = File.OpenRead(path);
        return JsonDocument.Parse(fstream, new JsonDocumentOptions() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
    }

    public static Utf8JsonWriter WriteJson(Stream fstream, bool indented = true) => new(fstream, new JsonWriterOptions() { Indented = indented });

    // TODO: remove all this and replace with system.text.json...
    public static Newtonsoft.Json.JsonSerializer BuildSerializer()
    {
        var res = new Newtonsoft.Json.JsonSerializer();
        res.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        return res;
    }

    public static void DeserializeFields(this Newtonsoft.Json.JsonSerializer ser, JObject from, object to)
    {
        foreach (var (f, data) in from)
            ser.DeserializeField(f, data, to);
    }

    public static void DeserializeField(this Newtonsoft.Json.JsonSerializer ser, string name, JToken? data, object to)
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
}
