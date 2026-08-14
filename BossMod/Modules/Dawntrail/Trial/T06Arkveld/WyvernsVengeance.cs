namespace BossMod.Dawntrail.Trial.T06Arkveld;

sealed class WyvernsVengeance(BossModule module) : Components.Exaflare(module, 6f)
{
    private readonly List<ulong> _casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsVengeance)
        {
            // advance doesn't matter for caster-matched tracking; use default WDir
            Lines.Add(new(
                caster.Position,
                default,
                Module.CastFinishAt(spell),
                timeToMove: 1.6d,
                explosionsLeft: 2,
                maxShownExplosions: 1
            ));
            _casters.Add(caster.InstanceID);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsVengeance1)
        {
            var idx = _casters.IndexOf(caster.InstanceID);
            if (idx < 0)
                return; // not one of our tracked helpers

            var line = Lines[idx];
            var loc = spell.TargetXZ;

            AdvanceLine(line, loc);

            if (line.ExplosionsLeft == 0)
            {
                Lines.RemoveAt(idx);
                _casters.RemoveAt(idx);
            }
        }
    }
}

// Don't think I am able to put the predicted path of the 'Laser' in, so this may have to do.

// Wyvern's Weal beam telegraphs (helpers)
sealed class WyvernsWealAOE(BossModule module)
    : Components.SimpleAOEGroups(module, [(uint)AID.WyvernsWeal1, (uint)AID.WyvernsWeal4], new AOEShapeRect(60f, 3f));

// Wyvern's Weal pulses (no-cast helper rects)
sealed class WyvernsWealPulses(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(60f, 3f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.AsSpan();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsWeal2)
            _aoes.Add(new AOEInstance(_shape, caster.Position, caster.Rotation, WorldState.FutureTime(0.8f)));
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _aoes.RemoveAll(a => a.Activation <= now);
    }
}

// Wyvern's Weal: deterministic GTFO lane based on boss sweep cast.
// 45046 = southeast/left sweep  → danger LEFT  → safe RIGHT
// 45047 = north/right sweep     → danger RIGHT → safe LEFT
// Lane extends 60y forward and 20y behind boss.
sealed class WyvernsWealIrregularCastLane(BossModule module) : Components.GenericAOEs(module)
{
    private const float LenFront = 60f;
    private const float LenBack = 20f;
    private const float Narrow = 1f;
    private const float Wide = 60f;

    private AOEShapeRect? _shape;
    private SDRect? _forbid;
    private WPos _origin;
    private Angle _rotation;
    private DateTime _until;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_shape == null || WorldState.CurrentTime >= _until)
            return default;

        return new AOEInstance[] { new(_shape, _origin, _rotation, _until) };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor)
            return;

        // Only the two sweep casts
        if (spell.Action.ID is not 45046u and not 45047u)
            return;

        // 45047 = north/right sweep → danger RIGHT
        // 45046 = southeast/left sweep → danger LEFT
        var dangerOnRight = spell.Action.ID == 45047u;

        _rotation = spell.Rotation;
        var forward = _rotation.ToDirection();
        var left = new WDir(-forward.Z, forward.X);

        // asymmetric lateral extents
        var lw = dangerOnRight ? Narrow : Wide;
        var rw = dangerOnRight ? Wide : Narrow;
        var halfW = (lw + rw) * 0.5f;
        var lateralShift = (lw - rw) * 0.5f;

        // origin shifted sideways so the narrow side is safe
        _origin = caster.Position + lateralShift * left;

        // pull the lane backward so it covers behind boss
        _origin -= forward * LenBack;

        // Build matching visual + forbidden shapes
        _shape = new AOEShapeRect(LenFront, halfW, LenBack);
        _forbid = new SDRect(_origin, forward, LenFront, LenBack, halfW);

        _until = Module.CastFinishAt(spell); // ~7s cast
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster == Module.PrimaryActor && (spell.Action.ID == 45046u || spell.Action.ID == 45047u))
        {
            _shape = null;
            _forbid = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_forbid == null || WorldState.CurrentTime >= _until)
            return;

        // Hard forbid entire lane immediately
        hints.AddForbiddenZone(_forbid, _until);
    }
}
