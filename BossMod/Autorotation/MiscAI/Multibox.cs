using BossMod.AI;

namespace BossMod.Autorotation.MiscAI;

public sealed class Multibox(RotationModuleManager manager, Actor player) : TypedRotationModule<Multibox.Strategy>(manager, player)
{
    readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();

    public enum Flag { Disabled, Enabled }

    public struct Strategy
    {
        [Number(DisplayName = "Follow slot", MinValue = 0, MaxValue = 7)]
        public Track<long> Master;
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Multibox utilities", "", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, Order: RotationModuleOrder.HighLevel);

        return def.WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var masterSlot = strategy.Master;
        var master = masterSlot > 0 ? World.Party[(int)masterSlot.Value] : null;
        if (master == null || master == Player)
            return;

        if (_aiConfig.FocusTargetMaster)
            Hints.ForcedFocusTarget = master;

        if (Bossmods.ActiveModule == null || _aiConfig.FollowDuringBoss)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(master, _aiConfig.DistanceToMaster, 0.2f));
    }
}
