namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.THM;

[ZoneModuleInfo(324)]
internal class TheThreatOfSuperiority(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => hints.PrioritizeAll())
            .WithConnection(new Vector3(100.94f, -24.09f, 257.01f))
            .WithInteract(0x1E8A3F)
    ];
}
