namespace BossMod.Autorotation.xan;

public static class RaiseUtil
{
    public enum Targets
    {
        [Option("Party members")]
        Party,
        [Option("Alliance raid members")]
        Alliance,
        [Option("Any dead player")]
        Everyone
    }

    public static IEnumerable<Actor> FindRaiseTargets(WorldState world, Targets targets, bool filterByRange = true, bool sortByClass = true)
    {
        var candidates = targets switch
        {
            Targets.Everyone => world.Actors.Where(x => x.Type is ActorType.Player or ActorType.DutySupport && x.IsAlly),
            Targets.Alliance => world.Party.WithoutSlot(includeDead: true, excludeAlliance: false, excludeNPCs: true),
            _ => world.Party.WithoutSlot(includeDead: true, excludeAlliance: true, excludeNPCs: true)
        };

        var t1 = candidates.Where(x => x.IsDead && !BeingRaised(x));
        if (filterByRange)
            t1 = t1.Where(t => world.Party.Player()?.DistanceToHitbox(t) <= 30);
        if (sortByClass)
            t1 = t1.OrderByDescending(t => t.Class.GetRole() switch
            {
                Role.Healer => 5,
                Role.Tank => 4,
                _ => t.Class is Class.RDM or Class.SMN or Class.ACN ? 3 : 2
            });

        return t1;
    }

    public static readonly uint[] RaiseStatus = [148, 1140, 2648];

    public static bool BeingRaised(Actor actor) => actor.Statuses.Any(s => RaiseStatus.Contains(s.ID)) || actor.PendingStatuses.Any(s => RaiseStatus.Contains(s.StatusId));
}
