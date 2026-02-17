using BossMod.Interfaces;

namespace BossMod.Mocks;

public sealed class MockActionManagerEx(ActionTweaksConfig cfg) : IAmex
{
    public ActionTweaksConfig Config => cfg;

    public float AnimationLockDelayEstimate => 0.025f;
    public float EffectiveAnimationLock => 0;
    public bool MoveMightInterruptCast => false;
    public float ComboTimeLeft => 0;

    public Event<ClientActionRequest> ActionRequestExecuted => new();
    public Event<ulong, ActorCastEvent> ActionEffectReceived => new();

    public ActionID CastSpell => default;
    public ActionID CastAction => default;
    public ActionID QueuedAction => default;

    public void Dispose() { }
    public void FinishActionGather() { }
    public void QueueManualActions() { }

    public unsafe uint GetActionStatus(ActionID action, ulong target, bool checkRecastActive = true, bool checkCastingActive = true, uint* outOptExtraInfo = null) => 0;
    public void GetCooldowns(Span<Cooldown> cooldowns) => cooldowns.Clear();
    public ClientState.DutyAction[] GetDutyActions() => Utils.MakeArray<ClientState.DutyAction>(5, default);
    public Vector3? GetWorldPosUnderCursor() => null;
    public void FaceDirection(Angle direction) => throw new NotImplementedException();
}
