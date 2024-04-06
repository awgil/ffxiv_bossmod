namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class Godsbane : Components.RaidwideCast
{
    public Godsbane() : base(ActionID.MakeSpell(AID.Godsbane), "Raidwide + DoT") { }
}

class Tempest : Components.RaidwideCast
{
    public Tempest() : base(ActionID.MakeSpell(AID.Tempest)) { }
}

class RightStrait : Components.SelfTargetedAOEs
{
    public RightStrait() : base(ActionID.MakeSpell(AID.RightStraitCone), new AOEShapeCone(60, 90.Degrees())) { }
}

class LeftStrait : Components.SelfTargetedAOEs
{
    public LeftStrait() : base(ActionID.MakeSpell(AID.LeftStraitCone), new AOEShapeCone(60, 90.Degrees())) { }
}

class Stormwhorl : Components.LocationTargetedAOEs
{
    public Stormwhorl() : base(ActionID.MakeSpell(AID.Stormwhorl), 6) { }
}

class Maelstrom : Components.LocationTargetedAOEs
{
    public Maelstrom() : base(ActionID.MakeSpell(AID.Maelstrom), 6) { }
}

class WindRose : Components.SelfTargetedAOEs
{
    public WindRose() : base(ActionID.MakeSpell(AID.WindRose), new AOEShapeCircle(12)) { }
}

class SeafoamSpiral : Components.SelfTargetedAOEs
{
    public SeafoamSpiral() : base(ActionID.MakeSpell(AID.SeafoamSpiral), new AOEShapeDonut(6, 70)) { }
}

class DeepDive1 : Components.StackWithCastTargets
{
    public DeepDive1() : base(ActionID.MakeSpell(AID.DeepDiveStack1), 6) { }
}

class DeepDive2 : Components.StackWithCastTargets
{
    public DeepDive2() : base(ActionID.MakeSpell(AID.DeepDiveStack2), 6) { }
}

class HardWater1 : Components.StackWithCastTargets
{
    public HardWater1() : base(ActionID.MakeSpell(AID.HardWaterStack1), 6) { }
}

class HardWater2 : Components.StackWithCastTargets
{
    public HardWater2() : base(ActionID.MakeSpell(AID.HardWaterStack2), 6) { }
}

class Stormwinds : Components.SpreadFromCastTargets
{
    public Stormwinds() : base(ActionID.MakeSpell(AID.StormwindsSpread), 6) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11299)]
public class A32Llymlaen : BossModule
{
    public A32Llymlaen(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, -900), 19, 29)) { }
}
