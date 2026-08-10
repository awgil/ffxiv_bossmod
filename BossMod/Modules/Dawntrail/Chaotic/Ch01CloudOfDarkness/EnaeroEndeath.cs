namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class EnaeroEndeath(BossModule module) : Components.GenericKnockback(module, stopAfterWall: true)
{
    private Knockback[] _source = [];
    private Kind _delayed;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _source;
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        ref var s = ref _source[0];
        return s.Kind == Kind.TowardsOrigin ? (pos - s.Origin).LengthSq() <= 36f : !Arena.InBounds(pos);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_delayed == Kind.None)
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_delayed != Kind.None)
        {
            hints.Add($"Delayed {(_delayed == Kind.TowardsOrigin ? "attract" : "knockback")}");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Aero:
                Start(Module.CastFinishAt(spell, 0.5d), Kind.AwayFromOrigin);
                break;
            case (uint)AID.Death:
                Start(Module.CastFinishAt(spell, 0.5d), Kind.TowardsOrigin);
                break;
            case (uint)AID.Enaero:
                _delayed = Kind.AwayFromOrigin;
                break;
            case (uint)AID.Endeath:
                _delayed = Kind.TowardsOrigin;
                break;
            case (uint)AID.BladeOfDarknessLAOE:
            case (uint)AID.BladeOfDarknessRAOE:
            case (uint)AID.BladeOfDarknessCAOE:
                if (_delayed != Kind.None)
                {
                    Start(Module.CastFinishAt(spell, 2.2d), _delayed);
                }
                break;
        }

        void Start(DateTime activation, Kind kind)
        {
            NumCasts = 0;
            _source = [new(new WPos(100f, 76.28427f).Quantized(), 15f, activation, kind: kind)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AeroKnockback:
            case (uint)AID.EnaeroKnockback:
            case (uint)AID.DeathVortex:
            case (uint)AID.EndeathVortex:
                ++NumCasts;
                _source = [];
                break;
            case (uint)AID.BladeOfDarknessLAOE:
            case (uint)AID.BladeOfDarknessRAOE:
            case (uint)AID.BladeOfDarknessCAOE:
                _delayed = Kind.None;
                break;
        }
    }
}

sealed class EnaeroAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private bool _delayed;
    private static readonly AOEShapeCircle _shape = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Aero:
                Start(Module.CastFinishAt(spell, 0.5d));
                break;
            case (uint)AID.Enaero:
                _delayed = true;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AeroAOE:
            case (uint)AID.EnaeroAOE:
                _aoe = [];
                break;
            case (uint)AID.BladeOfDarknessLAOE:
            case (uint)AID.BladeOfDarknessRAOE:
            case (uint)AID.BladeOfDarknessCAOE:
                if (_delayed)
                {
                    Start(WorldState.FutureTime(2.2d));
                }
                break;
        }
    }

    private void Start(DateTime activation)
    {
        NumCasts = 0;
        _aoe = [new(_shape, new WPos(100f, 76.28427f).Quantized(), default, activation)];
        _delayed = false;
    }
}

sealed class EndeathAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private bool _delayed;
    private static readonly AOEShapeCircle _shapeOut = new(6f);
    private static readonly AOEShapeDonut _shapeIn = new(6f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Death:
                Start(Module.CastFinishAt(spell, 0.5d));
                break;
            case (uint)AID.Endeath:
                _delayed = true;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DeathAOE1:
            case (uint)AID.DeathAOE2:
            case (uint)AID.EndeathAOE1:
            case (uint)AID.EndeathAOE2:
                if (_aoes.Count != 0)
                {
                    _aoes.RemoveAt(0);
                }
                break;
            case (uint)AID.BladeOfDarknessLAOE:
            case (uint)AID.BladeOfDarknessRAOE:
            case (uint)AID.BladeOfDarknessCAOE:
                if (_delayed)
                {
                    Start(WorldState.FutureTime(2.2d));
                }
                break;
        }
    }

    private void Start(DateTime activation)
    {
        NumCasts = 0;
        AddAOE(_shapeOut, 2d);
        AddAOE(_shapeIn, 4d);
        _delayed = false;
        void AddAOE(AOEShape shape, double delay) => _aoes.Add(new(shape, new WPos(100f, 76.28427f).Quantized(), default, activation.AddSeconds(delay)));
    }
}
