namespace BossMod.Interfaces;

public unsafe interface IAmex : IDisposable
{
    public delegate IAmex Factory(WorldState ws, AIHints hints);

    public ActionID CastSpell { get; }
    public ActionID CastAction { get; }
    public ActionID QueuedAction { get; }

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

    public Vector3? GetWorldPosUnderCursor();
    public void FaceDirection(Angle direction);
    public uint GetActionStatus(ActionID action, ulong target, bool checkRecastActive = true, bool checkCastingActive = true, uint* outOptExtraInfo = null);
}
