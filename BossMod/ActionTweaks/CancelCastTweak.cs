namespace BossMod;

// Utility for automatically cancelling casts in some conditions (when target dies, when ai wants it, etc).
// Since the game API is sending a packet, this implements some rate limiting.
public sealed class CancelCastTweak(WorldState ws)
{
    private readonly ActionTweaksConfig _config = Service.Config.Get<ActionTweaksConfig>();
    private readonly WorldState _ws = ws;
    private DateTime _nextCancelAllowed;

    public bool ShouldCancel(DateTime currentTime, bool force)
    {
        if (currentTime < _nextCancelAllowed)
            return false;

        var cancel = force || _config.CancelCastOnDeadTarget && (_ws.Actors.Find(_ws.Party.Player()?.CastInfo?.TargetID ?? 0)?.IsDead ?? false);
        if (!cancel)
            return false;

        _nextCancelAllowed = currentTime.AddSeconds(0.2f);
        return true;
    }
}
