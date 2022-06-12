using System;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P2AscalonMight : CommonComponents.CastCounter
    {
        private static AOEShapeCone _aoe = new(50, 30.Degrees());

        public P2AscalonMight() : base(ActionID.MakeSpell(AID.AscalonsMight)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (module.PrimaryActor.TargetID != actor.InstanceID && _aoe.Check(actor.Position, module.PrimaryActor))
                hints.Add("GTFO from cleave!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _aoe.Draw(arena, module.PrimaryActor);
        }
    }

}
