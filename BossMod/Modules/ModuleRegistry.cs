using System;

namespace BossMod
{
    public static class ModuleRegistry
    {
        public static Type? TypeForOID(uint oid)
        {
            return oid switch {
                (uint)Zodiark.OID.Boss => typeof(Zodiark),
                (uint)P1S.OID.Boss => typeof(P1S.P1S),
                (uint)P2S.OID.Boss => typeof(P2S.P2S),
                (uint)P3S.OID.Boss => typeof(P3S.P3S),
                (uint)P4S.OID.Boss2 => typeof(P4S.P4S),
                _ => null,
            };
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
