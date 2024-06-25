namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class DividingWings(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(60, 60.Degrees()), (uint)TetherID.DividingWings, ActionID.MakeSpell(AID.DividingWingsAOE));
class PandaemonsHoly(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PandaemonsHoly), new AOEShapeCircle(36));

// note: origin seems to be weird?
class CirclesOfPandaemonium(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CirclesOfPandaemonium), new AOEShapeDonut(12, 40))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, new(100, 85), default, c.CastInfo!.NPCFinishAt, Color, Risky));
}

class Imprisonment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ImprisonmentAOE), new AOEShapeCircle(4));
class Cannonspawn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CannonspawnAOE), new AOEShapeDonut(3, 8));
class PealOfDamnation(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PealOfDamnation), new AOEShapeRect(50, 3.5f));
class PandaemoniacPillars(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.Bury), 2);
class Touchdown(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TouchdownAOE), new AOEShapeCircle(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 939, NameID = 12354, PlanLevel = 90)]
public class P10SPandaemonium(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Rectangle(new(100, 100), 13, 15), new Rectangle(new(125, 85), 4, 15), new Rectangle(new(75, 85), 4, 15)];
    private static readonly List<Shape> BridgeL = [new Rectangle(new(83, 92.5f), 4, 1)];
    private static readonly List<Shape> BridgeR = [new Rectangle(new(117, 92.5f), 4, 1)];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union);
    public static readonly ArenaBounds arenaL = new ArenaBoundsComplex(union.Concat(BridgeL));
    public static readonly ArenaBounds arenaR = new ArenaBoundsComplex(union.Concat(BridgeR));
    public static readonly ArenaBounds arenaLR = new ArenaBoundsComplex(union.Concat(BridgeL).Concat(BridgeR));
}
