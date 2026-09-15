using System.Reflection;

namespace BossMod.ReplayAnalysis;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AnalyzerAttribute(uint bossOID, string label) : Attribute
{
    public uint BossOID { get; } = bossOID;
    public string Label { get; } = label;
}

public static class AnalyzerRegistry
{
    public record class Info(Type AnalyzerType, string Label, Func<List<Replay>, uint, CustomAnalyzer> Factory);

    private static readonly Dictionary<uint, Info> _analyzers = [];

    private static void ScanAssembly(Assembly assembly)
    {
        foreach (var t in Utils.GetDerivedTypes<CustomAnalyzer>(assembly).Where(t => !t.IsAbstract))
        {
            var attr = t.GetCustomAttribute<AnalyzerAttribute>();
            if (attr == null)
            {
                Service.Log($"[AnalyzerRegistry] Analyzer {t} has no Analyzer attribute, skipping");
                continue;
            }
            if (_analyzers.TryGetValue(attr.BossOID, out var existing))
            {
                Service.Log($"Analyzer already registered for OID {attr.BossOID}");
                continue;
            }
            _analyzers[attr.BossOID] = new(t, attr.Label, New<CustomAnalyzer>.ConstructorDerived<List<Replay>, uint>(t));
        }
    }

    private static void UnloadFrom(Assembly assembly)
    {
        foreach (var (k, _) in _analyzers.Where(k => k.Value.AnalyzerType.Assembly == assembly).ToList())
            _analyzers.Remove(k);
    }

    public static void Reload(IEnumerable<Assembly> old, IEnumerable<Assembly> @new)
    {
        foreach (var a in old)
            UnloadFrom(a);
        foreach (var a in @new)
            ScanAssembly(a);
    }

    public static Info? ByID(uint oid)
    {
        return _analyzers.TryGetValue(oid, out var info) ? info : null;
    }
}

public abstract class CustomAnalyzer(List<Replay> replays, uint oid)
{
    public readonly List<Replay> Replays = replays;
    public readonly uint ID = oid;

    public abstract void Draw(UITree tree);
}
