namespace BossMod;

// Framerate-dependent cooldown reduction.
// Imagine game is running at exactly 100fps (10ms frame time), and action is queued when remaining cooldown is 5ms.
// On next frame (+10ms), cooldown will be reduced and clamped to 0, action will be executed and it's cooldown set to X ms - so next time it can be pressed at X+10 ms.
// If we were running with infinite fps, cooldown would be reduced to 0 and action would be executed slightly (5ms) earlier.
// We can't fix that easily, but at least we can fix the cooldown after action execution - so that next time it can be pressed at X+5ms.
// We do that by reducing actual cooldown by difference between previously-remaining cooldown and frame delta, if action is executed at first opportunity.
public sealed class CooldownDelayTweak
{
    private readonly ActionManagerConfig _config = Service.Config.Get<ActionManagerConfig>();

    public float Adjustment { get; private set; } // if >0 while using an action, cooldown/anim lock will be reduced by this amount as if action was used a bit in the past

    public void StartAdjustment(float prevAnimLock, float prevRemainingCooldown, float dt) => Adjustment = CalculateAdjustment(prevAnimLock, prevRemainingCooldown, dt);
    public void StopAdjustment() => Adjustment = 0;

    private float CalculateAdjustment(float prevAnimLock, float prevRemainingCooldown, float dt)
    {
        if (!_config.RemoveCooldownDelay)
            return 0; // tweak is disabled, so no adjustment

        var maxDelay = Math.Max(prevAnimLock, prevRemainingCooldown);
        if (maxDelay <= 0)
            return 0; // nothing prevented us from executing the action on previous frame, so no adjustment

        var overflow = dt - maxDelay; // both cooldown and animation lock should expire this much before current frame start
        return Math.Clamp(overflow, 0, 0.1f); // use upper limit for time adjustment (if you have dogshit fps, adjusting too much could be suspicious)
    }
}
