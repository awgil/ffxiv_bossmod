namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class BiscuitMaker(BossModule module) : Components.TankSwap(module, AID.BiscuitMaker, AID.BiscuitMaker, AID.BiscuitMakerSecond, 2, null, true);
class QuadrupleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, AID.QuadrupleSwipeBossAOE, 4, 2);
class DoubleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, AID.DoubleSwipeBossAOE, 5, 4);
class QuadrupleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, AID.QuadrupleSwipeShadeAOE, 4, 2);
class DoubleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, AID.DoubleSwipeShadeAOE, 5, 4);
class Nailchipper(BossModule module) : Components.SpreadFromCastTargets(module, AID.NailchipperAOE, 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 986, NameID = 12686, PlanLevel = 100)]
public class RM01SBlackCat(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
