namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class SpiralScourge(BossModule module) : Components.SingleTargetCast(module, AID.SpiralScourge, "Use Manawall, Excellence, or Invuln.");
class CallousCrossfire(BossModule module) : Components.SingleTargetCast(module, AID.CallousCrossfire, "Use Light Curtain / Reflect.");

class ReactiveMunition(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb)
            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}

class SenseWeakness(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SenseWeakness)
            SetState(Raid.FindSlot(spell.TargetID), new(Requirement.Move, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SenseWeakness)
            ClearState(Raid.FindSlot(spell.MainTargetID));
    }
}

class MagitekImpetus(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 1);
class ProactiveMunition(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), AID.ProactiveMunitionTrackingStart, AID.ProactiveMunitionTrackingMove, 6, 1, 5);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "SourP", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 23)] // bnpcname=9695
public class Duel5Menenius(WorldState ws, Actor primary) : BossModule(ws, primary, new(-810, 520 /*y=260.3*/), new ArenaBoundsSquare(20));
