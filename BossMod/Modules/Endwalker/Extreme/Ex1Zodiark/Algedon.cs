namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to algedon mechanic
class Algedon : BossComponent
{
    private Actor? _caster;

    private static readonly AOEShapeRect _shape = new(60, 15);

    public bool Done => _caster == null;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_shape.Check(actor.Position, _caster))
            hints.Add("GTFO from diagonal aoe!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        _shape.Draw(arena, _caster);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AlgedonAOE)
            _caster = caster;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AlgedonAOE)
            _caster = null;
    }
}
