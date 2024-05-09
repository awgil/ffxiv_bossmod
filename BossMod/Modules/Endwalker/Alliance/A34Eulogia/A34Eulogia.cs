namespace BossMod.Endwalker.Alliance.A34Eulogia;

class Sunbeam(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SunbeamAOE), new AOEShapeCircle(6), true);
class DestructiveBolt(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DestructiveBoltAOE), 6, 8);
class HandOfTheDestroyerWrath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HandOfTheDestroyerWrathAOE), new AOEShapeRect(90, 20));
class HandOfTheDestroyerJudgment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HandOfTheDestroyerJudgmentAOE), new AOEShapeRect(90, 20));
class SoaringMinuet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees()));
class EudaimonEorzea(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.EudaimonEorzeaAOE));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11301, SortOrder = 7)]
public class A34Eulogia(WorldState ws, Actor primary) : BossModule(ws, primary, new(945, -945), DefaultBounds)
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(30);
    public static readonly ArenaBoundsSquare HieroglyphikaBounds = new(24);
}
