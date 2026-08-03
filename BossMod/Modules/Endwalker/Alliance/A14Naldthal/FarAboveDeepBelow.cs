namespace BossMod.Endwalker.Alliance.A14Naldthal;

// TODO: create and use generic 'line stack' component
class FarFlungFire(BossModule module) : Components.GenericWildCharge(module, 3f, fixedLength: 40f)
{
    private bool _real;
    private ulong _targetID;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FarAboveDeepBelowNald or (uint)AID.HearthAboveFlightBelowNald or (uint)AID.HearthAboveFlightBelowThalNald)
        {
            _real = true;
            InitIfReal();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FarFlungFireVisual:
                Source = caster;
                _targetID = spell.MainTargetID;
                InitIfReal();
                break;
            case (uint)AID.FarFlungFireAOE:
                ++NumCasts;
                _real = false;
                Source = null;
                break;
        }
    }

    private void InitIfReal()
    {
        if (_real && _targetID != 0)
        {
            var party = Raid.WithSlot(true, false, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                PlayerRoles[p.Item1] = p.Item2.InstanceID == _targetID ? PlayerRole.Target : PlayerRole.Share;
            }
        }
    }
}

class DeepestPit(BossModule module) : Components.GenericAOEs(module)
{
    private bool _real;
    private readonly List<Actor> _targets = [];
    private readonly List<AOEInstance> _aoes = [];

    public bool Active => _aoes.Count != 0;

    private static readonly AOEShapeCircle _shape = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _real && _targets.Contains(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_real)
        {
            var count = _targets.Count;
            for (var i = 0; i < count; ++i)
                Arena.ZoneCircleOutline(_targets[i].Position, _shape.Radius);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FarAboveDeepBelowThal:
            case (uint)AID.HearthAboveFlightBelowThal:
            case (uint)AID.HearthAboveFlightBelowNaldThal:
                _real = true;
                break;
            case (uint)AID.DeepestPitFirst:
            case (uint)AID.DeepestPitRest:
                _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell), actorID: caster.InstanceID));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DeepestPitFirst or (uint)AID.DeepestPitRest)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    break;
                }
            }
            if (_aoes.Count == 0)
            {
                _targets.Clear();
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DeepestPitTarget)
        {
            _targets.Add(actor);
        }
    }
}
