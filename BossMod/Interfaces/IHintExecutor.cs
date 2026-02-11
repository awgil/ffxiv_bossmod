namespace BossMod.Interfaces;

internal interface IHintExecutor
{
    public delegate IHintExecutor Factory(WorldState ws, AIHints hints);

    public void Execute();
}
