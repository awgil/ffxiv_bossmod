namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

sealed class PutrifiedSoulBurgeoningDreadGhastlyAura(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.PutrifiedSoul1, (uint)AID.PutrifiedSoul1, (uint)AID.BurgeoningDread1, (uint)AID.BurgeoningDread2,
(uint)AID.GhastlyAura1, (uint)AID.GhastlyAura2]);
sealed class MightOfMalice(BossModule module) : Components.SingleTargetCast(module, (uint)AID.MightOfMalice);
sealed class NecroticBillow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NecroticBillow, 8f);
sealed class AmbientPulsation : Components.SimpleAOEs
{
    public AmbientPulsation(BossModule module) : base(module, (uint)AID.AmbientPulsation, 12f, 6)
    {
        MaxDangerColor = 3;
    }
}

sealed class GhastlyAura(BossModule module) : Components.TemporaryMisdirection(module, (uint)AID.GhastlyAura1);
sealed class FellFlowAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FellFlow, new AOEShapeCone(50f, 60f.Degrees()));
sealed class FellFlowBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(50f, 15f.Degrees()), (uint)IconID.FellFlow, (uint)AID.FellFlowBait, 5.2d);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.TheDalriada, GroupID = 778u, NameID = 10004u, SortOrder = 3)]
public sealed class DAL2Cuchulainn : BossModule
{
    public DAL2Cuchulainn(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private DAL2Cuchulainn(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(650f, -187.4f), 25.199f, 48)], [new Rectangle(new(650f, -162f), 20f, 1.25f), new Rectangle(new(650f, -213f), 20f, 1.25f)]);
        return (arena.Center, arena);
    }
}
