namespace BossMod;

// This tweak controls auto attacks to prevent early pulls and to enable them asap when pulling, changing targets or starting casts.
public sealed class AutoAutosTweak(WorldState ws, AIHints hints)
{
    private readonly ActionTweaksConfig _config = Service.Config.Get<ActionTweaksConfig>();
    private bool _lastActionDisabledAutos;

    public const float PrePullThreshold = 0.5f; // effect result delay for autos

    public bool Enabled => _config.AutoAutos;

    public bool ShouldPreventAutoActivation(uint spellId)
    {
        var actionData = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(spellId);
        _lastActionDisabledAutos = actionData?.Unknown50 is 3 or 6 or 7;
        return Enabled && ws.Client.CountdownRemaining > PrePullThreshold && !(ws.Party.Player()?.InCombat ?? false);
    }

    public bool GetDesiredState(bool currentState)
    {
        if (!Enabled || _lastActionDisabledAutos)
            return currentState;

        var player = ws.Party.Player();
        if (player == null || player.IsDead || player.Statuses.Any(s => s.ID is 418 or 2648)) // transcendent
            return currentState;

        var target = ws.Actors.Find(player.TargetID);
        if (target == null || target.IsAlly)
            return currentState;

        if (_config.PyreticThreshold > 0 && hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Pyretic && hints.ImminentSpecialMode.activation < ws.FutureTime(_config.PyreticThreshold))
            return false; // pyretic => disable autos

        return player.InCombat || ws.Client.CountdownRemaining <= PrePullThreshold; // no reason not to enable autos!
    }
}
