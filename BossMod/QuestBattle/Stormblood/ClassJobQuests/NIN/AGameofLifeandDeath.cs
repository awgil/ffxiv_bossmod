namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.NIN;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 452)]
internal class AGameOfLifeAndDeath(WorldState ws) : QuestBattle(ws)
{
    private QuestObjective Hide() => new QuestObjective(World)
        .With(obj =>
        {
            // check here instead of OnStatusGain since NIN autorotation might have used hide (to reset cds) before step began
            obj.Update += () => obj.CompleteIf(World.Party.Player()?.FindStatus(BossMod.NIN.SID.Hidden) != null);
            obj.AddAIHints += (player, hints) => hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.NIN.AID.Hide), player, ActionQueue.Priority.Medium);
        });

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        Hide(),

        new QuestObjective(ws)
            .WithConnection(new Vector3(658.84f, 21.58f, 61.18f))
            .With(obj => {
                obj.Update += () => {
                    var yuki = World.Actors.FirstOrDefault(x => x.OID == 0x1E62);
                    if (yuki == null)
                        return;
                    obj.CompleteIf(yuki.Position.AlmostEqual(new(659.82f, 56.63f), 3));
                };
            }),

        new QuestObjective(ws)
            .WithInteract(0x1EA740)
            .With(obj => {
                obj.AddAIHints += (player, hints) => hints.StatusesToCancel.Add(((uint)BossMod.NIN.SID.Hidden, player.InstanceID));

                obj.OnModelStateChanged += (act) => obj.CompleteIf(act.OID == 0x1E64 && act.ModelState is {ModelState: 0, AnimState1: 0, AnimState2: 0});
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(607.88f, 29.84f, 43.83f))
            .CompleteAtDestination(),

        Hide(),

        new QuestObjective(ws)
            .WithConnection(new Vector3(577.94f, 40.52f, 18.86f))
            .With(obj => {
                obj.OnActorCombatChanged += (act) => obj.CompleteIf(act.OID == 0 && !act.InCombat);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(524.50f, 52.69f, -21.72f))
            .WithInteract(0x1EA740)
            .With(obj => {
                obj.OnModelStateChanged += (act) => obj.CompleteIf(act.OID == 0x1E63 && act.ModelState is {ModelState: 0, AnimState1: 0, AnimState2: 0});
            }),
    ];
}

