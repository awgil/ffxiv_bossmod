namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class StormwhorlLocAOE : Components.LocationTargetedAOEs
{
    public StormwhorlLocAOE() : base(ActionID.MakeSpell(AID.StormwhorlLocAOE), 6) { }
}
class MaelstromLocAOE : Components.LocationTargetedAOEs
{
    public MaelstromLocAOE() : base(ActionID.MakeSpell(AID.MaelstromLocAOE), 6) { }
}

class WindRoseAOE : Components.SelfTargetedAOEs
{
    public WindRoseAOE() : base(ActionID.MakeSpell(AID.WindRoseAOE), new AOEShapeCircle(12)) { }
}

class SurgingWaveAOE : Components.SelfTargetedAOEs
{
    public SurgingWaveAOE() : base(ActionID.MakeSpell(AID.SurgingWaveAOE), new AOEShapeCircle(6)) { }
}

class LandingAOE : Components.SelfTargetedAOEs
{
    public LandingAOE() : base(ActionID.MakeSpell(AID.LandingAOE), new AOEShapeCircle(18), 4) { }
}

class SeafoamSpiralDonut : Components.SelfTargetedAOEs
{
    public SeafoamSpiralDonut() : base(ActionID.MakeSpell(AID.SeafoamSpiralDonut), new AOEShapeDonut(6, 70)) { }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11299)]
public class A32Llymlaen : BossModule
{
    public A32Llymlaen(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, -900), 19, 29)) { }
}
