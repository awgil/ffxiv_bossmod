namespace BossMod.Autorotation;

public sealed record class TestWAR(WorldState World, Actor Player, AIHints Hints) : RotationModule(World, Player, Hints)
{
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("WAR test", "heavy swing ftw", BitMask.Build((int)Class.WAR), 90);
        return res;
    }

    public override void Execute(ReadOnlySpan<StrategyValue> strategy, Actor? primaryTarget, ActionQueue actions)
    {
        actions.Push(ActionID.MakeSpell(WAR.AID.HeavySwing), primaryTarget, ActionQueue.Priority.High + 500);
    }
}
