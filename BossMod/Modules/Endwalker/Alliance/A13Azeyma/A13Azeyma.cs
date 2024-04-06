namespace BossMod.Endwalker.Alliance.A13Azeyma;

class WardensWarmth : Components.SpreadFromCastTargets
{
    public WardensWarmth() : base(ActionID.MakeSpell(AID.WardensWarmthAOE), 6) { }
}

class FleetingSpark : Components.SelfTargetedAOEs
{
    public FleetingSpark() : base(ActionID.MakeSpell(AID.FleetingSpark), new AOEShapeCone(60, 135.Degrees())) { }
}

class SolarFold : Components.SelfTargetedAOEs
{
    public SolarFold() : base(ActionID.MakeSpell(AID.SolarFoldAOE), new AOEShapeCross(30, 5)) { }
}

class Sunbeam : Components.SelfTargetedAOEs
{
    public Sunbeam() : base(ActionID.MakeSpell(AID.Sunbeam), new AOEShapeCircle(9), 14) { }
}

class SublimeSunset : Components.LocationTargetedAOEs
{
    public SublimeSunset() : base(ActionID.MakeSpell(AID.SublimeSunsetAOE), 40) { } // TODO: check falloff
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11277, SortOrder = 5)]
public class A13Azeyma : BossModule
{
    public A13Azeyma(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-750, -750), 30)) { }
}
