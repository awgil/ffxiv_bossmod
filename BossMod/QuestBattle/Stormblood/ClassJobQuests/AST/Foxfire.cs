namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.AST;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 443)]
internal class Foxfire(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(31.35f, 3.00f, -59.26f))
            .PauseForCombat(false)
            .CompleteOnKilled(0x1DB2),

        new QuestObjective(ws)
            .WithConnection(new Vector3(13.27f, 4.00f, 33.05f))
            .PauseForCombat(false)
            .With(obj => {
                obj.OnActorTargetableChanged += (act) => obj.CompleteIf(act.OID == 0x1DB0 && !act.IsTargetable);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-41.13f, 15.00f, 34.20f))
            .PauseForCombat(false)
            .CompleteOnKilled(0x1DB3),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-47.43f, 16.88f, 1.69f))
            .PauseForCombat(false)
            .Hints((player, hints) => {
                if (World.Actors.FirstOrDefault(x => x.OID is 0x1DC2 or 0x1E13 && x.HPMP.CurHP < x.HPMP.MaxHP && x.IsAlly) is Actor w)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.AST.AID.BeneficII), w, ActionQueue.Priority.High + 500);

                if (World.Actors.FirstOrDefault(x => x.OID is 0x1E18 && x.IsAlly) is Actor u)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.AST.AID.BeneficII), u, ActionQueue.Priority.High + 500);
            })
            .With(obj => obj.OnActorEventStateChanged += (act) => obj.CompleteIf(act.OID == 0x1E19 && act.EventState == 1)),

        new QuestObjective(ws)
            .Hints((player, hints) => {
                if (World.Actors.FirstOrDefault(x => x.OID is 0x1E18 && x.IsAlly) is Actor u)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.AST.AID.AspectedBenefic), u, ActionQueue.Priority.High + 500);
            })
    ];
}

