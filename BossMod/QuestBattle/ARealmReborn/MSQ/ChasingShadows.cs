namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 296)]
internal class ChasingShadows(WorldState ws) : QuestBattle(ws)
{
    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        foreach (var p in hints.PotentialTargets)
            p.Priority = p.Actor.OID switch
            {
                0x224 => 0,
                0x376 => 1,
                _ => 2
            };
    }
}
