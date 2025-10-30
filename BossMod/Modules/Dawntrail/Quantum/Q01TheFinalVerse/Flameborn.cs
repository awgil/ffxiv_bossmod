namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

class Flameborn(BossModule module) : Components.Adds(module, (uint)OID.Flameborn)
{
    public int NumSpawned;

    private readonly Dictionary<Actor, Actor> _tethers = [];
    private readonly Q01TheFinalVerseConfig.FlamebornAssignment _assignments = Service.Config.Get<Q01TheFinalVerseConfig>().FlamebornAssignments;

    private record class Add(Actor Actor, int Index, int Owner);

    private readonly List<Add> _untetheredAdds = [];

    private static readonly WPos[] _spawns = [new(-617, -312), new(-583, -312), new(-600, -300), new(-617, -288), new(-583, -288)];

    public override void OnTargetable(Actor actor)
    {
        if ((OID)actor.OID == OID.Flameborn)
        {
            NumSpawned++;
            var spawnIdx = Array.FindIndex(_spawns, s => actor.Position.AlmostEqual(s, 1));
            if (spawnIdx >= 0)
            {
                var assignedRole = _assignments.AsArray()[spawnIdx];
                var assignedSlot = Raid.WithSlot().WhereActor(a => a.Role == assignedRole).Select(a => a.Item1).DefaultIfEmpty(-1).First();

                _untetheredAdds.Add(new(actor, spawnIdx, assignedSlot));
            }
            else
            {
                ReportError($"Flameborn spawned at unexpected position: {actor.Position}");
                _untetheredAdds.Add(new(actor, -1, -1));
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.Flameborn && WorldState.Actors.Find(tether.Target) is { } tar)
        {
            _tethers[source] = tar;
            _untetheredAdds.RemoveAll(t => t.Actor == source);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        _tethers.Remove(source);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var add in ActiveActors)
        {
            var en = hints.FindEnemy(add);

            if (_untetheredAdds.FirstOrDefault(a => a.Actor == add) is { } t)
            {
                // add belongs to us, prioritize tethering it before anything else
                var prio = t.Owner == slot
                    ? 1
                    // add belongs to someone else, we must not attack it
                    : t.Owner != slot && t.Owner >= 0
                        ? AIHints.Enemy.PriorityForbidden
                        // no valid assignments, add is a normal target that we can't kill but can build gauge on
                        : AIHints.Enemy.PriorityPointless;
                en?.Priority = prio;
            }
            else
                en?.Priority = -1;
            en?.ForbidDOTs = true;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        foreach (var (a, b) in _tethers)
            Arena.AddLine(a.Position, b.Position, ArenaColor.Danger);

        foreach (var add in _untetheredAdds.Where(a => a.Owner == pcSlot))
            Arena.AddCircle(add.Actor.Position, 1.5f, ArenaColor.Safe);
    }
}

// TODO: need another recording to figure out AOE size
class FlamebornAura(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<Actor> Adds = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Adds.Select(a => new AOEInstance(new AOEShapeCircle(4 * (a.HitboxRadius / 2.6f)), a.Position));

    public override void OnTargetable(Actor actor)
    {
        if ((OID)actor.OID == OID.Flameborn)
            Adds.Add(actor);
    }

    public override void OnUntargetable(Actor actor)
    {
        Adds.Remove(actor);
    }
}

class SelfDestruct(BossModule module) : Components.RaidwideCast(module, AID.SelfDestruct);
