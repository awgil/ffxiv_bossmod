using BossMod.Autorotation;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BossMod;

public class Serializer(Lazy<RotationModuleRegistry> registry, Lazy<BossModuleRegistry> bmr)
{
    public readonly JsonSerializerOptions Options = new()
    {
        IncludeFields = true,
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters = { new JsonStringEnumConverter(), new Serialization.JsonTypeConverter(), new JsonPlanConverter(registry, bmr), new JsonPresetConverter(registry) }
    };

    public T? FromJSON<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
    public T? FromJSON<T>(JsonNode node) => JsonSerializer.Deserialize<T>(node, Options);
    public string ToJSON<T>(T value) => JsonSerializer.Serialize(value, Options);
}
