namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.SCH;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 376)]
internal class ForgottenButNotGone(ZoneModuleArgs args) : QuestBattle(args)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws, ActionDefinitions defs) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(25.61f, 34.08f, 223.35f))
            .WithInteract(0x1E8E5A)
    ];
}

