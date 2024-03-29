namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

class Endsong : BossComponent
{
    private List<Actor> _active = new();

    private static readonly AOEShapeCircle _aoe = new(15);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_active.Any(a => _aoe.Check(actor.Position, a)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var a in _active)
            _aoe.Draw(arena, a);
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.EndsongFirst or TetherID.EndsongNext)
            _active.Add(source);
    }

    public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        _active.Remove(source);
    }
}
