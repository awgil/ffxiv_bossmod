namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

class BlowingBubbles : BossComponent
{
    private List<Actor> _actors = new();

    private static readonly AOEShapeCircle _shape = new(5);

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.NAiryBubbleExaflare or OID.SAiryBubbleExaflare)
            _actors.Add(actor);
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var a in _actors)
            _shape.Draw(arena, a);
    }
}
