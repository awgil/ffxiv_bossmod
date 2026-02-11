using BossMod.QuestBattle;

namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.THM;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 324)]
internal class TheThreatOfSuperiority(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => hints.PrioritizeAll())
            .WithConnection(new Vector3(100.94f, -24.09f, 257.01f))
            .WithInteract(0x1E8A3F)
    ];
}
