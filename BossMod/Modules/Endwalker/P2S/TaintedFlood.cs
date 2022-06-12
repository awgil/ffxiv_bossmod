using System.Linq;

namespace BossMod.Endwalker.P2S
{
    // note: if activated together with ChannelingFlow, it does not target next flow arrows
    class TaintedFlood : CommonComponents.CastCounter
    {
        private BitMask _ignoredTargets;

        private static float _radius = 6;

        public TaintedFlood() : base(ActionID.MakeSpell(AID.TaintedFloodAOE)) { }

        public override void Init(BossModule module)
        {
            var flow = module.FindComponent<ChannelingFlow>();
            if (flow != null)
            {
                _ignoredTargets = module.Raid.WithSlot().WhereSlot(s => flow.SlotActive(module, s)).Mask();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (NumCasts > 0)
                return;

            if (_ignoredTargets[slot])
            {
                // player is not a target of flood, so just make sure he is not clipped by others
                if (module.Raid.WithSlot().ExcludedFromMask(_ignoredTargets).InRadius(actor.Position, _radius).Any())
                    hints.Add("GTFO from flood!");
            }
            else
            {
                // player is target of flood => make sure no one is in range
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _radius).Any())
                    hints.Add("Spread!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (NumCasts > 0)
                return;

            if (_ignoredTargets[pcSlot])
            {
                foreach ((_, var actor) in module.Raid.WithSlot().ExcludedFromMask(_ignoredTargets))
                {
                    arena.Actor(actor, ArenaColor.Danger);
                    arena.AddCircle(actor.Position, _radius, ArenaColor.Danger);
                }
            }
            else
            {
                arena.AddCircle(pc.Position, _radius, ArenaColor.Danger);
                foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
                    arena.Actor(player, player.Position.InCircle(pc.Position, _radius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
            }
        }
    }
}
