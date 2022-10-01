using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    public class RaidCooldowns : IDisposable
    {
        private WorldState _ws;
        private List<(int Slot, ActionID Action, DateTime AvailableAt)> _damageCooldowns = new(); // TODO: this should be improved - determine available cooldowns by class?..

        public RaidCooldowns(WorldState ws)
        {
            _ws = ws;
            _ws.Party.Modified += HandlePartyUpdate;
            _ws.Actors.CastEvent += HandleCast;
            _ws.DirectorUpdate += HandleDirectorUpdate;
        }

        public void Dispose()
        {
            _ws.Party.Modified -= HandlePartyUpdate;
            _ws.Actors.CastEvent -= HandleCast;
            _ws.DirectorUpdate -= HandleDirectorUpdate;
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
            var firstAvailable = _damageCooldowns.Select(e => e.AvailableAt).Min();
            return MathF.Max(0, (float)(firstAvailable - now).TotalSeconds);
        }

        private void HandlePartyUpdate(object? sender, PartyState.OpModify op)
        {
            _damageCooldowns.RemoveAll(e => e.Slot == op.Slot);
        }

        private void HandleCast(object? sender, (Actor actor, ActorCastEvent cast) args)
        {
            if (!args.cast.IsSpell())
                return;
            // see https://i.redd.it/xrtgpras94881.png
            // TODO: AST card buffs?, all non-damage buffs
            _ = args.cast.Action.ID switch
            {
                118 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 15, 120), // BRD battle voice
                //2258 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 15, 60), // NIN trick attack - note that this results in debuff on enemy, which isn't handled properly for now
                3557 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 15, 120), // DRG battle litany
                7396 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 15, 120), // MNK brotherhood
                //7398 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 20, 120), // DRG dragon sight - note that it is single-target rather than raid
                //7436 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 15, 120), // SCH chain stratagem - note that this results in debuff on enemy, which isn't handled properly for now
                7520 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 20, 120), // RDM embolden
                16196 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 20, 120), // DNC technical finish
                16552 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 15, 120), // AST divination
                24405 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 20, 120), // RPR arcane circle
                25785 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 15, 120), // BRD radiant finale - note that even though CD is 110, it's used together with other 2min cds
                25801 => UpdateDamageCooldown(args.actor.InstanceID, args.cast.Action, 30, 120), // SMN searing light
                _ => false
            };
        }

        private bool UpdateDamageCooldown(ulong casterID, ActionID action, float duration, float cooldown)
        {
            int slot = _ws.Party.FindSlot(casterID);
            if (slot < 0 || slot >= PartyState.MaxPartySize) // ignore cooldowns from other alliance parties
                return false;

            var availableAt = _ws.CurrentTime.AddSeconds(cooldown);
            var index = _damageCooldowns.FindIndex(e => e.Slot == slot && e.Action == action);
            if (index < 0)
            {
                _damageCooldowns.Add((slot, action, availableAt));
            }
            else
            {
                _damageCooldowns[index] = (slot, action, availableAt);
            }
            Service.Log($"[RaidCooldowns] Updating damage cooldown: {action} by {_ws.Party[slot]?.Name} will last for {duration:f1}s and will next be available in {cooldown:f1}s; there are now {_damageCooldowns.Count} entries");
            return true;
        }

        private void HandleDirectorUpdate(object? sender, WorldState.OpDirectorUpdate op)
        {
            if (op.UpdateID is 0x40000001 or 0x40000005) // init or fade-out (wipe)
            {
                Service.Log($"[RaidCooldowns] Clearing damage cooldowns ({_damageCooldowns.Count} entries)");
                _damageCooldowns.Clear();
            }
        }
    }
}
