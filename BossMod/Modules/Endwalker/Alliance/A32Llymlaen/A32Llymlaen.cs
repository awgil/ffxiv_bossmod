namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class Godsbane(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Godsbane), "Raidwide + DoT");
class Tempest(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Tempest));
class RightStrait(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightStraitCone), new AOEShapeCone(60, 90.Degrees()));
class LeftStrait(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftStraitCone), new AOEShapeCone(60, 90.Degrees()));
class Stormwhorl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Stormwhorl), 6);
class Maelstrom(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Maelstrom), 6);
class WindRose(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindRose), new AOEShapeCircle(12));
class SeafoamSpiral(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SeafoamSpiral), new AOEShapeDonut(6, 70));
class DeepDive1(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DeepDiveStack1), 6);
class DeepDive2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DeepDiveStack2), 6);
class HardWater1(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HardWaterStack1), 6);
class HardWater2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HardWaterStack2), 6);
class Stormwinds(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.StormwindsSpread), 6);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11299, SortOrder = 3)]
public class A32Llymlaen(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsRect(new(0, -900), 19, 29));
