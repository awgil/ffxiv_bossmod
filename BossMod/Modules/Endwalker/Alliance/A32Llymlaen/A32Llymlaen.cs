namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class Godsbane() : Components.RaidwideCast(ActionID.MakeSpell(AID.Godsbane), "Raidwide + DoT");
class Tempest() : Components.RaidwideCast(ActionID.MakeSpell(AID.Tempest));
class RightStrait() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.RightStraitCone), new AOEShapeCone(60, 90.Degrees()));
class LeftStrait() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.LeftStraitCone), new AOEShapeCone(60, 90.Degrees()));
class Stormwhorl() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.Stormwhorl), 6);
class Maelstrom() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.Maelstrom), 6);
class WindRose() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.WindRose), new AOEShapeCircle(12));
class SeafoamSpiral() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.SeafoamSpiral), new AOEShapeDonut(6, 70));
class DeepDive1() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.DeepDiveStack1), 6);
class DeepDive2() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.DeepDiveStack2), 6);
class HardWater1() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.HardWaterStack1), 6);
class HardWater2() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.HardWaterStack2), 6);
class Stormwinds() : Components.SpreadFromCastTargets(ActionID.MakeSpell(AID.StormwindsSpread), 6);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11299)]
public class A32Llymlaen : BossModule
{
    public A32Llymlaen(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, -900), 19, 29)) { }
}
