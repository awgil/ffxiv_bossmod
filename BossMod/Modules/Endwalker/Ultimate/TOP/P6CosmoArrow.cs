namespace BossMod.Endwalker.Ultimate.TOP;

class P6CosmoArrow(BossModule module) : Components.GenericAOEs(module)
{
    public enum Pattern { Unknown, InOut, OutIn }
    public record struct Line(AOEShapeRect? Shape, WPos Next, Angle Direction, WDir Advance, DateTime NextExplosion, int ExplosionsLeft);

    public Pattern CurPattern { get; private set; }
    private readonly List<Line> _lines = [];

    public bool Active => _lines.Count > 0;

    private static readonly AOEShapeRect _shapeFirst = new(40, 5);
    private static readonly AOEShapeRect _shapeRest = new(100, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var l in _lines)
            if (l.Shape != null && l.ExplosionsLeft > 0)
                yield return new(l.Shape, l.Next, l.Direction, l.NextExplosion);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurPattern != Pattern.Unknown)
            hints.Add($"Pattern: {(CurPattern == Pattern.InOut ? "in -> out" : "out -> in")}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CosmoArrowFirst)
        {
            var offset = caster.Position - Module.Center;
            var offsetAbs = offset.Abs();
            if (offsetAbs.X < 5)
            {
                // central vertical
                _lines.Add(new(_shapeFirst, caster.Position, spell.Rotation, new(1, 0), spell.NPCFinishAt, 4));
                _lines.Add(new(null, caster.Position, spell.Rotation, new(-1, 0), spell.NPCFinishAt, 4));
                if (CurPattern == Pattern.Unknown)
                    CurPattern = Pattern.InOut;
            }
            else if (offsetAbs.Z < 5)
            {
                // central horizontal
                _lines.Add(new(_shapeFirst, caster.Position, spell.Rotation, new(0, 1), spell.NPCFinishAt, 4));
                _lines.Add(new(null, caster.Position, spell.Rotation, new(0, -1), spell.NPCFinishAt, 4));
                if (CurPattern == Pattern.Unknown)
                    CurPattern = Pattern.InOut;
            }
            else if (offsetAbs.X < 18)
            {
                // side vertical
                _lines.Add(new(_shapeFirst, caster.Position, spell.Rotation, new(offset.X < 0 ? 1 : -1, 0), spell.NPCFinishAt, 7));
                if (CurPattern == Pattern.Unknown)
                    CurPattern = Pattern.OutIn;
            }
            else if (offsetAbs.Z < 18)
            {
                // side horizontal
                _lines.Add(new(_shapeFirst, caster.Position, spell.Rotation, new(0, offset.Z < 0 ? 1 : -1), spell.NPCFinishAt, 7));
                if (CurPattern == Pattern.Unknown)
                    CurPattern = Pattern.OutIn;
            }
            else
            {
                ReportError($"Unexpected exasquare origin: {caster.Position}");
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var dist = (AID)spell.Action.ID switch
        {
            AID.CosmoArrowFirst => 7.5f,
            AID.CosmoArrowRest => 5.0f,
            _ => 0
        };
        if (dist == 0)
            return;

        ++NumCasts;

        int numLines = 0;
        foreach (ref var l in _lines.AsSpan())
        {
            if (!l.Next.AlmostEqual(caster.Position, 1) || !l.Direction.AlmostEqual(caster.Rotation, 0.1f) || (l.NextExplosion - WorldState.CurrentTime).TotalSeconds > 1)
                continue;

            if (l.ExplosionsLeft <= 0)
                ReportError($"Too many explosions: {caster.Position}");

            l.Shape = _shapeRest;
            l.Next += l.Advance * dist;
            l.NextExplosion = WorldState.FutureTime(2);
            --l.ExplosionsLeft;
            ++numLines;
        }
        if (numLines == 0)
            ReportError($"Failed to match any lines for {caster}");
    }
}
