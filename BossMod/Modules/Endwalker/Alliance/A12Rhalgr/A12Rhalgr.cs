﻿namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class DestructiveBolt(BossModule module) : Components.SpreadFromCastTargets(module, AID.DestructiveBoltAOE, 3);
class StrikingMeteor(BossModule module) : Components.LocationTargetedAOEs(module, AID.StrikingMeteor, 6);
class BronzeLightning(BossModule module) : Components.SelfTargetedAOEs(module, AID.BronzeLightning, new AOEShapeCone(50, 22.5f.Degrees()), 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11273, SortOrder = 3)]
public class A12Rhalgr(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15, 275), new ArenaBoundsSquare(30)) // note: arena has a really complex shape...
{
    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.PathLineTo(new(2.5f, 245));
        Arena.PathLineTo(new(2.6f, 275));
        Arena.PathLineTo(new(7.3f, 295));
        Arena.PathLineTo(new(3, 297));
        Arena.PathLineTo(new(-1, 286));
        Arena.PathLineTo(new(-6.5f, 288));
        Arena.PathLineTo(new(-6.5f, 305));
        Arena.PathLineTo(new(-13, 305));
        Arena.PathLineTo(new(-13, 288));
        Arena.PathLineTo(new(-21.5f, 288));
        Arena.PathLineTo(new(-21.5f, 305));
        Arena.PathLineTo(new(-28, 305));
        Arena.PathLineTo(new(-28, 283));
        Arena.PathLineTo(new(-42, 300));
        Arena.PathLineTo(new(-45.5f, 297));
        Arena.PathLineTo(new(-34, 271.5f));
        Arena.PathLineTo(new(-37, 245));
        Arena.PathStroke(true, ArenaColor.Border);
    }
}
