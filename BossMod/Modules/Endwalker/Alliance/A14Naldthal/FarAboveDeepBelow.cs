namespace BossMod.Endwalker.Alliance.A14Naldthal;

// TODO: create and use generic 'line stack' component
class FarFlungFire : Components.GenericWildCharge
{
    private bool _real;
    private ulong _targetID;

    public FarFlungFire() : base(3)
    {
        FixedLength = 40;
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FarAboveDeepBelowNald or AID.HearthAboveFlightBelowNald or AID.HearthAboveFlightBelowThalNald)
        {
            _real = true;
            InitIfReal(module);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FarFlungFireVisual:
                Source = caster;
                _targetID = spell.MainTargetID;
                InitIfReal(module);
                break;
            case AID.FarFlungFireAOE:
                ++NumCasts;
                _real = false;
                Source = null;
                break;
        }
    }

    private void InitIfReal(BossModule module)
    {
        if (_real && _targetID != 0)
            foreach (var (i, p) in module.Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == _targetID ? PlayerRole.Target : PlayerRole.Share;
    }
}

class DeepestPit : Components.GenericAOEs
{
    private bool _real;
    private List<Actor> _targets = new();
    private List<Actor> _casters = new();

    public bool Active => _casters.Count > 0;

    private static readonly AOEShapeCircle _shape = new(6);

    public DeepestPit() : base(default, "GTFO from puddle!") { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _casters.Select(c => new AOEInstance(_shape, c.CastInfo!.LocXZ, default, c.CastInfo.NPCFinishAt));

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _real && _targets.Contains(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_real)
            foreach (var t in _targets)
                arena.AddCircle(t.Position, _shape.Radius, ArenaColor.Danger);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FarAboveDeepBelowThal:
            case AID.HearthAboveFlightBelowThal:
            case AID.HearthAboveFlightBelowNaldThal:
                _real = true;
                break;
            case AID.DeepestPitFirst:
            case AID.DeepestPitRest:
                _casters.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DeepestPitFirst or AID.DeepestPitRest)
        {
            _casters.Remove(caster);
            if (_casters.Count == 0)
                _targets.Clear();
        }
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.DeepestPitTarget)
        {
            _targets.Add(actor);
        }
    }
}
