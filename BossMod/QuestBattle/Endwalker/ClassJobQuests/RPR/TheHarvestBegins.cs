namespace BossMod.QuestBattle.Endwalker.ClassJobQuests.RPR;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 814)]
internal class TheHarvestBegins(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithInteract(0x3836)
            .With(obj => {
                obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x80000015 && diru.Param1 == 1);
            }),

        new QuestObjective(ws)
            .WithConnection(new Waypoint(new Vector3(-143.87f, -4.34f, 171.46f), false))
            .With(obj => {
                var guillotine = false;

                obj.AddAIHints += (player, hints) => {
                    hints.PrioritizeAll();

                    if (!guillotine && player.FindStatus(BossMod.RPR.SID.SoulReaver) != null && World.Actors.Find(player.TargetID) is Actor tar)
                        hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.RPR.AID.Guillotine), tar, ActionQueue.Priority.VeryHigh);
                };

                obj.OnDirectorUpdate += (diru) => guillotine |= diru.UpdateID == 0x80000016 && diru.Param1 == 4;
            })
    ];
}

