using System;
using System.Linq;

namespace BossMod.PlanTarget
{
    public abstract class ISelector
    {
        public abstract Actor? Select(BossModule module, int playerSlot, Actor player);
        public virtual string Describe(ModuleRegistry.Info? moduleInfo) => "";
        public virtual bool Edit(ModuleRegistry.Info? moduleInfo) => false;
        public ISelector Clone() => (ISelector)MemberwiseClone();
    }

    public class Self : ISelector
    {
        public override Actor? Select(BossModule module, int playerSlot, Actor player) => player;
    }

    public class PrimaryEnemy : ISelector
    {
        public override Actor? Select(BossModule module, int playerSlot, Actor player) => module.PrimaryActor;
    }

    public class EnemyByOID : ISelector
    {
        public uint OID;

        public override Actor? Select(BossModule module, int playerSlot, Actor player) => module.WorldState.Actors.FirstOrDefault(a => a.OID == OID && !a.IsDead && a.IsTargetable);

        public override string Describe(ModuleRegistry.Info? moduleInfo)
        {
            if (moduleInfo?.ObjectIDType == null)
                return $"{OID:X}";
            return Enum.ToObject(moduleInfo.ObjectIDType, OID).ToString() ?? "";
        }

        public override bool Edit(ModuleRegistry.Info? moduleInfo)
        {
            if (moduleInfo?.ObjectIDType == null)
                return false;
            var v = (Enum)Enum.ToObject(moduleInfo.ObjectIDType, OID);
            if (!UICombo.Enum("OID", ref v))
                return false;
            OID = (uint)(object)v;
            return true;
        }
    }
}
