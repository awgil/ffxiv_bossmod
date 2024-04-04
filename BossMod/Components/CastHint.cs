namespace BossMod.Components;

// generic component that is 'active' when any actor casts specific spell
public class CastHint : CastCounter
{
    public string Hint;
    public bool ShowCastTimeLeft; // if true, show cast time left until next instance
    private List<Actor> _casters = new();
    public IReadOnlyList<Actor> Casters => _casters;
    public bool Active => _casters.Count > 0;

    public CastHint(ActionID action, string hint, bool showCastTimeLeft = false) : base(action)
    {
        Hint = hint;
        ShowCastTimeLeft = showCastTimeLeft;
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (Active && Hint.Length > 0)
            hints.Add(ShowCastTimeLeft ? $"{Hint} {((Casters.First().CastInfo?.NPCFinishAt ?? module.WorldState.CurrentTime) - module.WorldState.CurrentTime).TotalSeconds:f1}s left" : Hint);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }
}

public class CastInterruptHint : CastHint
{
    public bool CanBeInterrupted { get; init; }
    public bool CanBeStunned { get; init; }
    public bool ShowNameInHint { get ; init; } // important if there are several targets
    public string HintExtra { get; init; }

    public CastInterruptHint(ActionID aid, bool canBeInterrupted = true, bool canBeStunned = false, string hintExtra = "", bool showNameInHint = false) : base(aid, "")
    {
        CanBeInterrupted = canBeInterrupted;
        CanBeStunned = canBeStunned;
        ShowNameInHint = showNameInHint;
        HintExtra = hintExtra;
        UpdateHint();
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            var e = hints.PotentialTargets.Find(e => e.Actor == c);
            if (e != null)
            {
                e.ShouldBeInterrupted |= CanBeInterrupted;
                e.ShouldBeStunned |= CanBeStunned;
            }
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(module, caster, spell);
        if (ShowNameInHint && spell.Action == WatchedAction)
            UpdateHint();
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(module, caster, spell);
        if (ShowNameInHint && spell.Action == WatchedAction)
            UpdateHint();
    }

    private void UpdateHint()
    {
        if (!CanBeInterrupted && !CanBeStunned)
            return;
        var actionStr = !CanBeStunned ? "Interrupt" : !CanBeInterrupted ? "Stun" : "Interrupt/stun";
        var nameStr = ShowNameInHint && Casters.Count == 1 ? " " + Casters[0].Name : "";
        Hint = $"{actionStr}{nameStr}!";
        if (HintExtra.Length > 0)
            Hint += $" ({HintExtra})";
    }
}
