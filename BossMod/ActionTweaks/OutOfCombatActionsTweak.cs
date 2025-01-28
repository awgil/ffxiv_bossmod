namespace BossMod;

[ConfigDisplay(Name = "Automatic out-of-combat utility actions", Parent = typeof(ActionTweaksConfig), Since = "0.0.0.245")]
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
            ws.Actors.StatusGain.Subscribe(OnStatusGain),
            ws.Actors.StatusLose.Subscribe(OnStatusLose)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public void FillActions(Actor player, AIHints hints)
    {
        if (!_config.Enabled || player.InCombat || _ws.Client.CountdownRemaining != null || player.MountId != 0 || player.Statuses.Any(s => s.ID is 418 or 2648)) // note: in overworld content, you leave combat on death...
            return;

        if (_config.AutoPeloton && player.ClassCategory == ClassCategory.PhysRanged && _ws.CurrentTime >= _nextAutoPeloton)
        {
            var movementThreshold = 5 * _ws.Frame.Duration;
            if (player.LastFrameMovement.LengthSq() >= movementThreshold * movementThreshold)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Peloton), player, ActionQueue.Priority.VeryLow);
        }

        // TODO: other things
    }

    private void OnStatusGain(Actor actor, int index)
    {
        if (actor != _ws.Party.Player())
            return;

        switch (actor.Statuses[index].ID)
        {
            case (uint)BRD.SID.Peloton:
                _nextAutoPeloton = actor.Statuses[index].ExpireAt.AddSeconds(-1);
                break;
        }
    }

    private void OnStatusLose(Actor actor, int index)
    {
        if (actor != _ws.Party.Player())
            return;

        switch (actor.Statuses[index].ID)
        {
            case (uint)BRD.SID.Peloton:
                if (_ws.CurrentTime < _nextAutoPeloton)
                    _nextAutoPeloton = _ws.FutureTime(1); // if peloton expired earlier than expected, don't recast immediately - this could've been caused by entering combat, status is lost few frames before combat flag is set
                break;
        }
    }
}
