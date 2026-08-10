namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class AuroralUppercut(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];
    private RelSimplifiedComplexPolygon poly;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = spell.Action.ID switch
        {
            (uint)AID.AuroralUppercut1 => 12f,
            (uint)AID.AuroralUppercut2 => 25f,
            (uint)AID.AuroralUppercut3 => 38f,
            _ => default
        };
        if (distance != default)
        {
            _kb = [new(Arena.Center, distance, Module.CastFinishAt(spell, 1.6d), ignoreImmunes: true)];
            if (Arena.Bounds is ArenaBoundsCustom arena)
            {
                poly = arena.Polygon.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (_kb.Length != 0 && status.ID == (uint)SID.Knockback)
        {
            NumCasts = 1;
            _kb = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Length != 0)
        {
            ref readonly var kb = ref _kb[0];
            var center = Arena.Center;
            hints.AddForbiddenZone(new SDKnockbackInComplexPolygonAwayFromOrigin(center, center, kb.Distance, poly), kb.Activation);
        }
    }
}

sealed class AuroralUppercutHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArenaChanges arena = module.FindComponent<ArenaChanges>()!;
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = spell.Action.ID switch
        {
            (uint)AID.AuroralUppercut1 => 12f,
            (uint)AID.AuroralUppercut2 => 25f,
            (uint)AID.AuroralUppercut3 => 38f,
            _ => default
        };
        if (distance == default)
        {
            return;
        }
        Angle a45 = 45f.Degrees(), a135 = 135f.Degrees(), a44 = 44f.Degrees(), a13 = 12.5f.Degrees(), a59 = 59f.Degrees();
        var center = Arena.Center;
        switch (distance)
        {
            case 12f:
                if (arena.Curstate == 00020001u)
                {
                    SetAOE(new(center, [new ConeV(center, 5f, a135, a59, 8), new ConeV(center, 5f, -a45, a59, 8)]));
                }
                else
                {
                    SetAOE(new(center, [new ConeV(center, 5f, -a135, a59, 8), new ConeV(center, 5f, a45, a59, 8)]));
                }
                break;
            case 25f:
                if (arena.Curstate == 00020001u)
                {
                    SetAOE(new(center, [new DonutSegmentV(center, 4f, 10f, -144f.Degrees(), a44, 8), new DonutSegmentV(center, 4f, 10f, 36f.Degrees(), a44, 8)],
                        [new ConeV(center, 10f, -a135, a13, 8), new ConeV(center, 10f, a45, a13, 8)]));
                }
                else
                {
                    SetAOE(new(center, [new DonutSegmentV(center, 4f, 10f, 126f.Degrees(), a44, 8), new DonutSegmentV(center, 4f, 10f, -54f.Degrees(), a44, 8)],
                        [new ConeV(center, 10f, a135, a13, 8), new ConeV(center, 10f, -a45, a13, 8)]));
                }
                break;
            case 38f:
                if (arena.Curstate == 00020001u)
                {
                    SetAOE(new(center, [new ConeV(center, 5f, -a135, a13, 8), new ConeV(center, 5f, a45, a13, 8)]));
                }
                else
                {
                    SetAOE(new(center, [new ConeV(center, 5f, a135, a13, 8), new ConeV(center, 5f, -a45, a13, 8)]));
                }
                break;
        }
        void SetAOE(AOEShapeCustom shape) => _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 1.6d), Colors.SafeFromAOE, shapeDistance: shape.Distance(center, default))];
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (_aoe.Length != 0 && status.ID == (uint)SID.Knockback)
        {
            _aoe = [];
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}
