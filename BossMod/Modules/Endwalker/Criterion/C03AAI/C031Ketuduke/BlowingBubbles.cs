namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

class BlowingBubbles(BossModule module) : BossComponent(module)
{
    private List<Actor> _actors = new();

    private static readonly AOEShapeCircle _shape = new(5);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NAiryBubbleExaflare or OID.SAiryBubbleExaflare)
            _actors.Add(actor);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var a in _actors)
            _shape.Draw(Arena, a);
    }
}
