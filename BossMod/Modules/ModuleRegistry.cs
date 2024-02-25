using Dalamud.Utility;
using Lumina.Excel;
using Lumina;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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
            public string HuntRank = "";
            public int CarnivaleStage;

            public bool IsUncatalogued;

            public enum HuntRanks : byte
            {
                None = 0,
                B = 1,
                A = 2,
                S = 3,
            }

            public bool CooldownPlanningSupported => ConfigType?.IsSubclassOf(typeof(CooldownPlanningConfigNode)) ?? false;

            public bool IsFate() => !FateName!.RawString.IsNullOrEmpty();
            public bool IsHunt() => !HuntRank.IsNullOrEmpty();
            public bool IsCarnivale() => CarnivaleStage != 0;
            public bool IsCriticalEngagement() => !ForayName!.RawString.IsNullOrEmpty();

            // AFAIK, unreals are the only piece of content that regularly get removed. Their CFCID stays but the properties are all reverted to default.
            public bool IsRemovedContent()
            {
                var cfcRow = _cfcSheet.GetRow(CFCID);

                if (cfcRow == null)
                    return true;

                foreach (var prop in cfcRow.GetType().GetProperties())
                {
                    var propValue = prop.GetValue(cfcRow);
                    if (propValue != null && !propValue.Equals(default(PropertyInfo)))
                        return false; // Property has a non-default value, module is not removed content
                }

                return true; // All properties have default values, module is considered removed content
            }

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
                uint cfcID = infoAttr?.CFCID ?? 0;

                bool uncatalogued = (cfcID == 0 && nameID == 0 && nmID == 0 && fateID == 0 && dynamicEventID == 0) || (cfcID != 0 && _cfcSheet.GetRow(cfcID)!.ShortCode.RawString.IsNullOrEmpty());
                if (uncatalogued)
                    Service.Log($"[{nameof(ModuleRegistry)}] Module {module.Name} is uncatalogued. It does not provide sufficient {nameof(Info)} tags.");

                SeString contentType = new();
                uint contentIcon = default;
                SeString instanceName = new();
                uint exVersion = 69;
                string huntRank = "";
                int carnivaleStage = 0;
                SeString fateName = new();
                SeString forayName = new();
                SeString bossName = new();

                if (cfcID != 0)
                {
                    var cfcRow = _cfcSheet.GetRow(cfcID)!;
                    contentType = cfcRow.ContentType?.Value?.Name ?? new SeString();
                    exVersion = cfcRow.TerritoryType?.Value?.ExVersion.Value?.RowId ?? 0;
                    instanceName = cfcRow.Name;

                    if (cfcID is 735 or 760 or 761 or 778) // bozja et al
                    {
                        contentType = Service.LuminaGameData!.GetExcelSheet<CharaCardPlayStyle>()!.GetRow(6)!.Name;
                        contentIcon = (uint)Service.LuminaGameData!.GetExcelSheet<CharaCardPlayStyle>()!.GetRow(6)!.Icon;
                    }
                    else if (cfcRow.ShortCode.RawString.StartsWith("aoz")) // masked carnivale
                    {
                        contentType = Service.LuminaGameData!.GetExcelSheet<CharaCardPlayStyle>()!.GetRow(8)!.Name;
                        contentIcon = (uint)Service.LuminaGameData!.GetExcelSheet<CharaCardPlayStyle>()!.GetRow(8)!.Icon;
                        carnivaleStage = int.Parse(Regex.Replace(cfcRow.ShortCode.RawString, @"\D", "").TrimStart('0'));
                    }
                    else
                        contentIcon = cfcRow.ContentType?.Value?.Icon ?? 0;
                }

                if (nameID != 0)
                {
                    bossName = _npcNamesSheet.GetRow(nameID)!.Singular;
                }

                if (nmID != 0)
                {
                    bossName = _nmSheet.GetRow(nmID)!.BNpcName.Value?.Singular ?? new SeString();
                    huntRank = Enum.Parse<HuntRanks>(_nmSheet.GetRow(nmID)!.Rank.ToString()).ToString();
                    contentType = _playStyleSheet.GetRow(10)!.Name;
                    contentIcon = (uint)_playStyleSheet.GetRow(10)!.Icon;
                    foreach (var row in _nmtSheet)
                        if (row.Monster.Contains((ushort)nmID))
                            exVersion = _territorySheet.FirstOrDefault(x => x.Unknown42 == row.RowId)?.ExVersion.Value?.RowId ?? 0;
                }

                if (fateID != 0) // needs exversion
                {
                    contentType = _contentTypeSheet.GetRow(8)!.Name;
                    contentIcon = _contentTypeSheet.GetRow(8)!.Icon;
                    fateName = _fateSheet.GetRow(fateID)!.Name;
                }

                if (dynamicEventID != 0) // needs exversion?
                {
                    contentType = _playStyleSheet.GetRow(6)!.Name;
                    contentIcon = (uint)_playStyleSheet.GetRow(6)!.Icon;
                    forayName = _dynamicEventSheet.GetRow(dynamicEventID)!.Name;
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
                    CarnivaleStage = carnivaleStage,

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

        private static readonly ExcelSheet<ContentFinderCondition> _cfcSheet;
        private static readonly ExcelSheet<ContentType> _contentTypeSheet;
        private static readonly ExcelSheet<NotoriousMonster> _nmSheet;
        private static readonly ExcelSheet<NotoriousMonsterTerritory> _nmtSheet;
        private static readonly ExcelSheet<Fate> _fateSheet;
        private static readonly ExcelSheet<CharaCardPlayStyle> _playStyleSheet;
        private static readonly ExcelSheet<TerritoryType> _territorySheet;
        private static readonly ExcelSheet<DynamicEvent> _dynamicEventSheet;
        private static readonly ExcelSheet<BNpcName> _npcNamesSheet;

        private static readonly List<Info> _catalogued;
        private static readonly List<Info> _uncatalogued;
        private static readonly List<uint> _expacs;

        static ModuleRegistry()
        {
            _cfcSheet = Service.LuminaGameData!.GetExcelSheet<ContentFinderCondition>()!;
            _contentTypeSheet = Service.LuminaGameData!.GetExcelSheet<ContentType>()!;
            _nmSheet = Service.LuminaGameData!.GetExcelSheet<NotoriousMonster>()!;
            _nmtSheet = Service.LuminaGameData!.GetExcelSheet<NotoriousMonsterTerritory>()!;
            _fateSheet = Service.LuminaGameData!.GetExcelSheet<Fate>()!;
            _playStyleSheet = Service.LuminaGameData!.GetExcelSheet<CharaCardPlayStyle>()!;
            _territorySheet = Service.LuminaGameData!.GetExcelSheet<TerritoryType>()!;
            _dynamicEventSheet = Service.LuminaGameData!.GetExcelSheet<DynamicEvent>()!;
            _npcNamesSheet = Service.LuminaGameData!.GetExcelSheet<BNpcName>()!;

            foreach (var t in Utils.GetDerivedTypes<BossModule>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract && t != typeof(DemoModule)))
            {
                var info = Info.Build(t);
                if (info == null)
                    continue;

                if (_modules.ContainsKey(info.PrimaryActorOID))
                    throw new Exception($"Two boss modules have same primary actor OID: {t.Name} and {_modules[info.PrimaryActorOID].ModuleType.Name}");
                _modules[info.PrimaryActorOID] = info;
            }

            _catalogued = _modules.Values
                .Where(x => !x.IsUncatalogued)
                .OrderBy(x => x.ExVersion)
                .ThenBy(x => _cfcSheet.GetRow(x.CFCID)?.ClassJobLevelSync)
                .ThenBy(x => _cfcSheet.GetRow(x.CFCID)?.ItemLevelRequired)
                .ThenBy(x => _cfcSheet.GetRow(x.CFCID)?.SortKey)
                .ToList();
            _uncatalogued = _modules.Values.Where(x => x.IsUncatalogued || x.ExVersion == 69).Select(x => x).ToList();
            _expacs = _modules.Where(x => x.Value.ExVersion != 69).Select(x => x.Value.ExVersion).Distinct().ToList();
        }

        public static IReadOnlyList<Info> CataloguedModules => _catalogued;
        public static IReadOnlyList<Info> UncataloguedModules => _uncatalogued;
        public static IReadOnlyList<uint> AvailableExpansions => _expacs;

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

        [Sheet("NotoriousMonsterTerritory", columnHash: 0xf057da9c)]
        public partial class NotoriousMonsterTerritory : ExcelRow
        {
            public const int Length = 10;
            public ushort[] Monster { get; private set; } = new ushort[Length];

            public override void PopulateData(RowParser parser, GameData gameData, Lumina.Data.Language language)
            {
                base.PopulateData(parser, gameData, language);
                for (var i = 0; i < Length; ++i)
                {
                    Monster[i] = parser.ReadOffset<ushort>(2 * i);
                }
            }
        }
    }
}
