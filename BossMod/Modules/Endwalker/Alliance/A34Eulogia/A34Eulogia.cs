namespace BossMod.Endwalker.Alliance.A34Eulogia;

class EudaimonEorzea(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EudaimonEorzea2), "Raidwide x13");
class TheWhorl(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TheWhorl));
class DawnOfTime(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DawnOfTime));
class ClimbingShotRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ClimbingShotVisual), "Raidwide + Knockback");
class ClimbingShotRaidwide2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ClimbingShotVisual2), "Raidwide + Knockback");
class ClimbingShotRaidwide3(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ClimbingShotVisual2), "Raidwide + Knockback");
class ClimbingShotRaidwide4(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ClimbingShotVisual), "Raidwide + Knockback");
class AsAboveSoBelow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AsAboveSoBelow));
class AsAboveSoBelow2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AsAboveSoBelow2));
class SoaringMinuet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees()));
class HandOfTheDestroyerJudgment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HandOfTheDestroyerJudgmentAOE), new AOEShapeRect(90, 20));
class HandOfTheDestroyerWrath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HandOfTheDestroyerWrathAOE), new AOEShapeRect(90, 20));
class DestructiveBoltStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DestructiveBoltStack), 6);

class Sunbeam(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SunbeamTankBuster), new AOEShapeCircle(6), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}


[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11301, SortOrder = 7)]
public class A34Eulogia(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(945, -945), 35));
