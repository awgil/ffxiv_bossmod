namespace BossMod.Endwalker.Alliance.A35Eulogia;

class SoaringMinuet : Components.SelfTargetedAOEs
{
    public SoaringMinuet() : base(ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees())) { }
}
class LightningBolt : Components.SelfTargetedAOEs
{
    public LightningBolt() : base(ActionID.MakeSpell(AID.LightningBolt), new AOEShapeCircle(18)) { }
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

class ClimbingShotKnockback : Components.KnockbackFromCastTarget
{
    public ClimbingShotKnockback() : base(ActionID.MakeSpell(AID.ClimbingShot1), 20) { }
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