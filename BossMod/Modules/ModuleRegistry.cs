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

                return new Info(module, statesType) { ConfigType = configType, ObjectIDType = oidType, ActionIDType = aidType, StatusIDType = sidType, TetherIDType = tidType, IconIDType = iidType, PrimaryActorOID = primaryOID };
            }

            private Info(Type moduleType, Type statesType)
            {
                ModuleType = moduleType;
                StatesType = statesType;
            }
        }

        private static Dictionary<uint, Info> _modules = new(); // [primary-actor-oid] = module type

        static ModuleRegistry()
        {
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
                    return CreateModule(i.ModuleType, new(TimeSpan.TicksPerSecond), new(0, i.PrimaryActorOID, -1, "", ActorType.None, Class.None, new()));
            return null;
        }

        // TODO: this is a hack...
        public static BossModule? CreateModuleForTimeline(uint oid)
        {
            return CreateModule(FindByOID(oid)?.ModuleType, new(TimeSpan.TicksPerSecond), new(0, oid, -1, "", ActorType.None, Class.None, new()));
        }
    }
}
