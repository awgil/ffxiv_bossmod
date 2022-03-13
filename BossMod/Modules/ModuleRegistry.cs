using System;

namespace BossMod
{
    public static class ModuleRegistry
    {
        public static Type? TypeForOID(uint oid)
        {
            return oid switch {
                (uint)Zodiark.OID.Boss => typeof(Zodiark),
                (uint)Endwalker.P1S.OID.Boss => typeof(Endwalker.P1S.P1S),
                (uint)Endwalker.P2S.OID.Boss => typeof(Endwalker.P2S.P2S),
                (uint)Endwalker.P3S.OID.Boss => typeof(Endwalker.P3S.P3S),
                (uint)Endwalker.P4S1.OID.Boss => typeof(Endwalker.P4S1.P4S1),
                (uint)Endwalker.P4S2.OID.Boss => typeof(Endwalker.P4S2.P4S2),
                (uint)Endwalker.ARanks.Hulder.OID.Boss => typeof(Endwalker.ARanks.Hulder.Hulder),
                (uint)Endwalker.ARanks.Storsie.OID.Boss => typeof(Endwalker.ARanks.Storsie.Storsie),
                (uint)Endwalker.ARanks.Sugriva.OID.Boss => typeof(Endwalker.ARanks.Sugriva.Sugriva),
                (uint)Endwalker.ARanks.Yilan.OID.Boss => typeof(Endwalker.ARanks.Yilan.Yilan),
                (uint)Endwalker.ARanks.Minerva.OID.Boss => typeof(Endwalker.ARanks.Minerva.Minerva),
                (uint)Endwalker.ARanks.Aegeiros.OID.Boss => typeof(Endwalker.ARanks.Aegeiros.Aegeiros),
                (uint)Endwalker.ARanks.MoussePrincess.OID.Boss => typeof(Endwalker.ARanks.MoussePrincess.MoussePrincess),
                (uint)Endwalker.ARanks.LunatenderQueen.OID.Boss => typeof(Endwalker.ARanks.LunatenderQueen.LunatenderQueen),
                (uint)Endwalker.ARanks.Petalodus.OID.Boss => typeof(Endwalker.ARanks.Petalodus.Petalodus),
                (uint)Endwalker.ARanks.Gurangatch.OID.Boss => typeof(Endwalker.ARanks.Gurangatch.Gurangatch),
                (uint)Endwalker.ARanks.ArchEta.OID.Boss => typeof(Endwalker.ARanks.ArchEta.ArchEta),
                (uint)Endwalker.ARanks.FanAil.OID.Boss => typeof(Endwalker.ARanks.FanAil.FanAil),
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
