using Dalamud.Game.ClientState.Conditions;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // extra utilities for healers
    abstract class HealerActions : CommonActions
    {
        public struct PartyMemberState
        {
            public int PredictedHPCur;
            public int PredictedHPDeficit;
            public int NumAttackers;
            public float PredictedHPRatio; // reduced by 0.05 per attacker
            public bool HaveRemovableDebuffs;
        }

        protected bool AllowProtect { get; private set; }
        protected PartyMemberState[] PartyMemberStates { get; private set; } = new PartyMemberState[PartyState.MaxPartySize];

        protected HealerActions(Autorotation autorot, Actor player, QuestLockEntry[] unlockData, Dictionary<ActionID, ActionDefinition> supportedActions)
            : base(autorot, player, unlockData, supportedActions)
        {
        }

        public override void Dispose()
        {
        }

        protected override void UpdateInternalState(int autoAction)
        {
            AllowProtect = Service.Condition[ConditionFlag.BoundByDuty]; // TODO: validate...
            for (int i = 0; i < PartyMemberStates.Length; ++i)
            {
                var actor = Autorot.WorldState.Party[i];
                ref var state = ref PartyMemberStates[i];
                if (actor == null || actor.IsDead || actor.HP.Max == 0)
                {
                    state.PredictedHPCur = state.PredictedHPDeficit = 0;
                    state.PredictedHPRatio = 1;
                    state.HaveRemovableDebuffs = false;
                }
                else
                {
                    state.PredictedHPCur = (int)actor.HP.Cur + Autorot.WorldState.PendingEffects.PendingHPDifference(actor.InstanceID);
                    state.PredictedHPDeficit = (int)actor.HP.Max - state.PredictedHPCur;
                    state.PredictedHPRatio = state.PredictedHPCur / actor.HP.Max;
                    state.HaveRemovableDebuffs = actor.Statuses.Any(s => Utils.StatusIsRemovable(s.ID));
                }
                state.NumAttackers = 0;
            }
            foreach (var enemy in Autorot.PotentialTargets.Valid)
            {
                var targetSlot = Autorot.WorldState.Party.FindSlot(enemy.TargetID);
                if (targetSlot >= 0 && targetSlot < PartyMemberStates.Length)
                {
                    ref var state = ref PartyMemberStates[targetSlot];
                    ++state.NumAttackers;
                    state.PredictedHPRatio -= 0.05f;
                }
            }
        }

        // count alive players with hp ratio < threshold
        protected int CountAOEHealTargets(float radius, WPos center, float hpThreshold = 0.9f)
        {
            int res = 0;
            for (int i = 0; i < PartyMemberStates.Length; ++i)
            {
                var actor = Autorot.WorldState.Party[i];
                if (PartyMemberStates[i].PredictedHPRatio < hpThreshold && actor != null && !actor.IsDead && actor.Position.InCircle(center, radius))
                {
                    ++res;
                }
            }
            return res;
        }

        // find best target for single-target heals; return current predicted HP ratio
        protected (Actor? Target, float HPRatio) FindBestSTHealTarget(float hpThreshold = 0.5f)
        {
            Actor? best = null;
            float bestHPRatio = hpThreshold;
            for (int i = 0; i < PartyMemberStates.Length; ++i)
            {
                var actor = Autorot.WorldState.Party[i];
                if (PartyMemberStates[i].PredictedHPRatio < bestHPRatio && actor != null && !actor.IsDead)
                {
                    best = actor;
                }
            }
            return (best, bestHPRatio);
        }

        // find best target for regen/preshield
        protected Actor? FindProtectTarget(int minAttackersInCombat = 3)
        {
            // TODO: reconsider... consider multiple tanks (select one with stance), consider when protection is not needed (few attackers?)
            if (!AllowProtect)
                return null;
            Actor? best = null;
            int bestScore = 0; // 1 if out of combat and has stance, 2+N if have more attackers than threshold
            for (int i = 0; i < PartyMemberStates.Length; ++i)
            {
                var actor = Autorot.WorldState.Party[i];
                if (actor == null || actor.IsDead || actor.Role != Role.Tank)
                    continue; // consider only alive tanks

                int curScore = 0;
                var numAttackers = PartyMemberStates[i].NumAttackers;
                if (numAttackers >= minAttackersInCombat)
                {
                    curScore = 2 + numAttackers - minAttackersInCombat;
                }
                else if (numAttackers == 0 && !actor.InCombat && HasTankStance(actor))
                {
                    curScore = 1;
                }

                if (curScore > bestScore)
                {
                    best = actor;
                    bestScore = curScore;
                }
            }
            return best;
        }

        // check whether given actor has tank stance
        protected bool HasTankStance(Actor a)
        {
            var stanceSID = a.Class switch
            {
                Class.WAR => (uint)WAR.SID.Defiance,
                Class.PLD => (uint)PLD.SID.IronWill,
                _ => 0u
            };
            return stanceSID != 0 && a.FindStatus(stanceSID) != null;
        }

        // find target for esuna, if available
        protected Actor? FindEsunaTarget()
        {
            for (int i = 0; i < PartyMemberStates.Length; ++i)
            {
                if (PartyMemberStates[i].HaveRemovableDebuffs)
                {
                    var actor = Autorot.WorldState.Party[i];
                    if (actor != null && !actor.IsDead)
                        return actor;
                }
            }
            return null;
        }
    }
}
