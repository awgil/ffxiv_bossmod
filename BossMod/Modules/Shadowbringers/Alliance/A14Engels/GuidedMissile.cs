namespace BossMod.Shadowbringers.Alliance.A14Engels;

class GuidedMissileBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Chase, centerAtTarget: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaitsOn(actor).FirstOrNull() is { } bait)
            hints.AddForbiddenZone(new AOEShapeRect(25, 25, 25), Arena.Center, activation: bait.Activation);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GuidedMissileFirst)
            CurrentBaits.Clear();
    }
}
class GuidedMissile(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), AID.GuidedMissileFirst, AID.GuidedMissileRest, 6, 1, 4);
