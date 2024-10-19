namespace BossMod.QuestBattle.Endwalker.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 960)]
internal class TheGameIsAfoot(WorldState ws) : QuestBattle(ws)
{
    private QuestObjective AutoSkip() => new QuestObjective(World)
        .With(obj => obj.Update += () => obj.CompleteIf(World.Party.Player()?.Position.Z < -400));

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        AutoSkip()
            .WithConnection(new Vector3(257.86f, 4.99f, -1.79f))
            .WithInteract(0x4049)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru is { UpdateID: 0x80000000, Param1: 0x99D95F, Param2: 0 })),

        AutoSkip()
            .WithConnection(new Vector3(270.55f, 6.46f, -8.85f))
            .With(obj => obj.OnConditionChange += (flag, val) => obj.CompleteIf(flag == Dalamud.Game.ClientState.Conditions.ConditionFlag.Jumping61 && !val)),

        AutoSkip()
            .WithConnection(new Vector3(396.33f, 42.81f, -76.20f))
            .WithInteract(0xFF5C0)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru is { UpdateID: 0x80000000, Param1: 0x99DAD7, Param2: 0 })),

        AutoSkip()
            .WithConnection(new Vector3(421.76f, 44.60f, -79.09f))
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru is { UpdateID: 0x10000002, Param1: 0x936D })),

        AutoSkip()
            .WithConnection(new Vector3(436.55f, 50.14f, -105.24f))
            .WithInteract(0x1EB90A)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru is {UpdateID: 0x10000002, Param1: 0x9374})),

        AutoSkip()
            .WithConnection(new Vector3(466.44f, 53.63f, -47.43f))
            .WithInteract(0x1EB90B)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru is {UpdateID: 0x10000002, Param1: 0x9375})),

        AutoSkip()
            .WithConnection(new Vector3(393.72f, 47.66f, -6.72f))
            .WithInteract(0x1EB90C)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru is {UpdateID: 0x10000002, Param1: 0x936F})),

        AutoSkip()
            .WithConnection(new Vector3(407.79f, 43.68f, -78.16f))
            .With(obj => obj.Update += () => obj.CompleteIf(World.Party.Player()?.Position.Z < -400)),

        new QuestObjective(ws)
            .WithConnection(new Vector3(425, 20, -440))
    ];
}

