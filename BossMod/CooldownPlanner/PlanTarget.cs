using ImGuiNET;

namespace BossMod.PlanTarget;

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

// useful for tank buddy saves
public class CoTank : ISelector
{
    public override Actor? Select(BossModule module, int playerSlot, Actor player) => module.Raid.WithoutSlot(true).FirstOrDefault(a => a != player && a.Role == Role.Tank);
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

// useful for heals?
public class LowestHPPartyMember : ISelector
{
    public bool AllowSelf;

    public override Actor? Select(BossModule module, int playerSlot, Actor player) => module.Raid.WithoutSlot().Exclude(AllowSelf ? null : player).MinBy(a => a.HP.Cur);
    public override string Describe(ModuleRegistry.Info? moduleInfo) => $"Allow self: {AllowSelf}";
    public override bool Edit(ModuleRegistry.Info? moduleInfo) => ImGui.Checkbox("Allow self", ref AllowSelf);
}
