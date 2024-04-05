namespace BossMod.Endwalker.Alliance.A36Eulogia;

class SoaringMinuet : Components.SelfTargetedAOEs
{
    public SoaringMinuet() : base(ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees())) { }
}

class HandOfTheDestroyerJudgment : Components.SelfTargetedAOEs
{
    public HandOfTheDestroyerJudgment() : base(ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeRect(90, 20)) { }
}

class HandOfTheDestroyerWrath : Components.SelfTargetedAOEs
{
    public HandOfTheDestroyerWrath() : base(ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeRect(90, 20)) { }
}


class FirstBlush1 : Components.SelfTargetedAOEs
{
    public FirstBlush1() : base(ActionID.MakeSpell(AID.FirstBlush1), new AOEShapeRect(120, 12.5f)) { }
}
class FirstBlush2 : Components.SelfTargetedAOEs
{
    public FirstBlush2() : base(ActionID.MakeSpell(AID.FirstBlush2), new AOEShapeRect(120, 12.5f)) { }
}
class FirstBlush3 : Components.SelfTargetedAOEs
{
    public FirstBlush3() : base(ActionID.MakeSpell(AID.FirstBlush3), new AOEShapeRect(120, 12.5f)) { }
}
class FirstBlush4 : Components.SelfTargetedAOEs
{
    public FirstBlush4() : base(ActionID.MakeSpell(AID.FirstBlush4), new AOEShapeRect(120, 12.5f)) { }
}

class ClimbingShot1 : Components.KnockbackFromCastTarget
{
    public ClimbingShot1() : base(ActionID.MakeSpell(AID.ClimbingShot1), 20) { }
}

class ClimbingShot2 : Components.KnockbackFromCastTarget
{
    public ClimbingShot2() : base(ActionID.MakeSpell(AID.ClimbingShot2), 20) { }
}

class SunbeamSelf : Components.BaitAwayCast
{
    public SunbeamSelf() : base(ActionID.MakeSpell(AID.SunbeamTankBuster), new AOEShapeCircle(6), true) { }
}

class DestructiveBoltStack : Components.UniformStackSpread
{
    public DestructiveBoltStack() : base(6, 0) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Stackmarker)
            AddStack(actor, module.WorldState.CurrentTime.AddSeconds(6.9f));
    }
    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DestructiveBoltStack)
            Stacks.Clear();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11301)]
public class A35Eulogia : BossModule
{
    public A35Eulogia(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(945, -945), 35)) { }
}