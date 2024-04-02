namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko;

class ScarletAuspice : Components.SelfTargetedAOEs
{
    public ScarletAuspice(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCircle(6)) { }
}
class NScarletAuspice : ScarletAuspice { public NScarletAuspice() : base(AID.NScarletAuspice) { } }
class SScarletAuspice : ScarletAuspice { public SScarletAuspice() : base(AID.SScarletAuspice) { } }

class BoundlessScarletFirst : Components.SelfTargetedAOEs
{
    public BoundlessScarletFirst(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(30, 5, 30)) { }
}
class NBoundlessScarletFirst : BoundlessScarletFirst { public NBoundlessScarletFirst() : base(AID.NBoundlessScarletAOE) { } }
class SBoundlessScarletFirst : BoundlessScarletFirst { public SBoundlessScarletFirst() : base(AID.SBoundlessScarletAOE) { } }

class BoundlessScarletRest : Components.SelfTargetedAOEs
{
    public BoundlessScarletRest(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(30, 15, 30), 2) { }
}
class NBoundlessScarletRest : BoundlessScarletRest { public NBoundlessScarletRest() : base(AID.NBoundlessScarletExplosion) { } }
class SBoundlessScarletRest : BoundlessScarletRest { public SBoundlessScarletRest() : base(AID.SBoundlessScarletExplosion) { } }

class InvocationOfVengeance : Components.UniformStackSpread
{
    public int NumMechanics { get; private set; }
    private List<Actor> _spreadTargets = new();
    private List<Actor> _stackTargets = new();
    private DateTime _spreadResolve;
    private DateTime _stackResolve;

    public InvocationOfVengeance() : base(3, 3, alwaysShowSpreads: true) { }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_spreadResolve == default || _stackResolve == default)
            return;
        var orderHint = _spreadResolve > _stackResolve ? $"Stack -> Spread" : $"Spread -> Stack";
        hints.Add($"Debuff order: {orderHint}");
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.VengefulFlame:
                _spreadTargets.Add(actor);
                _spreadResolve = status.ExpireAt;
                UpdateStackSpread();
                break;
            case SID.VengefulPyre:
                _stackTargets.Add(actor);
                _stackResolve = status.ExpireAt;
                UpdateStackSpread();
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NVengefulFlame:
            case AID.SVengefulFlame:
                if (_spreadResolve != default)
                {
                    ++NumMechanics;
                    _spreadTargets.Clear();
                    _spreadResolve = default;
                    UpdateStackSpread();
                }
                break;
            case AID.NVengefulPyre:
            case AID.SVengefulPyre:
                if (_stackResolve != default)
                {
                    ++NumMechanics;
                    _stackTargets.Clear();
                    _stackResolve = default;
                    UpdateStackSpread();
                }
                break;
        }
    }

    private void UpdateStackSpread()
    {
        Spreads.Clear();
        Stacks.Clear();
        if (_stackResolve == default || _stackResolve > _spreadResolve)
            AddSpreads(_spreadTargets, _spreadResolve);
        if (_spreadResolve == default || _spreadResolve > _stackResolve)
            AddStacks(_stackTargets, _stackResolve);
    }
}
