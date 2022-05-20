using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BossMod
{
    public static class ModuleRegistry
    {
        private static Dictionary<uint, Type> _modules = new();

        private static IEnumerable<Type?> GetAllTypes()
        {
            try
            {
                return Assembly.GetExecutingAssembly().DefinedTypes;
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types;
            }
        }

        static ModuleRegistry()
        {
            var baseType = typeof(BossModule);
            foreach (var t in GetAllTypes().Where(baseType.IsAssignableFrom))
            {
                uint primaryOID = GetPrimaryActorOID(t!);
                if (primaryOID == 0)
                    continue;
                if (_modules.ContainsKey(primaryOID))
                    throw new Exception($"Two boss modules have same primary actor OID: {t!.Name} and {_modules[primaryOID].Name}");
                _modules[primaryOID] = t!;
            }
        }

        public static IReadOnlyDictionary<uint, Type> RegisteredModules => _modules;

        public static Type? TypeForOID(uint oid)
        {
            return _modules.GetValueOrDefault(oid);
        }

        public static BossModule? CreateModule(Type? type, BossModuleManager manager, Actor primary)
        {
            return type != null ? (BossModule?)Activator.CreateInstance(type, manager, primary) : null;
        }

        public static BossModule? CreateModule(uint oid, BossModuleManager manager, Actor primary)
        {
            return CreateModule(TypeForOID(oid), manager, primary);
        }

        private static uint GetPrimaryActorOID(Type module)
        {
            // first try to use explicit attribute
            var oidAttr = Attribute.GetCustomAttribute(module, typeof(PrimaryActorOIDAttribute)) as PrimaryActorOIDAttribute;
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
