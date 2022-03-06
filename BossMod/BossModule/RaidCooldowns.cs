using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    public class RaidCooldowns
    {
        private Dictionary<(uint, ActionID), DateTime> _damageCooldowns = new(); // TODO: this should be improved - determine available cooldowns by class?..

        public void HandleCast(DateTime t, CastEvent info)
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

        public void Clear()
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
            _damageCooldowns[(casterID, action)] = t.AddSeconds(cooldown);
            Service.Log($"[RaidState] Updating damage cooldown: {action} by {casterID:X} will last for {duration:f1}s and will next be available in {cooldown:f1}s; there are now {_damageCooldowns.Count} entries");
            return true;
        }
    }
}
