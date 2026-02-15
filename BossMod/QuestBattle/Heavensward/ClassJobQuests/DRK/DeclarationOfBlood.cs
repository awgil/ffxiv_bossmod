namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.DRK;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 432)]
internal class DeclarationOfBlood(ZoneModuleArgs args) : QuestBattle(args)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws, ActionDefinitions defs) => [
        new QuestObjective(ws).WithConnection(new Vector3(-142.37f, 3.20f, 677.68f)).WithInteract(0x1E9D44)
    ];
}

