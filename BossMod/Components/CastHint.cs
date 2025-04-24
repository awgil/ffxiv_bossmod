namespace BossMod.Components;

// generic component that is 'active' when any actor casts specific spell
public class CastHint(BossModule module, Enum? aid, string hint, bool showCastTimeLeft = false) : CastCounter(module, aid)
{
    public string Hint = hint;
    public bool ShowCastTimeLeft = showCastTimeLeft; // if true, show cast time left until next instance
    public readonly List<Actor> Casters = [];

    public bool Active => Casters.Count > 0;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Active && Hint.Length > 0)
            hints.Add(ShowCastTimeLeft ? $"{Hint} {Casters[0].CastInfo?.NPCRemainingTime ?? 0:f1}s left" : Hint);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }
}

public class CastInterruptHint : CastHint
{
    public bool CanBeInterrupted { get; init; }
    public bool CanBeStunned { get; init; }
    public bool ShowNameInHint { get; init; } // important if there are several targets
    public string HintExtra { get; init; }

    public CastInterruptHint(BossModule module, Enum? aid, bool canBeInterrupted = true, bool canBeStunned = false, string hintExtra = "", bool showNameInHint = false) : base(module, aid, "")
    {
        CanBeInterrupted = canBeInterrupted;
        CanBeStunned = canBeStunned;
        ShowNameInHint = showNameInHint;
        HintExtra = hintExtra;
        UpdateHint();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            var e = hints.FindEnemy(c);
            if (e != null)
            {
                e.ShouldBeInterrupted |= CanBeInterrupted;
                e.ShouldBeStunned |= CanBeStunned;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (ShowNameInHint && spell.Action == WatchedAction)
            UpdateHint();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
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
