namespace BossMod.RealmReborn.Alliance.A34CloudofDarkness;

class ZeroFormParticleBeam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ZeroFormParticleBeam), new AOEShapeRect(74, 12));
class ParticleBeam2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ParticleBeam2));

class FeintParticleBeam : Components.StandardChasingAOEs
{
    public FeintParticleBeam(BossModule module) : base(module, new AOEShapeCircle(10), ActionID.MakeSpell(AID.FeintParticleBeam1), ActionID.MakeSpell(AID.FeintParticleBeam2), 4, 1.5f, 5) //float moveDistance, float secondsBetweenActivations, int maxCasts
    {
        ExcludedTargets = Raid.WithSlot(true).Mask();
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FeintParticleBeam)
            ExcludedTargets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3240)]
public class A34CloudofDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -400), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.DarkCloud), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.DarkStorm), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.HyperchargedCloud), ArenaColor.Enemy);
    }
}
