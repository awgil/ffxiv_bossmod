using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    class CommonActions
    {
        private unsafe FFXIVClientStructs.FFXIV.Client.Game.ActionManager* _actionManager = null;

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

        // check whether specified status is a damage buff
        public bool IsDamageBuff(uint statusID)
        {
            // see https://i.redd.it/xrtgpras94881.png
            // TODO: AST, DRG, BRD, DNC, RDM buffs
            return statusID switch
            {
                49 => true, // medicated
                1185 => true, // MNK brotherhood
                2599 => true, // RPR arcane circle
                2703 => true, // SMN searing light
                _ => false
            };
        }
    }
}
