﻿namespace BossMod.QuestBattle.ARealmReborn.StarcrossedRivals;

[Quest(BossModuleInfo.Maturity.WIP, 321)]
internal class Quest(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        foreach (var h in hints.PotentialTargets)
        {
            h.Priority = h.Actor.OID switch
            {
                0x2BD or 0x2BE => 2,
                0x2BA => 0,
                _ => 1
            };
        }
    }
}
