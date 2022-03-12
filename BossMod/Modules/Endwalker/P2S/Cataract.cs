using System;

namespace BossMod.P2S
{
    using static BossModule;

    // state related to cataract mechanic
    class Cataract : Component
    {
        private P2S _module;
        private AOEShapeRect _aoeBoss = new(50, 7.5f, 50);
        private AOEShapeRect _aoeHead = new(50, 50);

        public Cataract(P2S module, bool winged)
        {
            _module = module;
            if (winged)
                _aoeHead.DirectionOffset = MathF.PI;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_aoeBoss.Check(actor.Position, _module.PrimaryActor) || _aoeHead.Check(actor.Position, _module.CataractHead()))
                hints.Add("GTFO from cataract!");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            _aoeBoss.Draw(arena, _module.PrimaryActor);
            _aoeHead.Draw(arena, _module.CataractHead());
        }
    }
}
