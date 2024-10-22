using System.Reflection;

namespace BossMod;

// attribute for defining zone module's metadata; it is required by each module to be loaded
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ZoneModuleInfoAttribute(BossModuleInfo.Maturity maturity, uint cfcId, uint territoryID = 0) : Attribute
{
    public BossModuleInfo.Maturity Maturity => maturity;
    public uint CFCID => cfcId;
    public uint TerritoryID => territoryID;
}

public static class ZoneModuleRegistry
{
    public record class Info(Type ModuleType, ZoneModuleInfoAttribute Desc, Func<WorldState, ZoneModule> Factory);

    private static readonly Dictionary<uint, Info> _modulesByCFC = [];

    static ZoneModuleRegistry()
    {
        foreach (var t in Utils.GetDerivedTypes<ZoneModule>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract))
        {
            var attr = t.GetCustomAttribute<ZoneModuleInfoAttribute>();
            if (attr == null)
            {
                Service.Log($"Zone module {t} has no ZoneModuleInfo attribute, skipping");
                continue;
            }
            if (_modulesByCFC.TryGetValue(attr.CFCID, out var existingModule))
            {
                Service.Log($"Two zone modules have same CFCID: {t.Name} and {existingModule.ModuleType.Name}");
                continue;
            }
            _modulesByCFC[attr.CFCID] = new Info(t, attr, New<ZoneModule>.ConstructorDerived<WorldState>(t));
        }
    }

    public static ZoneModule? CreateModule(WorldState ws, uint cfcId, BossModuleInfo.Maturity minMaturity)
    {
        return cfcId != 0 && _modulesByCFC.TryGetValue(cfcId, out var info) && info.Desc.Maturity >= minMaturity ? info.Factory(ws) : null;
    }
}
