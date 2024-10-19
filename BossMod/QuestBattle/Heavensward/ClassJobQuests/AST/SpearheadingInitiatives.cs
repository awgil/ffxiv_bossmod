namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.AST;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 413)]
internal class SpearheadingInitiatives(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(297.23f, 308.81f, -425.09f))
            .CompleteOnKilled(0x11FA),
        new QuestObjective(ws)
            .WithConnection(new Vector3(215.93f, 327.43f, -456.19f))
            .CompleteOnKilled(0x11F9),
        new QuestObjective(ws)
            .WithConnection(new Vector3(229.05f, 347.18f, -513.37f))
            .CompleteOnKilled(0x11FA),
        new QuestObjective(ws)
            .WithConnection(new Vector3(268.09f, 362.50f, -605.65f))
            .With(obj => {
                obj.OnConditionChange += (flag, value) => obj.CompleteIf(flag == Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas && !value);
            }),
        new QuestObjective(ws)
            .WithConnection(new Vector3(262.16f, 359.18f, -679.22f))
            .WithInteract(0x1E9A80)
    ];
}

