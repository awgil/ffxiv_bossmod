#if DEBUG
using BossMod.Autorotation;

namespace BossMod.StrikingDummy;

public enum OID : uint
{
    Boss = 0x385, // L100 trial
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
        var res = new RotationModuleDefinition("Custom dummy rotation", "Example encounter-specific rotation", "Other", "veyn", RotationModuleQuality.WIP, new(~1ul), 100, 1, typeof(StrikingDummy));
        res.Define(Track.Test).As<Strategy>("Test")
            .AddOption(Strategy.None, "None", "Do nothing")
            .AddOption(Strategy.Some, "Some", "I have some strategy and I follow it");
        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var cd = World.Client.CountdownRemaining;
        var boss = Bossmods.ActiveModule?.PrimaryActor;

        if (strategy.Option(Track.Test).As<Strategy>() == Strategy.Some && primaryTarget != null)
        {
            var dir = (primaryTarget.Position - Player.Position).OrthoL();
            Hints.GoalZones.Add(p => new AOEShapeRect(3, 1).Check(p, Player.Position + dir, Angle.FromDirection(dir)) ? 1 : 0);
        }
    }
}
#endif
