namespace BossMod;

// Preserving character facing direction tweak.
// When any action is executed, character is automatically rotated to face the target (this can be disabled in-game, but it would simply block an action if not facing target instead).
// This makes maintaining uptime during gaze mechanics unnecessarily complicated (requiring either moving or rotating mouse back-and-forth in non-legacy camera mode).
// This feature remembers original rotation before executing an action and then attempts to restore it.
// Just like any 'manual' way, it is not 100% reliable:
// * client rate-limits rotation updates, so even for instant casts there is a short window of time (~0.1s) following action execution when character faces a target on server
// * for movement-affecting abilities (jumps, charges, etc) rotation can't be restored until animation ends
// * for casted abilities, rotation isn't restored until slidecast window starts, as otherwise cast is interrupted
public sealed class RestoreRotationTweak
{
    private readonly ActionTweaksConfig _config = Service.Config.Get<ActionTweaksConfig>();
    private Angle _modified; // rotation immediately after action execution; as long as it's not unchanged, we'll try restoring (otherwise we assume player changed facing manually and abort)
    private Angle _original; // rotation immediately before action execution; this is what we're trying to restore
    private int _numRetries; // for some reason, sometimes after successfully restoring rotation it is snapped back on next frame; in this case we retry again - TODO investigate why this happens
    private bool _pending;

    public void Preserve(Angle original, Angle modified)
    {
        if (_config.RestoreRotation && original != modified)
        {
            _modified = modified;
            _original = original;
            _numRetries = 2;
            _pending = true;
            //Service.Log($"[RRT] Restore start: {modified.Rad} -> {original.Rad}");
        }
    }

    public Angle? TryRestore(Angle current)
    {
        //Service.Log($"[RRT] Restore rotation: {current.Rad}: {_modified.Rad}->{_original.Rad}");
        if (!_pending)
            return null; // we don't have any pending rotation to restore

        if (_modified.AlmostEqual(current, 0.01f))
            return _original; // we still have the 'post' rotation, try restoring

        if (--_numRetries == 0)
            _pending = false; // we have unexpected rotation and we're out of retries, stop trying
        return null;
    }
}
