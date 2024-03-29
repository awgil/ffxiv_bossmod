namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class DaemoniacBonds : Components.UniformStackSpread
{
    public int NumMechanics { get; private set; }
    private List<Actor> _spreadTargets = new();
    private List<Actor> _stackTargets = new();
    private DateTime _spreadResolve;
    private DateTime _stackResolve;

    public DaemoniacBonds() : base(4, 6, alwaysShowSpreads: true) { }

    public void Show()
    {
        if (_spreadResolve < _stackResolve)
            AddSpreads(_spreadTargets, _spreadResolve);
        else
            AddStacks(_stackTargets, _stackResolve);
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_spreadResolve == default || _stackResolve == default)
            return;
        var stackHint = MinStackSize == 2 ? "Pairs" : "Groups";
        var orderHint = _spreadResolve > _stackResolve ? $"{stackHint} -> Spread" : $"Spread -> {stackHint}";
        hints.Add($"Debuff order: {orderHint}");
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.DaemoniacBonds:
                _spreadTargets.Add(actor);
                _spreadResolve = status.ExpireAt;
                break;
            case SID.DuodaemoniacBonds:
            case SID.TetradaemoniacBonds:
                MinStackSize = (SID)status.ID == SID.TetradaemoniacBonds ? 4 : 2;
                _stackTargets.Add(actor);
                _stackResolve = status.ExpireAt;
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DaemoniacBondsAOE:
                Spreads.Clear();
                NumMechanics = _spreadResolve < _stackResolve ? 1 : 2;
                if (NumMechanics == 1 && Stacks.Count == 0)
                    AddStacks(_stackTargets, _stackResolve);
                break;
            case AID.DuodaemoniacBonds:
            case AID.TetradaemoniacBonds:
                Stacks.Clear();
                NumMechanics = _stackResolve < _spreadResolve ? 1 : 2;
                if (NumMechanics == 1 && Spreads.Count == 0)
                    AddSpreads(_spreadTargets, _spreadResolve);
                break;
        }
    }
}
