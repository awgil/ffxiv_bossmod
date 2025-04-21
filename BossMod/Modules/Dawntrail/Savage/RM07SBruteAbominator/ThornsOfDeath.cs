namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class ThornsOfDeath(BossModule module) : BossComponent(module)
{
    public readonly List<(Actor, Actor)> Tethers = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.ThornsOfDeathTank or TetherID.ThornsOfDeath or TetherID.ThornsOfDeathPre)
            Tethers.Add((source, WorldState.Actors.Find(tether.Target)!));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.ThornsOfDeathTank or TetherID.ThornsOfDeath or TetherID.ThornsOfDeathPre)
            Tethers.RemoveAll(t => t.Item1 == source && t.Item2.InstanceID == tether.Target);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (src, tar) in Tethers)
            Arena.AddLine(src.Position, tar.Position, ArenaColor.Danger);
    }
}
