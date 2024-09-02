using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

[JsonConverter(typeof(JsonColorConverter))]
public record struct Color(uint ABGR)
{
    public readonly uint R => ABGR & 0xFF;
    public readonly uint G => (ABGR >> 8) & 0xFF;
    public readonly uint B => (ABGR >> 16) & 0xFF;
    public readonly uint A => (ABGR >> 24) & 0xFF;

    private const float ToFloat = 1.0f / 255;

    public static Color FromComponents(uint r, uint g, uint b, uint a = 255) => new(((a & 0xFF) << 24) | ((b & 0xFF) << 16) | ((g & 0xFF) << 8) | (r & 0xFF));
    public static Color FromRGBA(uint rgba) => FromComponents(rgba >> 24, rgba >> 16, rgba >> 8, rgba);

    public static Color FromFloat4(Vector4 vec)
    {
        var r = Math.Clamp((uint)(vec.X * 255), 0, 255);
        var g = Math.Clamp((uint)(vec.Y * 255), 0, 255);
        var b = Math.Clamp((uint)(vec.Z * 255), 0, 255);
        var a = Math.Clamp((uint)(vec.W * 255), 0, 255);
        return FromComponents(r, g, b, a);
    }

    public readonly uint ToRGBA() => (R << 24) | (G << 16) | (B << 8) | A;
    public readonly Vector4 ToFloat4() => new Vector4(R, G, B, A) * ToFloat;
}

public class JsonColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str?.Length > 0 ? Color.FromRGBA(uint.Parse(str[1..], System.Globalization.NumberStyles.HexNumber)) : default;
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"#{value.ToRGBA():X8}");
    }
}
