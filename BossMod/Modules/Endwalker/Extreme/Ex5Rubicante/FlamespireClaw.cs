using System;

namespace BossMod.Endwalker.Extreme.Ex5Rubicante
{
    class FlamespireClaw : Components.GenericBaitAway
    {
        private int[] _order = new int[PartyState.MaxPartySize];
        private BitMask _tethers;

        private static AOEShapeCone _shape = new(20, 45.Degrees()); // TODO: verify angle

        public FlamespireClaw() : base(ActionID.MakeSpell(AID.FlamespireClawAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            var order = _order[slot];
            if (order != 0 && NumCasts < 8)
            {
                hints.Add($"Order: {order}", false);
                bool shouldBeTethered = order switch
                {
                    1 => NumCasts is 1 or 2,
                    2 => NumCasts is 2 or 3,
                    3 => NumCasts is 3 or 4,
                    4 => NumCasts is 4 or 5,
                    5 => NumCasts is 5 or 6,
                    6 => NumCasts is 6 or 7,
                    7 => NumCasts is 7 or 0,
                    _ => NumCasts is 0 or 1,
                };
                if (shouldBeTethered != _tethers[slot])
                    hints.Add(shouldBeTethered ? "Intercept tether!" : "Pass the tether!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            foreach (var (_, player) in module.Raid.WithSlot(true).IncludedInMask(_tethers))
                arena.AddLine(player.Position, module.PrimaryActor.Position, ArenaColor.Danger);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
            {
                CurrentBaits.Clear();
                var nextSlot = Array.IndexOf(_order, NumCasts + 1);
                var nextTarget = nextSlot >= 0 ? module.Raid[nextSlot] : null;
                if (nextTarget != null)
                    CurrentBaits.Add(new(module.PrimaryActor, nextTarget, _shape));
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.FlamespireClaw)
                _tethers.Set(module.Raid.FindSlot(source.InstanceID));
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.FlamespireClaw)
                _tethers.Clear(module.Raid.FindSlot(source.InstanceID));
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID is >= (uint)IconID.FlamespireClaw1 and <= (uint)IconID.FlamespireClaw8)
            {
                var slot = module.Raid.FindSlot(actor.InstanceID);
                var order = (int)iconID - (int)IconID.FlamespireClaw1 + 1;
                if (slot >= 0)
                    _order[slot] = order;
                if (order == 1)
                    CurrentBaits.Add(new(module.PrimaryActor, actor, _shape));
            }
        }
    }
}
