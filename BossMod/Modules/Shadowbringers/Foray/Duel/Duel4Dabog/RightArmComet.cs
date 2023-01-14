using System.Linq;

namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog
{
    class RightArmComet : Components.KnockbackFromCastTarget
    {
        private static float _radius = 5;

        public RightArmComet(AID aid, float distance) : base(ActionID.MakeSpell(aid), distance, shape: new AOEShapeCircle(_radius)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (Casters.Any(c => !Shape!.Check(actor.Position, c)))
                hints.Add("Soak the tower!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            foreach (var c in Casters)
                arena.AddCircle(c.Position, _radius, pc.Position.InCircle(c.Position, _radius) ? ArenaColor.Safe : ArenaColor.Danger, 2);
        }
    }

    class RightArmCometShort : RightArmComet
    {
        public RightArmCometShort() : base(AID.RightArmCometKnockbackShort, 12) { }
    }

    class RightArmCometLong : RightArmComet
    {
        public RightArmCometLong() : base(AID.RightArmCometKnockbackLong, 25) { }
    }
}
