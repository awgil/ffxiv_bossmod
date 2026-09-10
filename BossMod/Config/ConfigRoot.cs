using System.IO;
using System.Reflection;
using System.Text.Json;

namespace BossMod;

public sealed class ConfigRoot : IDisposable
{
    public Event Modified = new();
    public Version AssemblyVersion = new(); // we use this to show newly added config options
    record LazyNode(ConfigNode Node, EventSubscription OnModified);
    private readonly Dictionary<Type, LazyNode?> _nodes = []; // node is null if the type has been registered but the node has never been loaded before

    public IEnumerable<Type> Nodes => _nodes.Keys;

    private void ScanAssembly(Assembly assembly)
    {
        foreach (var t in Utils.GetDerivedTypes<ConfigNode>(assembly).Where(t => !t.IsAbstract))
            _nodes.Add(t, null);
    }

    private void UnloadFrom(Assembly assembly)
    {
        foreach (var k in _nodes.Keys.Where(k => k.Assembly == assembly))
            if (_nodes.Remove(k, out var node))
                node?.OnModified.Dispose();
    }

    public void Reload(IEnumerable<Assembly> old, IEnumerable<Assembly> @new)
    {
        foreach (var a in old)
            UnloadFrom(a);

        foreach (var a in @new)
            ScanAssembly(a);
    }

    public T Get<T>() where T : ConfigNode => Get<T>(typeof(T));
    public T Get<T>(Type derived) where T : ConfigNode
    {
        if (!_nodes.TryGetValue(derived, out var node))
            throw new InvalidOperationException($"{derived} is not a valid config type");

        if (node != null)
            return (T)node.Node;

        var t = (T)Activator.CreateInstance(derived)!;
        if (_payload.EnumerateObject().FirstOrNull(o => o.Name == derived.FullName) is { } obj)
        {
            try
            {
                t.Deserialize(obj.Value, _opts);
            }
            catch (AggregateException ex)
            {
                Service.PluginLog.Warning(ex, $"An error occurred while deserializing config option '{derived.FullName}'. It will have its default value.");
            }
        }

        _nodes[derived] = new(t, t.Modified.Subscribe(Modified.Fire));

        return t;
    }

    public ConfigListener<T> GetAndSubscribe<T>(Action<T> modified) where T : ConfigNode => new(Get<T>(), modified);

    private readonly JsonDocument _document;
    private readonly JsonElement _payload;
    private readonly JsonSerializerOptions _opts;

    public ConfigRoot(FileInfo file)
    {
        _opts = Serialization.BuildSerializationOptions();

        if (file.Exists)
        {
            try
            {
                (_document, _payload) = ConfigConverter.Schema.Load(file);
                AssemblyVersion = _document.RootElement.TryGetProperty(nameof(AssemblyVersion), out var jver) ? new(jver.GetString() ?? "") : new();
                return;
            }
            catch (Exception ex)
            {
                Service.PluginLog.Warning(ex, "Unable to load plugin configuration; a blank configuration will be loaded instead");
            }
        }

        // fallback for both missing file (first install) and corrupted config
        _document = JsonDocument.Parse("""{"payload":{}}""");
        _payload = _document.RootElement.GetProperty("payload");
    }

    public void SaveToFile(FileInfo file)
    {
        try
        {
            var tmp = new FileInfo(Path.GetTempFileName());
            ConfigConverter.Schema.Save(tmp, jwriter =>
            {
                jwriter.WriteStartObject();
                var ser = Serialization.BuildSerializationOptions();
                foreach (var t in _nodes.Keys)
                {
                    jwriter.WritePropertyName(t.FullName!);
                    Get<ConfigNode>(t).Serialize(jwriter, ser);
                }
                jwriter.WriteEndObject();
                jwriter.WriteString(nameof(AssemblyVersion), AssemblyVersion.ToString());
            });

            var (from, to) = (file.FullName, Path.ChangeExtension(file.FullName, "json.bak"));

            // why not use File.Replace? why, HRESULT 0x00000498 ERROR_UNABLE_TO_MOVE_REPLACEMENT of course!
            File.Delete(to);
            if (File.Exists(from))
                File.Move(from, to);

            tmp.MoveTo(from);
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
            foreach (var t in _nodes.Keys)
                if (t.Name.AsSpan().Contains(cmdType, StringComparison.CurrentCultureIgnoreCase))
                {
                    // check for exact match
                    if (t.Name.Length == cmdType.Length)
                    {
                        matchingNodes.Clear();
                        matchingNodes.Add(Get<ConfigNode>(t));
                        break;
                    }
                    matchingNodes.Add(Get<ConfigNode>(t));
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

    public void Dispose()
    {
        foreach (var (_, n) in _nodes)
            n?.OnModified.Dispose();

        _nodes.Clear();

        _document.Dispose();
    }
}
