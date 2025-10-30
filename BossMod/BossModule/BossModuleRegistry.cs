using System.Reflection;

namespace BossMod;

public static class BossModuleRegistry
{
    public class Info
    {
        public Type ModuleType;
        public Type StatesType;
        public Type? ConfigType;
        public Type? ObjectIDType;
        public Type? ActionIDType;
        public Type? StatusIDType;
        public Type? TetherIDType;
        public Type? IconIDType;
        public uint PrimaryActorOID;
        public Func<WorldState, Actor, BossModule> ModuleFactory;

        public BossModuleInfo.Maturity Maturity;
        public string Contributors = "";
        public BossModuleInfo.Expansion Expansion;
        public BossModuleInfo.Category Category;
        public BossModuleInfo.GroupType GroupType;
        public uint GroupID;
        public uint NameID;
        public int SortOrder;
        public int PlanLevel;

        public static Info? Build(Type module)
        {
            var infoAttr = module.GetCustomAttribute<ModuleInfoAttribute>();
            var statesType = infoAttr?.StatesType ?? module.Module.GetType($"{module.FullName}States");
            var configType = infoAttr?.ConfigType ?? module.Module.GetType($"{module.FullName}Config");
            var oidType = infoAttr?.ObjectIDType ?? module.Module.GetType($"{module.Namespace}.OID");
            var aidType = infoAttr?.ActionIDType ?? module.Module.GetType($"{module.Namespace}.AID");
            var sidType = infoAttr?.StatusIDType ?? module.Module.GetType($"{module.Namespace}.SID");
            var tidType = infoAttr?.TetherIDType ?? module.Module.GetType($"{module.Namespace}.TetherID");
            var iidType = infoAttr?.IconIDType ?? module.Module.GetType($"{module.Namespace}.IconID");

            // skip really-WIP modules without logging to user since they have no use for this information
            if (infoAttr?.DevOnly == true && !Service.IsDev)
                return null;

            if (statesType == null || !statesType.IsSubclassOf(typeof(StateMachineBuilder)) || statesType.GetConstructor([module]) == null)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} has incorrect associated states type: it should be derived from StateMachineBuilder and have a constructor accepting module");
                return null;
            }

