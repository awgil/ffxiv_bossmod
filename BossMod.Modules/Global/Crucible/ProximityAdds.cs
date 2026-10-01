namespace BossMod.Global.Crucible;

class ProximityAdds(BossModule module, Enum[] oids, int priority = 0) : Components.AddsMulti(module, oids, priority)
{
    public ProximityAdds(BossModule module, Enum oid, int priority = 0) : this(module, [oid], priority) { }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var target in hints.PotentialTargets.Where(t => OIDs.Contains(t.Actor.OID)))
        {
            target.Priority = Priority;
            if (target.Priority == Priority && actor.DistanceToHitbox(target.Actor) <= 3)
                target.ShouldBeTargeted = true;
        }
    }
}
