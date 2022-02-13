using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    class CommonActions
    {
        private unsafe FFXIVClientStructs.FFXIV.Client.Game.ActionManager* _actionManager = null;
        private Dictionary<(uint, uint), DateTime> _raidCooldowns = new();

        protected unsafe CommonActions()
        {
            _actionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        }

        public unsafe float ActionCooldown(ActionID action)
        {
            var type = (FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type;
            var id = action.ID;
            return _actionManager->GetRecastTime(type, id) - _actionManager->GetRecastTimeElapsed(type, id);
        }

        public float SpellCooldown<AID>(AID spell) where AID : Enum
        {
            return ActionCooldown(ActionID.MakeSpell(spell));
        }

        public float StatusDuration(float dur)
        {
            // note: when buff is applied, it has large negative duration, and it is updated to correct one ~0.6sec later
            return MathF.Abs(dur);
        }

        public void ClearRaidCooldowns()
        {
            _raidCooldowns.Clear();
        }

        public float NextDamageBuffIn()
        {
            return _raidCooldowns.Count == 0 ? 0 : MathF.Max(0, (float)(_raidCooldowns.Values.Min() - DateTime.Now).TotalSeconds);
        }

        // check whether specified status is a damage buff, and if so, update cooldown
        public bool CheckDamageBuff(Status status)
        {
            // see https://i.redd.it/xrtgpras94881.png
            // TODO: AST, DRG, BRD, DNC, RDM buffs
            return status.StatusId switch
            {
                49 => true, // medicated
                1185 => UpdateRaidCooldown(status, 15, 120), // MNK brotherhood
                2599 => UpdateRaidCooldown(status, 20, 120), // RPR arcane circle
                2703 => UpdateRaidCooldown(status, 30, 120), // SMN searing light
                _ => false
            };
        }

        private bool UpdateRaidCooldown(Status status, float duration, float cooldown)
        {
            _raidCooldowns[(status.SourceID, status.StatusId)] = DateTime.Now.AddSeconds(cooldown - duration + StatusDuration(status.RemainingTime));
            return true;
        }
    }
}