            if (configType != null && !configType.IsSubclassOf(typeof(ConfigNode)))
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} has incorrect associated config type: it should be derived from ConfigNode");
                configType = null;
            }

            if (oidType != null && !oidType.IsEnum)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} has incorrect associated object ID type: it should be an enum");
                oidType = null;
            }

            if (aidType != null && !aidType.IsEnum)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} has incorrect associated action ID type: it should be an enum");
                aidType = null;
            }

            if (sidType != null && !sidType.IsEnum)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} has incorrect associated status ID type: it should be an enum");
                sidType = null;
            }

            if (tidType != null && !tidType.IsEnum)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} has incorrect associated tether ID type: it should be an enum");
                tidType = null;
            }

            if (iidType != null && !iidType.IsEnum)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} has incorrect associated icon ID type: it should be an enum");
                iidType = null;
            }

            uint primaryOID = infoAttr?.PrimaryActorOID ?? 0;
            if (primaryOID == 0 && oidType != null)
            {
                if (Enum.TryParse(oidType, "Boss", out var oid))
                    primaryOID = (uint)oid!;
            }
            if (primaryOID == 0)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} has no associated primary actor OID: either specify one explicitly or ensure OID enum has Boss entry");
                return null;
            }

            var splitNamespace = module.Namespace?.Split('.') ?? []; // expected to be 'BossMod.expansion.category.rest'

            var expansion = infoAttr?.Expansion ?? BossModuleInfo.Expansion.Count;
            if (expansion == BossModuleInfo.Expansion.Count && splitNamespace.Length > 1 && Enum.TryParse(splitNamespace[1], out BossModuleInfo.Expansion parsedExpansion))
            {
                expansion = parsedExpansion;
            }
            if (expansion == BossModuleInfo.Expansion.Count)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} does not have valid expansion assigned; consider fixing namespace or specifying value manually");
                expansion = BossModuleInfo.Expansion.Global;
            }

            var category = infoAttr?.Category ?? BossModuleInfo.Category.Count;
            if (category == BossModuleInfo.Category.Count && splitNamespace.Length > 2 && Enum.TryParse(splitNamespace[2], out BossModuleInfo.Category parsedCategory))
            {
                category = parsedCategory;
            }
            if (category == BossModuleInfo.Category.Count)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} does not have valid category assigned; consider fixing namespace or specifying value manually");
                category = BossModuleInfo.Category.Uncategorized;
            }

            var groupType = infoAttr?.GroupType ?? BossModuleInfo.GroupType.None;
            var groupID = infoAttr?.GroupID ?? 0;
            var nameID = infoAttr?.NameID ?? 0;
            if (groupType == BossModuleInfo.GroupType.None && groupID == 0)
            {
                Service.Log($"[ModuleRegistry] Module {module.FullName} does not have group type/id assignments.");
            }

            var sortOrder = infoAttr?.SortOrder ?? 0;
            if (sortOrder == 0 && int.TryParse(module.Name.SkipWhile(c => !char.IsAsciiDigit(c)).TakeWhile(char.IsAsciiDigit).ToArray(), out var inferredSortOrder))
            {
                sortOrder = inferredSortOrder;
            }
            if (sortOrder == 0)
            {
                sortOrder = (int)primaryOID;
            }

            return new Info(module, statesType)
            {
                ConfigType = configType,
                ObjectIDType = oidType,
                ActionIDType = aidType,
                StatusIDType = sidType,
                TetherIDType = tidType,
                IconIDType = iidType,
                PrimaryActorOID = primaryOID,

                Maturity = infoAttr?.Maturity ?? BossModuleInfo.Maturity.WIP,
                Contributors = infoAttr?.Contributors ?? "",
                Expansion = expansion,
                Category = category,
                GroupType = groupType,
                GroupID = groupID,
                NameID = nameID,
                SortOrder = sortOrder,
                PlanLevel = infoAttr?.PlanLevel ?? 0,
            };
        }

        private Info(Type moduleType, Type statesType)
        {
            ModuleType = moduleType;
            StatesType = statesType;
            ModuleFactory = New<BossModule>.ConstructorDerived<WorldState, Actor>(moduleType);
        }
    }

    private static readonly Dictionary<uint, Info> _modulesByOID = []; // [primary-actor-oid] = module info
    private static readonly Dictionary<Type, Info> _modulesByType = []; // [module-type] = module info

    static BossModuleRegistry()
    {
        foreach (var t in Utils.GetDerivedTypes<BossModule>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract && t != typeof(DemoModule)))
        {
            var info = Info.Build(t);
            if (info == null)
                continue;
            _modulesByType[t] = info;
            if (!_modulesByOID.TryAdd(info.PrimaryActorOID, info))
                Service.Log($"[ModuleRegistry] Two boss modules have same primary actor OID: {t.FullName} and {_modulesByOID[info.PrimaryActorOID].ModuleType.FullName}");
        }
    }

    private static readonly BossModuleConfig _config = Service.Config.Get<BossModuleConfig>();

    public static IReadOnlyDictionary<uint, Info> RegisteredModules => _modulesByOID;

    public static Info? FindByOID(uint oid) => _modulesByOID.GetValueOrDefault(oid);
    public static Info? FindByType(Type type) => _modulesByType.GetValueOrDefault(type);

    public static BossModule? CreateModule(Info? info, WorldState ws, Actor primary) => info?.ModuleFactory(ws, primary);

    public static BossModule? CreateModuleForActor(WorldState ws, Actor primary, BossModuleInfo.Maturity minMaturity)
    {
        var info = primary.Type is ActorType.Enemy or ActorType.EventObj ? FindByOID(primary.OID) : null;
        if (info is { } inf)
        {
            if (_config.DisabledModules.Contains(inf.ModuleType.ToString()))
                return null;
            if (_config.DisabledCategories.Contains(inf.Category))
                return null;
        }
        return info?.Maturity >= minMaturity ? CreateModule(info, ws, primary) : null;
    }

    // TODO: this is a hack...
    public static BossModule? CreateModuleForConfigPlanning(Type module)
    {
        var info = FindByType(module);
        return info != null ? CreateModule(info, new(TimeSpan.TicksPerSecond, "fake"), new(0, info.PrimaryActorOID, -1, 0, "", 0, ActorType.None, Class.None, 0, new())) : null;
    }

    // TODO: this is a hack...
    public static BossModule? CreateModuleForTimeline(uint oid) => CreateModule(FindByOID(oid), new(TimeSpan.TicksPerSecond, "fake"), new(0, oid, -1, 0, "", 0, ActorType.None, Class.None, 0, new()));
}
