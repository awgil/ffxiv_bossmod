namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

// there are only 4 possible patterns for this mechanic, here are the findings:
// - wide/knockback are always NE/NW platforms, narrow are always SE/SW platforms
// - NE/NW are always mirrored (looking from platform to the main one, wide is on the left side on one of them and on the right side on the other); which one is which is random
// - SE/SW have two patterns (either inner or outer lanes change left/right side); which one is which is random
sealed class ForgedTrack(BossModule module) : Components.GenericAOEs(module)
{
    public enum Pattern { Unknown, A, B } // B is always inverted

    public readonly List<AOEInstance> NarrowAOEs = [];
    public readonly List<AOEInstance> WideAOEs = [];
    public readonly List<AOEInstance> KnockbackAOEs = [];
    private Pattern _patternN;
    private Pattern _patternS;

    private readonly AOEShapeRect _shape = new(10f, 2.5f, 10f);
    private readonly AOEShapeRect _shapeWide = new(10f, 7.5f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan([.. NarrowAOEs, .. WideAOEs, .. KnockbackAOEs]);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.ForgedTrackPreview)
        {
            return;
        }

        var casterOffset = caster.Position - Arena.Center;
        var rightDir = spell.Rotation.ToDirection().OrthoR();
        var laneOffset = casterOffset.Dot(rightDir);
        var laneRight = laneOffset > 0f;
        var laneInner = laneOffset is > -5f and < 5f;
        var west = casterOffset.X > 0f;
        if (casterOffset.Z < 0f)
        {
            // N => wide/knockback
            if (_patternN == Pattern.Unknown)
                return;
            var rightIsWide = west == (_patternN == Pattern.A);
            var adjustedLaneOffset = laneOffset + (laneRight ? -5f : 5f);
            if (rightIsWide == laneRight)
            {
                // wide
                WideAOEs.Add(new(_shapeWide, Arena.Center + rightDir * adjustedLaneOffset, spell.Rotation, Module.CastFinishAt(spell, 1.4d)));
            }
            else
            {
                // knockback
                KnockbackAOEs.Add(new(_shape, Arena.Center + rightDir * adjustedLaneOffset, spell.Rotation, Module.CastFinishAt(spell, 1.9d)));
            }
        }
        else
        {
            // S => narrow
            if (_patternS == Pattern.Unknown)
                return;
            var crossInner = west == (_patternS == Pattern.A);
            var cross = crossInner == laneInner;
            var adjustedRight = cross ^ laneRight;
            var adjustedLaneOffset = (laneInner ? 7.5f : 2.5f) * (adjustedRight ? 1f : -1f);
            NarrowAOEs.Add(new(_shape, Arena.Center + rightDir * adjustedLaneOffset, spell.Rotation, Module.CastFinishAt(spell, 1.3d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ForgedTrackAOE:
                ++NumCasts;
                NarrowAOEs.Clear();
                break;
            case (uint)AID.FieryEdgeAOECenter:
                if (WideAOEs.Count == 0)
                {
                    Module.ReportError(this, "Unexpected wide aoe");
                }
                WideAOEs.Clear();
                break;
            case (uint)AID.StormyEdgeAOE:
                if (KnockbackAOEs.Count == 0)
                {
                    Module.ReportError(this, "Unexpected knockback");
                }
                KnockbackAOEs.Clear();
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case 2:
                AssignPattern(ref _patternS, 0x00800040u, 0x02000100u, state);
                break;
            case 3:
                AssignPattern(ref _patternS, 0x02000100u, 0x00800040u, state);
                break;
            case 5:
                AssignPattern(ref _patternN, 0x00020001u, 0x00200010u, state);
                break;
            case 8:
                AssignPattern(ref _patternN, 0x00200010u, 0x00020001u, state);
                break;
        }
    }

    private void AssignPattern(ref Pattern field, uint stateA, uint stateB, uint state)
    {
        // 0x00020001 XX, 0x00200010 out and inner crossed, 0x02000100 inner crossed, 0x00800040 outer crossed, 0x00080004 disappear
        if (state == 0x00080004u)
        {
            return; // end
        }
        if (state != stateA && state != stateB)
        {
            Module.ReportError(this, $"Unknown pattern: {state:X8}, expected {stateA:X8} or {stateB:X8}");
            return;
        }
        var value = state == stateA ? Pattern.A : Pattern.B;
        if (field != Pattern.Unknown && field != value)
        {
            Module.ReportError(this, $"Inconsistent pattern assignments: {field} vs {value}");
        }
        field = value;
    }
}

sealed class ForgedTrackKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.StormyEdgeKnockback)
{
    private readonly ForgedTrack? _main = module.FindComponent<ForgedTrack>();

    private readonly AOEShapeRect _shape = new(20f, 10f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_main == null)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_main.KnockbackAOEs);
        var count = aoes.Length;
        Span<Knockback> sources = new Knockback[count * 2];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            var origin = aoe.Origin;
            var act = aoe.Activation;
            var a90 = 90f.Degrees();
            var rot = aoe.Rotation;
            sources[index++] = new(origin, 7f, act, _shape, rot + a90, Kind.DirForward);
            sources[index++] = new(origin, 7f, act, _shape, rot - a90, Kind.DirForward);
        }
        return sources;
    }
}
