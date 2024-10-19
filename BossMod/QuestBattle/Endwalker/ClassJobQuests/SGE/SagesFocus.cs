using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.QuestBattle.Endwalker.ClassJobQuests.SGE;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 813)]
internal class SagesFocus(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(7.71f, 0.00f, 108.57f))
            .With(obj => {
                obj.OnEventObjectAnimation += (act, p1, p2) => obj.CompleteIf(act.OID == 0x1EA1A1 && p1 == 4 && p2 == 8);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(47.85f, 3.00f, 108.20f))
            .Hints((player, hints) => {
                var lalah = World.Actors.FirstOrDefault(x => x.OID == 0x3589 && x.IsTargetable);
                if (lalah == null)
                    return;

                if (lalah.FindStatus(BossMod.SGE.SID.EukrasianDiagnosis) != null)
                    return;

                var gauge = World.Client.GetGauge<SageGauge>();
                if (gauge.Eukrasia > 0)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.SGE.AID.EukrasianDiagnosis), lalah, ActionQueue.Priority.High);
                else
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.SGE.AID.Eukrasia), player, ActionQueue.Priority.High);
            })
            .CompleteOnKilled(0x358B),

        new QuestObjective(ws)
            .WithConnection(new Vector3(85.40f, -0.02f, -43.60f))
            .CompleteAtDestination(),

        new QuestObjective(ws)
            .WithInteract(0x1EB49B)
            .CompleteOnState7(0x1EB49B),

        new QuestObjective(ws)
            .WithConnection(new Vector3(76.39f, -0.02f, -57.71f))
            .WithInteract(0x1EB49C)
            .CompleteOnState7(0x1EB49C),

        new QuestObjective(ws)
            .WithConnection(new Vector3(82.09f, -0.02f, -66.68f))
            .WithInteract(0x1EB49D)
            .CompleteOnState7(0x1EB49D),

        new QuestObjective(ws)
            .WithConnection(new Vector3(61.26f, 0.00f, -83.02f))
    ];
}

