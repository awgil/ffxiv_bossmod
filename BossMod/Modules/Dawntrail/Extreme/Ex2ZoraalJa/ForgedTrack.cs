namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

// there are only 4 possible patterns for this mechanic, here are the findings:
// - wide/knockback are always NE/NW platforms, narrow are always SE/SW platforms
// - NE/NW are always mirrored (looking from platform to the main one, wide is on the left side on one of them and on the right side on the other); which one is which is random
// - SE/SW have two patterns (either inner or outer lanes change left/right side); which one is which is random
class ForgedTrack(BossModule module) : Components.GenericAOEs(module)
{
    public enum Pattern { Unknown, A, B } // B is always inverted

    public readonly List<AOEInstance> NarrowAOEs = [];
    public readonly List<AOEInstance> WideAOEs = [];
    public readonly List<AOEInstance> KnockbackAOEs = [];
    private Pattern _patternN;
    private Pattern _patternS;

    private static readonly AOEShapeRect _shape = new(10, 2.5f, 10);
    private static readonly AOEShapeRect _shapeWide = new(10, 7.5f, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => NarrowAOEs.Concat(WideAOEs).Concat(KnockbackAOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.ForgedTrackPreview)
            return;

        var casterOffset = caster.Position - Module.Center;
        var rightDir = spell.Rotation.ToDirection().OrthoR();
        var laneOffset = casterOffset.Dot(rightDir);
        var laneRight = laneOffset > 0;
        var laneInner = laneOffset is > -5 and < 5;
        var west = casterOffset.X > 0;
        if (casterOffset.Z < 0)
        {
            // N => wide/knockback
            if (_patternN == Pattern.Unknown)
                return;
            var rightIsWide = west == (_patternN == Pattern.A);
            var adjustedLaneOffset = laneOffset + (laneRight ? -5 : 5);
            if (rightIsWide == laneRight)
            {
                // wide
                WideAOEs.Add(new(_shapeWide, Module.Center + rightDir * adjustedLaneOffset, spell.Rotation, Module.CastFinishAt(spell, 1.4f)));
            }
            else
            {
                // knockback
                KnockbackAOEs.Add(new(_shape, Module.Center + rightDir * adjustedLaneOffset, spell.Rotation, Module.CastFinishAt(spell, 1.9f)));
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
            var adjustedLaneOffset = (laneInner ? 7.5f : 2.5f) * (adjustedRight ? 1 : -1);
            NarrowAOEs.Add(new(_shape, Module.Center + rightDir * adjustedLaneOffset, spell.Rotation, Module.CastFinishAt(spell, 1.3f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ForgedTrackAOE:
                ++NumCasts;
                NarrowAOEs.Clear();
                break;
            case AID.FieryEdgeAOECenter:
                if (WideAOEs.Count == 0)
                    Module.ReportError(this, "Unexpected wide aoe");
                WideAOEs.Clear();
                break;
            case AID.StormyEdgeAOE:
                if (KnockbackAOEs.Count == 0)
                    Module.ReportError(this, "Unexpected knockback");
                KnockbackAOEs.Clear();
                break;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        switch (index)
        {
            case 2:
                AssignPattern(ref _patternS, 0x00800040, 0x02000100, state);
                break;
            case 3:
                AssignPattern(ref _patternS, 0x02000100, 0x00800040, state);
                break;
            case 5:
                AssignPattern(ref _patternN, 0x00020001, 0x00200010, state);
                break;
            case 8:
                AssignPattern(ref _patternN, 0x00200010, 0x00020001, state);
                break;
        }
    }

    private void AssignPattern(ref Pattern field, uint stateA, uint stateB, uint state)
    {
        if (state == 0x00080004)
            return; // end
        // we have two envcontrols that are always mirrored, don't know which is which
        if (state != stateA && state != stateB)
        {
            Module.ReportError(this, $"Unknown pattern: {state:X8}, expected {stateA:X8} or {stateB:X8}");
            return;
        }
        var value = state == stateA ? Pattern.A : Pattern.B;
        if (field != Pattern.Unknown && field != value)
            Module.ReportError(this, $"Inconsistent pattern assignments: {field} vs {value}");
        field = value;
    }

    //private void Print()
    //{
    //    var forgedDir = _forged[0].W.Radians();
    //    if (!forgedDir.AlmostEqual(_forged[1].W.Radians(), 0.1f))
    //    {
    //        Module.ReportError(this, $"Forged direction mismatch: {forgedDir} vs {_forged[1].W.Radians()}");
    //        return;
    //    }

    //    List<Vector4> srcs = [.. _initial.Where(x => x.W.Radians().AlmostEqual(forgedDir, 0.1f))];
    //    if (srcs.Count != 2)
    //    {
    //        Module.ReportError(this, $"{srcs.Count} sources for {forgedDir}");
    //        return;
    //    }

    //    var bigDir = _big.W.Radians();
    //    var bigSrc = _initial.FirstOrDefault(x => x.W.Radians().AlmostEqual(bigDir, 0.1f));
    //    if (bigSrc == default)
    //    {
    //        Module.ReportError(this, $"Big source not found for {bigDir}");
    //        return;
    //    }

    //    var envS = (_envc[1], _envc[2]) switch
    //    {
    //        (0x00800040, 0x02000100) => "A", // -135 => cross-inner, +135 => cross-outer
    //        (0x02000100, 0x00800040) => "B", // inverse
    //        _ => $"{_envc[1]:X8}/{_envc[2]:X8}"
    //    };
    //    var envN = (_envc[0], _envc[3]) switch
    //    {
    //        (0x00020001, 0x00200010) => "A", // -45 => R=Wide, +45 => R=KB
    //        (0x00200010, 0x00020001) => "B",
    //        _ => $"{_envc[0]:X8}/{_envc[3]:X8}"
    //    };

    //    var orthoBig = bigDir.ToDirection().OrthoR();
    //    var srcBig = (new WPos(bigSrc.XZ()) - Module.Center).Dot(orthoBig);
    //    var resBig = (new WPos(_big.XZ()) - Module.Center).Dot(orthoBig);
    //    var ortho = forgedDir.ToDirection().OrthoR();
    //    var src1 = (new WPos(srcs[0].XZ()) - Module.Center).Dot(ortho);
    //    var src2 = (new WPos(srcs[1].XZ()) - Module.Center).Dot(ortho);
    //    var res1 = (new WPos(_forged[0].XZ()) - Module.Center).Dot(ortho);
    //    var res2 = (new WPos(_forged[1].XZ()) - Module.Center).Dot(ortho);

    //    static (bool left, bool inner) classify(float x) => (x < 0, Math.Abs(x) < 5);
    //    var s1 = classify(src1);
    //    var s2 = classify(src2);
    //    var r1 = classify(res1);
    //    var r2 = classify(res2);
    //    var sb = classify(srcBig);
    //    var rb = classify(resBig);
    //    if (!rb.inner || (sb.inner ? sb.left == rb.left : sb.left != rb.left))
    //    {
    //        Module.ReportError(this, $"Unexpected big offset");
    //        return;
    //    }
    //    if (s1.inner == s2.inner)
    //    {
    //        Module.ReportError(this, $"{envS}/{envN}, {forgedDir}deg -> Symmetrical {(s1.inner ? "inner" : "outer")}, {bigDir}deg -> {(sb.left ? "L" : "R")}={(_split ? "Wide" : "KB")}");
    //        return;
    //    }
    //    if (r1.inner == r2.inner)
    //    {
    //        Module.ReportError(this, $"Weird: {src1}-{src2} > {res1}-{res2}");
    //        return;
    //    }

    //    // make order: inner/outer -> outer/inner
    //    if (!s1.inner)
    //        (s1, s2) = (s2, s1);
    //    if (r1.inner)
    //        (r1, r2) = (r2, r1);

    //    var innerCross = s1.left != r1.left;
    //    var outerCross = s2.left != r2.left;
    //    Module.ReportError(this, $"{envS}/{envN}, {forgedDir}deg -> Cross {(innerCross ? (outerCross ? "both" : "inner") : (outerCross ? "outer" : "none"))}, {bigDir}deg -> {(sb.left ? "L" : "R")}={(_split ? "Wide" : "KB")}");

    //    Array.Fill(_envc, 0u);
    //    _initial.Clear();
    //    _forged.Clear();
    //    _big = default;
    //}
}

class ForgedTrackKnockback(BossModule module) : Components.Knockback(module, AID.StormyEdgeKnockback)
{
    private readonly ForgedTrack? _main = module.FindComponent<ForgedTrack>();

    private static readonly AOEShapeRect _shape = new(20, 10);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_main == null)
            yield break;
        foreach (var aoe in _main.KnockbackAOEs)
        {
            yield return new(aoe.Origin, 7, aoe.Activation, _shape, aoe.Rotation + 90.Degrees(), Kind.DirForward);
            yield return new(aoe.Origin, 7, aoe.Activation, _shape, aoe.Rotation - 90.Degrees(), Kind.DirForward);
        }
    }
}
