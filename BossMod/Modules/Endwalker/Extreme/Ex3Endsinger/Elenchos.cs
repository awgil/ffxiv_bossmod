namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

class Elenchos(BossModule module) : BossComponent(module)
{
    private Actor? _center;
    private readonly List<Actor> _sides = [];

    private static readonly AOEShapeRect _aoeCenter = new(40, 7);
    private static readonly AOEShapeRect _aoeSides = new(40, 6.5f, 40);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoeCenter.Check(actor.Position, _center) || _sides.Any(s => _aoeSides.Check(actor.Position, s)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _aoeCenter.Draw(Arena, _center);
        foreach (var s in _sides)
            _aoeSides.Draw(Arena, s);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ElenchosCenter:
                _center = caster;
                break;
            case AID.ElenchosSidesAOE:
                _sides.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ElenchosCenter:
                _center = null;
                break;
            case AID.ElenchosSidesAOE:
                _sides.Remove(caster);
                break;
        }
    }
}
