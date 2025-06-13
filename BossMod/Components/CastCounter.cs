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

public class DebugCasts(BossModule module, Enum[] aids, AOEShape shape, float expireAfter = 30, uint color = 0) : CastCounterMulti(module, aids)
{
    private readonly List<(WPos Source, Angle Direction, DateTime Timestamp)> _casts = [];
    public float ExpireAfter = expireAfter;
    public uint Color = color == 0 ? ArenaColor.Object : color;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
        {
            NumCasts++;
            _casts.Add((spell.TargetXZ.AlmostEqual(default, 1) ? caster.Position : spell.TargetXZ, spell.Rotation, WorldState.CurrentTime));
        }
    }

    public override void Update()
    {
        if (ExpireAfter < float.MaxValue)
            _casts.RemoveAll(c => c.Timestamp.AddSeconds(ExpireAfter) < WorldState.CurrentTime);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_casts.Count > 0)
            hints.Add($"Casts of {string.Join(", ", WatchedActions)}: {string.Join(", ", _casts)}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var c in _casts)
            shape.Outline(Arena, c.Source, c.Direction, Color);
    }
}
