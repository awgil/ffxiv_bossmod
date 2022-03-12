namespace BossMod.P2S
{
    using static BossModule;

    // state related to dissociation mechanic
    class Dissociation : Component
    {
        private P2S _module;
        private AOEShapeRect? _shape = new(50, 10);

        public Dissociation(P2S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var head = _module.DissociatedHead();
            if (_shape == null || head == null || _module.Arena.InBounds(head.Position))
                return; // inactive or head not teleported yet

            if (_shape.Check(actor.Position, head))
            {
                hints.Add("GTFO from dissociation!");
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            var head = _module.DissociatedHead();
            if (_shape == null || head == null || _module.Arena.InBounds(head.Position))
                return; // inactive or head not teleported yet

            _shape.Draw(arena, head);
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor == _module.DissociatedHead() && actor.CastInfo!.IsSpell(AID.DissociationAOE))
                _shape = null;
        }
    }
}
