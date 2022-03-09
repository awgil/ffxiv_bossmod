using System.Linq;

namespace BossMod.P3S
{
    using static BossModule;

    // state related to heat of condemnation tethers
    class HeatOfCondemnation : CommonComponents.CastCounter
    {
        private P3S _module;
        private ulong _tetherTargets = 0;
        private ulong _inAnyAOE = 0; // players hit by aoe, excluding selves

        private static float _aoeRange = 6;

        public HeatOfCondemnation(P3S module)
            : base(ActionID.MakeSpell(AID.HeatOfCondemnationAOE))
        {
            _module = module;
        }

        public override void Update()
        {
            _tetherTargets = _inAnyAOE = 0;
            foreach ((int i, var player) in _module.Raid.WithSlot().Tethered(TetherID.HeatOfCondemnation))
            {
                BitVector.SetVector64Bit(ref _tetherTargets, i);
                _inAnyAOE |= _module.Raid.WithSlot().InRadiusExcluding(player, _aoeRange).Mask();
            }
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (actor.Role == Role.Tank)
            {
                if (_tetherTargets != 0 && actor.Tether.ID != (uint)TetherID.HeatOfCondemnation)
                {
                    hints.Add("Grab the tether!");
                }
                else if (_module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRange).Any())
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
                if (BitVector.IsVector64BitSet(_inAnyAOE, slot))
                {
                    hints.Add("GTFO from aoe!");
                }
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            // currently we always show tethered targets with circles, and if pc is a tank, also untethered players
            foreach ((int i, var player) in _module.Raid.WithSlot())
            {
                if (player.Tether.ID == (uint)TetherID.HeatOfCondemnation)
                {
                    arena.AddLine(player.Position, _module.PrimaryActor.Position, player.Role == Role.Tank ? arena.ColorSafe : arena.ColorDanger);
                    arena.Actor(player, arena.ColorDanger);
                    arena.AddCircle(player.Position, _aoeRange, arena.ColorDanger);
                }
                else if (pc.Role == Role.Tank)
                {
                    arena.Actor(player, BitVector.IsVector64BitSet(_inAnyAOE, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
            }
        }
    }
}
