namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class FeintParticleBeam : Components.StandardChasingAOEs
{
    public FeintParticleBeam(BossModule module) : base(module, new AOEShapeCircle(3), AID.FeintParticleBeamAOEFirst, AID.FeintParticleBeamAOERest, 2.1f, 0.4f, 18)
    {
        ExcludedTargets = new(~0u);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FeintParticleBeam)
            ExcludedTargets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}
