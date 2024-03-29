namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to dissociation mechanic
class Dissociation : BossComponent
{
    private AOEShapeRect? _shape = new(50, 10);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var head = module.Enemies(OID.DissociatedHead).FirstOrDefault();
        if (_shape == null || head == null || module.Bounds.Contains(head.Position))
            return; // inactive or head not teleported yet

        if (_shape.Check(actor.Position, head))
        {
            hints.Add("GTFO from dissociation!");
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var head = module.Enemies(OID.DissociatedHead).FirstOrDefault();
        if (_shape == null || head == null || module.Bounds.Contains(head.Position))
            return; // inactive or head not teleported yet

        _shape.Draw(arena, head);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (caster.OID == (uint)OID.DissociatedHead && (AID)spell.Action.ID == AID.DissociationAOE)
            _shape = null;
    }
}
