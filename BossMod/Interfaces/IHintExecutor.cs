namespace BossMod.Interfaces;

internal interface IHintExecutor
{
    public delegate IHintExecutor Factory(WorldState world, AIHints hints);

    public void Execute();
}
