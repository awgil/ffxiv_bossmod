namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.PGL;

[ZoneModuleInfo(322)]
[ZoneModuleInfo(323)]
internal class ReturnOfTheHolyfist(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeAll();

        foreach (var p in hints.PotentialTargets)
            // phase 1 holyfist: force automnk to use standard rotation
            if (p.Actor.Level == 20 && p.Actor.OID == 0x36F)
                p.ForcePriority(0);
    }
}
