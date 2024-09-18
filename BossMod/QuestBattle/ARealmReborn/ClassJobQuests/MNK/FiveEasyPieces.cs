﻿namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[Quest(BossModuleInfo.Maturity.Contributed, 364)]
internal class FiveEasyPieces(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x640 ? 0 : 1;
    }
}

