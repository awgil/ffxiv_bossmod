namespace BossMod.Interfaces;

public interface IAmex : IDisposable
{
    public delegate IAmex Factory(WorldState ws, AIHints hints);

    public ActionTweaksConfig Config { get; }
    public float AnimationLockDelayEstimate { get; }
    public float EffectiveAnimationLock { get; }
    public float ComboTimeLeft { get; }
    public bool MoveMightInterruptCast { get; }

    public Event<ClientActionRequest> ActionRequestExecuted { get; }
    public Event<ulong, ActorCastEvent> ActionEffectReceived { get; }

    public void QueueManualActions();
    public void FinishActionGather();
    public void GetCooldowns(Span<Cooldown> cooldowns);
    public ClientState.DutyAction[] GetDutyActions();
}
