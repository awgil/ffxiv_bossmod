namespace BossMod.Components;

// generic component that counts specified casts
public class CastCounter(BossModule module, Enum? aid) : BossComponent(module)
{
    public ActionID WatchedAction { get; private set; } = ActionID.MakeSpell(aid);
    public int NumCasts { get; protected set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            ++NumCasts;
    }
}

public class CastCounterMulti(BossModule module, Enum[] aids) : BossComponent(module)
{
    public ActionID[] WatchedActions = [.. aids.Select(ActionID.MakeSpell)];
    public int NumCasts { get; protected set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
            ++NumCasts;
    }
}
