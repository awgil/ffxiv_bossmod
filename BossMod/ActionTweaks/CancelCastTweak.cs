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

        if (!force && !WantCancel())
            return false;

        _nextCancelAllowed = currentTime.AddSeconds(0.2f);
        return true;
    }

    private bool WantCancel()
    {
        if (!_config.CancelCastOnDeadTarget)
            return false;

        var cast = _ws.Party.Player()?.CastInfo;
        var target = _ws.Actors.Find(cast?.TargetID ?? 0);
        if (target == null)
            return false;

        var isRaise = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(cast?.Action.SpellId() ?? 0)?.Unknown24 == 1;
        if (!isRaise)
            return target.IsDead;

        // for raise spells, we want to cancel them if target becomes alive or gains 'raise' status
        return !target.IsDead || target.Statuses.Any(s => s.ID is 148 or 1140);
    }
}
