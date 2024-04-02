namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

class Elenchos : BossComponent
{
    private Actor? _center;
    private List<Actor> _sides = new();

    private static readonly AOEShapeRect _aoeCenter = new(40, 7);
    private static readonly AOEShapeRect _aoeSides = new(40, 6.5f, 40);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_aoeCenter.Check(actor.Position, _center) || _sides.Any(s => _aoeSides.Check(actor.Position, s)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        _aoeCenter.Draw(arena, _center);
        foreach (var s in _sides)
            _aoeSides.Draw(arena, s);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
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

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
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
