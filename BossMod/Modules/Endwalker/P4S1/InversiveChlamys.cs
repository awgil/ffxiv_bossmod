using System.Linq;

namespace BossMod.Endwalker.P4S1
{
    using static BossModule;

    // state related to inversive chlamys mechanic (tethers)
    // note that forbidden targets are selected either from bloodrake tethers (first instance of mechanic) or from tower types (second instance of mechanic)
    class InversiveChlamys : Component
    {
        private bool _assigned = false;
        private ulong _tetherForbidden = 0;
        private ulong _tetherTargets = 0;
        private ulong _tetherInAOE = 0;

        private static float _aoeRange = 5;

        public bool TethersActive => _tetherTargets != 0;

        public override void Update(BossModule module)
        {
            if (!_assigned)
            {
                var coils = module.FindComponent<BeloneCoils>();
                if (coils == null)
                {
                    // assign from bloodrake tethers
                    _tetherForbidden = module.Raid.WithSlot().Tethered(TetherID.Bloodrake).Mask();
                    _assigned = true;
                }
                else if (coils.ActiveSoakers != BeloneCoils.Soaker.Unknown)
                {
                    // assign from coils (note that it happens with some delay)
                    _tetherForbidden = module.Raid.WithSlot().WhereActor(coils.IsValidSoaker).Mask();
                    _assigned = true;
                }
            }

            _tetherTargets = _tetherInAOE = 0;
            if (_tetherForbidden == 0)
                return;

            foreach ((int i, var player) in module.Raid.WithSlot().Tethered(TetherID.Chlamys))
            {
                BitVector.SetVector64Bit(ref _tetherTargets, i);
                _tetherInAOE |= module.Raid.WithSlot().InRadiusExcluding(player, _aoeRange).Mask();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_tetherForbidden == 0)
                return;

            if (!BitVector.IsVector64BitSet(_tetherForbidden, slot))
            {
                // we should be grabbing tethers
                if (_tetherTargets == 0)
                {
                    hints.Add("Tethers: prepare to intercept", false);
                }
                else if (!BitVector.IsVector64BitSet(_tetherTargets, slot))
                {
                    hints.Add("Tethers: intercept!");
                }
                else if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRange).Any())
                {
                    hints.Add("Tethers: GTFO from others!");
                }
                else
                {
                    hints.Add("Tethers: OK", false);
                }
            }
            else
            {
                // we should be passing tethers
                if (_tetherTargets == 0)
                {
                    hints.Add("Tethers: prepare to pass", false);
                }
                else if (BitVector.IsVector64BitSet(_tetherTargets, slot))
                {
                    hints.Add("Tethers: pass!");
                }
                else if (BitVector.IsVector64BitSet(_tetherInAOE, slot))
                {
                    hints.Add("Tethers: GTFO from aoe!");
                }
                else
                {
                    hints.Add("Tethers: avoid", false);
                }
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            var forbidden = module.Raid.WithSlot(true).IncludedInMask(_tetherForbidden).FirstOrDefault().Item2;
            if (forbidden != null)
            {
                hints.Add($"Intercept: {(forbidden.Role is Role.Tank or Role.Healer ? "DD" : "Tanks/Healers")}");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_tetherTargets == 0)
                return;

            ulong failingPlayers = _tetherForbidden & _tetherTargets;
            foreach ((int i, var player) in module.Raid.WithSlot())
            {
                bool failing = BitVector.IsVector64BitSet(failingPlayers, i);
                bool inAOE = BitVector.IsVector64BitSet(_tetherInAOE, i);
                arena.Actor(player, failing ? arena.ColorDanger : (inAOE ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric));

                if (player.Tether.ID == (uint)TetherID.Chlamys)
                {
                    arena.AddLine(player.Position, module.PrimaryActor.Position, failing ? arena.ColorDanger : arena.ColorSafe);
                    arena.AddCircle(player.Position, _aoeRange, arena.ColorDanger);
                }
            }
        }
    }
}
