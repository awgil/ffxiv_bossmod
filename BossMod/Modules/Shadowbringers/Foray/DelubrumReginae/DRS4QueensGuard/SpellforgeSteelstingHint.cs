namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

// TODO: improve hints (check player's class; for healers, hints for party members having incorrect buff)
class SpellforgeSteelstingHint : BossComponent
{
    private string _hint = "";
    public bool Active => _hint.Length > 0;

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (Active)
            hints.Add(_hint);
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        var hint = (TetherID)tether.ID switch
        {
            TetherID.PhysicalVulnerabilityDown => "Spellforge",
            TetherID.MagicVulnerabilityDown => "Steelsting",
            _ => ""
        };
        if (hint.Length > 0)
            _hint = hint;
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.PhysicalVulnerabilityDown or SID.MagicVulnerabilityDown)
            _hint = "";
    }
}
