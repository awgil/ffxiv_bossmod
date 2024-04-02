namespace BossMod.Endwalker.Ultimate.DSW2;

class P7ExaflaresEdge : Components.Exaflare
{
    public P7ExaflaresEdge() : base(6)
    {
        ImminentColor = ArenaColor.AOE;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var p in SafeSpots(module))
            arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ExaflaresEdgeFirst)
        {
            var advance = 7 * spell.Rotation.ToDirection();
            Lines.Add(new() { Next = caster.Position, Advance = advance, NextExplosion = spell.NPCFinishAt, TimeToMove = 1.9f, ExplosionsLeft = 6, MaxShownExplosions = 1 });
            Lines.Add(new() { Next = caster.Position, Advance = advance.OrthoL(), NextExplosion = spell.NPCFinishAt, TimeToMove = 1.9f, ExplosionsLeft = 6, MaxShownExplosions = 1 });
            Lines.Add(new() { Next = caster.Position, Advance = advance.OrthoR(), NextExplosion = spell.NPCFinishAt, TimeToMove = 1.9f, ExplosionsLeft = 6, MaxShownExplosions = 1 });
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ExaflaresEdgeFirst or AID.ExaflaresEdgeRest)
        {
            foreach (var l in Lines.Where(l => l.Next.AlmostEqual(caster.Position, 1)))
            {
                AdvanceLine(module, l, caster.Position);
            }
            ++NumCasts;
        }
    }

    private IEnumerable<WPos> SafeSpots(BossModule module)
    {
        if (NumCasts > 0 || Lines.Count < 9)
            yield break;
        foreach (var l in MainLines().Where(l => !MainLines().Any(o => o != l && Clips(o, l.Next))))
            yield return l.Next;
    }

    private IEnumerable<Line> MainLines()
    {
        for (int i = 0; i < Lines.Count; i += 3)
            yield return Lines[i];
    }

    private bool Clips(Line l, WPos p)
    {
        var off = p - l.Next;
        var px = l.Advance.Dot(off);
        var pz = l.Advance.OrthoL().Dot(off);
        return Math.Abs(px) < 42 || Math.Abs(pz) < 42 && px > 0; // 42 == 6*7 (radius * advance)
    }
}
