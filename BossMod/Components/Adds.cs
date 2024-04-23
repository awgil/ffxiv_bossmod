namespace BossMod.Components;

// generic component used for drawing adds
public class Adds(BossModule module, uint oid) : BossComponent(module)
{
    public readonly IReadOnlyList<Actor> Actors = module.Enemies(oid);
    public IEnumerable<Actor> ActiveActors => Actors.Where(a => a.IsTargetable && !a.IsDead);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Actors, ArenaColor.Enemy);
    }
}
