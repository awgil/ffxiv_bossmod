namespace BossMod.Endwalker.Alliance.A14Naldthal;

// TODO: create and use generic 'line stack' component
class FarFlungFire(BossModule module) : Components.GenericWildCharge(module, 3, fixedLength: 40)
{
    private bool _real;
    private ulong _targetID;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FarAboveDeepBelowNald or AID.HearthAboveFlightBelowNald or AID.HearthAboveFlightBelowThalNald)
        {
            _real = true;
            InitIfReal();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FarFlungFireVisual:
                Source = caster;
                _targetID = spell.MainTargetID;
                InitIfReal();
                break;
            case AID.FarFlungFireAOE:
                ++NumCasts;
                _real = false;
                Source = null;
                break;
        }
    }

    private void InitIfReal()
    {
        if (_real && _targetID != 0)
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == _targetID ? PlayerRole.Target : PlayerRole.Share;
    }
}

class DeepestPit(BossModule module) : Components.GenericAOEs(module, default, "GTFO from puddle!")
{
    private bool _real;
    private readonly List<Actor> _targets = [];
    private readonly List<Actor> _casters = [];

    public bool Active => _casters.Count > 0;

    private static readonly AOEShapeCircle _shape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(_shape, c.CastInfo!.LocXZ, default, c.CastInfo.NPCFinishAt));

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _real && _targets.Contains(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_real)
            foreach (var t in _targets)
                Arena.AddCircle(t.Position, _shape.Radius, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DeepestPitFirst or AID.DeepestPitRest)
        {
            _casters.Remove(caster);
            if (_casters.Count == 0)
                _targets.Clear();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.DeepestPitTarget)
        {
            _targets.Add(actor);
        }
    }
}
