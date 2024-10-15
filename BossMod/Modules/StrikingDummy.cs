#if false
using BossMod.Autorotation;

namespace BossMod.StrikingDummy;

public enum OID : uint
{
    Boss = 0x385, // normal striking dummy
    Boss = 0x41CD, // L100 trial
}

class StrikingDummyStates : StateMachineBuilder
{
    public StrikingDummyStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PlanLevel = 100)]
public class StrikingDummy(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

public sealed class StrikingDummyRotation(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Test }
    public enum Strategy { None, Some }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Custom dummy rotation", "Example encounter-specific rotation", "veyn", RotationModuleQuality.WIP, new(~1ul), 100, 1, typeof(StrikingDummy));
        res.Define(Track.Test).As<Strategy>("Test")
            .AddOption(Strategy.None, "None", "Do nothing")
            .AddOption(Strategy.Some, "Some", "I have some strategy and I follow it");
        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Option(Track.Test).As<Strategy>() == Strategy.Some && primaryTarget != null)
        {
            Hints.ForcedMovement = (primaryTarget.Position - Player.Position).OrthoL().ToVec3();
        }
    }
}
#endif
