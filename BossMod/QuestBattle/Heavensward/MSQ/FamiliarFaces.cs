﻿namespace BossMod.QuestBattle.Heavensward.MSQ;

[Quest(BossModuleInfo.Maturity.Contributed, 401)]
internal class FamiliarFaces(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x106C ? 1 : 0;
    }
}

