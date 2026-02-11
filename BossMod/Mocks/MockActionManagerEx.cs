using BossMod.Interfaces;

namespace BossMod.Mocks;

public sealed class MockActionManagerEx : IAmex
{
    public ActionTweaksConfig Config => Service.Config.Get<ActionTweaksConfig>();

    public float AnimationLockDelayEstimate => 0.025f;
    public float EffectiveAnimationLock => 0;
    public bool MoveMightInterruptCast => false;
    public float ComboTimeLeft => 0;

    public Event<ClientActionRequest> ActionRequestExecuted => new();
    public Event<ulong, ActorCastEvent> ActionEffectReceived => new();

    public void Dispose() { }
    public void FinishActionGather() { }

    public void GetCooldowns(Span<Cooldown> cooldowns) => cooldowns.Clear();
    public ClientState.DutyAction[] GetDutyActions() => Utils.MakeArray<ClientState.DutyAction>(5, default);

    public void QueueManualActions() { }
}
