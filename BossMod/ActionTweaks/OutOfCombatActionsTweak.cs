namespace BossMod;

[ConfigDisplay(Name = "Automatic out-of-combat utility actions", Parent = typeof(ActionTweaksConfig))]
class OutOfCombatActionsConfig : ConfigNode
{
    [PropertyDisplay("Enable the feature")]
    public bool Enabled = false;

    [PropertyDisplay("Auto use Peloton when moving out of combat")]
    public bool AutoPeloton = true;
}

// Tweak to automatically use out-of-combat convenience actions (peloton, pet summoning, etc).
public sealed class OutOfCombatActionsTweak : IDisposable
{
    private readonly OutOfCombatActionsConfig _config = Service.Config.Get<OutOfCombatActionsConfig>();
    private readonly WorldState _ws;
    private readonly EventSubscriptions _subscriptions;
    private DateTime _nextAutoPeloton;

    public OutOfCombatActionsTweak(WorldState ws)
    {
        _ws = ws;
        _subscriptions = new
        (
            ws.Actors.CastEvent.Subscribe(OnCastEvent)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public void FillActions(Actor player, AIHints hints)
    {
        if (!_config.Enabled || player.InCombat || _ws.Client.CountdownRemaining != null || player.MountId != 0)
            return;

        if (_config.AutoPeloton && player.ClassCategory == ClassCategory.PhysRanged && player.Position != player.PrevPosition && _ws.CurrentTime >= _nextAutoPeloton && player.FindStatus(BRD.SID.Peloton) == null)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Peloton), player, ActionQueue.Priority.VeryLow);

        // TODO: other things
    }

    private void OnCastEvent(Actor actor, ActorCastEvent evt)
    {
        if (actor != _ws.Party.Player())
            return;

        switch (evt.Action.ID)
        {
            case (uint)ClassShared.AID.Peloton:
                _nextAutoPeloton = _ws.FutureTime(30);
                break;
        }
    }
}
