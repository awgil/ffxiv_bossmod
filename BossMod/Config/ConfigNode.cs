using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
public sealed class PropertyDisplayAttribute(string label, uint color = 0xffffffff) : Attribute
{
    public string Label { get; } = label;
    public uint Color { get; } = color;
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
    public event Action? Modified;

    // notify that configuration node was modified; should be called by derived classes whenever they make any modifications
    // implementation dispatches modification event and forwards request to parent
    // root should subscribe to modification event to save updated configuration
    public void NotifyModified() => Modified?.Invoke();

    // draw custom contents; override this for complex config nodes
    public virtual void DrawCustom(UITree tree, WorldState ws) { }

    // deserialize fields from json; default implementation should work fine for most cases
    public virtual void Deserialize(JObject j, JsonSerializer ser)
    {
        ser.DeserializeFields(j, this);
    }

    // serialize node to json; default implementation should work fine for most cases
    public virtual JObject Serialize(JsonSerializer ser)
    {
        return JObject.FromObject(this, ser);
    }
}
