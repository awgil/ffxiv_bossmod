using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    public class RaidState
    {
        public int PlayerSlot { get; private set; } = -1;
        public WorldState.Actor?[] Members { get; init; } // this is fixed-size, but some slots could be empty; when player is removed, gap is created - existing players keep their indices
        private Dictionary<(uint, ActionID), DateTime> _damageCooldowns = new(); // TODO: this should be improved - determine available cooldowns by class?..

        public RaidState(int maxMembers)
        {
            Members = new WorldState.Actor?[maxMembers];
        }

        public WorldState.Actor? this[int slot] => (slot >= 0 && slot < Members.Length) ? Members[slot] : null; // bounds-checking accessor
        public WorldState.Actor? Player() => this[PlayerSlot];

        // select non-null and optionally alive raid members
        public IEnumerable<WorldState.Actor> WithoutSlot(bool includeDead = false)
        {
            for (int i = 0; i < Members.Length; ++i)
            {
                var player = Members[i];
                if (player == null)
                    continue;
                if (player.IsDead && !includeDead)
                    continue;
                yield return player;
            }
        }

        public IEnumerable<(int, WorldState.Actor)> WithSlot(bool includeDead = false)
        {
            for (int i = 0; i < Members.Length; ++i)
            {
                var player = Members[i];
                if (player == null)
                    continue;
                if (player.IsDead && !includeDead)
                    continue;
                yield return (i, player);
            }
        }

        // find a slot index containing specified player (by instance ID); returns -1 if not found
        public int FindSlot(uint instanceID)
        {
            return instanceID != 0 ? Array.FindIndex(Members, x => x?.InstanceID == instanceID) : -1;
        }

        public void UpdatePlayer(uint instanceID)
        {
            PlayerSlot = FindSlot(instanceID);
        }

        public void AddMember(WorldState.Actor actor, bool isPlayer)
        {
            int slot = Array.FindIndex(Members, x => x == null);
            if (slot != -1)
            {
                Members[slot] = actor;
                if (isPlayer)
                    PlayerSlot = slot;
            }
            else
            {
                Service.Log($"[RaidState] Too many raid members: {Members.Length} already exist; skipping new actor {actor.InstanceID:X}");
            }
        }

        public void RemoveMember(WorldState.Actor actor)
        {
            int slot = FindSlot(actor.InstanceID);
            if (slot != -1)
            {
                Members[slot] = null;
                if (PlayerSlot == slot)
                    PlayerSlot = -1;
            }
            else
            {
                Service.Log($"[RaidState] Destroyed player actor {actor.InstanceID:X} not found among raid members");
            }
        }

        public void HandleCast(DateTime t, WorldState.CastResult info)
        {
            if (!info.IsSpell())
                return;
            // see https://i.redd.it/xrtgpras94881.png
            // TODO: AST, DRG, BRD, DNC, RDM damage buffs, all non-damage buffs
            _ = info.Action.ID switch
            {
                7396 => UpdateDamageCooldown(info.CasterID, info.Action, t, 15, 120), // MNK brotherhood
                24405 => UpdateDamageCooldown(info.CasterID, info.Action, t, 20, 120), // RPR arcane circle
                25801 => UpdateDamageCooldown(info.CasterID, info.Action, t, 30, 120), // SMN searing light
                _ => false
            };
        }

        public void ClearRaidCooldowns()
        {
            Service.Log($"[RaidState] Clearing damage cooldowns ({_damageCooldowns.Count} entries)");
            _damageCooldowns.Clear();
        }

        public float NextDamageBuffIn(DateTime now)
        {
            // TODO: this is currently quite hacky
            if (_damageCooldowns.Count == 0)
            {
                // if there are no entries, assume it is an opener and cooldowns are imminent
                // this doesn't handle e.g. someone not pressing CD during opener (but fuck that?)
                return 0;
            }
            // find first ability coming off CD and return time until it happens
            var firstAvailable = _damageCooldowns.Values.Min();
            return MathF.Max(0, (float)(firstAvailable - now).TotalSeconds);
        }

        private bool UpdateDamageCooldown(uint casterID, ActionID action, DateTime t, float duration, float cooldown)
        {
            var member = this[FindSlot(casterID)];
            if (member == null)
                return false;

            _damageCooldowns[(casterID, action)] = t.AddSeconds(cooldown);
            Service.Log($"[RaidState] Updating damage cooldown: {action} by {member.Name} will last for {duration:f1}s and will next be available in {cooldown:f1}s; there are now {_damageCooldowns.Count} entries");
            return true;
        }
    }
}
