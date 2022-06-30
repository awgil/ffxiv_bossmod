using System;
using System.Collections.Generic;
using System.Reflection;

namespace BossMod
{
    public static class ModuleRegistry
    {
        private static Dictionary<uint, Type> _modules = new(); // [primary-actor-oid] = module type
        private static Dictionary<Type, Type> _plannableModules = new(); // [config-type] = module type (contains only modules that support cooldown planning)

        static ModuleRegistry()
        {
            foreach (var t in Utils.GetDerivedTypes<BossModule>(Assembly.GetExecutingAssembly()))
            {
                uint primaryOID = GetPrimaryActorOID(t!);
                if (primaryOID == 0)
                    continue;

                if (_modules.ContainsKey(primaryOID))
                    throw new Exception($"Two boss modules have same primary actor OID: {t.Name} and {_modules[primaryOID].Name}");
                _modules[primaryOID] = t;

                var configType = PlanConfigType(t);
                if (configType != null)
                {
                    if (!configType.IsSubclassOf(typeof(CooldownPlanningConfigNode)))
                        throw new Exception($"ModuleConfig should specify config type derived from CooldownPlanningConfigNode");
                    if (_plannableModules.ContainsKey(configType))
                        throw new Exception($"Two boss modules have same config type: {t.Name} and {_plannableModules[configType].Name}");
                    _plannableModules[configType] = t;
                }
            }
        }

        public static IReadOnlyDictionary<uint, Type> RegisteredModules => _modules;

        public static Type? TypeForOID(uint oid) => _modules.GetValueOrDefault(oid);
        public static Type? TypeForConfig(Type cfg) => _plannableModules.GetValueOrDefault(cfg);

        public static BossModule? CreateModule(Type? type, WorldState ws, Actor primary)
        {
            return type != null ? (BossModule?)Activator.CreateInstance(type, ws, primary) : null;
        }

        public static BossModule? CreateModuleForActor(WorldState ws, Actor primary)
        {
            return CreateModule(TypeForOID(primary.OID), ws, primary);
        }

        // TODO: this is a hack...
        public static BossModule? CreateModuleForConfigPlanning(Type cfg)
        {
            var t = TypeForConfig(cfg);
            return t != null ? CreateModule(t, new(), new(0, 0, "", ActorType.None, Class.None, new())) : null;
        }

        public static Type? PlanConfigType(Type module)
        {
            var attr = module.GetCustomAttribute<CooldownPlanningAttribute>();
            return attr?.ConfigType;
        }

        private static uint GetPrimaryActorOID(Type module)
        {
            // first try to use explicit attribute
            var oidAttr = module.GetCustomAttribute<PrimaryActorOIDAttribute>();
            if (oidAttr != null)
                return oidAttr.OID;

            // if not available, search for "Boss" OID enum
            var oidType = module.Module.GetType($"{module.Namespace}.OID");
            if (oidType != null && oidType.IsEnum)
            {
                object? oid;
                if (Enum.TryParse(oidType, "Boss", out oid))
                    return (uint)oid!;
            }

            // nothing found...
            return 0;
        }
    }
}
