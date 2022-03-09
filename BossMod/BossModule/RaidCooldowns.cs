using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    public class RaidCooldowns : IDisposable
    {
        private WorldState _ws;
        private Dictionary<(uint, ActionID), DateTime> _damageCooldowns = new(); // TODO: this should be improved - determine available cooldowns by class?..

        public RaidCooldowns(WorldState ws)
        {
            _ws = ws;
            _ws.Events.Cast += HandleCast;
        }

        public void Dispose()
        {
            _ws.Events.Cast -= HandleCast;
        }

        public void HandleCast(object? sender, CastEvent info)
        {
            if (!info.IsSpell())
                return;
            // see https://i.redd.it/xrtgpras94881.png
            // TODO: AST, DRG, BRD, DNC, RDM damage buffs, all non-damage buffs
            _ = info.Action.ID switch
            {
                7396 => UpdateDamageCooldown(info.CasterID, info.Action, 15, 120), // MNK brotherhood
                24405 => UpdateDamageCooldown(info.CasterID, info.Action, 20, 120), // RPR arcane circle
                25801 => UpdateDamageCooldown(info.CasterID, info.Action, 30, 120), // SMN searing light
                _ => false
            };
        }

        public void Clear()
        {
            Service.Log($"[RaidCooldowns] Clearing damage cooldowns ({_damageCooldowns.Count} entries)");
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

        private bool UpdateDamageCooldown(uint casterID, ActionID action, float duration, float cooldown)
        {
            int slot = _ws.Party.FindSlot(casterID);
            if (slot < 0)
                return false;

            _damageCooldowns[(casterID, action)] = _ws.CurrentTime.AddSeconds(cooldown);
            Service.Log($"[RaidCooldowns] Updating damage cooldown: {action} by {_ws.Party[slot]?.Name} will last for {duration:f1}s and will next be available in {cooldown:f1}s; there are now {_damageCooldowns.Count} entries");
            return true;
        }
    }
}
