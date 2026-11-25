namespace BossMod.QuestBattle.Dawntrail.ClassJobQuests.VPR;

public enum OID : uint
{
    Keshkwa = 0x429F,
    WanderingGowrow = 0x42A0,
}

[ZoneModuleInfo(925, territoryID: 1235)]
internal class FangsOfTheViper(WorldState ws) : QuestBattle(ws)
{
    private static bool StepReached(WorldState.OpDirectorUpdate diru, uint step) => diru.UpdateID == 0x80000015 && diru.Param1 >= step;

    private static QuestObjective Talk(WorldState ws, uint nextStep) => new QuestObjective(ws)
        .Named("Speak with Keshkwa")
        .WithInteract((uint)OID.Keshkwa, true)
        .With(obj => obj.OnDirectorUpdate += diru => obj.CompleteIf(StepReached(diru, nextStep)));

    private static QuestObjective Fight(WorldState ws, string name, uint nextStep) => new QuestObjective(ws)
        .Named(name)
        .Hints((player, hints) => hints.PrioritizeAll())
        .With(obj => obj.OnDirectorUpdate += diru => obj.CompleteIf(StepReached(diru, nextStep)));

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        Talk(ws, 2),
        Fight(ws, "Death Rattle", 3),
        Talk(ws, 4),
        Fight(ws, "Flank and rear finishers", 6),
        Talk(ws, 7),
        Fight(ws, "Coils and bites", 8),
        new QuestObjective(ws)
            .Named("Wandering gowrow")
            .Hints((player, hints) => hints.PrioritizeAll())
            .CompleteOnKilled((uint)OID.WanderingGowrow)
    ];
}
