namespace BossMod.Components;

// generic component used for drawing adds
public class Adds(BossModule module, uint oid, int priority = 0, bool forbidDots = false) : BossComponent(module)
{
    public readonly IReadOnlyList<Actor> Actors = module.Enemies(oid);
    public IEnumerable<Actor> ActiveActors => Actors.Where(a => a.IsTargetable && !a.IsDead);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (priority >= 0)
            foreach (var target in hints.PotentialTargets.Where(t => t.Actor.OID == oid))
            {
                target.Priority = priority;
                if (forbidDots)
                    target.ForbidDOTs = true;
            }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Actors, ArenaColor.Enemy);
    }
}

// component for adds that shouldn't be targeted at all, but should still be drawn
public class AddsPointless(BossModule module, uint oid) : Adds(module, oid)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var act in ActiveActors)
            hints.SetPriority(act, AIHints.Enemy.PriorityPointless);
    }
}

// generic component used for drawing multiple adds with multiple oids, when it's not useful to distinguish between them
public class AddsMulti(BossModule module, uint[] oids, int priority = 0) : BossComponent(module)
{
    public readonly uint[] OIDs = oids;
    public readonly List<Actor> Actors = [];
    public IEnumerable<Actor> ActiveActors => Actors.Where(a => a.IsTargetable && !a.IsDead);

    public AddsMulti(BossModule module, Enum[] oids, int priority = 0) : this(module, oids.Select(s => (uint)(object)s).ToArray(), priority) { }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (priority >= 0)
            hints.PrioritizeTargetsByOID(OIDs, priority);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Actors, ArenaColor.Enemy);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (OIDs.Contains(actor.OID))
            Actors.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        Actors.Remove(actor);
    }
}
