using System.Linq;

namespace BossMod.Endwalker.P2S
{
    class OminousBubbling : CommonComponents.CastCounter
    {
        private static float _radius = 6;

        public OminousBubbling() : base(ActionID.MakeSpell(AID.OminousBubblingAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            int healersInRange = module.Raid.WithoutSlot().Where(a => a.Role == Role.Healer).InRadius(actor.Position, _radius).Count();
            if (healersInRange > 1)
                hints.Add("Hit by two aoes!");
            else if (healersInRange == 0)
                hints.Add("Stack with healer!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var player in module.Raid.WithoutSlot())
            {
                if (player.Role == Role.Healer)
                {
                    arena.Actor(player, arena.ColorDanger);
                    arena.AddCircle(player.Position, _radius, arena.ColorDanger);
                }
                else
                {
                    arena.Actor(player, arena.ColorPlayerGeneric);
                }
            }
        }
    }
}
