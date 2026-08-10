namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class StonegaIVShatteringStomp(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.StonegaIV, (uint)AID.ShatteringStomp]);
sealed class Punishment(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Punishment);
sealed class CrimsonRiddle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CrimsonRiddle1, (uint)AID.CrimsonRiddle2], new AOEShapeCone(30f, 90f.Degrees()));
sealed class StonegaIII1(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.StonegaIII1, 6f);
sealed class StonegaIII2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.StonegaIII2, 6f);
sealed class QuakeSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.QuakeSmall, 6f);
sealed class QuakeBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.QuakeBig, 10f);
sealed class VermilionFlight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VermilionFlight, new AOEShapeRect(60f, 10f));
sealed class ArmOfPurgatory(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArmOfPurgatory, 15f); // 3 + 12 from status effect
sealed class WallArenaChange(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WallArenaChange, new AOEShapeRect(5f, 8f));
sealed class GloamingGleam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GloamingGleam, new AOEShapeRect(50f, 6f));
sealed class RazorFang(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RazorFang, 20f);

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.FaithboundKirin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058u, NameID = 14053u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 2)]
public sealed class A21FaithboundKirin : BossModule
{
    public A21FaithboundKirin(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private A21FaithboundKirin(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-850f, 780f), 29.5f, 60)]);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ChiseledArm2));
        Arena.Actors(Enemies((uint)OID.ChiseledArm3));
    }
}
