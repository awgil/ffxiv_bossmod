namespace BossMod.Services;

public interface IAmex : IDisposable
{
    public ActionTweaksConfig Config { get; }

    float EffectiveAnimationLock { get; }
    float AnimationLockDelayEstimate { get; }
    float ComboTimeLeft { get; }
    bool MoveMightInterruptCast { get; }

    void QueueManualActions();
    void FinishActionGather();
}

internal sealed class MockAmex : IAmex
{
    public ActionTweaksConfig Config { get; } = Service.Config.Get<ActionTweaksConfig>();

    public float EffectiveAnimationLock => 0;
    public float AnimationLockDelayEstimate => 0.02f;
    public float ComboTimeLeft => 0;
    public bool MoveMightInterruptCast => false;

    public void QueueManualActions() { }
    public void FinishActionGather() { }

    public void Dispose() { }
}
