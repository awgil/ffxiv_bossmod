namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P7ExaflaresEdge : Components.Exaflare
{
    public P7ExaflaresEdge(BossModule module) : base(module, 6f)
    {
        ImminentColor = Colors.AOE;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in SafeSpots())
            Arena.ZoneCircleOutline(p, 1f, Colors.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExaflaresEdgeFirst)
        {
            var advance = 7f * spell.Rotation.ToDirection();
            AddLine(advance);
            AddLine(advance.OrthoL());
            AddLine(advance.OrthoR());

            void AddLine(WDir dir)
            => Lines.Add(new(caster.Position, dir, Module.CastFinishAt(spell), 1.9d, 6, 1));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ExaflaresEdgeFirst or (uint)AID.ExaflaresEdgeRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var l = Lines[i];
                if (l.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(l, pos);
                }
            }
            ++NumCasts;
        }
    }

    private List<WPos> SafeSpots()
    {
        if (NumCasts > 0 || Lines.Count < 9)
            return [];

        List<WPos> safespots = [];
        var mainLines = MainLines();
        var len = mainLines.Length;

        for (var i = 0; i < len; ++i)
        {
            var l = mainLines[i];
            var isSafe = true;
            for (var j = 0; j < len; ++j)
            {
                var o = mainLines[j];
                if (o != l && Clips(o, l.Next))
                {
                    isSafe = false;
                    break;
                }
            }
            if (isSafe)
                safespots.Add(l.Next);
        }
        return safespots;
    }

    private Line[] MainLines()
    {
        var count = Lines.Count;
        var mainlines = new Line[count / 3];
        var index = 0;
        for (var i = 0; i < count; i += 3)
            mainlines[index++] = Lines[i];
        return mainlines;
    }

    private bool Clips(Line l, WPos p)
    {
        var off = p - l.Next;
        var px = l.Advance.Dot(off);
        var pz = l.Advance.OrthoL().Dot(off);
        return Math.Abs(px) < 42 || Math.Abs(pz) < 42f && px > 0f; // 42 == 6*7 (radius * advance)
    }
}
