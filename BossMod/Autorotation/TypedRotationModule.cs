namespace BossMod.Autorotation;

interface IStrategy<T>
{
}

public struct Values<T>
{
    public static Values<T> Create(StrategyValues values)
    {

    }
}

public abstract class TypedRotationModule<TStrategy>(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    protected abstract void Execute(TStrategy.Values strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving);

    public sealed override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Execute(Values<TStrategy>.Create(strategy), ref primaryTarget, estimatedAnimLockDelay, isMoving);
    }
}

public sealed partial class MNKTyped(RotationModuleManager manager, Actor player) : TypedRotationModule<MNKTyped.Strategies>(manager, player)
{
    [Strategy]
    public partial struct Strategies : IStrategy<Strategies>
    {
        [Track] public Targeting Targeting;
        [Track] public AOEStrategy AOE;
    }

    protected override void Execute(Strategies strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) => throw new NotImplementedException();
}
