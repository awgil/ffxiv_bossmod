using System.Linq;

namespace BossMod.Endwalker.P4S1
{
    using static BossModule;

    // state related to director's belone (debuffs) mechanic
    // note that forbidden targets are selected either from bloodrake tethers (first instance of mechanic) or from tower types (second instance of mechanic)
    class DirectorsBelone : Component
    {
        private bool _assigned = false;
        private ulong _debuffForbidden = 0;
        private ulong _debuffTargets = 0;
        private ulong _debuffImmune = 0;

        private static float _debuffPassRange = 3; // not sure about this...

        public override void Update(BossModule module)
        {
            if (!_assigned)
            {
                var coils = module.FindComponent<BeloneCoils>();
                if (coils == null)
                {
                    // assign from bloodrake tethers
                    _debuffForbidden = module.Raid.WithSlot().Tethered(TetherID.Bloodrake).Mask();
                    _assigned = true;
                }
                else if (coils.ActiveSoakers != BeloneCoils.Soaker.Unknown)
                {
                    // assign from coils (note that it happens with some delay)
                    _debuffForbidden = module.Raid.WithSlot().WhereActor(coils.IsValidSoaker).Mask();
                    _assigned = true;
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_debuffForbidden == 0)
                return;

            if (!BitVector.IsVector64BitSet(_debuffForbidden, slot))
            {
                // we should be grabbing debuff
                if (_debuffTargets == 0)
                {
                    // debuffs not assigned yet => spread and prepare to grab
                    bool stacked = module.Raid.WithoutSlot().InRadiusExcluding(actor, _debuffPassRange).Any();
                    hints.Add("Debuffs: spread and prepare to handle!", stacked);
                }
                else if (BitVector.IsVector64BitSet(_debuffImmune, slot))
                {
                    hints.Add("Debuffs: failed to handle");
                }
                else if (BitVector.IsVector64BitSet(_debuffTargets, slot))
                {
                    hints.Add("Debuffs: OK", false);
                }
                else
                {
                    hints.Add("Debuffs: grab!");
                }
            }
            else
            {
                // we should be passing debuff
                if (_debuffTargets == 0)
                {
                    bool badStack = module.Raid.WithSlot().Exclude(slot).IncludedInMask(_debuffForbidden).OutOfRadius(actor.Position, _debuffPassRange).Any();
                    hints.Add("Debuffs: stack and prepare to pass!", badStack);
                }
                else if (BitVector.IsVector64BitSet(_debuffTargets, slot))
                {
                    hints.Add("Debuffs: pass!");
                }
                else
                {
                    hints.Add("Debuffs: avoid", false);
                }
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            var forbidden = module.Raid.WithSlot(true).IncludedInMask(_debuffForbidden).FirstOrDefault().Item2;
            if (forbidden != null)
            {
                hints.Add($"Stack: {(forbidden.Role is Role.Tank or Role.Healer ? "Tanks/Healers" : "DD")}");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_debuffTargets == 0)
                return;

            ulong failingPlayers = _debuffForbidden & _debuffTargets;
            foreach ((int i, var player) in module.Raid.WithSlot())
            {
                bool failing = BitVector.IsVector64BitSet(failingPlayers, i);
                arena.Actor(player, failing ? arena.ColorDanger : arena.ColorPlayerGeneric);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.RoleCall:
                    ModifyDebuff(module, actor, ref _debuffTargets, true);
                    break;
                case SID.Miscast:
                    ModifyDebuff(module, actor, ref _debuffImmune, true);
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.RoleCall:
                    ModifyDebuff(module, actor, ref _debuffTargets, false);
                    break;
                case SID.Miscast:
                    ModifyDebuff(module, actor, ref _debuffImmune, false);
                    break;
            }
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.CursedCasting1) || info.IsSpell(AID.CursedCasting2))
                _debuffForbidden = 0;
        }

        private void ModifyDebuff(BossModule module, Actor actor, ref ulong vector, bool active)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                BitVector.ModifyVector64Bit(ref vector, slot, active);
        }
    }
}
