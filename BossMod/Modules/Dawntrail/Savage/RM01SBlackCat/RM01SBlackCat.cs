namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class BiscuitMaker(BossModule module) : Components.TankSwap(module, ActionID.MakeSpell(AID.BiscuitMaker), ActionID.MakeSpell(AID.BiscuitMaker), ActionID.MakeSpell(AID.BiscuitMakerSecond), 2, null, true);
class QuadrupleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.QuadrupleSwipeBossAOE), 4, 2);
class DoubleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DoubleSwipeBossAOE), 5, 4);
class QuadrupleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.QuadrupleSwipeShadeAOE), 4, 2);
class DoubleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DoubleSwipeShadeAOE), 5, 4);
class Nailchipper(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.NailchipperAOE), 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 986, NameID = 12686, PlanLevel = 100)]
public class RM01SBlackCat(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
