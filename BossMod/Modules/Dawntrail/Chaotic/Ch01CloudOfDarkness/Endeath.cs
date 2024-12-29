namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class EndeathVortex(BossModule module) : Components.Knockback(module)
{
    private Source? _source;
    private bool _delayed;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (pos - Ch01CloudOfDarkness.Phase1Midpoint).LengthSq() <= 36;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_delayed)
            base.AddHints(slot, actor, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_delayed)
            hints.Add("Delayed attract");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Death:
                Start(Module.CastFinishAt(spell, 0.5f));
                break;
            case AID.Endeath:
                _delayed = true;
                break;
            case AID.DeathVortex:
            case AID.EndeathVortex:
                if (_source == null || !_source.Value.Origin.AlmostEqual(caster.Position, 1))
                    ReportError("Death vortex mispredicted");
                break;
            case AID.BladeOfDarknessLAOE:
            case AID.BladeOfDarknessRAOE:
            case AID.BladeOfDarknessCAOE:
                if (_delayed)
                    Start(Module.CastFinishAt(spell, 2.2f));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DeathVortex:
            case AID.EndeathVortex:
                _source = null;
                break;
            case AID.BladeOfDarknessLAOE:
            case AID.BladeOfDarknessRAOE:
            case AID.BladeOfDarknessCAOE:
                _delayed = false;
                break;
        }
    }

    private void Start(DateTime activation) => _source = new(Ch01CloudOfDarkness.Phase1Midpoint, 15, activation, Kind: Kind.TowardsOrigin);
}

class EndeathAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private bool _delayed;

    private static readonly AOEShapeCircle _shapeOut = new(6);
    private static readonly AOEShapeDonut _shapeIn = new(6, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Death:
                Start(Module.CastFinishAt(spell, 0.5f));
                break;
            case AID.Endeath:
                _delayed = true;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DeathAOE1:
            case AID.DeathAOE2:
            case AID.EndeathAOE1:
            case AID.EndeathAOE2:
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                break;
            case AID.BladeOfDarknessLAOE:
            case AID.BladeOfDarknessRAOE:
            case AID.BladeOfDarknessCAOE:
                if (_delayed)
                    Start(WorldState.FutureTime(2.2f));
                break;
        }
    }

    private void Start(DateTime activation)
    {
        _aoes.Add(new(_shapeOut, Ch01CloudOfDarkness.Phase1Midpoint, default, activation.AddSeconds(2)));
        _aoes.Add(new(_shapeIn, Ch01CloudOfDarkness.Phase1Midpoint, default, activation.AddSeconds(4)));
        _delayed = false;
    }
}
