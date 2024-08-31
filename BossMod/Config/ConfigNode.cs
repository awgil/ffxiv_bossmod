using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

// attribute that specifies how config node should be shown in the UI
[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigDisplayAttribute : Attribute
{
    public string? Name { get; set; }
    public int Order { get; set; }
    public Type? Parent { get; set; }
}

// attribute that specifies how config node field or enumeration value is shown in the UI
[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyDisplayAttribute(string label, uint color = 0xffffffff, string tooltip = "") : Attribute
{
    public string Label { get; } = label;
    public uint Color { get; } = color;
    public string Tooltip { get; } = tooltip;
}

// attribute that specifies combobox should be used for displaying int/bool property
[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyComboAttribute(string[] values) : Attribute
{
    public string[] Values { get; } = values;

#pragma warning disable CA1019 // this is just a shorthand
    public PropertyComboAttribute(string falseText, string trueText) : this([falseText, trueText]) { }
#pragma warning restore CA1019
}

// attribute that specifies slider should be used for displaying float/int property
[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertySliderAttribute(float min, float max) : Attribute
{
    public float Speed { get; set; } = 1;
    public float Min { get; } = min;
    public float Max { get; } = max;
    public bool Logarithmic { get; set; }
}

// base class for configuration nodes
public abstract class ConfigNode
{
    // event fired when configuration node was modified; should be fired by anyone making any modifications
    // root subscribes to modification event to save updated configuration
    [JsonIgnore]
    public Event Modified = new();

    // draw custom contents; override this for complex config nodes
    public virtual void DrawCustom(UITree tree, WorldState ws) { }

    // deserialize fields from json; default implementation should work fine for most cases
    public virtual void Deserialize(JsonElement j, JsonSerializerOptions ser)
    {
        var type = GetType();
        foreach (var jfield in j.EnumerateObject())
        {
            var field = type.GetField(jfield.Name);
            if (field != null)
            {
                var value = jfield.Value.Deserialize(field.FieldType, ser);
                if (value != null)
                {
                    field.SetValue(this, value);
                }
            }
        }
    }

    // serialize node to json; default implementation should work fine for most cases
    public virtual void Serialize(Utf8JsonWriter jwriter, JsonSerializerOptions ser)
    {
        JsonSerializer.Serialize(jwriter, this, GetType(), ser);
    }
}

// utility to simplify listening for config modifications; callback is executed immediately for initial state
public sealed class ConfigListener<T>(T data, Action<T> modified) : IDisposable where T : ConfigNode
{
    public readonly T Data = data;
    private readonly EventSubscription _listener = data.Modified.ExecuteAndSubscribe(() => modified(data));

    public void Dispose() => _listener.Dispose();
}
