namespace BossMod.Endwalker.Alliance.A36Eulogia;

class EudaimonEorzea : Components.RaidwideCast
{
    public EudaimonEorzea() : base(ActionID.MakeSpell(AID.EudaimonEorzea2), "Raidwide x12") { }
}

class TheWhorl : Components.RaidwideCast
{
    public TheWhorl() : base(ActionID.MakeSpell(AID.TheWhorl)) { }
}

class DawnOfTime : Components.RaidwideCast
{
    public DawnOfTime() : base(ActionID.MakeSpell(AID.DawnOfTime)) { }
}

class SoaringMinuet : Components.SelfTargetedAOEs
{
    public SoaringMinuet() : base(ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees())) { }
}

class HandOfTheDestroyerJudgment : Components.SelfTargetedAOEs
{
    public HandOfTheDestroyerJudgment() : base(ActionID.MakeSpell(AID.HandOfTheDestroyerJudgmentAOE), new AOEShapeRect(90, 20)) { }
}

class HandOfTheDestroyerWrath : Components.SelfTargetedAOEs
{
    public HandOfTheDestroyerWrath() : base(ActionID.MakeSpell(AID.HandOfTheDestroyerWrathAOE), new AOEShapeRect(90, 20)) { }
}

class Sunbeam : Components.BaitAwayCast
{
    public Sunbeam() : base(ActionID.MakeSpell(AID.SunbeamTankBuster), new AOEShapeCircle(6), true) { }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class DestructiveBoltStack : Components.StackWithCastTargets
{
    public DestructiveBoltStack() : base(ActionID.MakeSpell(AID.DestructiveBoltStack), 6) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11301)]
public class A36Eulogia : BossModule
{
    public A36Eulogia(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(945, -945), 35)) { }
}
