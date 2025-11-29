using System.IO;
using System.Reflection;

namespace BossMod;

public class ConfigRoot
{
    public Event Modified = new();
    public Version AssemblyVersion = new(); // we use this to show newly added config options
    private readonly Dictionary<Type, ConfigNode> _nodes = [];

    public IEnumerable<ConfigNode> Nodes => _nodes.Values;

    public void Initialize()
    {
        foreach (var t in Utils.GetDerivedTypes<ConfigNode>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract))
        {
            if (Activator.CreateInstance(t) is not ConfigNode inst)
            {
                Service.Log($"[Config] Failed to create an instance of {t}");
                continue;
            }
            inst.Modified.Subscribe(Modified.Fire);
            _nodes[t] = inst;
        }
    }

    public T Get<T>() where T : ConfigNode => (T)_nodes[typeof(T)];
    public T Get<T>(Type derived) where T : ConfigNode => (T)_nodes[derived];

    public ConfigListener<T> GetAndSubscribe<T>(Action<T> modified) where T : ConfigNode => new(Get<T>(), modified);

    public void LoadFromFile(FileInfo file)
    {
        try
        {
            var data = ConfigConverter.Schema.Load(file);
            using var json = data.document;
            var ser = Serialization.BuildSerializationOptions();

            foreach (var jconfig in data.payload.EnumerateObject())
            {
                var type = Type.GetType(jconfig.Name);
                var node = type != null ? _nodes.GetValueOrDefault(type) : null;
                try
                {
                    node?.Deserialize(jconfig.Value, ser);
                }
                catch (AggregateException exc)
                {
                    Service.Logger.Warning(exc, "An error occurred while deserializing the plugin config. As a result, some settings may have unexpected values.");
                }
            }
            AssemblyVersion = json.RootElement.TryGetProperty(nameof(AssemblyVersion), out var jver) ? new(jver.GetString() ?? "") : new();
        }
        catch (Exception e)
        {
            Service.Log($"Failed to load config from {file.FullName}: {e}");
        }
    }

    public void SaveToFile(FileInfo file)
    {
        try
        {
            ConfigConverter.Schema.Save(file, jwriter =>
            {
                jwriter.WriteStartObject();
                var ser = Serialization.BuildSerializationOptions();
                foreach (var (t, n) in _nodes)
                {
                    jwriter.WritePropertyName(t.FullName!);
                    n.Serialize(jwriter, ser);
                }
                jwriter.WriteEndObject();
                jwriter.WriteString(nameof(AssemblyVersion), AssemblyVersion.ToString());
            });
        }
        catch (Exception e)
        {
            Service.Log($"Failed to save config to {file.FullName}: {e}");
        }
    }

    public List<string> ConsoleCommand(ReadOnlySpan<char> cmd, bool save = true)
    {
        Span<Range> ranges = stackalloc Range[3];
        var numRanges = cmd.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<string> result = [];
        if (numRanges == 0)
        {
            result.Add("Usage: /vbm cfg <config-type> <field> <value>");
            result.Add("Both config-type and field can be shortened. Valid config-types:");
            foreach (var t in _nodes.Keys)
                result.Add($"- {t.Name}");
        }
        else
        {
            var cmdType = cmd[ranges[0]];
            List<ConfigNode> matchingNodes = [];
            foreach (var (t, n) in _nodes)
                if (t.Name.AsSpan().Contains(cmdType, StringComparison.CurrentCultureIgnoreCase))
                {
                    // check for exact match
                    if (t.Name.Length == cmdType.Length)
                    {
                        matchingNodes.Clear();
                        matchingNodes.Add(n);
                        break;
                    }
                    matchingNodes.Add(n);
                }
            if (matchingNodes.Count == 0)
            {
                result.Add("Config type not found. Valid types:");
                foreach (var t in _nodes.Keys)
                    result.Add($"- {t.Name}");
            }
            else if (matchingNodes.Count > 1)
            {
                result.Add("Ambiguous config type, pass longer pattern. Matches:");
                foreach (var n in matchingNodes)
                    result.Add($"- {n.GetType().Name}");
            }
            else if (numRanges == 1)
            {
                result.Add("Usage: /vbm cfg <config-type> <field> <value>");
                result.Add($"Valid fields for {matchingNodes[0].GetType().Name}:");
                foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                    result.Add($"- {f.Name}");
            }
            else
            {
                var cmdField = cmd[ranges[1]];
                List<FieldInfo> matchingFields = [];
                foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                {
                    if (f.Name.AsSpan().Contains(cmdField, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // check for exact match
                        if (f.Name.Length == cmdField.Length)
                        {
                            matchingFields.Clear();
                            matchingFields.Add(f);
                            break;
                        }
                        matchingFields.Add(f);
                    }
                }
                if (matchingFields.Count == 0)
                {
                    result.Add($"Field not found {cmdField}, Valid fields:");
                    foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                        result.Add($"- {f.Name}");
                }
                else if (matchingFields.Count > 1)
                {
                    result.Add("Ambiguous field name, pass longer pattern. Matches:");
                    foreach (var f in matchingFields)
                        result.Add($"- {f.Name}");
                }
                else if (numRanges == 2)
                {
                    try
                    {
                        result.Add(matchingFields[0].GetValue(matchingNodes[0])?.ToString() ?? $"Failed to get value of {matchingNodes[0].GetType().Name}.{matchingFields[0].Name}");
                    }
                    catch (Exception e)
                    {
                        result.Add($"Failed to get value of {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} : {e}");
                    }
                }
                else
                {
                    var cmdValue = cmd[ranges[2]];
                    try
                    {
                        var val = FromConsoleString(cmdValue, matchingFields[0].FieldType);
                        if (val == null)
                        {
                            result.Add($"Failed to convert '{cmdValue}' to {matchingFields[0].FieldType}");
                        }
                        else
                        {
                            matchingFields[0].SetValue(matchingNodes[0], val);
                            if (save)
                                matchingNodes[0].Modified.Fire();
                        }
                    }
                    catch (Exception e)
                    {
                        result.Add($"Failed to set {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} to {cmdValue}: {e}");
                    }
                }
            }
        }
        return result;
    }

    private object? FromConsoleString(ReadOnlySpan<char> str, Type t)
        => t == typeof(bool) ? bool.Parse(str)
        : t == typeof(float) ? float.Parse(str)
        : t == typeof(int) ? int.Parse(str)
        : t.IsAssignableTo(typeof(Enum)) ? Enum.Parse(t, str)
        : null;
}
