#if DEBUG
namespace BossMod.Autorotation.MiscAI;

public sealed class Multibox(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Multibox functionality for linked clients", "", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, Order: RotationModuleOrder.HighLevel);

        def.Configs.Add(new StrategyConfigInt("Leader", "Leader (Content ID)", 0, 0, 0));

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var leaderId = (ulong)((StrategyValueInt)strategy.Values[0]).Value;
        var leaderSlot = Array.FindIndex(World.Party.Members, m => m.ContentId == leaderId);
        var leader = World.Party[leaderSlot];
        if (leader == null)
            return;

        Hints.ForcedFocusTarget = leader;
        Hints.ForcedTarget = World.Actors.Find(leader.TargetID);
    }
}
#endif
