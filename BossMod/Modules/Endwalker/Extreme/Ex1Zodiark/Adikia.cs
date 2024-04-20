namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to adikia mechanic
class Adikia(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _casters = [];

    private static readonly AOEShapeCircle _shape = new(21);

    public bool Done => _casters.Count == 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_casters.Any(c => _shape.Check(actor.Position, c)))
            hints.Add("GTFO from side smash aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in _casters)
            _shape.Draw(Arena, c);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AdikiaL or AID.AdikiaR)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AdikiaL or AID.AdikiaR)
            _casters.Remove(caster);
    }
}
