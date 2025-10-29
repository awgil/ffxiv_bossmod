namespace BossMod.Autorotation.MiscAI;

public sealed class Multibox(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Leader }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Multibox functionality for linked clients", "", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, Order: RotationModuleOrder.HighLevel);

        def.DefineInt(Track.Leader, "Leader");

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var leaderId = (ulong)strategy.GetInt(Track.Leader);
        var leaderSlot = Array.FindIndex(World.Party.Members, m => m.ContentId == leaderId);
        var leader = World.Party[leaderSlot];
        if (leader == null)
            return;

        Hints.ForcedFocusTarget = leader;
        Hints.ForcedTarget = World.Actors.Find(leader.TargetID);
    }
}
