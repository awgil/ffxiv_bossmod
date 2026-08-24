namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class EnaeroEndeath(BossModule module) : Components.Knockback(module)
{
    private Source? _source;
    private Kind _delayed;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => _source?.Kind == Kind.TowardsOrigin ? (pos - _source.Value.Origin).LengthSq() <= 36 : !Module.InBounds(pos);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_delayed == Kind.None)
            base.AddHints(slot, actor, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_delayed != Kind.None)
            hints.Add($"Delayed {(_delayed == Kind.TowardsOrigin ? "attract" : "knockback")}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Aero:
                Start(Module.CastFinishAt(spell, 0.5f), Kind.AwayFromOrigin);
                break;
            case AID.Death:
                Start(Module.CastFinishAt(spell, 0.5f), Kind.TowardsOrigin);
                break;
            case AID.Enaero:
                _delayed = Kind.AwayFromOrigin;
                break;
            case AID.Endeath:
                _delayed = Kind.TowardsOrigin;
                break;
            case AID.AeroKnockback:
            case AID.EnaeroKnockback:
                if (_source == null || _source.Value.Kind != Kind.AwayFromOrigin || !_source.Value.Origin.AlmostEqual(caster.Position, 1))
                    ReportError("Aero knockback mispredicted");
                break;
            case AID.DeathVortex:
            case AID.EndeathVortex:
                if (_source == null || _source.Value.Kind != Kind.TowardsOrigin || !_source.Value.Origin.AlmostEqual(caster.Position, 1))
                    ReportError("Death vortex mispredicted");
                break;
            case AID.BladeOfDarknessLAOE:
            case AID.BladeOfDarknessRAOE:
            case AID.BladeOfDarknessCAOE:
                if (_delayed != Kind.None)
                    Start(Module.CastFinishAt(spell, 2.2f), _delayed);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AeroKnockback:
            case AID.EnaeroKnockback:
            case AID.DeathVortex:
            case AID.EndeathVortex:
                ++NumCasts;
                _source = null;
                break;
            case AID.BladeOfDarknessLAOE:
            case AID.BladeOfDarknessRAOE:
            case AID.BladeOfDarknessCAOE:
                _delayed = Kind.None;
                break;
        }
    }

    private void Start(DateTime activation, Kind kind)
    {
        NumCasts = 0;
        _source = new(Ch01CloudOfDarkness.Phase1Midpoint, 15, activation, Kind: kind);
    }
}

class EnaeroAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private bool _delayed;

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Aero:
                Start(Module.CastFinishAt(spell, 0.5f));
                break;
            case AID.Enaero:
                _delayed = true;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AeroAOE:
            case AID.EnaeroAOE:
                _aoe = null;
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
        NumCasts = 0;
        _aoe = new(_shape, Ch01CloudOfDarkness.Phase1Midpoint, default, activation);
        _delayed = false;
    }
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
        NumCasts = 0;
        _aoes.Add(new(_shapeOut, Ch01CloudOfDarkness.Phase1Midpoint, default, activation.AddSeconds(2)));
        _aoes.Add(new(_shapeIn, Ch01CloudOfDarkness.Phase1Midpoint, default, activation.AddSeconds(4)));
        _delayed = false;
    }
}
