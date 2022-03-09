using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.P4S1
{
    using static BossModule;

    // state related to vengeful belone mechanic
    class VengefulBelone : Component
    {
        private P4S1 _module;
        private List<Actor> _orbs;
        private Dictionary<uint, Role> _orbTargets = new();
        private int _orbsExploded = 0;
        private int[] _playerRuinCount = new int[8];
        private Role[] _playerActingRole = new Role[8];

        private static float _burstRadius = 8;

        private Role OrbTarget(uint instanceID) => _orbTargets.GetValueOrDefault(instanceID, Role.None);

        public VengefulBelone(P4S1 module)
        {
            _module = module;
            _orbs = module.Enemies(OID.Orb);
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_orbTargets.Count == 0 || _orbsExploded == _orbTargets.Count)
                return; // inactive

            int ruinCount = _playerRuinCount[slot];
            if (ruinCount > 2 || (ruinCount == 2 && _playerActingRole[slot] != Role.None))
            {
                hints.Add("Failed orbs...");
            }

            if (_orbs.Where(orb => IsOrbLethal(slot, actor, OrbTarget(orb.InstanceID))).InRadius(actor.Position, _burstRadius).Any())
            {
                hints.Add("GTFO from wrong orb!");
            }

            if (ruinCount < 2)
            {
                // TODO: stack check...
                hints.Add($"Pop next orb {ruinCount + 1}/2!", false);
            }
            else if (ruinCount == 2 && _playerActingRole[slot] == Role.None)
            {
                hints.Add($"Avoid orbs", false);
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_orbTargets.Count == 0 || _orbsExploded == _orbTargets.Count)
                return;

            foreach (var orb in _orbs)
            {
                var orbRole = OrbTarget(orb.InstanceID);
                if (orbRole == Role.None)
                    continue; // this orb has already exploded

                bool lethal = IsOrbLethal(pcSlot, pc, orbRole);
                arena.Actor(orb, lethal ? arena.ColorEnemy : arena.ColorDanger);

                var target = _module.WorldState.Actors.Find(orb.Tether.Target);
                if (target != null)
                {
                    arena.AddLine(orb.Position, target.Position, arena.ColorDanger);
                }

                int goodInRange = 0, badInRange = 0;
                foreach ((var i, var player) in _module.Raid.WithSlot().InRadius(orb.Position, _burstRadius))
                {
                    if (IsOrbLethal(i, player, orbRole))
                        ++badInRange;
                    else
                        ++goodInRange;
                }

                bool goodToExplode = goodInRange == 2 && badInRange == 0;
                arena.AddCircle(orb.Position, _burstRadius, goodToExplode ? arena.ColorSafe : arena.ColorDanger);
            }

            foreach ((int i, var player) in _module.Raid.WithSlot())
            {
                bool nearLethalOrb = _orbs.Where(orb => IsOrbLethal(i, player, OrbTarget(orb.InstanceID))).InRadius(player.Position, _burstRadius).Any();
                arena.Actor(player, nearLethalOrb ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }
        }

        public override void OnStatusGain(Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.OrbRole:
                    _orbTargets[actor.InstanceID] = OrbRoleFromStatusParam(actor.Statuses[index].Extra);
                    break;
                case SID.ThriceComeRuin:
                    ModifyRuinStacks(actor, actor.Statuses[index].Extra);
                    break;
                case SID.ActingDPS:
                    ModifyActingRole(actor, Role.Melee);
                    break;
                case SID.ActingHealer:
                    ModifyActingRole(actor, Role.Healer);
                    break;
                case SID.ActingTank:
                    ModifyActingRole(actor, Role.Tank);
                    break;
            }
        }

        public override void OnStatusLose(Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.ThriceComeRuin:
                    ModifyRuinStacks(actor, 0);
                    break;
                case SID.ActingDPS:
                case SID.ActingHealer:
                case SID.ActingTank:
                    ModifyActingRole(actor, Role.None);
                    break;
            }
        }

        public override void OnStatusChange(Actor actor, int index)
        {
            if ((SID)actor.Statuses[index].ID == SID.ThriceComeRuin)
                ModifyRuinStacks(actor, actor.Statuses[index].Extra);
        }

        public override void OnEventCast(CastEvent info)
        {
            if (info.IsSpell(AID.BeloneBurstsAOETank) || info.IsSpell(AID.BeloneBurstsAOEHealer) || info.IsSpell(AID.BeloneBurstsAOEDPS))
            {
                _orbTargets[info.CasterID] = Role.None;
                ++_orbsExploded;
            }
        }

        private Role OrbRoleFromStatusParam(uint param)
        {
            return param switch
            {
                0x13A => Role.Tank,
                0x13B => Role.Melee,
                0x13C => Role.Healer,
                _ => Role.None
            };
        }

        private bool IsOrbLethal(int slot, Actor player, Role orbRole)
        {
            int ruinCount = _playerRuinCount[slot];
            if (ruinCount >= 2)
                return true; // any orb is now lethal

            var actingRole = _playerActingRole[slot];
            if (ruinCount == 1 && actingRole != Role.None)
                return orbRole != actingRole; // player must clear acting debuff, or he will die

            var playerRole = player.Role == Role.Ranged ? Role.Melee : player.Role;
            return orbRole == playerRole;
        }

        private void ModifyRuinStacks(Actor actor, ushort count)
        {
            int slot = _module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerRuinCount[slot] = count;
        }

        private void ModifyActingRole(Actor actor, Role role)
        {
            int slot = _module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerActingRole[slot] = role;
        }
    }
}
