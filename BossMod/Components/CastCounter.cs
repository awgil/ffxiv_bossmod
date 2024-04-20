namespace BossMod.Components;

// generic component that counts specified casts
public class CastCounter(BossModule module, ActionID aid) : BossComponent(module)
{
    public ActionID WatchedAction { get; private set; } = aid;
    public int NumCasts { get; protected set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            ++NumCasts;
    }
}
