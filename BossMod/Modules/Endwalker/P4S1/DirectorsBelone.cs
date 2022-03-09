using System.Linq;

namespace BossMod.P4S1
{
    using static BossModule;

    // state related to director's belone (debuffs) mechanic
    class DirectorsBelone : Component
    {
        private P4S1 _module;
        private bool _assignFromCoils = false;
        private ulong _debuffForbidden = 0;
        private ulong _debuffTargets = 0;
        private ulong _debuffImmune = 0;

        private static float _debuffPassRange = 3; // not sure about this...

        public DirectorsBelone(P4S1 module, bool fromBloodrake)
        {
            _module = module;
            if (fromBloodrake)
            {
                _debuffForbidden = _module.Raid.WithSlot().Tethered(TetherID.Bloodrake).Mask();
            }
            else
            {
                _assignFromCoils = true; // assignment happens with some delay
            }
        }

        public override void Update()
        {
            if (_assignFromCoils)
            {
                var coils = _module.FindComponent<BeloneCoils>();
                if (coils != null && coils.ActiveSoakers != BeloneCoils.Soaker.Unknown)
                {
                    _debuffForbidden = _module.Raid.WithSlot().WhereActor(coils.IsValidSoaker).Mask();
                    _assignFromCoils = false;
                }
            }
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_debuffForbidden == 0)
                return;

            if (!BitVector.IsVector64BitSet(_debuffForbidden, slot))
            {
                // we should be grabbing debuff
                if (_debuffTargets == 0)
                {
                    // debuffs not assigned yet => spread and prepare to grab
                    bool stacked = _module.Raid.WithoutSlot().InRadiusExcluding(actor, _debuffPassRange).Any();
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
                    bool badStack = _module.Raid.WithSlot().Exclude(slot).IncludedInMask(_debuffForbidden).OutOfRadius(actor.Position, _debuffPassRange).Any();
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

        public override void AddGlobalHints(GlobalHints hints)
        {
            if (_debuffForbidden != 0)
            {
                var forbidden = _module.Raid.WithSlot().IncludedInMask(_debuffForbidden).FirstOrDefault().Item2.Role;
                hints.Add($"Stack: {(forbidden == Role.Tank || forbidden == Role.Healer ? "Tanks/Healers" : "DD")}");
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_debuffTargets == 0)
                return;

            ulong failingPlayers = _debuffForbidden & _debuffTargets;
            foreach ((int i, var player) in _module.Raid.WithSlot())
            {
                bool failing = BitVector.IsVector64BitSet(failingPlayers, i);
                arena.Actor(player, failing ? arena.ColorDanger : arena.ColorPlayerGeneric);
            }
        }

        public override void OnStatusGain(Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.RoleCall:
                    ModifyDebuff(actor, ref _debuffTargets, true);
                    break;
                case SID.Miscast:
                    ModifyDebuff(actor, ref _debuffImmune, true);
                    break;
            }
        }

        public override void OnStatusLose(Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.RoleCall:
                    ModifyDebuff(actor, ref _debuffTargets, false);
                    break;
                case SID.Miscast:
                    ModifyDebuff(actor, ref _debuffImmune, false);
                    break;
            }
        }

        public override void OnEventCast(CastEvent info)
        {
            if (info.IsSpell(AID.CursedCasting1) || info.IsSpell(AID.CursedCasting2))
                _debuffForbidden = 0;
        }

        private void ModifyDebuff(Actor actor, ref ulong vector, bool active)
        {
            int slot = _module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                BitVector.ModifyVector64Bit(ref vector, slot, active);
        }
    }
}
