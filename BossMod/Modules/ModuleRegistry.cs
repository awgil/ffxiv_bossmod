using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BossMod
{
    public static class ModuleRegistry
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

            public uint CFCID;
            public uint ExVersion;
            public uint ContentIcon;
            public SeString? ContentType;
            public SeString? InstanceName;
            public SeString? ForayName;
            public SeString? FateName;
            public SeString? BossName;
            public string HuntRank;

            public bool IsUncatalogued;

            public enum HuntRanks : byte
            {
                None = 0,
                B = 1,
                A = 2,
                S = 3,
            }

            public bool CooldownPlanningSupported => ConfigType?.IsSubclassOf(typeof(CooldownPlanningConfigNode)) ?? false;

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

                if (statesType == null || !statesType.IsSubclassOf(typeof(StateMachineBuilder)) || statesType.GetConstructor(new[] { module }) == null)
                {
                    Service.Log($"[ModuleRegistry] Module {module.Name} has incorrect associated states type: it should be derived from StateMachineBuilder and have a constructor accepting module");
                    return null;
                }

                if (configType != null && !configType.IsSubclassOf(typeof(ConfigNode)))
                {
                    Service.Log($"[ModuleRegistry] Module {module.Name} has incorrect associated config type: it should be derived from ConfigNode");
                    configType = null;
                }

                if (oidType != null && !oidType.IsEnum)
                {
                    Service.Log($"[ModuleRegistry] Module {module.Name} has incorrect associated object ID type: it should be an enum");
                    oidType = null;
                }

                if (aidType != null && !aidType.IsEnum)
                {
                    Service.Log($"[ModuleRegistry] Module {module.Name} has incorrect associated action ID type: it should be an enum");
                    aidType = null;
                }

                if (sidType != null && !sidType.IsEnum)
                {
                    Service.Log($"[ModuleRegistry] Module {module.Name} has incorrect associated status ID type: it should be an enum");
                    sidType = null;
                }

                if (tidType != null && !tidType.IsEnum)
                {
                    Service.Log($"[ModuleRegistry] Module {module.Name} has incorrect associated tether ID type: it should be an enum");
                    tidType = null;
                }

                if (iidType != null && !iidType.IsEnum)
                {
                    Service.Log($"[ModuleRegistry] Module {module.Name} has incorrect associated icon ID type: it should be an enum");
                    iidType = null;
                }

                uint primaryOID = infoAttr?.PrimaryActorOID ?? 0;
                if (primaryOID == 0 && oidType != null)
                {
                    object? oid;
                    if (Enum.TryParse(oidType, "Boss", out oid))
                        primaryOID = (uint)oid!;
                }
                if (primaryOID == 0)
                {
                    Service.Log($"[ModuleRegistry] Module {module.Name} has no associated primary actor OID: either specify one explicitly or ensure OID enum has Boss entry");
                    return null;
                }

                uint nameID = infoAttr?.NameID ?? 0;
                uint nmID = infoAttr?.NotoriousMonsterID ?? 0;
                uint fateID = infoAttr?.FateID ?? 0;
                uint dynamicEventID = infoAttr?.DynamicEventID ?? 0;
                if (nameID == 0 && nmID == 0 && fateID == 0 && dynamicEventID == 0)
                    Service.Log($"[{nameof(ModuleRegistry)}] Module {module.Name} does not provide a Name/Notorious Monster/Fate/Dyanamic Event ID: this is needed for overworld, bozja, and multi fight instances (dungeons) to be catalogued properly. Please add one.");

                uint cfcID = infoAttr?.CFCID ?? 0;
                if (cfcID == 0)
                    Service.Log($"[{nameof(ModuleRegistry)}] Module {module.Name} does not provide a CFC ID: this will prevent it from being catalogued properly. Please add one.");

                bool uncatalogued = (cfcID == 0 && nameID == 0 && nmID == 0 && fateID == 0 && dynamicEventID == 0) || (cfcID != 0 && CFCSheet.GetRow(cfcID)!.ShortCode.RawString.IsNullOrEmpty());
                if (uncatalogued)
                    Service.Log($"{module.Name} {uncatalogued}");

                SeString contentType = new();
                uint contentIcon = default;
                SeString instanceName = new();
                uint exVersion = 69;
                string huntRank = "";
                SeString fateName = new();
                SeString forayName = new();
                SeString bossName = new();

                if (cfcID != 0)
                {
                    var cfcRow = CFCSheet.GetRow(cfcID)!;
                    contentType = cfcRow.ContentType?.Value?.Name ?? new SeString();
                    exVersion = cfcRow.TerritoryType?.Value?.ExVersion.Value?.RowId ?? 0;
                    instanceName = cfcRow.Name;
                    // needed because bozja et al does not have a ContentType
                    if (cfcID is 735 or 760 or 761 or 778)
                    {
                        contentType = Service.DataManager.GetExcelSheet<CharaCardPlayStyle>()!.GetRow(6)!.Name;
                        contentIcon = (uint)Service.DataManager.GetExcelSheet<CharaCardPlayStyle>()!.GetRow(6)!.Icon;
                    }
                    else
                        contentIcon = cfcRow.ContentType?.Value?.Icon ?? 0;
                }

                if (nameID != 0)
                {
                    bossName = NPCNamesSheet.GetRow(nameID)!.Singular;
                }

                if (nmID != 0)
                {
                    bossName = NMSheet.GetRow(nmID)!.BNpcName.Value?.Singular ?? new SeString();
                    huntRank = Enum.Parse<HuntRanks>(NMSheet.GetRow(nmID)!.Rank.ToString()).ToString();
                    contentType = PlayStyleSheet.GetRow(10)!.Name;
                    contentIcon = (uint)PlayStyleSheet.GetRow(10)!.Icon;
                    foreach (var row in NMTSheet)
                        foreach (var prop in row.GetType().GetProperties())
                            if (prop.GetValue(row) is uint row_nmID)
                                exVersion = TerritorySheet.FirstOrDefault(x => x.Unknown42 == row.RowId)?.ExVersion.Value?.RowId ?? 0;
                }

                if (fateID != 0) // needs exversion
                {
                    contentType = ContentTypeSheet.GetRow(8)!.Name;
                    contentIcon = ContentTypeSheet.GetRow(8)!.Icon;
                    fateName = FateSheet.GetRow(fateID)!.Name;
                }

                if (dynamicEventID != 0) // needs exversion?
                {
                    contentType = PlayStyleSheet.GetRow(6)!.Name;
                    contentIcon = (uint)PlayStyleSheet.GetRow(6)!.Icon;
                    forayName = DynamicEventSheet.GetRow(dynamicEventID)!.Name;
                }
                
                return new Info(module, statesType) {
                    ConfigType = configType,
                    ObjectIDType = oidType,
                    ActionIDType = aidType,
                    StatusIDType = sidType,
                    TetherIDType = tidType,
                    IconIDType = iidType,
                    PrimaryActorOID = primaryOID,

                    CFCID = cfcID,
                    ContentType = contentType,
                    ContentIcon = contentIcon,
                    InstanceName = instanceName,
                    ExVersion = exVersion,
                    BossName = bossName, 
                    FateName = fateName,
                    ForayName = forayName,
                    HuntRank = huntRank,

                    IsUncatalogued = uncatalogued,
                };
            }

            private Info(Type moduleType, Type statesType)
            {
                ModuleType = moduleType;
                StatesType = statesType;
            }
        }

        private static Dictionary<uint, Info> _modules = new(); // [primary-actor-oid] = module type

        private static Lumina.Excel.ExcelSheet<ContentFinderCondition> CFCSheet;
        private static Lumina.Excel.ExcelSheet<ContentType> ContentTypeSheet;
        private static Lumina.Excel.ExcelSheet<NotoriousMonster> NMSheet;
        private static Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets2.NotoriousMonsterTerritory> NMTSheet;
        private static Lumina.Excel.ExcelSheet<Fate> FateSheet;
        private static Lumina.Excel.ExcelSheet<CharaCardPlayStyle> PlayStyleSheet;
        private static Lumina.Excel.ExcelSheet<TerritoryType> TerritorySheet;
        private static Lumina.Excel.ExcelSheet<DynamicEvent> DynamicEventSheet;
        private static Lumina.Excel.ExcelSheet<BNpcName> NPCNamesSheet;

        static ModuleRegistry()
        {
            CFCSheet = Service.DataManager.GetExcelSheet<ContentFinderCondition>()!;
            ContentTypeSheet = Service.DataManager.GetExcelSheet<ContentType>()!;
            NMSheet = Service.DataManager.GetExcelSheet<NotoriousMonster>()!;
            NMTSheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets2.NotoriousMonsterTerritory>()!;
            FateSheet = Service.DataManager.GetExcelSheet<Fate>()!;
            PlayStyleSheet = Service.DataManager.GetExcelSheet<CharaCardPlayStyle>()!;
            TerritorySheet = Service.DataManager.GetExcelSheet<TerritoryType>()!;
            DynamicEventSheet = Service.DataManager.GetExcelSheet<DynamicEvent>()!;
            NPCNamesSheet = Service.DataManager.GetExcelSheet<BNpcName>()!;

            foreach (var t in Utils.GetDerivedTypes<BossModule>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract && t != typeof(DemoModule)))
            {
                var info = Info.Build(t);
                if (info == null)
                    continue;

                if (_modules.ContainsKey(info.PrimaryActorOID))
                    throw new Exception($"Two boss modules have same primary actor OID: {t.Name} and {_modules[info.PrimaryActorOID].ModuleType.Name}");
                _modules[info.PrimaryActorOID] = info;
            }
        }

        public static bool IsFate(this KeyValuePair<uint, Info> module) => !module.Value.FateName!.RawString.IsNullOrEmpty();
        public static bool IsHunt(this KeyValuePair<uint, Info> module) => !module.Value.HuntRank.IsNullOrEmpty();
        public static bool IsCriticalEngagement(this KeyValuePair<uint, Info> module) => !module.Value.ForayName!.RawString.IsNullOrEmpty();
        public static bool IsRemovedContent(this KeyValuePair<uint, Info> module)
        {
            var cfcRow = CFCSheet.GetRow(module.Value.CFCID);

            if (cfcRow == null)
            {
                // Handle the case where the row is null (optional)
                return true; // or false, depending on your logic
            }

            foreach (var prop in cfcRow.GetType().GetProperties())
            {
                var propValue = prop.GetValue(cfcRow);

                // Check if the property value is the default value for its type
                if (propValue != null && !propValue.Equals(default(PropertyInfo)))
                {
                    return false; // Property has a non-default value, module is not removed content
                }
            }

            return true; // All properties have default values, module is considered removed content
        }

        public static IReadOnlyDictionary<uint, Info> RegisteredModules => _modules;

        public static Info? FindByOID(uint oid) => _modules.GetValueOrDefault(oid);

        public static BossModule? CreateModule(Type? type, WorldState ws, Actor primary)
        {
            return type != null ? (BossModule?)Activator.CreateInstance(type, ws, primary) : null;
        }

        public static BossModule? CreateModuleForActor(WorldState ws, Actor primary)
        {
            return primary.Type is ActorType.Enemy or ActorType.EventObj ? CreateModule(FindByOID(primary.OID)?.ModuleType, ws, primary) : null;
        }

        // TODO: this is a hack...
        public static BossModule? CreateModuleForConfigPlanning(Type cfg)
        {
            foreach (var i in _modules.Values)
                if (i.ConfigType == cfg)
                    return CreateModule(i.ModuleType, new(TimeSpan.TicksPerSecond, "fake"), new(0, i.PrimaryActorOID, -1, "", ActorType.None, Class.None, new()));
            return null;
        }

        // TODO: this is a hack...
        public static BossModule? CreateModuleForTimeline(uint oid)
        {
            return CreateModule(FindByOID(oid)?.ModuleType, new(TimeSpan.TicksPerSecond, "fake"), new(0, oid, -1, "", ActorType.None, Class.None, new()));
        }
    }
}
