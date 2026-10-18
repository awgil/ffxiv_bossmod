namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.DRK;

[ZoneModuleInfo(432)]
internal class DeclarationOfBlood(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnection(new Vector3(-142.37f, 3.20f, 677.68f)).WithInteract(0x1E9D44)
    ];
}

