namespace BossMod.Endwalker.Alliance.A34Eulogia;

class EudaimonEorzea() : Components.RaidwideCast(ActionID.MakeSpell(AID.EudaimonEorzea2), "Raidwide x13");
class TheWhorl() : Components.RaidwideCast(ActionID.MakeSpell(AID.TheWhorl));
class DawnOfTime() : Components.RaidwideCast(ActionID.MakeSpell(AID.DawnOfTime));
class ClimbingShotRaidwide() : Components.RaidwideCast(ActionID.MakeSpell(AID.ClimbingShotVisual), "Raidwide + Knockback");
class ClimbingShotRaidwide2() : Components.RaidwideCast(ActionID.MakeSpell(AID.ClimbingShotVisual2), "Raidwide + Knockback");
class ClimbingShotRaidwide3() : Components.RaidwideCast(ActionID.MakeSpell(AID.ClimbingShotVisual2), "Raidwide + Knockback");
class ClimbingShotRaidwide4() : Components.RaidwideCast(ActionID.MakeSpell(AID.ClimbingShotVisual), "Raidwide + Knockback");
class AsAboveSoBelow() : Components.RaidwideCast(ActionID.MakeSpell(AID.AsAboveSoBelow));
class AsAboveSoBelow2() : Components.RaidwideCast(ActionID.MakeSpell(AID.AsAboveSoBelow2));
class SoaringMinuet() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees()));
class HandOfTheDestroyerJudgment() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.HandOfTheDestroyerJudgmentAOE), new AOEShapeRect(90, 20));
class HandOfTheDestroyerWrath() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.HandOfTheDestroyerWrathAOE), new AOEShapeRect(90, 20));
class DestructiveBoltStack() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.DestructiveBoltStack), 6);

class Sunbeam : Components.BaitAwayCast
{
    public Sunbeam() : base(ActionID.MakeSpell(AID.SunbeamTankBuster), new AOEShapeCircle(6), true) { }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11301)]
public class A36Eulogia(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(945, -945), 35));
