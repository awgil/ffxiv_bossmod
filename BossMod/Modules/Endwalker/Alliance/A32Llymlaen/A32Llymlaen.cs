﻿namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class WindRose(BossModule module) : Components.SelfTargetedAOEs(module, AID.WindRose, new AOEShapeCircle(12));
class SeafoamSpiral(BossModule module) : Components.SelfTargetedAOEs(module, AID.SeafoamSpiral, new AOEShapeDonut(6, 70));
class DeepDiveNormal(BossModule module) : Components.StackWithCastTargets(module, AID.DeepDiveNormal, 6, 8);
class TorrentialTridentLanding(BossModule module) : Components.CastCounter(module, AID.TorrentialTridentLanding);
class TorrentialTridentAOE(BossModule module) : Components.SelfTargetedAOEs(module, AID.TorrentialTridentAOE, new AOEShapeCircle(18), 5);
class Stormwhorl(BossModule module) : Components.LocationTargetedAOEs(module, AID.Stormwhorl, 6);
class Stormwinds(BossModule module) : Components.SpreadFromCastTargets(module, AID.Stormwinds, 6);
class Maelstrom(BossModule module) : Components.LocationTargetedAOEs(module, AID.Maelstrom, 6);
class Godsbane(BossModule module) : Components.CastCounter(module, AID.GodsbaneAOE);
class DeepDiveHardWater(BossModule module) : Components.StackWithCastTargets(module, AID.DeepDiveHardWater, 6);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11299, SortOrder = 3)]
public class A32Llymlaen(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, DefaultBounds)
{
    public const float CorridorHalfLength = 40;
    public static readonly WPos DefaultCenter = new(0, -900);
    public static readonly ArenaBoundsRect DefaultBounds = new(19, 29);
    public static readonly ArenaBoundsCustom EastCorridorBounds = BuildCorridorBounds(+1);
    public static readonly ArenaBoundsCustom WestCorridorBounds = BuildCorridorBounds(-1);

    public static ArenaBoundsCustom BuildCorridorBounds(float dx)
    {
        var corridor = new PolygonClipper.Operand(CurveApprox.Rect(DefaultBounds.Orientation, CorridorHalfLength, 10));
        var standard = new PolygonClipper.Operand(CurveApprox.Rect(DefaultBounds.Orientation, DefaultBounds.HalfWidth, DefaultBounds.HalfHeight).Select(o => new WDir(o.X - dx * CorridorHalfLength, o.Z)));
        return new(CorridorHalfLength, DefaultBounds.Clipper.Union(corridor, standard));
    }
}
