namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 299)]
internal class LeiasLegacy(WorldState ws) : QuestBattle(ws)
{
    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        foreach (var p in hints.PotentialTargets)
            p.Priority = p.Actor.OID switch
            {
                0x9A => 2,
                0x98 => 0,
                _ => 1
            };
    }
}
