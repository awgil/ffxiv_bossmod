namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class EnaeroKnockback(BossModule module) : Components.Knockback(module)
{
    private Source? _source;
    private bool _delayed;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_delayed)
            base.AddHints(slot, actor, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_delayed)
            hints.Add("Delayed knockback");
    }

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
            case AID.AeroKnockback:
            case AID.EnaeroKnockback:
                if (_source == null || !_source.Value.Origin.AlmostEqual(caster.Position, 1))
                    ReportError("Aero knockback mispredicted");
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
            case AID.AeroKnockback:
            case AID.EnaeroKnockback:
                _source = null;
                break;
            case AID.BladeOfDarknessLAOE:
            case AID.BladeOfDarknessRAOE:
            case AID.BladeOfDarknessCAOE:
                _delayed = false;
                break;
        }
    }

    private void Start(DateTime activation) => _source = new(Ch01CloudOfDarkness.Phase1Midpoint, 15, activation);
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
        _aoe = new(_shape, Ch01CloudOfDarkness.Phase1Midpoint, default, activation);
        _delayed = false;
    }
}
