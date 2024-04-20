namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class DestructiveBolt(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DestructiveBoltAOE), 3);
class StrikingMeteor(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.StrikingMeteor), 6);
class BronzeLightning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BronzeLightning), new AOEShapeCone(50, 22.5f.Degrees()), 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11273, SortOrder = 3)]
public class A12Rhalgr(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsPolygon(arenacoords))
{
    private static readonly List<WPos> arenacoords = [new(-29.2f, 235.5f), new(-40.3f, 248f), new(-47.5f, 260), new(-52.64f, 274.2f), new(-46.3f, 276.04f), new(-45.1f, 274.8f),
    new(-43.2f, 272.1f), new(-40.4f, 270.7f), new(-38.8f, 271.4f), new(-38.3f, 272.6f), new(-38.2f, 275f), new(-39.1f, 278.5f), new(-40.7f, 282.4f), new(-46.1f, 291.3f),
    new(-49.39f, 296.73f), new(-40.96f, 300.2f), new(-37.1f, 293.4f), new(-34.9f, 291f), new(-32.5f, 290.2f), new(-30.7f, 291.1f), new(-30.5f, 295.8f), new(-31.3f, 304.94f),
    new(-22.35f, 306.16f), new(-19.8f, 290.5f), new(-18f, 288.7f), new(-16f, 289.2f), new(-14f, 290.9f), new(-13.9f, 303.98f), new(-6.23f, 304.72f), new(-4.5f, 288.2f),
    new(-3.7f, 287f), new(-1.3f, 287.8f), new(-0.1f, 289.2f), new(3.31f, 297.2f), new(9.09f, 293.91f), new(6.4f, 286.6f), new(6.2f, 283.2f), new(7.3f, 276.4f), 
    new(7.7f, 267.2f), new(6.8f, 253f), new(4.5f, 242.7f), new(2.23f, 235.6f)];
}

