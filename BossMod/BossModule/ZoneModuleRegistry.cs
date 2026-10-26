using System.Reflection;

namespace BossMod;

// attribute for defining zone module's metadata; it is required by each module to be loaded
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class ZoneModuleInfoAttribute(uint cfcId, uint territoryID = 0) : Attribute
{
    public uint CFCID => cfcId;
    public uint TerritoryID => territoryID;
}

public static class ZoneModuleRegistry
{
    public static readonly Event Modified = new();

    public record class Info(Type ModuleType, ZoneModuleInfoAttribute Desc, Func<WorldState, ZoneModule> Factory);

    private static readonly Dictionary<uint, Info> _modulesByCFC = [];

    private static bool ScanAssembly(Assembly assembly)
    {
        var modified = false;

        foreach (var t in Utils.GetDerivedTypes<ZoneModule>(assembly).Where(t => !t.IsAbstract))
        {
            var haveAttr = false;
            foreach (var attr in t.GetCustomAttributes<ZoneModuleInfoAttribute>())
            {
                haveAttr = true;
                if (_modulesByCFC.TryGetValue(attr.CFCID, out var existingModule))
                {
                    Service.Log($"[ZoneModuleRegistry] Two zone modules have same CFCID: {t.FullName} and {existingModule.ModuleType.FullName}");
                    continue;
                }
                _modulesByCFC[attr.CFCID] = new Info(t, attr, New<ZoneModule>.ConstructorDerived<WorldState>(t));
                modified = true;
            }

            if (!haveAttr)
                Service.Log($"[ZoneModuleRegistry] Zone module {t} has no ZoneModuleInfo attribute");
        }

        return modified;
    }

    private static bool UnloadFrom(Assembly assembly)
    {
        var modified = 0;

        foreach (var (k, _) in _modulesByCFC.Where(k => k.Value.ModuleType.Assembly == assembly).ToList())
        {
            _modulesByCFC.Remove(k);
            modified++;
        }

        Service.PluginLog.Verbose($"[ZMM] Unloaded {modified} modules from {assembly}");

        return modified > 0;
    }

    public static void Reload(IEnumerable<Assembly> old, IEnumerable<Assembly> @new)
    {
        var modified = false;

        foreach (var a in old)
            modified |= UnloadFrom(a);

        foreach (var a in @new)
            modified |= ScanAssembly(a);

        if (modified)
            Modified.Fire();
    }

    public static ZoneModule? CreateModule(WorldState ws, uint cfcId)
    {
        return cfcId != 0 && _modulesByCFC.TryGetValue(cfcId, out var info) ? info.Factory(ws) : null;
    }
}
