namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class DividingWings(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(60, 60.Degrees()), (uint)TetherID.DividingWings, AID.DividingWingsAOE);
class PandaemonsHoly(BossModule module) : Components.StandardAOEs(module, AID.PandaemonsHoly, new AOEShapeCircle(36));

// note: origin seems to be weird?
class CirclesOfPandaemonium(BossModule module) : Components.StandardAOEs(module, AID.CirclesOfPandaemonium, new AOEShapeDonut(12, 40))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, new(Module.Center.X, Border.MainPlatformCenterZ - Border.MainPlatformHalfSize.Z), c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), Color, Risky));
}

class Imprisonment(BossModule module) : Components.StandardAOEs(module, AID.ImprisonmentAOE, new AOEShapeCircle(4));
class Cannonspawn(BossModule module) : Components.StandardAOEs(module, AID.CannonspawnAOE, new AOEShapeDonut(3, 8));
class PealOfDamnation(BossModule module) : Components.StandardAOEs(module, AID.PealOfDamnation, new AOEShapeRect(50, 3.5f));
class PandaemoniacPillars(BossModule module) : Components.CastTowers(module, AID.Bury, 2);
class Touchdown(BossModule module) : Components.StandardAOEs(module, AID.TouchdownAOE, new AOEShapeCircle(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 939, NameID = 12354, PlanLevel = 90)]
public class P10SPandaemonium(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 92.5f), new ArenaBoundsRect(30, 22.5f));
