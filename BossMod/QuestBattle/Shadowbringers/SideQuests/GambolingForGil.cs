using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.QuestBattle.Shadowbringers.SideQuests;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 670)]
internal class GambolingForGil(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => {
                var g = ws.Client.GetGauge<DancerGauge>();

                if (player.FindStatus(DNC.SID.ClosedPosition) == null)
                {
                    var nashmeira = World.Party.WithoutSlot().FirstOrDefault(x => x.OID == 0x29D1);
                    if (nashmeira != null)
                        hints.ActionsToExecute.Push(ActionID.MakeSpell(DNC.AID.ClosedPosition), nashmeira, ActionQueue.Priority.High);
                }

                if (g.DanceSteps[0] == 0)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(DNC.AID.StandardStep), player, ActionQueue.Priority.High);

                if (g.StepIndex == 2)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(DNC.AID.DoubleStandardFinish), player, ActionQueue.Priority.High);
            })
    ];
}

