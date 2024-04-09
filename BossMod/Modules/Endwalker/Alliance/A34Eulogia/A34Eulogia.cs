namespace BossMod.Endwalker.Alliance.A34Eulogia;

class EudaimonEorzea(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EudaimonEorzea2), "Raidwide x12");
class TheWhorl(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TheWhorl));
class DawnOfTime(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DawnOfTime));
class SoaringMinuet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees()));
class HandOfTheDestroyerJudgment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HandOfTheDestroyerJudgmentAOE), new AOEShapeRect(90, 20));
class HandOfTheDestroyerWrath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HandOfTheDestroyerWrathAOE), new AOEShapeRect(90, 20));

class Sunbeam(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SunbeamTankBuster), new AOEShapeCircle(6), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class DestructiveBoltStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DestructiveBoltStack), 6);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11301, SortOrder = 7)]
public class A34Eulogia(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(945, -945), 35));
