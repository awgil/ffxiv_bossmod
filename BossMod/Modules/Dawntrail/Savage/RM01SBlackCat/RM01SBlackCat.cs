namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class BiscuitMaker(BossModule module) : Components.TankSwap(module, ActionID.MakeSpell(AID.BiscuitMaker), ActionID.MakeSpell(AID.BiscuitMaker), ActionID.MakeSpell(AID.BiscuitMakerSecond), 2, null, true);
class QuadrupleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.QuadrupleSwipeBossAOE), 4, 2);
class DoubleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DoubleSwipeBossAOE), 5, 4);
class QuadrupleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.QuadrupleSwipeShadeAOE), 4, 2);
class DoubleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DoubleSwipeShadeAOE), 5, 4);

public class RM01SBlackCat(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
