using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.AI
{
    // utility for using actions safely (without spamming, not in cutscenes, etc)
    class UseAction
    {
        public DateTime LastUsedTimestamp { get; private set; }
        private unsafe FFXIVClientStructs.FFXIV.Client.Game.ActionManager* _actionManager = null;

        public bool InCutscene => Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78];

        public unsafe UseAction()
        {
            _actionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        }

        public unsafe float Cooldown(ActionID action)
        {
            var recastGroup = _actionManager->GetRecastGroup((int)action.Type, action.ID);
            var recast = _actionManager->GetRecastGroupDetail(recastGroup);
            return recast != null ? recast->Total - recast->Elapsed : 0;
        }

        public unsafe float Range(ActionID action)
        {
            return action.Type == ActionType.Spell ? FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(action.ID) : 0;
        }

        public unsafe bool Execute(ActionID action, ulong target)
        {
            var now = DateTime.Now;
            if (InCutscene || (now - LastUsedTimestamp).TotalMilliseconds < 100)
                return false;

            _actionManager->UseAction((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID, (long)target);
            LastUsedTimestamp = now;
            return true;
        }
    }
}
