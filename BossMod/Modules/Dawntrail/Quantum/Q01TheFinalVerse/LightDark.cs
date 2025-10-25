namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

[Flags]
public enum Color
{
    None,
    Dark,
    Light
}

public class ColorTime
{
    public Color Color;
    public DateTime Expire;
}

class LightDark(BossModule module) : BossComponent(module)
{
    private readonly ColorTime[] _playerStates = Utils.GenArray(4, () => new ColorTime());

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkVengeance && Raid.TryFindSlot(actor, out var slot))
        {
            _playerStates[slot].Color |= Color.Dark;
            _playerStates[slot].Expire = status.ExpireAt;
        }

        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot2))
        {
            _playerStates[slot2].Color |= Color.Light;
            _playerStates[slot2].Expire = status.ExpireAt;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkVengeance && Raid.TryFindSlot(actor, out var slot))
            _playerStates[slot].Color ^= Color.Dark;

        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot2))
            _playerStates[slot2].Color ^= Color.Light;
    }

    public Color GetColor(int slot) => _playerStates.BoundSafeAt(slot)?.Color ?? Color.None;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerStates[slot].Color != Color.None)
        {
            var timer = (_playerStates[slot].Expire - WorldState.CurrentTime).TotalSeconds;
            if (timer < 10)
                hints.Add($"Buff remaining: {timer:f1}s");
        }
    }
}

class BossLightDark(BossModule module) : Components.GenericInvincible(module)
{
    private readonly LightDark _lightDark = module.FindComponent<LightDark>()!;

    private Actor? Eater;

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        // personal preference, don't like seeing the radar turn red when i die
        if (actor.IsDead)
            yield break;

        var color = _lightDark.GetColor(slot);
        if (!color.HasFlag(Color.Dark) && Eater != null)
            yield return Eater;
        if (!color.HasFlag(Color.Light))
            yield return Module.PrimaryActor;
    }

    public override void Update()
    {
        Eater ??= Module.Enemies(OID.DevouredEater).FirstOrDefault();
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CalcHPDifference() is var (boss, pct) && pct < 25)
            hints.Add($"HP: {boss} +{pct:f1}%");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (CalcHPDifference() is var (boss, pct) && pct >= 25)
            hints.Add($"HP: {boss} +{pct:f1}%");
    }

    private (string Boss, float Percent)? CalcHPDifference()
    {
        if (Eater is { } e && Module.PrimaryActor.HPMP.CurHP > 1 && e.HPMP.CurHP > 1)
        {
            var ratioDiff = Module.PrimaryActor.HPRatio - e.HPRatio;
            return (ratioDiff > 0 ? "Eminent Grief" : "Devoured Eater", MathF.Abs(ratioDiff) * 100);
        }

        return null;
    }
}

class DrainAether(BossModule module) : Components.CastCounterMulti(module, [AID.DrainAetherLightFast, AID.DrainAetherLightSlow, AID.DrainAetherDarkFast, AID.DrainAetherDarkSlow])
{
    private readonly LightDark _ld = module.FindComponent<LightDark>()!;
    private readonly List<(Color, DateTime)> _colors = [];
    private Actor? _sinBearer;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SinBearer)
            _sinBearer = actor;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SinBearer)
            _sinBearer = null;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            // eminent grief
            case AID.DrainAetherLightFast:
            case AID.DrainAetherLightSlow:
                _colors.Add((Color.Light, Module.CastFinishAt(spell)));
                _colors.SortBy(c => c.Item2);
                break;
            case AID.DrainAetherDarkFast:
            case AID.DrainAetherDarkSlow:
                _colors.Add((Color.Dark, Module.CastFinishAt(spell)));
                _colors.SortBy(c => c.Item2);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_colors.Count > 0)
        {
            var nextColor = _colors[0].Item1;
            if (nextColor == Color.Light && actor == _sinBearer)
                hints.Add($"Stay dark!", false);
            else
                hints.Add($"Correct color: {nextColor}", !_ld.GetColor(slot).HasFlag(nextColor));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
        {
            NumCasts++;
            if (_colors.Count > 0)
                _colors.RemoveAt(0);
        }
    }
}
