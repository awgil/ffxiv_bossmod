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
}
