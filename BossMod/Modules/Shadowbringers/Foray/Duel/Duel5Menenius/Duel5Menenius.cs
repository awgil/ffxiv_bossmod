namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class SpiralScourge : Components.SingleTargetCast
{
    public SpiralScourge() : base(ActionID.MakeSpell(AID.SpiralScourge), "Use Manawall, Excellence, or Invuln.") { }
}

class CallousCrossfire : Components.SingleTargetCast
{
    public CallousCrossfire() : base(ActionID.MakeSpell(AID.CallousCrossfire), "Use Light Curtain / Reflect.") { }
}

class ReactiveMunition : Components.StayMove
{
    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb)
        {
            if (module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.Stay;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb)
        {
            if (module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.None;
        }
    }
}

class SenseWeakness : Components.StayMove
{
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SenseWeakness)
        {
            if (module.Raid.FindSlot(caster.TargetID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.Move;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SenseWeakness)
        {
            if (module.Raid.FindSlot(caster.TargetID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.None;
        }
    }
}

class MagitekImpetus : Components.StatusDrivenForcedMarch
{
    public MagitekImpetus() : base(3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
    {
        ActivationLimit = 1;
    }
}

class ProactiveMunition : Components.StandardChasingAOEs
{
    public ProactiveMunition() : base(new AOEShapeCircle(6), ActionID.MakeSpell(AID.ProactiveMunitionTrackingStart), ActionID.MakeSpell(AID.ProactiveMunitionTrackingMove), 6, 1, 5) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "SourP", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 23)] // bnpcname=9695
public class Duel5Menenius : BossModule
{
    public Duel5Menenius(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-810, 520 /*y=260.3*/), 20)) { }
}
