using Dalamud.Game.ClientState.Conditions;

namespace BossMod;

// extra utilities for healers
abstract class HealerActions(Autorotation autorot, Actor player, uint[] unlockData, Dictionary<ActionID, ActionDefinition> supportedActions) : CommonActions(autorot, player, unlockData, supportedActions)
{
    public struct PartyMemberState
    {
        public int PredictedHPCur;
        public int PredictedHPDeficit;
        public float AttackerStrength; // 0.05 per attacker by default
        public float PredictedHPRatio; // reduced by sum of attacker strengths, unless at full hp; increased by active hots and incoming heals from other healers
        public bool HaveRemovableDebuffs;
    }

    protected bool AllowProtect { get; private set; }
    protected readonly PartyMemberState[] PartyMemberStates = new PartyMemberState[PartyState.MaxPartySize];

    protected override void UpdateInternalState(int autoAction)
    {
        AllowProtect = Service.Condition[ConditionFlag.BoundByDuty]; // TODO: validate...

        BitMask incomingEsunas = new();
        foreach (var esunaCaster in Autorot.WorldState.Party.WithoutSlot().Where(a => a.CastInfo?.IsSpell(WHM.AID.Esuna) ?? false))
            incomingEsunas.Set(Autorot.WorldState.Party.FindSlot(esunaCaster.CastInfo!.TargetID));

        for (int i = 0; i < PartyMemberStates.Length; ++i)
        {
            var actor = Autorot.WorldState.Party[i];
            ref var state = ref PartyMemberStates[i];
            state.HaveRemovableDebuffs = false;
            if (actor == null || actor.IsDead || actor.HPMP.MaxHP == 0)
            {
                state.PredictedHPCur = state.PredictedHPDeficit = 0;
                state.PredictedHPRatio = 1;
            }
            else
            {
                state.PredictedHPCur = (int)actor.HPMP.CurHP + Autorot.WorldState.PendingEffects.PendingHPDifference(actor.InstanceID);
                state.PredictedHPDeficit = (int)actor.HPMP.MaxHP - state.PredictedHPCur;
                state.PredictedHPRatio = (float)state.PredictedHPCur / actor.HPMP.MaxHP;
                bool actorValidForEsuna = actor.IsTargetable && !incomingEsunas[i];
                foreach (var s in actor.Statuses)
                {
                    if (!state.HaveRemovableDebuffs && actorValidForEsuna && Utils.StatusIsRemovable(s.ID))
                        state.HaveRemovableDebuffs = true;
                    if (IsHOT(s.ID))
                        state.PredictedHPRatio += 0.1f;
                }
            }
            state.AttackerStrength = 0;
        }
        foreach (var incomingHealTargets in Autorot.WorldState.Party.WithoutSlot().Select(a => CastedHealTargets(Autorot.WorldState, a)))
        {
            foreach (var slot in incomingHealTargets.SetBits())
            {
                if (slot < PartyMemberStates.Length)
                {
                    PartyMemberStates[slot].PredictedHPRatio += 0.2f;
                }
            }
        }
        foreach (var enemy in Autorot.Hints.PotentialTargets)
        {
            var targetSlot = Autorot.WorldState.Party.FindSlot(enemy.Actor.TargetID);
            if (targetSlot >= 0 && targetSlot < PartyMemberStates.Length)
            {
                ref var state = ref PartyMemberStates[targetSlot];
                state.AttackerStrength += enemy.AttackStrength;
                if (state.PredictedHPRatio < 0.99f)
                    state.PredictedHPRatio -= enemy.AttackStrength;
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

    // count alive players with hp ratio < threshold -or- that are valid preshield targets (have no specified status and have predicted incoming damage in specified time)
    protected int CountAOEPreshieldTargets(float radius, WPos center, uint shieldSID, float minTime, float maxTime, float hpThreshold = 0.9f)
    {
        int res = 0;
        for (int i = 0; i < PartyMemberStates.Length; ++i)
        {
            var actor = Autorot.WorldState.Party[i];
            if (actor != null && !actor.IsDead && actor.Position.InCircle(center, radius))
            {
                bool valid = PartyMemberStates[i].PredictedHPRatio < hpThreshold;
                if (!valid)
                {
                    var shield = StatusDetails(actor, shieldSID, Autorot.WorldState.Party.Player()?.InstanceID ?? 0).Left;
                    if (shield <= minTime)
                    {
                        foreach (var e in Autorot.Hints.PredictedDamage.Where(e => e.players[i]))
                        {
                            var time = MathF.Max(0, (float)(e.activation - Autorot.WorldState.CurrentTime).TotalSeconds);
                            if (time >= minTime && time <= maxTime)
                            {
                                valid = true;
                                break;
                            }
                        }
                    }
                }

                if (valid)
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
    protected Actor? FindProtectTarget(float minAttackerStrengthInCombat = 0.15f)
    {
        if (!AllowProtect)
            return null;
        Actor? best = null;
        float bestScore = 0; // 1 if out of combat and has stance, 2+X if have more attacker strength than threshold
        for (int i = 0; i < PartyMemberStates.Length; ++i)
        {
            var actor = Autorot.WorldState.Party[i];
            if (actor == null || actor.IsDead || actor.Role != Role.Tank)
                continue; // consider only alive tanks

            float curScore = 0;
            var strength = PartyMemberStates[i].AttackerStrength;
            if (strength >= minAttackerStrengthInCombat)
            {
                curScore = 2 + strength - minAttackerStrengthInCombat;
            }
            else if (strength == 0 && !actor.InCombat && CommonDefinitions.HasTankStance(actor))
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

    protected static bool IsHOT(uint sid)
    {
        return (WHM.SID)sid is WHM.SID.Medica2 or WHM.SID.Asylum or WHM.SID.Regen;
    }

    protected static BitMask CastedHealTargets(WorldState ws, Actor a)
    {
        BitMask res = new();
        if (a.CastInfo == null || a.Role != Role.Healer || !a.CastInfo.IsSpell())
            return res;
        switch (a.CastInfo.Action.ID)
        {
            case (uint)WHM.AID.Cure1:
            case (uint)WHM.AID.Cure2:
            case (uint)SCH.AID.Physick:
            case (uint)SCH.AID.Adloquium:
                res.Set(ws.Party.FindSlot(a.CastInfo.TargetID));
                break;
            case (uint)WHM.AID.Medica1:
            case (uint)SCH.AID.Succor:
                res = ws.Party.WithSlot().InRadius(a.Position, 15).Mask();
                break;
            case (uint)WHM.AID.Medica2:
                res = ws.Party.WithSlot().InRadius(a.Position, 20).Mask();
                break;
            case (uint)WHM.AID.Cure3:
                res = ws.Party.WithSlot().InRadius((ws.Actors.Find(a.CastInfo.TargetID) ?? a).Position, 20).Mask();
                break;
        }
        return res;
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
