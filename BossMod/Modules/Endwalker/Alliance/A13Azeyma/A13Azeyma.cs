namespace BossMod.Endwalker.Alliance.A13Azeyma;

sealed class WardensWarmth(BossModule module) : Components.BaitAwayCast(module, (uint)AID.WardensWarmthAOE, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class FleetingSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FleetingSpark, new AOEShapeCone(60f, 135f.Degrees()));
sealed class SolarFold(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SolarFoldAOE, new AOEShapeCross(30f, 5f));
sealed class Sunbeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sunbeam, 9f, 14);
sealed class SublimeSunset(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SublimeSunsetAOE, 40f); // TODO: check falloff

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866u, NameID = 11277u, SortOrder = 5, PlanLevel = 90)]
public sealed class A13Azeyma : BossModule
{
    public A13Azeyma(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private A13Azeyma(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-750f, -750f), 29.5f, 180)], [new Rectangle(new(-750f, -719.981f), 20f, 1.25f),
        new Rectangle(new(-750f, -779.985f), 20f, 1.25f)]);
        return (arena.Center, arena);
    }
}
