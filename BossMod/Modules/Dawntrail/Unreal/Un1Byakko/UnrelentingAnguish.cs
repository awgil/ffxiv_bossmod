namespace BossMod.Dawntrail.Unreal.Un1Byakko;

class UnrelentingAnguish(BossModule module) : Components.PersistentVoidzone(module, 2, m => m.Enemies(OID.AratamaForce).Where(z => !z.IsDead), 1);

// TODO: show something
class OminousWind(BossModule module) : BossComponent(module)
{
    public BitMask Targets;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.OminousWind)
            Targets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.OminousWind)
            Targets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class GaleForce(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Bombogenesis, ActionID.MakeSpell(AID.GaleForce), 8.1f, true);

class VacuumClaw(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.VacuumClaw), "GTFO from voidzone!")
{
    private readonly IReadOnlyList<Actor> _sources = module.Enemies(OID.VacuumClaw);

    private static readonly AOEShapeCircle _shape = new(10); // TODO: verify radius; initial voidzone is 6, but spell radius is 1 in sheets; after 5th hit we get 4 stacks of area-of-influence increase

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sources.Select(s => new AOEInstance(_shape, s.Position));
}

class VacuumBlade(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.VacuumBlade));
