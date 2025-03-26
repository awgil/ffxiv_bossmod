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
        return ReadJson(fstream);
    }

    public static JsonDocument ReadJson(Stream stream) => JsonDocument.Parse(stream, new JsonDocumentOptions() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
    public static Utf8JsonWriter WriteJson(Stream fstream, bool indented = true) => new(fstream, new JsonWriterOptions() { Indented = indented });

    public static unsafe T ReadStruct<T>(this Stream stream) where T : unmanaged
    {
        T res = default;
        stream.ReadExactly(new(&res, sizeof(T)));
        return res;
    }

    public static unsafe void WriteStruct<T>(this Stream stream, in T value) where T : unmanaged
    {
        fixed (T* ptr = &value)
            stream.Write(new(ptr, sizeof(T)));
    }
}
