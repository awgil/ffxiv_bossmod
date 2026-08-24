using BossMod;
using BossMod.Autorotation;

namespace ExampleLib;

public sealed class ExampleRotation(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Sample", "Sample rotation, dynamically loaded!", "Misc", "xan", RotationModuleQuality.Ok, new(~0ul), 1000, 1);
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Hints.ForcedMovement = Player.Rotation.ToDirection().OrthoL().ToVec3();
    }
}

public enum OID : uint
{
    // Boss = 0x385, // normal striking dummy
    Boss = 0x2DE0, // explorer mode dummy
    // Boss = 0x41CD, // L100 trial
}

class StrikingDummyStates : StateMachineBuilder
{
    public StrikingDummyStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(PlanLevel = 100, Category = BossModuleInfo.Category.Uncategorized, Expansion = BossModuleInfo.Expansion.Global)]
public class StrikingDummy(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position, new ArenaBoundsCircle(10))
{
    public override bool CheckReset() => !PrimaryActor.InCombat;
}
