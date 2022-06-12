namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn
{
    class Spectrum : CommonComponents.CastCounter
    {
        private static float _radius = 5;

        public Spectrum() : base(ActionID.MakeSpell(AID.BrightSpectrum)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            int tanksInRange = 0, nonTanksInRange = 0;
            foreach (var other in module.Raid.WithoutSlot().InRadiusExcluding(actor, _radius))
            {
                if (other.Role == Role.Tank)
                    ++tanksInRange;
                else
                    ++nonTanksInRange;
            }

            if (nonTanksInRange != 0 || actor.Role != Role.Tank && tanksInRange != 0)
                hints.Add("Spread!");

            if (actor.Role == Role.Tank && tanksInRange == 0)
                hints.Add("Stack with co-tank");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.AddCircle(pc.Position, _radius, ArenaColor.Danger);
            foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
                arena.Actor(player, player.Position.InCircle(pc.Position, _radius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
    }
}
