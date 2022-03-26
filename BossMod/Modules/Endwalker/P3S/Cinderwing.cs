using System;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to cinderwing
    class Cinderwing : Component
    {
        private AOEShapeCone _aoe = new(60, MathF.PI / 2);

        public override void Init(BossModule module)
        {
            _aoe.DirectionOffset = (AID)(module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
            {
                AID.RightCinderwing => -MathF.PI / 2,
                AID.LeftCinderwing => MathF.PI / 2,
                _ => 0
            };
            if (_aoe.DirectionOffset == 0)
                module.ReportError(this, $"Failed to initialize cinderwing; unexpected boss cast {module.PrimaryActor.CastInfo?.Action}");
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_aoe.Check(actor.Position, module.PrimaryActor))
                hints.Add("GTFO from wing!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _aoe.Draw(arena, module.PrimaryActor);
        }
    }
}
