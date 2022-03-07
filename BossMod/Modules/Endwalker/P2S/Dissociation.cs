namespace BossMod.P2S
{
    using static BossModule;

    // state related to dissociation mechanic
    class Dissociation : Component
    {
        private P2S _module;
        private bool _done = false;

        private static float _halfWidth = 10;

        public Dissociation(P2S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var head = _module.DissociatedHead();
            if (_done || head == null || _module.Arena.InBounds(head.Position))
                return; // inactive or head not teleported yet

            if (GeometryUtils.PointInRect(actor.Position - head.Position, head.Rotation, 50, 0, _halfWidth))
            {
                hints.Add("GTFO from dissociation!");
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            var head = _module.DissociatedHead();
            if (_done || head == null || _module.Arena.InBounds(head.Position))
                return; // inactive or head not teleported yet

            arena.ZoneQuad(head.Position, head.Rotation, 50, 0, _halfWidth, arena.ColorAOE);
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor == _module.DissociatedHead() && actor.CastInfo!.IsSpell(AID.DissociationAOE))
                _done = true;
        }
    }
}
