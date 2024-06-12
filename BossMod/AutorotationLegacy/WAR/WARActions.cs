using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.WAR;

sealed class Actions(Autorotation autorot, Actor player) : TankActions(autorot, player, Definitions.UnlockQuests)
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private bool _aoe;
    private readonly Rotation.State _state = new(autorot.WorldState);
    private readonly Rotation.Strategy _strategy = new();
    private readonly WARConfig _config = Service.Config.Get<WARConfig>();

    public override CommonRotation.PlayerState GetState() => _state;
    public override CommonRotation.Strategy GetStrategy() => _strategy;

    protected override void UpdateInternalState(int autoAction)
    {
        base.UpdateInternalState(autoAction);
        _aoe = autoAction switch
        {
            AutoActionST => false,
            AutoActionAOE => true, // TODO: consider making AI-like check
            AutoActionAIFight => NumTargetsHitByAOE() >= 3,
            _ => false, // irrelevant...
        };
        UpdatePlayerState();
        FillCommonStrategy(_strategy, ActionDefinitions.IDPotionStr);
        _strategy.ApplyStrategyOverrides(Autorot.Bossmods.ActiveModule?.PlanExecution?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []);
        _strategy.OnslaughtHeadroom = _config.OnslaughtHeadroom;
    }

    protected override void QueueAIActions(ActionQueue queue)
    {
        if (_state.Unlocked(AID.Interject))
        {
            var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 3 + e.Actor.HitboxRadius + Player.HitboxRadius));
            if (interruptibleEnemy != null)
                queue.Push(ActionID.MakeSpell(AID.Interject), interruptibleEnemy.Actor, ActionQueue.Priority.VeryLow + 100);
        }
        if (_state.Unlocked(AID.Defiance))
        {
            var wantStance = WantStance();
            if (_state.HaveTankStance != wantStance)
                queue.Push(ActionID.MakeSpell(wantStance ? AID.Defiance : AID.ReleaseDefiance), Player, ActionQueue.Priority.VeryLow + 200);
        }
        if (_state.Unlocked(AID.Provoke))
        {
            var provokeEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeTanked && e.PreferProvoking && e.Actor.TargetID != Player.InstanceID && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
            if (provokeEnemy != null)
                queue.Push(ActionID.MakeSpell(AID.Provoke), provokeEnemy.Actor, ActionQueue.Priority.VeryLow + 300);
        }
    }

    protected override void CalculateAutomaticActions(ActionQueue queue)
    {
        if (AutoAction < AutoActionAIFight)
            return;

        // TODO: refactor all that, it's kinda senseless now
        AID gcd = AID.None;
        if (Autorot.PrimaryTarget != null)
        {
            bool wantTomahawk = AutoAction == AutoActionAIFight && !Autorot.PrimaryTarget.Position.InCircle(Player.Position, 3 + Autorot.PrimaryTarget.HitboxRadius + Player.HitboxRadius) && _state.Unlocked(AID.Tomahawk);
            gcd = wantTomahawk ? AID.Tomahawk : Rotation.GetNextBestGCD(_state, _strategy, _aoe);
        }

        ActionID ogcd = default;
        var deadline = _state.GCD > 0 && gcd != AID.None ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            ogcd = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength, _aoe);
        if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
            ogcd = Rotation.GetNextBestOGCD(_state, _strategy, deadline, _aoe);

        PushResult(queue, gcd, Autorot.PrimaryTarget);
        PushResult(queue, ogcd, Autorot.PrimaryTarget);
    }

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);
        _state.HaveTankStance = Player.FindStatus(SID.Defiance) != null;

        _state.Gauge = Service.JobGauges.Get<WARGauge>().BeastGauge;

        _state.SurgingTempestLeft = _state.NascentChaosLeft = _state.PrimalRendLeft = _state.InnerReleaseLeft = 0;
        _state.InnerReleaseStacks = 0;
        foreach (var status in Player.Statuses)
        {
            switch ((SID)status.ID)
            {
                case SID.SurgingTempest:
                    _state.SurgingTempestLeft = StatusDuration(status.ExpireAt);
                    break;
                case SID.NascentChaos:
                    _state.NascentChaosLeft = StatusDuration(status.ExpireAt);
                    break;
                case SID.Berserk:
                case SID.InnerRelease:
                    _state.InnerReleaseLeft = StatusDuration(status.ExpireAt);
                    _state.InnerReleaseStacks = status.Extra & 0xFF;
                    break;
                case SID.PrimalRend:
                    _state.PrimalRendLeft = StatusDuration(status.ExpireAt);
                    break;
            }
        }
    }

    private AID ComboLastMove => (AID)Autorot.ActionManager.ComboLastMove;

    private int NumTargetsHitByAOE() => Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
}
