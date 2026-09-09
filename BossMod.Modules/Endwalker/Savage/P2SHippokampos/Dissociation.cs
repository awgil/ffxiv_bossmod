namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to dissociation mechanic
class Dissociation(BossModule module) : BossComponent(module)
{
    private AOEShapeRect? _shape = new(50, 10);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var head = Module.Enemies(OID.DissociatedHead).FirstOrDefault();
        if (_shape == null || head == null || Module.InBounds(head.Position))
            return; // inactive or head not teleported yet

        if (_shape.Check(actor.Position, head))
        {
            hints.Add("GTFO from dissociation!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var head = Module.Enemies(OID.DissociatedHead).FirstOrDefault();
        if (_shape == null || head == null || Module.InBounds(head.Position))
            return; // inactive or head not teleported yet

        _shape.Draw(Arena, head);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster.OID == (uint)OID.DissociatedHead && (AID)spell.Action.ID == AID.DissociationAOE)
            _shape = null;
    }
}
