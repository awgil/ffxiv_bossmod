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
                var oidType = t?.Module.GetType($"{t.Namespace}.OID");
                if (oidType == null || !oidType.IsEnum)
                    continue;
                object? oid;
                if (Enum.TryParse(oidType, "Boss", out oid))
                    _modules[(uint)oid!] = t!;
            }
        }

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
    }
}
