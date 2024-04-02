namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to adikia mechanic
class Adikia : BossComponent
{
    private List<Actor> _casters = new();

    private static readonly AOEShapeCircle _shape = new(21);

    public bool Done => _casters.Count == 0;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_casters.Any(c => _shape.Check(actor.Position, c)))
            hints.Add("GTFO from side smash aoe!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var c in _casters)
            _shape.Draw(arena, c);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AdikiaL or AID.AdikiaR)
            _casters.Add(caster);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AdikiaL or AID.AdikiaR)
            _casters.Remove(caster);
    }
}
