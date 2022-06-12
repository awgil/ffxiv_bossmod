using System.Linq;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to heat of condemnation tethers
    class HeatOfCondemnation : CommonComponents.CastCounter
    {
        private BitMask _tetherTargets;
        private BitMask _inAnyAOE; // players hit by aoe, excluding selves

        private static float _aoeRange = 6;

        public HeatOfCondemnation() : base(ActionID.MakeSpell(AID.HeatOfCondemnationAOE)) { }

        public override void Update(BossModule module)
        {
            _tetherTargets = _inAnyAOE = new();
            foreach ((int i, var player) in module.Raid.WithSlot().Tethered(TetherID.HeatOfCondemnation))
            {
                _tetherTargets.Set(i);
                _inAnyAOE |= module.Raid.WithSlot().InRadiusExcluding(player, _aoeRange).Mask();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (actor.Role == Role.Tank)
            {
                if (_tetherTargets.Any() && actor.Tether.ID != (uint)TetherID.HeatOfCondemnation)
                {
                    hints.Add("Grab the tether!");
                }
                else if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRange).Any())
                {
                    hints.Add("GTFO from raid!");
                }
            }
            else
            {
                if (actor.Tether.ID == (uint)TetherID.HeatOfCondemnation)
                {
                    hints.Add("Hit by tankbuster");
                }
                if (_inAnyAOE[slot])
                {
                    hints.Add("GTFO from aoe!");
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            // currently we always show tethered targets with circles, and if pc is a tank, also untethered players
            foreach ((int i, var player) in module.Raid.WithSlot())
            {
                if (player.Tether.ID == (uint)TetherID.HeatOfCondemnation)
                {
                    arena.AddLine(player.Position, module.PrimaryActor.Position, player.Role == Role.Tank ? ArenaColor.Safe : ArenaColor.Danger);
                    arena.Actor(player, ArenaColor.Danger);
                    arena.AddCircle(player.Position, _aoeRange, ArenaColor.Danger);
                }
                else if (pc.Role == Role.Tank)
                {
                    arena.Actor(player, _inAnyAOE[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
                }
            }
        }
    }
}
