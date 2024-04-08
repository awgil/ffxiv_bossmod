namespace BossMod.Components;

// generic component used for drawing adds
public class Adds : BossComponent
{
    public readonly IReadOnlyList<Actor> Actors;
    public IEnumerable<Actor> ActiveActors => Actors.Where(a => a.IsTargetable && !a.IsDead);

    public Adds(BossModule module, uint oid) : base(module)
    {
        Actors = module.Enemies(oid);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Actors, ArenaColor.Enemy);
    }
}
