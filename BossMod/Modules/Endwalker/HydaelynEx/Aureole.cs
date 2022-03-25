using System;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    // component tracking [lateral] aureole mechanic
    class Aureole : Component
    {
        public bool Done { get; private set; }
        private AOEShapeCone _aoe = new(40, 5 * MathF.PI / 12);

        public override void Init(BossModule module)
        {
            _aoe.DirectionOffset = (AID)(module.PrimaryActor.CastInfo?.Action.ID ?? 0) is AID.LateralAureole1 or AID.LateralAureole2 ? -MathF.PI / 2 : 0;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_aoe.Check(actor.Position, module.PrimaryActor) || _aoe.Check(actor.Position, module.PrimaryActor.Position, module.PrimaryActor.Rotation + MathF.PI))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _aoe.Draw(arena, module.PrimaryActor);
            _aoe.Draw(arena, module.PrimaryActor.Position, module.PrimaryActor.Rotation + MathF.PI);
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell() && (AID)info.Action.ID is AID.Aureole1AOE or AID.Aureole2AOE or AID.LateralAureole1AOE or AID.LateralAureole2AOE)
                Done = true;
        }
    }
}
