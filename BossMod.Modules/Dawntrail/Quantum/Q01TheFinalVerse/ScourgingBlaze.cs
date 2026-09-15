namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

class ScourgingBlaze(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(5))
{
    private WDir _nextDir;
    private WPos? _safeSpot;

    public bool Draw;

    private readonly List<(WPos Source, WDir Direction)> _orbs = [];

    private readonly WPos[] _safeSpots = [new(-605.75f, -287), new(-594.25f, -287), new(-605.75f, -313), new(-594.25f, -313)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? base.ActiveAOEs(slot, actor) : [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ScourgingBlazeHorizontalFirst or AID.ScourgingBlazeHorizontalSecond)
            _nextDir = new WDir(1, 0);

        if ((AID)spell.Action.ID is AID.ScourgingBlazeVerticalFirst or AID.ScourgingBlazeVerticalSecond)
            _nextDir = new WDir(0, 1);

        if ((AID)spell.Action.ID == AID.CrystalAppear)
        {
            _orbs.Add((spell.TargetXZ, _nextDir));
            CalcSafespot();
        }

        if ((AID)spell.Action.ID is AID.ScourgingBlazeFirst)
        {
            _safeSpot = null;
            NumCasts++;
            var lines = Lines.Where(l => l.Next.InCircle(spell.TargetXZ, 1));
            foreach (var l in lines)
                AdvanceLine(l, spell.TargetXZ);
        }

        if ((AID)spell.Action.ID is AID.ScourgingBlazeRest)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(spell.TargetXZ, 0.5f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], spell.TargetXZ);
            else
                ReportError($"unrecognized exaflare at {spell.TargetXZ}");

            Lines.RemoveAll(l => l.ExplosionsLeft < 1);
            if (Lines.Count == 0)
            {
                Draw = false;
                NumCasts = 0;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ScourgingBlazeHorizontalFirst or AID.ScourgingBlazeVerticalFirst)
        {
            _orbs.Clear();
            Lines.Clear();
        }

        if ((AID)spell.Action.ID == AID.ScourgingBlazeFirst)
        {
            foreach (var orb in _orbs)
            {
                if (orb.Source.InCircle(spell.LocXZ, 1))
                {
                    AddLine(spell.LocXZ, orb.Direction * 4, Module.CastFinishAt(spell));
                    AddLine(spell.LocXZ, orb.Direction * -4, Module.CastFinishAt(spell));
                }
            }
        }
    }

    private void AddLine(WPos source, WDir advance, DateTime next)
    {
        var numExplosions = 0;
        var tmp = source;
        while (tmp.InRect(Arena.Center, 0.Degrees(), 14.5f, 14.5f, 19.5f))
        {
            tmp += advance;
            numExplosions++;
        }

        Lines.Add(new()
        {
            Next = source,
            Advance = advance,
            NextExplosion = next,
            TimeToMove = 1.2f,
            ExplosionsLeft = numExplosions,
            MaxShownExplosions = Math.Min(5, numExplosions)
        });
    }

    private void CalcSafespot()
    {
        var candidates = _safeSpots.ToList();
        foreach (var orb in _orbs)
            candidates.RemoveAll(c => c.InRect(orb.Source, orb.Direction, 60, 60, 5));

        if (candidates.Count == 1)
            _safeSpot = candidates[0];
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (!Draw && _safeSpot is { } spot)
            Arena.AddCircle(spot, 0.75f, ArenaColor.Safe, 1);
    }
}
